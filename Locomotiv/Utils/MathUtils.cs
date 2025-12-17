using GMap.NET;
using Locomotiv.Model;

namespace Locomotiv.Utils
{
    public class MathUtils
    {
        public static double DistanceKm(PointLatLng a, PointLatLng b)
        {
            const double R = 6371;
            var dLat = (b.Lat - a.Lat) * Math.PI / 180;
            var dLon = (b.Lng - a.Lng) * Math.PI / 180;
            var lat1 = a.Lat * Math.PI / 180;
            var lat2 = b.Lat * Math.PI / 180;

            var aCalc =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(aCalc), Math.Sqrt(1 - aCalc));
            return R * c;
        }

        public static PointLatLng GetCenter(Block block)
        {
            return new PointLatLng(
                (block.LatitudeDepart + block.LatitudeArrivee) / 2,
                (block.LongitudeDepart + block.LongitudeArrivee) / 2
            );
        }

        public static Itineraire ConstruireItineraire(
            Train train,
            List<PointArret> arretsSelectionnes,
            DateTime dateDepart,
            DateTime dateArrivee,
            TimeSpan? tempsArret = null
        )
        {
            var nbSegments = Math.Max(1, arretsSelectionnes.Count - 1);
            var dureeTotale = dateArrivee - dateDepart;
            var dureeParSegment = TimeSpan.FromTicks(dureeTotale.Ticks / nbSegments);
            var arretDuration = tempsArret ?? TimeSpan.FromMinutes(2);

            return new Itineraire
            {
                Nom = $"Itinéraire {dateDepart:yyyyMMdd_HHmm}",
                DateDepart = dateDepart,
                DateArrivee = dateArrivee,
                TrainId = train.Id,
                Etapes = arretsSelectionnes
                    .Select(
                        (arret, index) =>
                            new Etape
                            {
                                Ordre = index,
                                Lieu = arret.Nom,
                                HeureArrivee =
                                    index == arretsSelectionnes.Count - 1
                                        ? dateArrivee
                                        : dateDepart + dureeParSegment * index,
                                HeureDepart =
                                    index == arretsSelectionnes.Count - 1
                                        ? null
                                        : dateDepart + dureeParSegment * index + arretDuration,
                                TrainId = train.Id,
                            }
                    )
                    .ToList(),
            };
        }

        public static IEnumerable<PointLatLng> GetPolylinePointsForBlock(Block block)
        {
            if (
                RailGeometry.PolylinesParNomBlock.TryGetValue(block.Nom, out var geoPoints)
                && geoPoints != null
                && geoPoints.Count >= 2
            )
            {
                return geoPoints;
            }

            return
            [
                new PointLatLng(block.LatitudeDepart, block.LongitudeDepart),
                new PointLatLng(block.LatitudeArrivee, block.LongitudeArrivee),
            ];
        }

        public static List<int> GetBlocksIdsForItinerary(Itineraire itineraire)
        {
            return itineraire
                .Etapes.Where(e => e.BlockId != null)
                .Select(e => e.BlockId!.Value)
                .Distinct()
                .ToList();
        }

        public static bool PartageUnBlockAvecUnAutreTrain(
            List<int> blocksItineraireIds,
            List<Train> autresTrainsEnTransit
        )
        {
            return autresTrainsEnTransit.Any(t =>
                t.BlockId != null && blocksItineraireIds.Contains(t.BlockId.Value)
            );
        }
    }
}
