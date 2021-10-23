using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExertMiniBlink.Core
{
    public enum MiniBlinkMouseMessage: uint
    {
        MOUSE_MOVE = 0x0200,

        LBUTTON_DOWN = 0x0201,
        LBUTTON_UP = 0x0202,
        LBUTTON_DBLCLK = 0x0203,

        RBUTTON_DOWN = 0x0204,
        RBUTTON_UP = 0x0205,
        RBUTTON_DBLCLK = 0x0206,

        MBUTTON_DOWN = 0x0207,
        MBUTTON_UP = 0x0208,
        MBUTTON_DBLCLK = 0x0209,

        MOUSE_WHEEL = 0x020A,
    }
}
