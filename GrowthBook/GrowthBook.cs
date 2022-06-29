using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrowthBook
{
    /// <summary>
    /// This is the Python client library for GrowthBook, the open-source
    //  feature flagging and A/B testing platform.
    //  More info at https://www.growthbook.io
    /// </summary>
    public class GrowthBook
    {
        private bool enabled;
        private JObject attributes;
        private string url;
        private IDictionary<string, Feature> features;
        private JObject forcedVariations;
        private bool qaMode;
        private Action<Experiment, ExperimentResult> trackingCallback;

        private Dictionary<string, ExperimentAssignment> assigned;
        private HashSet<string> tracked;
        private List<Action<Experiment, ExperimentResult>> subscriptions;

        public GrowthBook(Context context) {
            enabled = context.Enabled;
            attributes = context.Attributes;
            url = context.Url;
            features = context.Features;
            forcedVariations = context.ForcedVariations;
            qaMode = context.QaMode;
            trackingCallback = context.TrackingCallback;

            tracked = new HashSet<string>();
            assigned = new Dictionary<string, ExperimentAssignment>();
            subscriptions = new List<Action<Experiment, ExperimentResult>>();
        }

        ~GrowthBook() {
            Destroy();
        }

        public JObject Attributes {
            get { return attributes; }
            set { attributes = value; }
        }

        public IDictionary<string, Feature> Features {
            get { return features; }
        }

        public void Destroy() {
            subscriptions.Clear();
            tracked.Clear();
            assigned.Clear();
            trackingCallback = null;
            forcedVariations = null;
            attributes = null;
            features.Clear();
        }

        public bool IsOn(string key) {
            return EvalFeature(key).On;
        }

        public bool IsOff(string key) {
            return EvalFeature(key).Off;
        }

        public T GetFeatureValue<T>(string key, T fallback) {
            FeatureResult result = EvalFeature(key);
            if (result == null) {
                return fallback;
            }
            return result.Value.ToObject<T>();
        }

        public IDictionary<string, ExperimentAssignment> GetAllResults() {
            return assigned;
        }

        public Action Subscribe(Action<Experiment, ExperimentResult> callback) {
            subscriptions.Add(callback);
            return () => subscriptions.Remove(callback);
        }

        public FeatureResult EvalFeature(string key) {
            Feature feature;
            if (!features.TryGetValue(key, out feature)) {
                return new FeatureResult { Source = "unknownFeature" };
            }

            foreach (FeatureRule rule in feature.Rules) {
                if (rule.Condition != null) {
                    if (!Utilities.EvalCondition(attributes, rule.Condition)) {
                        continue;
                    }
                }
                if (rule.Force != null) {
                    if (rule.Coverage < 1) {
                        string hashValue = GetHashValue(rule.HashAttribute);
                        if (string.IsNullOrEmpty(hashValue)) {
                            continue;
                        }

                        double n = Utilities.Hash(hashValue + key);

                        if (n > rule.Coverage) {
                            continue;
                        }
                    }

                    return new FeatureResult { Value = rule.Force, Source = "force" };
                }
                
                if (rule.Variations == null) {
                    continue;
                }

                Experiment exp = new Experiment {
                    Key = rule.Key ?? key,
                    Variations = rule.Variations,
                    Coverage = rule.Coverage,
                    Weights = rule.Weights,
                    HashAttribute = rule.HashAttribute,
                    Namespace = rule.Namespace
                };

                ExperimentResult result = Run(exp);

                if (!result.InExperiment) {
                    continue;
                }

                return new FeatureResult { Value = result.Value, Source = "experiment", Experiment = exp, ExperimentResult = result };
            }

            return new FeatureResult { Value = feature.DefaultValue, Source = "defaultValue" };
        }

        public ExperimentResult Run(Experiment experiment) {
            ExperimentResult result = RunExperiment(experiment);

            ExperimentAssignment prev;
            if (!assigned.TryGetValue(experiment.Key, out prev) || prev.Result.InExperiment != result.InExperiment || prev.Result.VariationId != result.VariationId) {
                assigned.Add(experiment.Key, new ExperimentAssignment { Experiment = experiment, Result = result });
                foreach(Action<Experiment, ExperimentResult> cb in subscriptions) {
                    try {
                        cb.Invoke(experiment, result);
                    } catch (Exception) { }
                }
            }

            return result;
        }

        private ExperimentResult RunExperiment(Experiment experiment) {
            // 1. If experiment has less than 2 variations, return immediately
            if (experiment.Variations.Count < 2) {
                return GetExperimentResult(experiment);
            }

            // 2. If growthbook is disabled, return immediately
            if (!enabled) {
                return GetExperimentResult(experiment);
            }

            // 3. If experiment is forced via a querystring in the url
            int? queryString = Utilities.GetQueryStringOverride(experiment.Key, url, experiment.Variations.Count);
            if (queryString != null) {
                return GetExperimentResult(experiment, (int)queryString);
            }

            // 4. If variation is forced in the context
            JToken forcedVariation;
            if (forcedVariations.TryGetValue(experiment.Key, out forcedVariation)) {
                return GetExperimentResult(experiment, forcedVariation.ToObject<int>());
            }

            // 5. If experiment is a draft or not active, return immediately
            if (!experiment.Active) {
                return GetExperimentResult(experiment);
            }

            // 6. Get the user hash attribute and value
            string hashAttribute = experiment.HashAttribute ?? "id";
            string hashValue = GetHashValue(hashAttribute);
            if (string.IsNullOrEmpty(hashValue)) {
                return GetExperimentResult(experiment);
            }

            // 7. Exclude if user not in experiment.namespace
            if (experiment.Namespace != null && !Utilities.InNamespace(hashValue, experiment.Namespace)) {
                return GetExperimentResult(experiment);
            }

            // 8. Exclude if condition is false
            if (experiment.Condition != null && !Utilities.EvalCondition(attributes, experiment.Condition)) {
                return GetExperimentResult(experiment);
            }

            // 9. Get bucket ranges and choose variation
            IList<BucketRange> ranges = Utilities.GetBucketRanges(experiment.Variations.Count, experiment.Coverage, experiment.Weights);
            double n = Utilities.Hash(hashValue + experiment.Key);
            int assigned = Utilities.ChooseVariation(n, ranges);

            // 10. Return if not in experiment
            if (assigned < 0) {
                return GetExperimentResult(experiment);
            }

            // 11. If experiment is forced, return immediately
            if (experiment.Force != null) {
                return GetExperimentResult(experiment, (int)experiment.Force);
            }

            // 12. Exclude if in QA mode
            if (qaMode) {
                return GetExperimentResult(experiment);
            }

            // 13. Build the result object
            ExperimentResult result = GetExperimentResult(experiment, assigned, true);

            // 14. Fire the tracking callback if set
            if (trackingCallback != null) {
                // TODO: CALL THE TRACKING CALLBACK
                //self._track(experiment, result)
            }

            // 15. Return the result
            return result;
        }

        private ExperimentResult GetExperimentResult(Experiment experiment, int variationId = -1, bool hashUsed = false) {
            string hashAttribute = experiment.HashAttribute ?? "id";
            
            bool inExperiment = true;
            if (variationId < 0 || variationId > experiment.Variations.Count - 1) {
                variationId = 0;
                inExperiment = false;
            }

            return new ExperimentResult {
                InExperiment = inExperiment,
                HashAttribute = hashAttribute,
                HashUsed = hashUsed,
                HashValue = GetHashValue(hashAttribute),
                Value = experiment.Variations[variationId],
                VariationId = variationId
            };
        }

        private string GetHashValue(string attr) {
            if (attributes.ContainsKey(attr)) {
                return attributes[attr].ToString();
            }
            return string.Empty;
        }

        private bool UrlIsValid(string pattern) {
            if (string.IsNullOrEmpty(url)) {
                return false;
            }

            try {
                Regex r = new Regex(pattern);
                if (r.IsMatch(url)) {
                    return true;
                }

                string pathOnly = Regex.Replace("^[^/]*/", "/", Regex.Replace(@"^https?:\/\/", "", url));
                if (r.IsMatch(pathOnly)) {
                    return true;
                }

                return false;
            } catch (Exception) {
                return true;
            }
        }

        private void Track(Experiment experiment, ExperimentResult result) {
            if (trackingCallback == null) {
                return;
            }

            string key = result.HashAttribute + result.HashValue + experiment.Key + result.VariationId;
            if (!tracked.Contains(key)) {
                try {
                    trackingCallback(experiment, result);
                    tracked.Add(key);
                } catch (Exception) { }
            }
        }
    }
}