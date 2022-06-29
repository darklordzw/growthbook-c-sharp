using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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
    }

    public class NamespaceTupleConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Namespace);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array) {
                return new Namespace((JArray)token);
            }
            return token.ToObject<Namespace>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            Namespace valueNamespace = (Namespace)value;
            JArray t = new JArray();
            t.Add(JToken.FromObject(valueNamespace.Id));
            t.Add(JToken.FromObject(valueNamespace.Start));
            t.Add(JToken.FromObject(valueNamespace.End));
            t.WriteTo(writer);
        }
    }
}
