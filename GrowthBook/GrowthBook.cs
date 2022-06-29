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
        private Context context;
        private Dictionary<string, ExperimentAssignment> _assigned;
        private List<Func<Experiment, ExperimentResult, Task>> _subscriptions;

        public GrowthBook(Context context) {
            this.context = context;

            //# Deprecated args
            //_user = user
            //_groups = groups
            //_overrides = overrides
            //_forcedVariations = forcedVariations

            //_tracked = { }
            _assigned = new Dictionary<string, ExperimentAssignment>();
            _subscriptions = new List<Func<Experiment, ExperimentResult, Task>>();
        }

        public JObject Attributes {
            get { return context.Attributes; }
            set { context.Attributes = value; }
        }

        public IDictionary<string, Feature> Features {
            get { return context.Features; }
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

        public FeatureResult EvalFeature(string key) {
            Feature feature;
            if (!context.Features.TryGetValue(key, out feature)) {
                return new FeatureResult { Source = "unknownFeature" };
            }

            foreach (FeatureRule rule in feature.Rules) {
                if (rule.Condition != null) {
                    if (!Utilities.EvalCondition(context.Attributes, rule.Condition)) {
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
            ExperimentResult result = _Run(experiment);

            ExperimentAssignment prev;
            if (!_assigned.TryGetValue(experiment.Key, out prev) || prev.Result.InExperiment != result.InExperiment || prev.Result.VariationId != result.VariationId) {
                _assigned.Add(experiment.Key, new ExperimentAssignment { Experiment = experiment, Result = result });
                foreach(Func<Experiment, ExperimentResult, Task> cb in _subscriptions) {
                    try {
                        cb.Invoke(experiment, result);
                    } catch (Exception) { }
                }
            }

            return result;
        }

        private ExperimentResult _Run(Experiment experiment) {
            // 1. If experiment has less than 2 variations, return immediately
            if (experiment.Variations.Count < 2) {
                return GetExperimentResult(experiment);
            }

            // 2. If growthbook is disabled, return immediately
            if (!context.Enabled) {
                return GetExperimentResult(experiment);
            }

            // 3. If experiment is forced via a querystring in the url
            int? queryString = Utilities.GetQueryStringOverride(experiment.Key, context.Url, experiment.Variations.Count);
            if (queryString != null) {
                return GetExperimentResult(experiment, (int)queryString);
            }

            // 4. If variation is forced in the context
            JToken forcedVariation;
            if (context.ForcedVariations.TryGetValue(experiment.Key, out forcedVariation)) {
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
            if (experiment.Condition != null && !Utilities.EvalCondition(context.Attributes, experiment.Condition)) {
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
            if (context.QaMode) {
                return GetExperimentResult(experiment);
            }

            // 13. Build the result object
            ExperimentResult result = GetExperimentResult(experiment, assigned, true);

            // 14. Fire the tracking callback if set
            if (context.TrackingCallback != null) {
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
            if (context.Attributes.ContainsKey(attr)) {
                return context.Attributes[attr].ToString();
            }
            return string.Empty;
        }

        private bool UrlIsValid(string pattern) {
            if (string.IsNullOrEmpty(context.Url)) {
                return false;
            }

            try {
                Regex r = new Regex(pattern);
                if (r.IsMatch(context.Url)) {
                    return true;
                }

                string pathOnly = Regex.Replace("^[^/]*/", "/", Regex.Replace(@"^https?:\/\/", "", context.Url));
                if (r.IsMatch(pathOnly)) {
                    return true;
                }

                return false;
            } catch (Exception) {
                return true;
            }
        }
    }
}

//class GrowthBook(object):
//    def destroy(self) -> None:
//        self._subscriptions.clear()
//        self._tracked.clear()
//        self._assigned.clear()
//        self._trackingCallback = None
//        self._forcedVariations.clear()
//        self._overrides.clear()
//        self._groups.clear()
//        self._attributes.clear()
//        self._features.clear()



//    def getAllResults(self):
//        return self._assigned.copy()





//    def subscribe(self, callback) :
//        self._subscriptions.add(callback)
//        return lambda: self._subscriptions.remove(callback)



//    def _track(self, experiment: Experiment, result: Result) -> None:
//        if not self._trackingCallback:
//            return None
//        key = (
//            result.hashAttribute
//            + str(result.hashValue)
//            + experiment.key
//            + str(result.variationId)
//        )
//        if not self._tracked.get(key):
//            try:
//                self._trackingCallback(experiment= experiment, result= result)
//                self._tracked[key] = True
//            except Exception:
//                pass




