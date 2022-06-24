using System;
using System.Collections.Generic;
using System.Text;

namespace GrowthBook {
    public class Experiment {
        public string Key { get; set; }
        public List<object> Variations { get; set; }
        public List<double> Weights { get; set; }
        public bool Active { get; set; } = true;
        public double Coverage { get; set; } = 1;
        public Dictionary<string, object> Condition { get; set; }
        public Namespace Namespace { get; set; }
        public int Force { get; set; }
        public string HashAttribute { get; set; } = "Id";
    }
}

//class Experiment(object):
//    def __init__(
//        self,
//        key: str,
//        variations: list,
//        weights: "list[float]" = None,
//        active: bool = True,
//        status: str = "running",
//        coverage: int = 1,
//        condition: dict = None,
//        namespace: "tuple[str,float,float]" = None,
//        url: str = "",
//        include=None,
//        groups: list = None,
//        force: int = None,
//        hashAttribute: str = "id",
//    ) -> None:
//        self.key = key
//        self.variations = variations
//        self.weights = weights
//        self.active = active
//        self.coverage = coverage
//        self.condition = condition
//        self.namespace = namespace
//        self.force = force
//        self.hashAttribute = hashAttribute

//# Deprecated properties
//        self.status = status
//        self.url = url
//        self.include = include
//        self.groups = groups

//    def to_dict(self):
//        return {
//    "key": self.key,
//            "variations": self.variations,
//            "weights": self.weights,
//            "active": self.active,
//            "coverage": self.coverage,
//            "condition": self.condition,
//            "namespace": self.namespace,
//            "force": self.force,
//            "hashAttribute": self.hashAttribute,
//        }

