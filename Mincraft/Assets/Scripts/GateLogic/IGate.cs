using System.Threading.Tasks;

namespace GateLogic
{
    public interface IGate
    {
        bool[] Pins { get; set; }
        bool Output { get; set; }
        int Delay { get; set; } //Delay in milliseconds
        
        Task<bool> Evaluate();
    }
}