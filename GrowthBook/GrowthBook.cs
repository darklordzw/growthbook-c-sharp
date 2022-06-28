using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        public GrowthBook(Context context) {
            this.context = context;

            //# Deprecated args
            //_user = user
            //_groups = groups
            //_overrides = overrides
            //_forcedVariations = forcedVariations

            //_tracked = { }
            //_assigned = { }
            //_subscriptions = set()
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
            // TODO: Port this next!
            return null;
        }
    }
}

//class GrowthBook(object):
//    def getFeatures(self) -> "dict[str,Feature]":
//        return self._features

//    def setAttributes(self, attributes: dict) -> None:
//        self._attributes = attributes

//    def getAttributes(self) -> dict:
//        return self._attributes

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

//    def evalFeature(self, key: str) -> FeatureResult:
//        if key not in self._features:
//            return FeatureResult(None, "unknownFeature")

//        feature = self._features[key]
//        for rule in feature.rules:
//            if rule.condition:
//                if not evalCondition(self._attributes, rule.condition):
//                    continue
//            if rule.force is not None:
//                if rule.coverage< 1:
//                    hashValue = self._getHashValue(rule.hashAttribute)
//                    if not hashValue:
//                        continue

//                    n = hash(hashValue + key)

//                    if n > rule.coverage:
//                        continue
//                return FeatureResult(rule.force, "force")

//            if rule.variations is None:
//                continue

//            exp = Experiment(
//                key= rule.key or key,
//                variations= rule.variations,
//                coverage= rule.coverage,
//                weights= rule.weights,
//                hashAttribute= rule.hashAttribute,
//                namespace=rule.namespace,
//            )

//            result = self.run(exp)

//            if not result.inExperiment:
//                continue

//            return FeatureResult(result.value, "experiment", exp, result)

//        return FeatureResult(feature.defaultValue, "defaultValue")

//    def getAllResults(self):
//        return self._assigned.copy()

//    def _getHashValue(self, attr: str) -> str:
//        if attr in self._attributes:
//            return str(self._attributes[attr])
//        if attr in self._user:
//            return str(self._user[attr])
//        return ""

//    def run(self, experiment: Experiment) -> Result:
//        result = self._run(experiment)

//        prev = self._assigned.get(experiment.key, None)
//        if (
//            not prev
//            or prev["result"].inExperiment != result.inExperiment
//            or prev["result"].variationId != result.variationId
//        ):
//            self._assigned[experiment.key] = {
//                "experiment": experiment,
//                "result": result,
//            }
//            for cb in self._subscriptions:
//                try:
//                    cb(experiment, result)
//                except Exception:
//                    pass

//        return result

//    def subscribe(self, callback) :
//        self._subscriptions.add(callback)
//        return lambda: self._subscriptions.remove(callback)

//    def _run(self, experiment: Experiment) -> Result:
//        # 1. If experiment has less than 2 variations, return immediately
//        if len(experiment.variations) < 2:
//            return self._getExperimentResult(experiment)
//        # 2. If growthbook is disabled, return immediately
//        if not self._enabled:
//            return self._getExperimentResult(experiment)
//        # 2.5. If the experiment props have been overridden, merge them in
//        if self._overrides.get(experiment.key, None):
//            experiment.update(self._overrides[experiment.key])
//# 3. If experiment is forced via a querystring in the url
//        qs = getQueryStringOverride(
//            experiment.key, self._url, len(experiment.variations)
//        )
//        if qs is not None:
//            return self._getExperimentResult(experiment, qs)
//        # 4. If variation is forced in the context
//        if self._forcedVariations.get(experiment.key, None) is not None:
//            return self._getExperimentResult(
//                experiment, self._forcedVariations[experiment.key]
//            )
//        # 5. If experiment is a draft or not active, return immediately
//        if experiment.status == "draft" or not experiment.active:
//            return self._getExperimentResult(experiment)
//# 6. Get the user hash attribute and value
//        hashAttribute = experiment.hashAttribute or "id"
//        hashValue = self._getHashValue(hashAttribute)
//        if not hashValue:
//            return self._getExperimentResult(experiment)

//        # 7. Exclude if user not in experiment.namespace
//        if experiment.namespace and not inNamespace(hashValue, experiment.namespace):
//            return self._getExperimentResult(experiment)

//        # 7.5. If experiment has an include property
//        if experiment.include:
//            try:
//                if not experiment.include():
//                    return self._getExperimentResult(experiment)
//            except Exception:
//                return self._getExperimentResult(experiment)

//        # 8. Exclude if condition is false
//        if experiment.condition and not evalCondition(
//            self._attributes, experiment.condition
//        ):
//            return self._getExperimentResult(experiment)

//        # 8.1. Make sure user is in a matching group
//        if experiment.groups and len(experiment.groups) :
//            expGroups = self._groups or { }
//matched = False
//            for group in experiment.groups:
//                if expGroups[group]:
//                    matched = True
//            if not matched:
//                return self._getExperimentResult(experiment)
//        # 8.2. If experiment.url is set, see if it's valid
//        if experiment.url:
//            if not self._urlIsValid(experiment.url):
//                return self._getExperimentResult(experiment)

//# 9. Get bucket ranges and choose variation
//        ranges = getBucketRanges(
//            len(experiment.variations), experiment.coverage or 1, experiment.weights
//        )
//        n = hash(hashValue + experiment.key)
//        assigned = chooseVariation(n, ranges)

//        # 10. Return if not in experiment
//        if assigned< 0:
//            return self._getExperimentResult(experiment)

//        # 11. If experiment is forced, return immediately
//        if experiment.force is not None:
//            return self._getExperimentResult(experiment, experiment.force)

//        # 12. Exclude if in QA mode
//        if self._qaMode:
//            return self._getExperimentResult(experiment)

//        # 12.5. If experiment is stopped, return immediately
//        if experiment.status == "stopped":
//            return self._getExperimentResult(experiment)

//# 13. Build the result object
//        result = self._getExperimentResult(experiment, assigned, True)

//        # 14. Fire the tracking callback if set
//self._track(experiment, result)

//        # 15. Return the result
//        return result

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

//    def _urlIsValid(self, pattern) -> bool:
//        if not self._url:
//            return False

//        try:
//            r = re.compile(pattern)
//            if r.search(self._url):
//                return True

//            pathOnly = re.sub(r"^[^/]*/", "/", re.sub(r"^https?:\/\/", "", self._url))
//            if r.search(pathOnly):
//                return True
//            return False
//        except Exception:
//            return True

//    def _getExperimentResult(
//        self, experiment: Experiment, variationId: int = -1, hashUsed: bool = False
//    ) -> Result:
//        hashAttribute = experiment.hashAttribute or "id"

//        inExperiment = True
//        if variationId< 0 or variationId> len(experiment.variations) - 1:
//            variationId = 0
//            inExperiment = False

//        return Result(
//            inExperiment= inExperiment,
//            variationId= variationId,
//            value= experiment.variations[variationId],
//            hashUsed= hashUsed,
//            hashAttribute= hashAttribute,
//            hashValue= self._getHashValue(hashAttribute),
//        )
