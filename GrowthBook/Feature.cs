using System;
using System.Collections.Generic;
using System.Text;

namespace GrowthBook {
    public class Feature { 
        public object DefaultValue { get; set; }
        public List<FeatureRule> Rules { get; set; } = new List<FeatureRule>();
    }
}




//class Feature(object):
//    def __init__(self, defaultValue= None, rules: list = []) -> None:
//        self.defaultValue = defaultValue
//        self.rules: list[FeatureRule] = []
//        for rule in rules:
//            if isinstance(rule, FeatureRule):
//                self.rules.append(rule)
//            else:
//                self.rules.append(FeatureRule(** rule))

//    def to_dict(self) -> dict:
//        return {
//            "defaultValue": self.defaultValue,
//            "rules": [rule.to_dict() for rule in self.rules],
//        }