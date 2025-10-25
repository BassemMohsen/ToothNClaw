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
            public string DisplayName { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Frequency { get; set; }

            public override string ToString() => $"{DisplayName} - {Width}x{Height} @{Frequency}Hz";
        }

        #region Native structs and imports
        private const int QDC_ONLY_ACTIVE_PATHS = 0x00000002;
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x00000001;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        [StructLayout(LayoutKind.Sequential)]
        struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_PATH_INFO
        {
            public LUID sourceAdapterId;
            public uint sourceId;
            public LUID targetAdapterId;
            public uint targetId;
            public DISPLAYCONFIG_PATH_FLAGS flags;
            public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
            public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_SOURCE_MODE
        {
            public uint width;  // placeholder
            public uint height; // placeholder
            public DISPLAYCONFIG_PIXELFORMAT pixelFormat;
            public POINTL position;
        }

        enum DISPLAYCONFIG_PIXELFORMAT : uint
        {
            PIXELFORMAT_8BPP = 1,
            PIXELFORMAT_16BPP = 2,
            PIXELFORMAT_24BPP = 3,
            PIXELFORMAT_32BPP = 4,
            PIXELFORMAT_NONGDI = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTL
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_MODE_INFO
        {
            public DISPLAYCONFIG_MODE_INFO_TYPE infoType;
            public uint id;
            public LUID adapterId;
            public DISPLAYCONFIG_TARGET_MODE targetMode;
            public DISPLAYCONFIG_SOURCE_MODE sourceMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            public LUID adapterId;
            public uint id;
            public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            public DISPLAYCONFIG_ROTATION rotation;
            public DISPLAYCONFIG_SCALING scaling;
            public DISPLAYCONFIG_RATIONAL refreshRate;
            public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
            public bool targetAvailable;
            public uint statusFlags;
            public DISPLAYCONFIG_TARGET_MODE targetMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;
        }

        enum DISPLAYCONFIG_MODE_INFO_TYPE : uint
        {
            SOURCE = 1,
            TARGET = 2,
            DESKTOP_IMAGE = 3
        }

        [Flags]
        enum DISPLAYCONFIG_PATH_FLAGS : uint
        {
            NONE = 0,
            ACTIVE = 0x00000001
        }

        enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY : uint { OTHER = 4294967295, INTERNAL = 12 }

        enum DISPLAYCONFIG_ROTATION : uint { IDENTITY = 1 }

        enum DISPLAYCONFIG_SCALING : uint { IDENTITY = 1 }

        enum DISPLAYCONFIG_SCANLINE_ORDERING : uint { UNSPECIFIED = 0, PROGRESSIVE = 1 }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_TARGET_MODE
        {
            public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
        {
            public ulong pixelRate;
            public DISPLAYCONFIG_RATIONAL hSyncFreq;
            public DISPLAYCONFIG_RATIONAL vSyncFreq;
            public DISPLAYCONFIG_2DREGION activeSize;
            public DISPLAYCONFIG_2DREGION totalSize;
            public uint videoStandard;
            public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_2DREGION
        {
            public uint cx;
            public uint cy;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DEVMODE
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
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        private static extern int GetDisplayConfigBufferSizes(uint flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);

        [DllImport("user32.dll")]
        private static extern int QueryDisplayConfig(uint flags,
            ref uint numPathArrayElements,
            [Out] DISPLAYCONFIG_PATH_INFO[] pathInfoArray,
            ref uint numModeInfoArrayElements,
            [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            IntPtr currentTopologyId);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);
        #endregion

        /// <summary>
        /// Get the current resolution of the primary display.
        /// </summary>
        public static Resolution GetPrimaryDisplayResolution()
        {
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode))
                throw new InvalidOperationException("Failed to get primary display settings.");

            return new Resolution
            {
                Id = 0,
                DisplayName = "Primary Display",
                Width = devMode.dmPelsWidth,
                Height = devMode.dmPelsHeight,
                Frequency = devMode.dmDisplayFrequency
            };
        }

        /// <summary>
        /// Set the resolution of the primary display.
        /// </summary>
        public static bool SetPrimaryResolution(int width, int height)
        {
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode))
                throw new InvalidOperationException("Failed to get current display settings.");

            devMode.dmPelsWidth = width;
            devMode.dmPelsHeight = height;
            devMode.dmFields = 0x00080000 | 0x00100000; // DM_PELSWIDTH | DM_PELSHEIGHT

            int result = ChangeDisplaySettings(ref devMode, CDS_UPDATEREGISTRY);
            return result == DISP_CHANGE_SUCCESSFUL;
        }

        /// <summary>
        /// Get all supported resolutions for the primary display.
        /// </summary>
        public static List<Resolution> GetPrimaryDisplaySupportedResolutions()
        {
            var resList = new List<Resolution>();
            var seen = new HashSet<(int width, int height)>();

            // Get current/native resolution first
            DEVMODE current = new DEVMODE();
            current.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref current))
                throw new InvalidOperationException("Failed to get primary display settings.");

            var nativeRes = new Resolution
            {
                Id = 0,
                DisplayName = "Primary Display",
                Width = current.dmPelsWidth,
                Height = current.dmPelsHeight,
                Frequency = current.dmDisplayFrequency
            };

            resList.Add(nativeRes);
            seen.Add((nativeRes.Width, nativeRes.Height));

            // Enumerate all Windows-supported resolutions
            int modeNum = 0;
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

            while (EnumDisplaySettings(null, modeNum++, ref dm))
            {
                var key = (dm.dmPelsWidth, dm.dmPelsHeight);
                if (seen.Contains(key)) continue; // only unique width x height

                seen.Add(key);

                resList.Add(new Resolution
                {
                    Id = 0, // temporary, will reassign
                    DisplayName = "Primary Display",
                    Width = dm.dmPelsWidth,
                    Height = dm.dmPelsHeight,
                    Frequency = dm.dmDisplayFrequency
                });
            }

            // Sort descending width, then height, except native resolution first
            var sorted = resList.Skip(1)
                                .OrderByDescending(r => r.Width)
                                .ThenByDescending(r => r.Height)
                                .ToList();

            // Reassign IDs starting from 1
            for (int i = 0; i < sorted.Count; i++)
                sorted[i] = sorted[i] with { Id = i + 1 };

            // Insert native resolution at top
            sorted.Insert(0, nativeRes);

            return sorted;
        }
    }
}
