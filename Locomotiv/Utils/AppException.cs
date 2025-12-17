using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locomotiv.Utils
{
    public class AppFriendlyException : Exception
    {
        public AppFriendlyException(string message)
            : base(message) { }
    }
}
