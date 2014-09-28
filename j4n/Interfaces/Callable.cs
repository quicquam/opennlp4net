using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j4n.Interfaces
{
// ReSharper disable InconsistentNaming
    public interface Callable<out T>
    {
        T call();
    }
}
