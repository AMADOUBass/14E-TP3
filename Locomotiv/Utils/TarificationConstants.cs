using Locomotiv.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils
{
    public static class TarificationConstants
    {
        public const decimal PrixBase = 500m;

        public static readonly Dictionary<TypeMarchandise, decimal> FacteurPoids = new()
        {
            { TypeMarchandise.Bois, 1.0m },
            { TypeMarchandise.Liquide, 1.3m },
            { TypeMarchandise.Chimique, 1.6m },
            { TypeMarchandise.Fragile, 1.4m }
        };

        public static readonly Dictionary<TypeMarchandise, decimal> FacteurVolume = new()
        {
            { TypeMarchandise.Bois, 0.8m },
            { TypeMarchandise.Liquide, 1.2m },
            { TypeMarchandise.Chimique, 1.5m },
            { TypeMarchandise.Fragile, 1.3m }
        };
    }

    public static class CapaciteConstants
    {
        public const int PoidsMaxParWagon = 25000; // kg
        public const int VolumeMaxParWagon = 60;   // m³
    }
}
