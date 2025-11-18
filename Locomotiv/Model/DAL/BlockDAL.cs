using Locomotiv.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class BlockDAL : IBlockDAL
    {
        private readonly ApplicationDbContext _context;
        // Implementation for Train Data Access Layer

        public BlockDAL(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Block> GetAllBlocks()
        {
            return _context.Blocks.ToList();
        }

        public void UpdateBlock(Block block)
        {
            if (block.Id == 0)
                throw new ArgumentException("Les blocs doivent avoir un ID valide pour être mis à jour." , nameof(block));

            _context.Blocks.Attach(block);
            _context.Entry(block).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
        }




    }
}
