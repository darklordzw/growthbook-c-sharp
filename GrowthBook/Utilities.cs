using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace GrowthBook {
    public static class Utilities {
        public static double Hash(string str) {
            uint n = FNV32A(str);
            return (n % 1000) / 1000.0;
        }

        public static bool InNamespace(string userId, Namespace nSpace) {
            double n = Hash(userId + "__" + nSpace.Id);
            return n >= nSpace.Start && n < nSpace.End;
        }

        public static IList<double> GetEqualWeights(int numVariations) {
            List<double> weights = new List<double>();

            if (numVariations >= 1) {
                for (int i = 0; i < numVariations; i++) {
                    weights.Add(1.0 / numVariations);
                }
            }

            return weights;
        }

        public static IList<BucketRange> GetBucketRanges(int numVariations, double coverage = 1, IList<double> weights = null) {
            if (coverage < 0) {
                coverage = 0;
            } else if (coverage > 1) {
                coverage = 1;
            }

            if (weights?.Count != numVariations) {
                weights = GetEqualWeights(numVariations);
            }

            double totalWeight = weights.Sum();
            if (totalWeight < 0.99 || totalWeight > 1.01f) {
                weights = GetEqualWeights(numVariations);
            }

            double cumulative = 0;
            List<BucketRange> ranges = new List<BucketRange>();

            for (int i = 0; i < weights.Count; i++) {
                double start = cumulative;
                cumulative += weights[i];
                ranges.Add(new BucketRange(start, start + coverage * weights[i]));
            }

            return ranges;
        }

        public static int ChooseVariation(double n, IList<BucketRange> ranges) {
            for (int i = 0; i < ranges.Count; i++) {
                if (n >= ranges[i].Start && n < ranges[i].End) {
                    return i;
                }
            }

            return -1;
        }

        public static int? GetQueryStringOverride(string id, string url, int numVariations) {
            if (string.IsNullOrWhiteSpace(url)) {
                return null;
            }

            Uri res = new Uri(url);
            if (string.IsNullOrWhiteSpace(res.Query)) {
                return null;
            }

            NameValueCollection qs = HttpUtility.ParseQueryString(res.Query);
            if (string.IsNullOrWhiteSpace(qs.Get(id))) {
                return null;
            }

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

        public static bool EvalCondition(JToken attributes, JObject condition) {
            if (condition.ContainsKey("$or")) {
                return EvalOr(attributes, (JArray)condition["$or"]);
            }
            if (condition.ContainsKey("$nor")) {
                return !EvalOr(attributes, (JArray)condition["$nor"]);
            }
            if (condition.ContainsKey("$and")) {
                return EvalAnd(attributes, (JArray)condition["$and"]);
            }
            if (condition.ContainsKey("$not")) {
                return !EvalCondition(attributes, (JObject)condition["$not"]);
            }

            foreach (JProperty property in condition.Properties()) {
                if (!EvalConditionValue(property.Value, attributes.SelectToken(property.Name))) {
                    return false;
                }
            }

            return true;
        }

        // #region Private Helpers

        private static uint FNV32A(string value) {
            uint hash = 0x811c9dc5;
            uint prime = 0x01000193;

            foreach (char c in value.ToCharArray()) {
                hash ^= c;
                hash *= prime;
            }

            return hash;
        }


        private static bool EvalOr(JToken attributes, JArray conditions) {
            if (conditions.Count == 0) {
                return true;
            }

            foreach (JObject condition in conditions) {
                if (EvalCondition(attributes, condition)) {
                    return true;
                }
            }

            return false;
        }

        private static bool EvalAnd(JToken attributes, JArray conditions) {
            foreach (JObject condition in conditions) {
                if (!EvalCondition(attributes, condition)) {
                    return false;
                }
            }

            return true;
        }

        private static bool IsOperatorObject(JObject obj) {
            foreach (JProperty property in obj.Properties()) {
                if (!property.Name.StartsWith("$")) {
                    return false;
                }
            }

            return true;
        }

        private static bool EvalConditionValue(JToken conditionValue, JToken attributeValue) {
            if (conditionValue.Type == JTokenType.Object) {
                JObject conditionObj = (JObject)conditionValue;

                if (IsOperatorObject(conditionObj)) {
                    foreach (JProperty property in conditionObj.Properties()) {
                        if (!EvalOperatorCondition(property.Name, attributeValue, property.Value)) {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return JToken.DeepEquals(conditionValue ?? JValue.CreateNull(), attributeValue ?? JValue.CreateNull());
        }

        private static bool ElemMatch(JObject condition, JToken attributeVaue) {
            if (attributeVaue.Type != JTokenType.Array) {
                return false;
            }

            foreach (JToken elem in (JArray)attributeVaue) {
                if (IsOperatorObject(condition) && EvalConditionValue(condition, elem)) {
                    return true;
                }
                if (EvalCondition(elem, condition)) {
                    return true;
                }
            }

            return false;
        }

        private static bool EvalOperatorCondition(string op, JToken attributeValue, JToken conditionValue) {
            if (op.Equals("$eq")) {
                return conditionValue.Equals(attributeValue);
            }
            if (op.Equals("$ne")) {
                return !conditionValue.Equals(attributeValue);
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
                try {
                    return Regex.IsMatch(attributeValue?.ToString(), conditionValue?.ToString());
                } catch (ArgumentException) {
                    return false;
                }
            }
            if (conditionValue.Type == JTokenType.Array) {
                JArray conditionList = (JArray)conditionValue;

                if (op.Equals("$in")) {
                    return conditionList.FirstOrDefault(j => j.ToString().Equals(attributeValue?.ToString())) != null;
                }
                if (op.Equals("$nin")) {
                    return conditionList.FirstOrDefault(j => j.ToString().Equals(attributeValue?.ToString())) == null;
                }
                if (op.Equals("$all")) {
                    if (attributeValue?.Type == JTokenType.Array) {
                        foreach (JToken condition in conditionList) {
                            bool passing = false;

                            foreach (JToken attr in (JArray)attributeValue) {
                                if (EvalConditionValue(condition, attr)) {
                                    passing = true;
                                }
                            }

                            if (!passing) {
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }
            if (op.Equals("$elemMatch")) {
                return ElemMatch((JObject)conditionValue, attributeValue);
            }
            if (op.Equals("$size")) {
                if (attributeValue?.Type == JTokenType.Array) {
                    return EvalConditionValue(conditionValue, ((JArray)attributeValue).Count);
                }
            }
            if (op.Equals("$exists")) {
                return conditionValue.ToObject<bool>() ? attributeValue != null && attributeValue.Type != JTokenType.Null && attributeValue.Type != JTokenType.Undefined :
                    attributeValue == null || attributeValue.Type == JTokenType.Null || attributeValue.Type == JTokenType.Undefined;
            }
            if (op.Equals("$type")) {
                return GetType(attributeValue).Equals(conditionValue.ToString());
            }
            if (op.Equals("$not")) {
                return !EvalConditionValue(conditionValue, attributeValue);
            }
            return false;
        }

        private static string GetType(JToken attributeValue) {
            if (attributeValue == null) {
                return "null";
            }

            switch (attributeValue.Type) {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return "null";
                case JTokenType.Integer:
                case JTokenType.Float:
                    return "number";
                case JTokenType.Array:
                    return "array";
                case JTokenType.Boolean:
                    return "boolean";
                case JTokenType.String:
                    return "string";
                case JTokenType.Object:
                    return "object";
                default:
                    return "unknown";
            }
        }

        // #endregion
    }
}
