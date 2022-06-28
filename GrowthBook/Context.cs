using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrowthBook {
    public class Context {
        public bool Enabled { get; set; } = true;
        public JObject Attributes { get; set; } = new JObject();
        public string Url { get; set; }
        public JObject Features { get; set; } = new JObject();
        public JObject ForcedVariations { get; set; } = new JObject();
        public bool QaMode { get; set; } = false;
        public Func<Experiment, ExperimentResult, Task> TrackingCallback { get; set; }
    }
}
