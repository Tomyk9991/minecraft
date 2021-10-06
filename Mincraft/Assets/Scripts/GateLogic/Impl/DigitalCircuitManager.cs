using System.Collections.Generic;
using Core.Builder;

namespace GateLogic.Impl
{
    public class DigitalCircuitManager
    {
        public static readonly BlockUV[] CircuitBlocks = {
            BlockUV.AndGate,
            BlockUV.OrGate,
            BlockUV.NotGate,
        };
        
        private List<Circuit> circuits = new List<Circuit>();
        
        public void AddConnection(IGate g1, IGate g2)
        {
            //Order matters. g1 always points to g2.
            GatePair pair = new GatePair(g1, g2);
            Circuit circuit = GetOrCreateCircuit(pair);

            circuit.AddConnection(pair);
        }

        private Circuit GetOrCreateCircuit(GatePair pair)
        {
            foreach (Circuit circuit in circuits)
            {
                IGate first = pair.First;
                IGate second = pair.Second;

                if (circuit.Contains(first)) 
                    return circuit;
                
                if (circuit.Contains(second)) 
                    return circuit;
            }

            Circuit c = new Circuit();
            this.circuits.Add(c);
            
            return c;
        }
    }
}