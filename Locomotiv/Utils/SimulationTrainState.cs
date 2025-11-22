using GMap.NET;
using Locomotiv.Model;

namespace Locomotiv.Utils
{
    public class SimulationTrainState
    {
        public Train Train { get; set; } = null!;
        public Itineraire Itineraire { get; set; } = null!;
        public List<Etape> Etapes { get; set; } = new();
        public List<PointArret> PointsArret { get; set; } = new();
        public int IndexSegment { get; set; }
        public DateTime DepartSegment { get; set; }
        public DateTime ArriveeSegment { get; set; }
        public PointLatLng PositionCourante { get; set; }

        public DateTime DateDepartPlanifie { get; set; }
        public DateTime DateDebutSimulation { get; set; }
    }
}
