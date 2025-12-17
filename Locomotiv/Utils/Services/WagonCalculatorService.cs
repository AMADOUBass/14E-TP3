using Locomotiv.Model;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using System;

namespace Locomotiv.Utils.Services
{
    public class WagonCalculatorService : IWagonCalculatorService
    {
        public CalculReservationResult Calculer(
            CommercialRoute route,
            TypeMarchandise type,
            double poids,
            double volume)
        {
            if (poids <= 0 || volume <= 0)
            {
                return new CalculReservationResult
                {
                    WagonsNecessaires = 0,
                    TarifFinal = 0,
                    Message = "Le poids et le volume doivent être supérieurs à 0."
                };
            }

            double capacitePoidsParWagonTonnes =
                CapaciteConstants.PoidsMaxParWagon / 1000.0; // kg → tonnes

            double capaciteVolumeParWagon =
                CapaciteConstants.VolumeMaxParWagon;

            int wagonsPoids =
                (int)Math.Ceiling(poids / capacitePoidsParWagonTonnes);

            int wagonsVolume =
                (int)Math.Ceiling(volume / capaciteVolumeParWagon);

            int wagonsNecessaires =
                Math.Max(wagonsPoids, wagonsVolume);

            if (wagonsNecessaires > route.AvailableWagons)
            {
                return new CalculReservationResult
                {
                    WagonsNecessaires = wagonsNecessaires,
                    TarifFinal = 0,
                    Message =
                        $"Impossible de réserver : " +
                        $"{wagonsNecessaires} wagons requis, " +
                        $"{route.AvailableWagons} disponibles."
                };
            }

            var facteurPoids = TarificationConstants.FacteurPoids[type];
            var facteurVolume = TarificationConstants.FacteurVolume[type];

            decimal tarif =
                TarificationConstants.PrixBase +
                ((decimal)poids * facteurPoids) +
                ((decimal)volume * facteurVolume);

            tarif = Math.Round(tarif, 2);

            return new CalculReservationResult
            {
                WagonsNecessaires = wagonsNecessaires,
                TarifFinal = tarif,
                Message = "Réservation possible."
            };
        }
    }
}