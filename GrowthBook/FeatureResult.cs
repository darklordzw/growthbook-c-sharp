using System;
using System.Collections.Generic;
using System.Text;

namespace GrowthBook {
    public class FeatureResult {
        public object Value { get; set; }
        public bool On { get { return Convert.ToBoolean(Value); } }
        public bool Off { get { return !On; } }
        public string Source { get; set; }
        public Experiment Experiment { get; set; }
        public ExperimentResult ExperimentResult { get; set; }
    }
}

//class FeatureResult(object):
//    def __init__(
//        self,
//        value,
//        source: str,
//        experiment: Experiment = None,
//        experimentResult: Result = None,
//    ) -> None:
//        self.value = value
//        self.source = source
//        self.experiment = experiment
//        self.experimentResult = experimentResult
//        self.on = bool(value)
//        self.off = not bool(value)

//    def to_dict(self) -> dict:
//        data = {
//    "value": self.value,
//            "source": self.source,
//            "on": self.on,
//            "off": self.off,
//        }
//        if self.experiment:
//            data["experiment"] = self.experiment.to_dict()
//        if self.experimentResult:
//            data["experimentResult"] = self.experimentResult.to_dict()

//        return data
