using Locomotiv.Model;
using Locomotiv.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils.Services.Interfaces
{
    public interface IWagonCalculatorService
    {
        public CalculReservationResult Calculer(
           CommercialRoute route,
           TypeMarchandise type,
           double poids,
           double volume);
    }
}
