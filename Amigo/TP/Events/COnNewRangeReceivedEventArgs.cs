using System;
using System.Collections.Generic;

namespace Amigo.Events
{
    public sealed class COnNewRangeReceivedEventArgs : EventArgs
    {
        public List<(ulong, double)> PRange { get; private set; }

        public COnNewRangeReceivedEventArgs(List<(ulong, double)> _range)
        {
            PRange = _range ?? throw new ArgumentNullException("_range", "Invalid range");
        }
    }
}
