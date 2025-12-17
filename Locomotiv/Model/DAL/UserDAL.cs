using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class UserDAL : IUserDAL
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public UserDAL(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public User? FindByUsernameAndPassword(string username, string password)
        {
            try
            {
                var user = _context.Users
                    .Include(u => u.Station)
                    .FirstOrDefault(u => u.Username == username);

                if (user == null)
                {
                    _logger.Warning($"Tentative de connexion échouée : utilisateur {username} inexistant.");
                    return null;
                }

                bool isPasswordValid = PassWordHelper.VerifyPassword(
                    password,
                    user.PasswordHash,
                    user.PasswordSalt
                );

                if (!isPasswordValid)
                {
                    _logger.Warning($"Tentative de connexion échouée : mot de passe invalide pour {username}.");
                    return null;
                }

                _logger.Info($"Connexion réussie pour l’utilisateur {username}.");
                return user;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur critique lors de la tentative de connexion pour {username}.", ex);
                throw new AppFriendlyException("Impossible de vérifier les informations de connexion pour le moment.");
            }
        }
    }
}