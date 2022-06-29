using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GrowthBook {
    [JsonConverter(typeof(NamespaceTupleConverter))]
    public class Namespace {
        public Namespace(string id, double start, double end) {
            Id = id;
            Start = start;
            End = end;
        }

        public Namespace(JArray jArray) :
            this(jArray[0].ToString(), jArray[1].ToObject<double>(), jArray[2].ToObject<double>()) { }

        public string Id { get; }
        public double Start { get; }
        public double End { get; }

        public override string ToString() => $"({Id}, {Start}, {End})";

        public override bool Equals(object obj) {
            if (obj.GetType() == typeof(Namespace)) {
                Namespace objNamspace = (Namespace)obj;
                return Id == objNamspace.Id && Start == objNamspace.Start && End == objNamspace.End;
            }
            return false;
        }
    }
}
