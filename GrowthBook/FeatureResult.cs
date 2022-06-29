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
        public JToken Value { get; set; }

        public T GetValue<T>() {
            return Value.ToObject<T>();
        }

        public override bool Equals(object obj) {
            if (obj.GetType() == typeof(FeatureResult)) {
                FeatureResult objResult = (FeatureResult)obj;
                return object.Equals(Experiment, objResult.Experiment)
                    && object.Equals(ExperimentResult, objResult.ExperimentResult)
                    && Off == objResult.Off
                    && On == objResult.On
                    && Source == objResult.Source
                    && JToken.DeepEquals(Value ?? JValue.CreateNull(), objResult.Value ?? JValue.CreateNull());
            }
            return false;
        }
    }
}