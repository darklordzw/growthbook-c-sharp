using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;

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
            if (string.IsNullOrWhiteSpace(url))
                return null;

            Uri res = new Uri(url);
            if (string.IsNullOrWhiteSpace(res.Query))
                return null;

            NameValueCollection qs = HttpUtility.ParseQueryString(res.Query);
            if (string.IsNullOrWhiteSpace(qs.Get(id)))
                return null;

            string variation = qs.Get(id);
            int varId;
            if (!int.TryParse(variation, out varId)) {
                return null;
            }
            if (varId < 0 || varId >= numVariations) {
                return null;
            }
            return varId;
        }

        public static bool EvalCondition(IDictionary<string, object> attributes, IDictionary<string, object> condition) {
            if (condition.ContainsKey("$or")) {
                return EvalOr(attributes, (IList<IDictionary<string, object>>)condition["$or"]);
            }
            if (condition.ContainsKey("$nor")) {
                return !EvalOr(attributes, (IList<IDictionary<string, object>>)condition["$nor"]);
            }
            if (condition.ContainsKey("$and")) {
                return EvalAnd(attributes, (IList<IDictionary<string, object>>)condition["$and"]);
            }
            if (condition.ContainsKey("$not")) {
                return !EvalCondition(attributes, (IDictionary<string, object>)condition["$not"]);
            }

            foreach (string key in condition.Keys) {
                if (!EvalConditionValue(condition[key], GetPath(attributes, key))) {
                    return false;
                }
            }

            return true;
        }

        private static bool EvalOr(IDictionary<string, object> attributes, IList<IDictionary<string, object>> conditions) {
            if (conditions.Count == 0) {
                return true;
            }

            foreach (IDictionary<string, object> condition in conditions) {
                if (EvalCondition(attributes, condition)) {
                    return true;
                }
            }

            return false;
        }

        private static bool EvalAnd(IDictionary<string, object> attributes, IList<IDictionary<string, object>> conditions) {
            foreach (Dictionary<string, object> condition in conditions) {
                if (!EvalCondition(attributes, condition)) {
                    return false;
                }
            }

            return true;
        }

        private static bool IsOperatorObject(IDictionary<string, object> obj) {
            foreach(string key in obj.Keys) {
                if (!key.StartsWith("$")) {
                    return false;
                }
            }
            return true;
        }

        private static object GetPath(IDictionary<string, object> attributes, string path) {
            object currentValue = attributes;
            IDictionary<string, object> currentDict;
            foreach (string segment in path.Split('.')) {
                currentDict = currentValue as IDictionary<string, object>;
                if (currentDict != null && currentDict.ContainsKey(segment)) {
                    currentValue = currentDict[segment];
                } else {
                    return null;
                }
            }
            return currentValue;
        }

        private static bool EvalConditionValue(object conditionValue, object attributeValue) {
            IDictionary<string, object> conditionDict = conditionValue as IDictionary<string, object>;

            if (conditionDict != null && IsOperatorObject(conditionDict)) {
                foreach (string key in conditionDict.Keys) {
                    if (!EvalOperatorCondition(key, attributeValue, conditionDict[key])) {
                        return false;
                    }
                }
                return true;
            }
            return conditionValue.Equals(attributeValue);
        }

        private static bool ElemMatch(IDictionary<string, object> condition, object attributeVaue) {
            if (!(attributeVaue is IEnumerable)) {
                return false;
            }

            foreach (object item in (IEnumerable)attributeVaue) {
                if (IsOperatorObject(condition) && EvalConditionValue(condition, item)) {
                    return true;
                }
                if (EvalCondition((IDictionary<string, object>)item, condition)) {
                    return true;
                }
            }

            return false;
        }

        private static bool EvalOperatorCondition(string op, object attributeValue, object conditionValue) {
            if (op.Equals("$eq")) {
                return attributeValue.Equals(conditionValue);
            }
            if (op.Equals("$ne")) {
                return !attributeValue.Equals(conditionValue);
            }
            if (attributeValue is IComparable) {
                IComparable attrComp = (IComparable)attributeValue;
                if (op.Equals("$lt")) {
                    return attrComp.CompareTo(conditionValue) < 0;
                }
                if (op.Equals("$lte")) {
                    return attrComp.CompareTo(conditionValue) <= 0;
                }
                if (op.Equals("$gt")) {
                    return attrComp.CompareTo(conditionValue) > 0;
                }
                if (op.Equals("$gte")) {
                    return attrComp.CompareTo(conditionValue) >= 0;
                }
            }
            if (op.Equals("$regex")) {
                Regex regex = new Regex(conditionValue.ToString(), RegexOptions.Compiled);
                return regex.IsMatch(attributeValue.ToString());
            }
            if (conditionValue is IList) {
                IList conditionList = (IList)conditionValue;
                if (op.Equals("$in")) {
                    return conditionList.IndexOf(attributeValue) != -1;
                }
                if (op.Equals("$nin")) {
                    return conditionList.IndexOf(attributeValue) == -1;
                }
                if (op.Equals("$all")) {
                    IList attrList = attributeValue as IList;
                    if (attrList == null) {
                        return false;
                    }
                    foreach (object obj in attrList) {
                        if (!EvalConditionValue(conditionValue, obj)) {
                            return false;
                        }
                    }
                    return true;
                }
            }
            if (op.Equals("$elemMatch")) {
                return ElemMatch((IDictionary<string, object>)conditionValue, attributeValue);
            }
            if (attributeValue is IList) {
                IList attrList = (IList)attributeValue;
                if (op.Equals("$size")) {
                    EvalConditionValue(conditionValue, attrList.Count);
                }
            }
            if (op.Equals("$exists")) {
                return conditionValue == null || conditionValue.Equals(false) ? attributeValue == null : attributeValue != null;
            }
            if (op.Equals("$type")) {
                return GetType(attributeValue).Equals(conditionValue);
            }
            if (op.Equals("$not")) {
                return !EvalConditionValue(conditionValue, attributeValue);
            }
            return false;
        }

        private static string GetType(object attributeValue) {
            if (attributeValue == null) {
                return null;
            }
            if (attributeValue is int || attributeValue is float || attributeValue is double) {
                return "number";
            }
            if (attributeValue is string) {
                return "string";
            }
            if (attributeValue is IList) {
                return "array";
            }
            if (attributeValue is IDictionary) {
                return "object";
            }
            if (attributeValue is bool) {
                return "boolean";
            }
            return "unknown";
        }
    }
}
