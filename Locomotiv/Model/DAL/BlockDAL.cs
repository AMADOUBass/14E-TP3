using Locomotiv.Model.Interfaces;

namespace Locomotiv.Model.DAL
{
    public class BlockDAL : IBlockDAL
    {
        private readonly ApplicationDbContext _context;

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
                throw new ArgumentException(
                    "Les blocs doivent avoir un ID valide pour être mis à jour.",
                    nameof(block)
                );

            _context.Blocks.Attach(block);
            _context.Entry(block).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
        }
    }
}
