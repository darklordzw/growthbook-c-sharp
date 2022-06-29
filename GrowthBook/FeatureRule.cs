using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace GrowthBook {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class FeatureRule {
        public JObject Condition { get; set; }
        public double Coverage { get; set; } = 1;
        public JValue Force { get; set; }
        public string HashAttribute { get; set; } = "id";
        public string Key { get; set; }
        public Namespace Namespace { get; set; }
        public JArray Variations { get; set; }
        public IList<double> Weights { get; set; }

        public T GetVariations<T>() {
            return Variations.ToObject<T>();
        }
    }
}