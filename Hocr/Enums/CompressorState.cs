using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hocr.Enums
{
    public enum CompressorState
    {
        Pending,
        Paused,
        Running,
        Complete,
        Cancelled
    }
}
