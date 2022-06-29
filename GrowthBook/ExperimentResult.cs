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
        public JToken Value { get; set; }
        public int VariationId { get; set; }

        public T GetValue<T>() {
            return Value.ToObject<T>();
        }

        public override bool Equals(object obj) {
            if (obj.GetType() == typeof(ExperimentResult)) {
                ExperimentResult objResult = (ExperimentResult)obj;
                return InExperiment == objResult.InExperiment
                    && HashAttribute == objResult.HashAttribute
                    && HashUsed == objResult.HashUsed
                    && HashValue == objResult.HashValue
                    && JToken.DeepEquals(Value ?? JValue.CreateNull(), objResult.Value ?? JValue.CreateNull())
                    && VariationId == objResult.VariationId;
            }
            return false;
        }
    }
}