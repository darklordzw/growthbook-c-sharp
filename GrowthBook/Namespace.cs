using System;
using System.Collections.Generic;
using System.Text;

namespace GrowthBook
{
    public struct Namespace
    {
        public Namespace(string id, float start, float end)
        {
            Id = id;
            Start = start;
            End = end;
        }

        public string Id { get; }
        public float Start { get; }
        public float End { get; }

        public override string ToString() => $"({Id}, {Start}, {End})";
    }
}
