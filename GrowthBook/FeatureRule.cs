using System;
using System.Collections.Generic;
using System.Text;

namespace GrowthBook {
    public class FeatureRule {
        public Dictionary<string, object> Condition { get; set; }
        public double Coverage { get; set; } = 1;
        public int Force { get; set; }
        public List<object> Variations { get; set; }
        public string Key { get; set; }
        public List<double> Weights { get; set; }
        public Namespace Namespace { get; set; }
        public string HashAttribute { get; set; } = "Id";
    }
}


//class FeatureRule(object):
//    def __init__(
//        self,
//        key: str = "",
//        variations: list = None,
//        weights: "list[float]" = None,
//        coverage: int = 1,
//        condition: dict = None,
//        namespace: "tuple[str,float,float]" = None,
//        force=None,
//        hashAttribute: str = "id",
//    ) -> None:
//        self.key = key
//        self.variations = variations
//        self.weights = weights
//        self.coverage = coverage
//        self.condition = condition
//        self.namespace = namespace
//        self.force = force
//        self.hashAttribute = hashAttribute

//    def to_dict(self) -> dict:
//        data = { }
//        if self.key:
//            data["key"] = self.key
//        if self.variations is not None:
//            data["variations"] = self.variations
//        if self.weights is not None:
//            data["weights"] = self.weights
//        if self.coverage != 1:
//            data["coverage"] = self.coverage
//        if self.condition is not None:
//            data["condition"] = self.condition
//        if self.namespace is not None:
//            data["namespace"] = self.namespace
//        if self.force is not None:
//            data["force"] = self.force
//        if self.hashAttribute != "id":
//            data["hashAttribute"] = self.hashAttribute

//        return data
