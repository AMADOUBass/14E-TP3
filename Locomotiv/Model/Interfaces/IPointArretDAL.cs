using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Locomotiv.Model;

namespace Locomotiv.Model.Interfaces
{
    public interface IPointArretDAL
    {
        List<PointArret> GetAllPointArrets();
    }
}
