using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils.EnumUtils
{
    public class EtatTrainHelper
    {
        public static ObservableCollection<EtatTrain> Valeurs { get;} =
            new ObservableCollection<EtatTrain>((EtatTrain[])Enum.GetValues(typeof(EtatTrain)));

    }
}
