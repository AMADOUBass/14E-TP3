using System.Collections.ObjectModel;
using Locomotiv.Model.enums;

namespace Locomotiv.Utils.EnumUtils
{
    public class EtatTrainHelper
    {
        public static ObservableCollection<EtatTrain> Valeurs { get; } =
            new ObservableCollection<EtatTrain>((EtatTrain[])Enum.GetValues(typeof(EtatTrain)));
    }
}
