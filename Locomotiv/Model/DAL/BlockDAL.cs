using Locomotiv.Model.Interfaces;
using Locomotiv.Utils.Services.Interfaces;

namespace Locomotiv.Model.DAL
{
    public class BlockDAL : IBlockDAL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public BlockDAL(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<Block> GetAllBlocks()
        {
            try
            {
                return _context.Blocks.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de la récupération des blocs.", ex);
                return Enumerable.Empty<Block>();
            }
        }

        public void UpdateBlock(Block block)
        {
            try
            {
                if (block.Id == 0)
                    throw new ArgumentException(
                        "Les blocs doivent avoir un ID valide pour être mis à jour.",
                        nameof(block)
                    );

                _context.Blocks.Attach(block);
                _context.Entry(block).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.SaveChanges();

                _logger.Info ($"Bloc mis à jour (Id={block.Id})");
            }
            catch (ArgumentException ex)
            {
                _logger.Error("Tentative de mise à jour d’un bloc sans ID valide.", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur critique lors de la mise à jour du bloc {Id}.", ex);
                throw new InvalidOperationException("Impossible de mettre à jour le bloc. Veuillez réessayer.", ex);
            }
        }
    }
}