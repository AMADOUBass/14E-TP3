using Locomotiv.Model.Interfaces;

namespace Locomotiv.Model
{
    public class PointArretDAL : IPointArretDAL
    {
        private readonly ApplicationDbContext _context;

        public PointArretDAL(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PointArret> GetAllPointArrets()
        {
            return _context.PointArrets.ToList();
        }
    }
}
