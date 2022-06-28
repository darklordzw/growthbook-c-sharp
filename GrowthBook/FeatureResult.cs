using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace GrowthBook {
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class FeatureResult {
        public Experiment Experiment { get; set; }
        public ExperimentResult ExperimentResult { get; set; }
        public bool Off { get { return !On; } }
        public bool On { get { return Value != null && Value.Type != JTokenType.Null && Value.Type != JTokenType.Undefined; } }
        public string Source { get; set; }
        public JValue Value { get; set; }

        public T GetValue<T>() {
            return Value.ToObject<T>();
        }
    }
}