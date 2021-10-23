using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExertMiniBlink.Core
{
    public enum MiniBlinkMouseFlags: uint
    {
        LBUTTON = 0x01,
        RBUTTON = 0x02,
        SHIFT = 0x04,
        CONTROL = 0x08,
        MBUTTON = 0x10,
    }
}
