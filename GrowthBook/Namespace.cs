using System.Collections.Generic;

namespace GrowthBook {
    public class Namespace {
        public Namespace(string id, double start, double end) {
            Id = id;
            Start = start;
            End = end;
        }

        public string Id { get; }
        public double Start { get; }
        public double End { get; }

        public override string ToString() => $"({Id}, {Start}, {End})";
    }
}
