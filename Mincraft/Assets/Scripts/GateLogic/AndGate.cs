using System.Threading.Tasks;

namespace GateLogic
{
    public class AndGate : IGate
    {
        public bool[] Pins { get; set; }
        public bool Output { get; set; }
        public int Delay { get; set; }
        
        public async Task<bool> Evaluate()
        {
            bool result = Pins[0];
            
            for (int i = 1; i < Pins.Length; i++)
                result &= Pins[i];

            await Task.Delay(Delay);

            this.Output = result;
            return result;
        }
    }
}