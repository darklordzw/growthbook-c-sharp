using System;
using System.Collections.Generic;
using System.Text;

namespace GrowthBook {
    public class ExperimentResult {
        public bool InExperiment { get; set; }
        public int VariationId { get; set; }
        public object Value { get; set; }
        public bool HashUsed { get; set; }
        public string HashAttribute { get; set; }
        public string HashValue { get; set; }
    }
}



//class Result(object):
//    def __init__(
//        self,
//        variationId: int,
//        inExperiment: bool,
//        value,
//        hashUsed: bool,
//        hashAttribute: str,
//        hashValue: str,
//    ) -> None:
//        self.variationId = variationId
//        self.inExperiment = inExperiment
//        self.value = value
//        self.hashUsed = hashUsed
//        self.hashAttribute = hashAttribute
//        self.hashValue = hashValue

//    def to_dict(self) -> dict:
//        return {
//    "variationId": self.variationId,
//            "inExperiment": self.inExperiment,
//            "value": self.value,
//            "hashUsed": self.hashUsed,
//            "hashAttribute": self.hashAttribute,
//            "hashValue": self.hashValue,
//        }