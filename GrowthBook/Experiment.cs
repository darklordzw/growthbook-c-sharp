using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace GrowthBook {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Experiment {
        public bool Active { get; set; } = true;
        public JObject Condition { get; set; }
        public double Coverage { get; set; } = 1;
        public int? Force { get; set; }
        public string HashAttribute { get; set; } = "id";
        public string Key { get; set; }
        public Namespace Namespace { get; set; }
        public JArray Variations { get; set; }
        public IList<double> Weights { get; set; }

        public T GetVariations<T>() {
            return Variations.ToObject<T>();
        }

        public override bool Equals(object obj) {
            if (obj.GetType() == typeof(Experiment)) {
                Experiment objExp = (Experiment)obj;
                return Active == objExp.Active
                    && JToken.DeepEquals(Condition, objExp.Condition)
                    && Coverage == objExp.Coverage
                    && Force == objExp.Force
                    && HashAttribute == objExp.HashAttribute
                    && Key == objExp.Key
                    && object.Equals(Namespace, objExp.Namespace)
                    && JToken.DeepEquals(Variations, objExp.Variations)
                    && ((Weights == null && objExp.Weights == null) || Weights.SequenceEqual(objExp.Weights));
            }
            return false;
        }
    }
}