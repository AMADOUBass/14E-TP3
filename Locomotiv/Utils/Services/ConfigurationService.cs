using Locomotiv.Utils.Services.Interfaces;
using System.Configuration;

namespace Locomotiv.Utils.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public string GetDbPath()
        {
            return ConfigurationManager.AppSettings["DbPath"]!;
        }

        public string GetAdminPassword()
        {
            return ConfigurationManager.AppSettings["AdminPassword"]!;
        }
    }
}