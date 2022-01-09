using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure
{
    public static class Messenger<T>
    {
        public static event Action<T>? MessageRecieved;

        public static void Send(T message) => MessageRecieved?.Invoke(message);
    }
}
