namespace Locomotiv.Utils.Services.Interfaces
{
    public interface IConfigurationService
    {
        string GetDbPath();
        string GetAdminPassword();
    }
}