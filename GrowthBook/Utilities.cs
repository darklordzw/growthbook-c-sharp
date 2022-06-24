using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace GrowthBook {
    public static class Utilities {
        public static uint FNV32A(string value) {
            uint hash = 0x811c9dc5;
            uint prime = 0x01000193;

            foreach (var c in value.ToCharArray()) {
                hash ^= c;
                hash *= prime;
            }

            return hash;
        }

        public static double Hash(string str) {
            uint n = FNV32A(str);
            return (n % 1000) / 1000.0;
        }

        public static bool InNamespace(string userId, Namespace nSpace) {
            double n = Hash(userId + "__" + nSpace.Id);
            return n >= nSpace.Start && n < nSpace.End;
        }

        public static double[] GetEqualWeights(int numVariations) {
            if (numVariations < 1) {
                return new double[0];
            }

            double[] weights = new double[numVariations];
            for (int i = 0; i < numVariations; i++) {
                weights[i] = 1.0 / numVariations;
            }
            return weights;
        }

        public static List<BucketRange> GetBucketRanges(int numVariations, double coverage = 1, double[] weights = null) {
            if (coverage < 0)
                coverage = 0;
            if (coverage > 1)
                coverage = 1;
            if (weights == null)
                weights = GetEqualWeights(numVariations);
            if (weights.Length != numVariations)
                weights = GetEqualWeights(numVariations);

            double totalWeight = 0;
            foreach (double w in weights) {
                totalWeight += w;
            }
            if (totalWeight < 0.99 || totalWeight > 1.01f)
                weights = GetEqualWeights(numVariations);

            double cumulative = 0;
            List<BucketRange> ranges = new List<BucketRange>();
            for (int i = 0; i < weights.Length; i++) {
                double start = cumulative;
                cumulative += weights[i];
                ranges.Add(new BucketRange(start, start + coverage * weights[i]));
            }

            return ranges;
        }

        public static int ChooseVariation(double n, List<BucketRange> ranges) {
            for (int i = 0; i < ranges.Count; i++) {
                if (n >= ranges[i].Start && n < ranges[i].End) {
                    return i;
                }
            }
            return -1;
        }

        public static int? GetQueryStringOverride(string id, string url, int numVariations) {
            Uri res = new Uri(url);

            if (string.IsNullOrWhiteSpace(res.Query))
                return null;

            NameValueCollection qs = System.Web.HttpUtility.ParseQueryString(res.Query);
            if (string.IsNullOrWhiteSpace(qs.Get("id")))
                return null;

            string variation = qs.Get("id");
            int varId;
            if (!int.TryParse(variation, out varId))
                return null;
            if (varId < 0 || varId >= numVariations)
                return null;
            return varId;
        }


        //def getQueryStringOverride(id: str, url: str, numVariations: int) -> Optional[int]:
        //    res = urlparse(url)
        //    if not res.query:
        //        return None
        //   qs = parse_qs(res.query)
        //    if id not in qs:
        //        return None
        //   variation = qs[id][0]
        //    if variation is None or not variation.isdigit():
        //        return None
        //   varId = int(variation)
        //    if varId< 0 or varId >= numVariations:
        //        return None
        //    return varId



        //def evalCondition(attributes: dict, condition: dict) -> bool:
        //    if "$or" in condition:
        //        return evalOr(attributes, condition["$or"])
        //    if "$nor" in condition:
        //        return not evalOr(attributes, condition["$nor"])
        //    if "$and" in condition:
        //        return evalAnd(attributes, condition["$and"])
        //    if "$not" in condition:
        //        return not evalCondition(attributes, condition["$not"])

        //    for key, value in condition.items():
        //        if not evalConditionValue(value, getPath(attributes, key)):
        //            return False

        //    return True


        //def evalOr(attributes, conditions) -> bool:
        //    if len(conditions) == 0:
        //        return True

        //    for condition in conditions:
        //        if evalCondition(attributes, condition) :
        //            return True
        //    return False


        //def evalAnd(attributes, conditions) -> bool:
        //    for condition in conditions:
        //        if not evalCondition(attributes, condition):
        //            return False
        //    return True


        //def isOperatorObject(obj) -> bool:
        //    for key in obj.keys():
        //        if key[0] != "$":
        //            return False
        //    return True


        //def getType(attributeValue) -> str:
        //    t = type(attributeValue)

        //    if attributeValue is None:
        //        return "null"
        //    if t is int or t is float:
        //        return "number"
        //    if t is str:
        //        return "string"
        //    if t is list or t is set:
        //        return "array"
        //    if t is dict:
        //        return "object"
        //    if t is bool:
        //        return "boolean"
        //    return "unknown"


        //def getPath(attributes, path):
        //    current = attributes
        //    for segment in path.split("."):
        //        if type(current) is dict and segment in current:
        //            current = current[segment]
        //        else:
        //            return None
        //    return current


        //def evalConditionValue(conditionValue, attributeValue) -> bool:
        //    if type(conditionValue) is dict and isOperatorObject(conditionValue) :
        //        for key, value in conditionValue.items():
        //            if not evalOperatorCondition(key, attributeValue, value):
        //                return False
        //        return True
        //    return conditionValue == attributeValue


        //def elemMatch(condition, attributeValue) -> bool:
        //    if not type(attributeValue) is list:
        //        return False

        //    for item in attributeValue:
        //        if isOperatorObject(condition) :
        //            if evalConditionValue(condition, item) :
        //                return True
        //        else:
        //            if evalCondition(item, condition) :
        //                return True

        //    return False


        //def evalOperatorCondition(operator, attributeValue, conditionValue) -> bool:
        //    if operator == "$eq":
        //        return attributeValue == conditionValue
        //    elif operator == "$ne":
        //        return attributeValue != conditionValue
        //    elif operator == "$lt":
        //        return attributeValue<conditionValue
        //    elif operator == "$lte":
        //        return attributeValue <= conditionValue
        //    elif operator == "$gt":
        //        return attributeValue> conditionValue
        //    elif operator == "$gte":
        //        return attributeValue >= conditionValue
        //    elif operator == "$regex":
        //        try:
        //            r = re.compile(conditionValue)
        //            return bool(r.search(attributeValue))
        //        except Exception:
        //            return False
        //    elif operator == "$in":
        //        return attributeValue in conditionValue
        //    elif operator == "$nin":
        //        return not (attributeValue in conditionValue)
        //    elif operator == "$elemMatch":
        //        return elemMatch(conditionValue, attributeValue)
        //    elif operator == "$size":
        //        if not (type(attributeValue) is list):
        //            return False
        //        return evalConditionValue(conditionValue, len(attributeValue))
        //    elif operator == "$all":
        //        if not (type(attributeValue) is list):
        //            return False
        //        for cond in conditionValue:
        //            passing = False
        //            for attr in attributeValue:
        //                if evalConditionValue(cond, attr) :
        //                    passing = True
        //            if not passing:
        //                return False
        //        return True
        //    elif operator == "$exists":
        //        if not conditionValue:
        //            return attributeValue is None
        //        return attributeValue is not None
        //    elif operator == "$type":
        //        return getType(attributeValue) == conditionValue
        //    elif operator == "$not":
        //        return not evalConditionValue(conditionValue, attributeValue)
        //    return False
    }
}
