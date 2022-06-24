using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrowthBook {
    public class Context {
        public bool Enabled { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public string Url { get; set; }
        public Dictionary<string, Feature> Features { get; set; }
        public Dictionary<string, int> ForcedVariations { get; set; }
        public bool QaMode { get; set; }
        public Func<Experiment, ExperimentResult, Task> TrackingCallback { get; set; }
    }
}
