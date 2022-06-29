using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrowthBook {
    public class Context {
        public bool Enabled { get; set; } = true;
        public JObject Attributes { get; set; } = new JObject();
        public string Url { get; set; }
        public IDictionary<string, Feature> Features { get; set; } = new Dictionary<string, Feature>();
        public JObject ForcedVariations { get; set; } = new JObject();
        public bool QaMode { get; set; } = false;
        public Action<Experiment, ExperimentResult> TrackingCallback { get; set; }
    }
}
