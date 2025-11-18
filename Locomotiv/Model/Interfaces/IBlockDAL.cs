using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Model.Interfaces
{
    public interface IBlockDAL
    {
     
            IEnumerable<Block> GetAllBlocks();
            void UpdateBlock(Block block);


    }
}
