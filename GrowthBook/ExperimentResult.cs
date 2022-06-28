using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace GrowthBook {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ExperimentResult {
        public bool InExperiment { get; set; }
        public string HashAttribute { get; set; }
        public bool HashUsed { get; set; }
        public string HashValue { get; set; }
        public JValue Value { get; set; }
        public int VariationId { get; set; }

        public T GetValue<T>() {
            return Value.ToObject<T>();
        }
    }
}