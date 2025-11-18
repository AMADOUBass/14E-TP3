using System.ComponentModel;

namespace Locomotiv.Utils.Services.Interfaces
{
    public interface IUserSessionService : INotifyPropertyChanged
    {
        User ConnectedUser { get; set; }

        bool IsUserConnected { get; }
    }
}
