using System;
using System.Runtime.InteropServices;

namespace Tooth.IGCL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ctl_fps_limiter_t
    {

        public bool isLimiterEnabled;
        public int fpsLimitValue;
    }
}
