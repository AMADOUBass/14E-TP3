using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils.Services.Interfaces
{
    public interface ICalculReservationResult
    {
        int WagonsNecessaires { get; }
        decimal TarifFinal { get; }
        string Message { get; }
    }
}
