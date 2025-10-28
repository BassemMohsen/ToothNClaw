using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tooth.Backend
{
    public class DisplayController
    {
        public struct Resolution
        {
            public int Id { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Frequency { get; set; }
            public bool IsNative { get; set; }

            public override string ToString() =>
                IsNative ? $"{Width}x{Height} (Native)" : $"{Width}x{Height}";
        }

        private const int QDC_ONLY_ACTIVE_PATHS = 0x00000002;
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x00000001;
        private const int CDS_NORESET = 0x04;

        private const int CDS_TEST = 0x00000002;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        const int DM_PELSWIDTH = 0x00080000;
        const int DM_PELSHEIGHT = 0x00100000;

        public static int nativeWidth = 0;
        public static int nativeHeight = 0;


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
        }

        public enum DISP_CHANGE : int
        {
            Successful = 0,
            Restart = 1,
            Failed = -1,
            BadMode = -2,
            NotUpdated = -3,
            BadFlags = -4,
            BadParam = -5,
            BadDualView = -6
        }

        [Flags()]
        public enum ChangeDisplaySettingsFlags : uint
        {
            CDS_NONE = 0,
            CDS_UPDATEREGISTRY = 0x00000001,
            CDS_TEST = 0x00000002,
            CDS_FULLSCREEN = 0x00000004,
            CDS_GLOBAL = 0x00000008,
            CDS_SET_PRIMARY = 0x00000010,
            CDS_VIDEOPARAMETERS = 0x00000020,
            CDS_ENABLE_UNSAFE_MODES = 0x00000100,
            CDS_DISABLE_UNSAFE_MODES = 0x00000200,
            CDS_RESET = 0x40000000,
            CDS_RESET_EX = 0x20000000,
            CDS_NORESET = 0x10000000
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        [DllImport("user32.dll")]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, IntPtr lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern int GetDisplayConfigBufferSizes(uint flags, ref uint numPathArrayElements, ref uint numModeInfoArrayElements);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int QueryDisplayConfig(
            uint flags,
            ref uint numPathArrayElements,
            [Out] DISPLAYCONFIG_PATH_INFO[] pathArray,
            ref uint modeInfoArrayElements,
            [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            IntPtr currentTopologyId);

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_INFO
        {
            public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
            public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_MODE_INFO
        {
            public DISPLAYCONFIG_MODE_INFO_TYPE infoType;
            public uint id;
            public LUID adapterId;
            public DISPLAYCONFIG_TARGET_MODE targetMode; // union type, use targetMode
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DISPLAYCONFIG_TARGET_MODE
        {
            [FieldOffset(0)] public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
            // Other union fields omitted if not used
        }

        public enum DISPLAYCONFIG_MODE_INFO_TYPE : uint
        {
            DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1,
            DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2,
            DISPLAYCONFIG_MODE_INFO_TYPE_DESKTOP_IMAGE = 3,
            DISPLAYCONFIG_MODE_INFO_TYPE_FORCE_UINT32 = 0xFFFFFFFF
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            [FieldOffset(0)] public LUID adapterId;   // 8 bytes

            [FieldOffset(8)] public uint id;

            // Union: either full modeInfoIdx or split into cloneGroupId/sourceModeInfoIdx
            [FieldOffset(12)] public uint modeInfoIdx;  // full union value
            [FieldOffset(12)] private uint _dummyStruct; // bitfield representation

            [FieldOffset(16)] public uint statusFlags;

            // Helpers to extract bitfields
            public ushort CloneGroupId => (ushort)(_dummyStruct & 0xFFFF);
            public ushort SourceModeInfoIdx => (ushort)((_dummyStruct >> 16) & 0xFFFF);
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            [FieldOffset(0)] public LUID adapterId;       // 8 bytes
            [FieldOffset(8)] public uint id;

            // Union: either full modeInfoIdx or split into desktopModeInfoIdx/targetModeInfoIdx
            [FieldOffset(12)] public uint modeInfoIdx;    // full union
            [FieldOffset(12)] private uint _dummyStruct; // bitfield representation

            [FieldOffset(16)] public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            [FieldOffset(20)] public DISPLAYCONFIG_ROTATION rotation;
            [FieldOffset(24)] public DISPLAYCONFIG_SCALING scaling;
            [FieldOffset(28)] public DISPLAYCONFIG_RATIONAL refreshRate;
            [FieldOffset(36)] public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
            [FieldOffset(40)][MarshalAs(UnmanagedType.Bool)] public bool targetAvailable;
            [FieldOffset(44)] public uint statusFlags;

            // Bitfield helpers
            public ushort DesktopModeInfoIdx => (ushort)(_dummyStruct & 0xFFFF);
            public ushort TargetModeInfoIdx => (ushort)((_dummyStruct >> 16) & 0xFFFF);
        }

        public enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY : uint
        {
            OTHER = 0,
            HD15 = 1,
            SVIDEO = 2,
            COMPOSITE_VIDEO = 3,
            COMPONENT_VIDEO = 4,
            DVI = 5,
            HDMI = 6,
            LVDS = 8,
            DJPN = 9,
            SDI = 10,
            DISPLAYPORT_EXTERNAL = 11,
            DISPLAYPORT_EMBEDDED = 12,
            UDI_EXTERNAL = 13,
            UDI_EMBEDDED = 14,
            INTERNAL = 0x80000000,
            FORCE_UINT32 = 0xFFFFFFFF
        }

        public enum DISPLAYCONFIG_ROTATION : uint
        {
            IDENTITY = 1,
            ROTATE90 = 2,
            ROTATE180 = 3,
            ROTATE270 = 4,
            FORCE_UINT32 = 0xFFFFFFFF
        }

        public enum DISPLAYCONFIG_SCALING : uint
        {
            IDENTITY = 1,
            CENTERED = 2,
            STRETCHED = 3,
            ASPECTRATIOCENTEREDMAX = 4,
            CUSTOM = 5,
            PREFERRED = 128,
            FORCE_UINT32 = 0xFFFFFFFF
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
        {
            [FieldOffset(0)] public ulong pixelRate;

            [FieldOffset(8)] public DISPLAYCONFIG_RATIONAL hSyncFreq;
            [FieldOffset(16)] public DISPLAYCONFIG_RATIONAL vSyncFreq;

            [FieldOffset(24)] public DISPLAYCONFIG_2DREGION activeSize;
            [FieldOffset(32)] public DISPLAYCONFIG_2DREGION totalSize;

            // Union: either full uint or bitfield access
            [FieldOffset(40)] public uint videoStandard; // full value
            [FieldOffset(40)] private uint _additionalSignalInfo; // bits: 16+6+10

            [FieldOffset(44)] public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;

            // Helpers to extract bitfields
            public ushort AdditionalVideoStandard => (ushort)(_additionalSignalInfo & 0xFFFF);
            public byte VSyncFreqDivider => (byte)((_additionalSignalInfo >> 16) & 0x3F);
            public ushort ReservedBits => (ushort)((_additionalSignalInfo >> 22) & 0x3FF);
        }

        public enum DISPLAYCONFIG_SCANLINE_ORDERING : uint
        {
            UNSPECIFIED = 0,
            PROGRESSIVE = 1,
            INTERLACED = 2,
            INTERLACED_UPPERFIELDFIRST = 3,
            INTERLACED_LOWERFIELDFIRST = 3, // same as 3 in C typedef
            FORCE_UINT32 = 0xFFFFFFFF
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VideoSignalAdditional
        {
            private uint value;

            public uint VideoStandard
            {
                get => value & 0xFFFF;       // bits 0-15
                set => value = (value & 0xFFFF0000) | (value & 0xFFFF);
            }

            public uint VSyncFreqDivider
            {
                get => (value >> 16) & 0x3F; // bits 16-21
                set => value = (value & 0xFFC03FFF) | ((value & 0x3F) << 16);
            }

            public uint Reserved
            {
                get => (value >> 22) & 0x3FF; // bits 22-31
                set => value = (value & 0x003FFFFF) | ((value & 0x3FF) << 22);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_2DREGION
        {
            public uint cx;
            public uint cy;
        }

        public static List<Resolution> GetPrimaryDisplaySupportedResolutions()
        {
            List<Resolution> result = new();
            DEVMODE devMode = new();
            devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            // 1. Get Windows-recommended/native resolution using QueryDisplayConfig
            uint pathCount = 0;
            uint modeCount = 0;


            // Get required buffer sizes first
            int sizeResult = GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, ref pathCount, ref modeCount);
            if (sizeResult != 0)
                throw new Exception($"GetDisplayConfigBufferSizes failed: {sizeResult}");

            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

            int res = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);

            for (int i = 0; i < paths.Length; i++)
            {
                var target = paths[i].targetInfo;


                // Get the index of the mode for this target
                int modeIdx = (int)target.modeInfoIdx;
                var modeInfo = modes[modeIdx];

                // Make sure it's a target mode
                if (modeInfo.infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
                {
                    nativeWidth = (int) modeInfo.targetMode.targetVideoSignalInfo.activeSize.cx;
                    nativeHeight = (int) modeInfo.targetMode.targetVideoSignalInfo.activeSize.cy;

                    Console.WriteLine($"Display {target.id}: Native Resolution is: {nativeWidth}x{nativeHeight}");
                }
            }

            // 2. Enumerate all Windows-visible resolutions
            int id = 0;
            HashSet<(int, int)> seen = new();
            int modeNum = 0;
            while (EnumDisplaySettings(null, modeNum, ref devMode))
            {
                var resTuple = (devMode.dmPelsWidth, devMode.dmPelsHeight);
                if (seen.Contains(resTuple))
                {
                    modeNum++;
                    continue;
                }

                seen.Add(resTuple);
                bool isNative = devMode.dmPelsWidth == nativeWidth && devMode.dmPelsHeight == nativeHeight;

                result.Add(new Resolution
                {
                    Id = id++,
                    Width = devMode.dmPelsWidth,
                    Height = devMode.dmPelsHeight,
                    Frequency = devMode.dmDisplayFrequency,
                    IsNative = isNative
                });

                modeNum++;
            }

            // Ensure native resolution is first in list
            var nativeItem = result.FirstOrDefault(r => r.IsNative);
            if (nativeItem.Width != 0)
            {
                result.Remove(nativeItem);
                result.Insert(0, nativeItem);
            }

            // Re-index IDs
            for (int i = 0; i < result.Count; i++)
            {
                result[i] = new Resolution
                {
                    Id = i,
                    Width = result[i].Width,
                    Height = result[i].Height,
                    Frequency = result[i].Frequency,
                    IsNative = result[i].IsNative
                };
            }

            // Sort descending by width/height except native stays on top
            var nativeRes = result.First();
            var sortedRest = result.Skip(1).OrderByDescending(r => r.Width).ThenByDescending(r => r.Height).ToList();
            sortedRest.Insert(0, nativeRes);
            return sortedRest;
        }

        public static Resolution GetPrimaryDisplayResolution()
        {
            DEVMODE devMode = new();
            devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode))
                throw new Exception("Failed to get current display settings.");

            Resolution resolution = new Resolution();
            resolution.Id = 0;
            resolution.Width = devMode.dmPelsWidth;
            resolution.Height = devMode.dmPelsHeight;
            resolution.Frequency = devMode.dmDisplayFrequency;

            if (nativeWidth != 0 && nativeHeight != 0 &&
                devMode.dmPelsWidth == nativeWidth && devMode.dmPelsHeight == nativeHeight)
                resolution.IsNative = true;
            else
                resolution.IsNative = false; // current resolution is not necessarily native


            return resolution;
        }

        private static DEVMODE GetDevMode1()
        {
            DEVMODE dm = new DEVMODE();
            dm.dmDeviceName = new string(new char[32]);
            dm.dmFormName = new string(new char[32]);
            dm.dmSize = (short)Marshal.SizeOf(dm);
            return dm;
        }

        public static DEVMODE GetCurrentDisplayMode(string deviceName)
        {
            DEVMODE dm = GetDevMode1();
            if (false != EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref dm))
            {
                return dm;
            }
            throw new InvalidOperationException("An error occurred getting current display mode for '" + deviceName + "'.");
        }

        public static int ComputeDisplayModeScore(String deviceName, DEVMODE mode)
        {
            return ComputeDisplayModeScore(deviceName, mode, true);

        }

        public static int ComputeDisplayModeScore(String deviceName, DEVMODE mode, bool checkIfSupportedMode)
        {
            if (checkIfSupportedMode && !IsDeviceSupportedDisplayMode(deviceName, mode))
            {
                return 0;
            }

            return mode.dmPelsWidth * mode.dmPelsHeight * mode.dmBitsPerPel + mode.dmDisplayFrequency;
        }
        public static bool DisplayModesAreEqual(DEVMODE first, DEVMODE second)
        {
            return first.dmPelsWidth == second.dmPelsWidth && first.dmPelsHeight == second.dmPelsHeight && first.dmBitsPerPel == second.dmBitsPerPel && first.dmDisplayFrequency == second.dmDisplayFrequency;
        }

        public static bool IsDeviceSupportedDisplayMode(string deviceName, DEVMODE checkedDevMode)
        {
            DEVMODE mode = new DEVMODE();
            mode.dmSize = (short)Marshal.SizeOf(mode);
            int modeIndex = 0;

            while (false != EnumDisplaySettings(deviceName, modeIndex, ref mode))
            {
                if (DisplayModesAreEqual(checkedDevMode, mode))
                {
                    return true;
                }

                modeIndex++;
            }

            return false;
        }

        public static DEVMODE GetOptimalDisplayMode(string deviceName)
        {
            DEVMODE mode = new DEVMODE();
            mode.dmSize = (short)Marshal.SizeOf(mode);

            int modeIndex = 0;
            int bestModeIndex = 0;
            int bestScore = 0;

            //Console.WriteLine("Get optimal displaymode " + deviceName);

            while (false != EnumDisplaySettings(deviceName, modeIndex, ref mode))
            {
                int currentModeScore = ComputeDisplayModeScore(deviceName, mode, false);

                //Console.WriteLine("Found a mode for " + deviceName + ": " + mode.dmPelsWidth + "x" + mode.dmPelsHeight + " at " + mode.dmDisplayFrequency + "Hz");

                if (currentModeScore > bestScore)
                {
                    bestModeIndex = modeIndex;
                    bestScore = currentModeScore;
                    Console.WriteLine("Found a better mode for " + deviceName + ": " + mode.dmPelsWidth + "x" + mode.dmPelsHeight + " at " + mode.dmDisplayFrequency + "Hz" + " at index " + modeIndex);
                }
                modeIndex++;
            }

            if (bestModeIndex < 0)
            {
                throw new InvalidOperationException("An error occurred getting optimal display mode for '" + deviceName + "'.");
            }

            if (false != EnumDisplaySettings(deviceName, bestModeIndex, ref mode))
            {
                return mode;
            }

            throw new InvalidOperationException("An error occurred getting optimal display mode for '" + deviceName + "' at index " + bestModeIndex + ".");
        }

        /**
         * Compares two display modes to determine the best. 
         * Returns a negative integer, zero, or a positive integer 
         * as the first mode is worse than, equal to, or better than the 
         * second mode based on the "score" of the display mode
         **/
        public static int CompareDisplayModes(String deviceName, DEVMODE first, DEVMODE second)
        {
            int firstScore = ComputeDisplayModeScore(deviceName, first);
            int secondScore = ComputeDisplayModeScore(deviceName, second);
            if (firstScore < secondScore)
            {
                return -1;
            }
            else if (firstScore > secondScore)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Set the resolution of the primary display.
        /// </summary>
        public static bool SetPrimaryResolution(int width, int height)
        {
            // Device name for the primary display
            string primaryDisplayName = @"\\.\DISPLAY1";

            DEVMODE devMode = GetCurrentDisplayMode(primaryDisplayName);

            devMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT;
            devMode.dmPelsWidth = width;
            devMode.dmPelsHeight = height;

            // Apply the mode (and store in registry)
            int applyResult = ChangeDisplaySettings( ref devMode, CDS_UPDATEREGISTRY | CDS_NORESET);
            return applyResult == 0;

            /*DISP_CHANGE applyResult =  ChangeDisplaySettingsEx(
                        primaryDisplayName,
                        ref devMode,
                        (IntPtr)null,
                        ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_NORESET,
                        IntPtr.Zero);

            if (applyResult != DISP_CHANGE.Successful)
            {
                Console.WriteLine($"Failed to apply resolution {width}x{height}. Error code: {applyResult}");
                return false;
            }
            else
            {

                // Apply the settings and update the registry
                ChangeDisplaySettingsEx(null, IntPtr.Zero, (IntPtr)null, ChangeDisplaySettingsFlags.CDS_NONE, (IntPtr)null);
                Console.WriteLine($"Successfully applied resolution {width}x{height}.");

                return true;
            }*/
        }
    }
}
