using System.Collections.Generic;

namespace GrowthBook {
    public class Namespace {
        public Namespace(string id, double start, double end) {
            Id = id;
            Start = start;
            End = end;
        }

        public Namespace(List<object> tuple) : this((string)tuple[0], (double)tuple[1], (double)tuple[2]) { }

        public string Id { get; }
        public double Start { get; }
        public double End { get; }

        public override string ToString() => $"({Id}, {Start}, {End})";
    }
}
