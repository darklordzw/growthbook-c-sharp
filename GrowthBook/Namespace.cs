using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GrowthBook {
    public class Namespace {
        public Namespace(string id, double start, double end) {
            Id = id;
            Start = start;
            End = end;
        }

        [JsonConstructor]
        public Namespace(JArray jArray) :
            this(jArray[0].ToString(), jArray[1].ToObject<double>(), jArray[2].ToObject<double>()) { }

        public string Id { get; }
        public double Start { get; }
        public double End { get; }

        public override string ToString() => $"({Id}, {Start}, {End})";
    }
}
