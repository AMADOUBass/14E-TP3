using System.ComponentModel;
using Locomotiv.Model;

namespace Locomotiv.Utils.Services.Interfaces
{
    public interface IUserSessionService : INotifyPropertyChanged
    {
        User? ConnectedUser { get; set; }

        bool IsUserConnected { get; }
    }
}
