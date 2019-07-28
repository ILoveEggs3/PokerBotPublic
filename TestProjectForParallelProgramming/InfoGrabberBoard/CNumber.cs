using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoGrabberBoard
{
    public class CNumber
    {
        public ulong PNumber { set; get; }

        public CNumber(ulong _number)
        {
            PNumber = _number;
        }
    }
}
