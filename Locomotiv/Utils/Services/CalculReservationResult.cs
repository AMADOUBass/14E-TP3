using Locomotiv.Utils.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils.Services
{
    public class CalculReservationResult: ICalculReservationResult
    {
        public int WagonsNecessaires { get; set; }
        public decimal TarifFinal { get; set; }
        public string Message { get; set; }
    }
}
