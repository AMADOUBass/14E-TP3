using Locomotiv.Model;
using Locomotiv.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //public PointArret? GetPointArretById(int id)
        //{
        //    return _context.PointsInteret.FirstOrDefault(p => p.Id == id);
        //}

        //public void AjouterPointArret(PointArret pointArret)
        //{
        //    _context.PointsInteret.Add(pointArret);
        //    _context.SaveChanges();
        //}

        //public void SupprimerPointArret(int id)
        //{
        //    var poi = GetPointArretById(id);
        //    if (poi != null)
        //    {
        //        _context.PointsInteret.Remove(poi);
        //        _context.SaveChanges();
        //    }



        //}
    }
}


