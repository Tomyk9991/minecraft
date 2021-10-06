using System.Collections.Generic;
using System.Linq;

namespace GateLogic.Impl
{
    public class Circuit
    {
        private List<GatePair> gates;

        public Circuit()
        {
            this.gates = new List<GatePair>();
        }

        public bool Contains(IGate gate) => gates.Any(t => t.First == gate || t.Second == gate);
        public void AddConnection(GatePair pair) => this.gates.Add(pair);

        public void Simulate()
        {
        }
    }

    public class GatePair
    {
        public IGate First;
        public IGate Second;

        public GatePair(IGate first, IGate second)
        {
            First = first;
            Second = second;
        }
    }
}