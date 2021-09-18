using System.Threading.Tasks;

namespace GateLogic
{
    public class NotGate : IGate, IMaxPin
    {
        public int MaxPinCount { get; set; } = 1;
        public bool[] Pins { get; set; }
        
        public bool Output { get; set; }
        public int Delay { get; set; }
        
        public async Task<bool> Evaluate()
        {
            bool result = !Pins[0];
            await Task.Delay(Delay);
            this.Output = result;

            return result;
        }

    }
}