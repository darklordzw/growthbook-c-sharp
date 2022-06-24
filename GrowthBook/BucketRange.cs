using System.Collections.Generic;

namespace GrowthBook {
    public class BucketRange {
        public BucketRange(double start, double end) {
            Start = start;
            End = end;
        }

        public BucketRange(List<double> tuple) : this(tuple[0], tuple[1]) { }

        public double Start { get; set; }
        public double End { get; set; }

        public override bool Equals(object obj) {
            if (obj.GetType() == typeof(BucketRange)) {
                BucketRange objRange = (BucketRange)obj;
                return this.Start == objRange.Start && this.End == objRange.End;
            }
            return false;
        }
    }
}
