using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace GrowthBook {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class Feature {
        public JToken DefaultValue { get; set; }
        public IList<FeatureRule> Rules { get; set; } = new List<FeatureRule>();

        public T GetDefaultValue<T>() {
            return DefaultValue.ToObject<T>();
        }
    }
}