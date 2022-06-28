using System;

namespace GrowthBook {
    public class BucketRange {
        public BucketRange(double start, double end) {
            Start = start;
            End = end;
        }

        public double Start { get; set; }
        public double End { get; set; }

        public override bool Equals(object obj) {
            if (obj.GetType() == typeof(BucketRange)) {
                BucketRange objRange = (BucketRange)obj;
                return this.Start == objRange.Start && this.End == objRange.End;
            }
            return false;
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }

        public override string ToString() => $"({Start}, {End})";
    }
}
