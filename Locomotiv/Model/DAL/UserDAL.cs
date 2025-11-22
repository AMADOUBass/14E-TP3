using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Microsoft.EntityFrameworkCore;

namespace Locomotiv.Model.DAL
{
    public class UserDAL : IUserDAL
    {
        private readonly ApplicationDbContext _context;

        public UserDAL(ApplicationDbContext c)
        {
            _context = c;
        }

        public User? FindByUsernameAndPassword(string username, string password)
        {
            var user = _context
                .Users.Include(u => u.Station)
                .FirstOrDefault(u => u.Username == username);
            if (user == null)
                return null;

            bool isPasswordValid = PassWordHelper.VerifyPassword(
                password,
                user.PasswordHash,
                user.PasswordSalt
            );
            return isPasswordValid ? user : null;
        }
    }
}
