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
            foreach(JProperty property in obj.Properties()) {
                if (!property.Name.StartsWith("$")) {
                    return false;
                }
            }
            return true;
        }

        private static bool EvalConditionValue(JToken conditionValue, JToken attributeValue) {
            if (conditionValue.Type == JTokenType.Object) {
                JObject conditionDict = (JObject)conditionValue;
                if (conditionDict != null && IsOperatorObject(conditionDict)) {
                    foreach (JProperty property in conditionDict.Properties()) {
                        if (!EvalOperatorCondition(property.Name, attributeValue, property.Value)) {
                            return false;
                        }
                    }
                    return true;
                }
            }
            
            return JToken.DeepEquals(conditionValue, attributeValue);
        }

        private static bool ElemMatch(JObject condition, JToken attributeVaue) {
            if (attributeVaue.Type != JTokenType.Array) {
                return false;
            }

            foreach (JToken item in (JArray)attributeVaue) {
                if (IsOperatorObject(condition) && EvalConditionValue(condition, item)) {
                    return true;
                }
                if (EvalCondition(item, condition)) {
                    return true;
                }
            }

            return false;
        }

        private static bool EvalOperatorCondition(string op, JToken attributeValue, JToken conditionValue) {
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
                try {
                    Regex regex = new Regex(conditionValue.ToString(), RegexOptions.Compiled);
                    return regex.IsMatch(attributeValue.ToString());
                } catch (ArgumentException) {
                    return false;
                }
            }
            if (conditionValue.Type == JTokenType.Array) {
                JArray conditionList = (JArray)conditionValue;
                if (op.Equals("$in")) {
                    return attributeValue != null && conditionList.FirstOrDefault(j => j.ToString().Equals(attributeValue.ToString())) != null;
                }
                if (op.Equals("$nin")) {
                    return conditionList.FirstOrDefault(j => j.ToString().Equals(attributeValue.ToString())) == null;
                }
                if (op.Equals("$all")) {
                    if (attributeValue.Type != JTokenType.Array) {
                        return false;
                    }
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
            if (op.Equals("$elemMatch")) {
                return ElemMatch((JObject)conditionValue, attributeValue);
            }
            if (op.Equals("$size")) {
                if (attributeValue?.Type == JTokenType.Array) {
                    return EvalConditionValue(conditionValue, ((JArray)attributeValue).Count);
                }
                return false;
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

            switch(attributeValue.Type) {
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
    }
}
