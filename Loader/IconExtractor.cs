using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Loader
{
    /// <summary>
    /// Extract .ico file
    /// </summary>
    public class IconExtractor
    {
        /// <summary>
        /// Extract first .ico file from Win32 resource of PE format file (.exe, .dll).
        /// </summary>
        /// <param name="sourceFile">path of PE format file to extract .ico file</param>
        /// <param name="stream">stream that written to .ico file which is extracted.</param>
        public static void Extract1stIconTo(string sourceFile, Stream stream)
        {
            var hModule = Kernel32.LoadLibraryEx(sourceFile, IntPtr.Zero, LOAD_LIBRARY.AS_DATAFILE);
            if (hModule == null) throw new Win32Exception();

            try
            {
                Kernel32.EnumResourceNames(hModule, RT.GROUP_ICON,
                    (IntPtr _hModule, RT type, IntPtr lpszName, IntPtr lParam) =>
                    {
                        var iconResInfos = GetIconResInfo(_hModule, lpszName);
                        WriteIconData(hModule, iconResInfos, stream);
                        return false;
                    }, IntPtr.Zero);
            }
            finally
            {
                Kernel32.FreeLibrary(hModule);
            }
        }

        private static ICONRESINF[] GetIconResInfo(IntPtr hModule, IntPtr lpszName)
        {
            var hResInf = Kernel32.FindResource(hModule, lpszName, RT.GROUP_ICON);
            if (hResInf == null) throw new Win32Exception();

            var hResource = Kernel32.LoadResource(hModule, hResInf);
            if (hResource == null) throw new Win32Exception();

            var ptrResource = Kernel32.LockResource(hResource);
            if (ptrResource == null) throw new Win32Exception();

            var iconResHead = (ICONRESHEAD)Marshal.PtrToStructure(ptrResource, typeof(ICONRESHEAD));
            var s1 = Marshal.SizeOf(typeof(ICONRESHEAD));
            var s2 = Marshal.SizeOf(typeof(ICONRESINF));

            var iconResInfos = Enumerable.Range(0, iconResHead.Count)
                .Select(i => (ICONRESINF)Marshal.PtrToStructure(ptrResource + s1 + (s2 * i), typeof(ICONRESINF)))
                .ToArray();

            return iconResInfos;
        }

        private static void WriteIconData(IntPtr hModule, ICONRESINF[] iconResInfos, Stream stream)
        {
            var s1 = Marshal.SizeOf(typeof(ICONFILEHEAD));
            var s2 = Marshal.SizeOf(typeof(ICONFILEINF));
            var address = s1 + (s2 * iconResInfos.Length);

            var iconFiles = iconResInfos
                .Select(iconResInf =>
                {
                    var iconBytes = GetResourceBytes(hModule, (IntPtr)iconResInf.ID, RT.ICON);
                    var iconFileInf = new ICONFILEINF
                    {
                        Cx = iconResInf.Cx,
                        Cy = iconResInf.Cy,
                        ColorCount = iconResInf.ColorCount,
                        Planes = iconResInf.Planes,
                        BitCount = iconResInf.BitCount,
                        Size = iconResInf.Size,
                        Address = (uint)address
                    };
                    address += iconBytes.Length;
                    return new { iconBytes, iconFileInf };
                }).ToList();

            // write headers
            var iconFileHead = new ICONFILEHEAD
            {
                Type = 1,
                Count = (ushort)iconResInfos.Length
            };
            var iconFileHeadBytes = StructureToBytes(iconFileHead);
            stream.Write(iconFileHeadBytes, 0, iconFileHeadBytes.Length);
            iconFiles.ForEach(iconFile =>
            {
                var bytes = StructureToBytes(iconFile.iconFileInf);
                stream.Write(bytes, 0, bytes.Length);
            });

            // write images
            iconFiles.ForEach(iconFile =>
            {
                stream.Write(iconFile.iconBytes, 0, iconFile.iconBytes.Length);
            });
        }

        private static byte[] GetResourceBytes(IntPtr hModule, IntPtr lpszName, RT type)
        {
            var hResInf = Kernel32.FindResource(hModule, lpszName, type);
            if (hResInf == null) throw new Win32Exception();

            var hResource = Kernel32.LoadResource(hModule, hResInf);
            if (hResource == null) throw new Win32Exception();

            var ptrResource = Kernel32.LockResource(hResource);
            if (ptrResource == null) throw new Win32Exception();

            var size = Kernel32.SizeofResource(hModule, hResInf);
            var buff = new byte[size];
            Marshal.Copy(ptrResource, buff, 0, buff.Length);

            return buff;
        }

        private static byte[] StructureToBytes(object obj)
        {
            var size = Marshal.SizeOf(obj);
            var bytes = new byte[size];
            var gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            Marshal.StructureToPtr(obj, gch.AddrOfPinnedObject(), false);
            gch.Free();

            return bytes;
        }
    }

    [Flags]
    internal enum LOAD_LIBRARY : uint
    {
        DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
        IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
        AS_DATAFILE = 0x00000002,
        AS_DATAFILE_EXCLUSIVE = 0x00000040,
        AS_IMAGE_RESOURCE = 0x00000020,
        WITH_ALTERED_SEARCH_PATH = 0x00000008
    }

    internal enum RT
    {
        CURSOR = 1,
        BITMAP = 2,
        ICON = 3,
        MENU = 4,
        DIALOG = 5,
        STRING = 6,
        FONTDIR = 7,
        FONT = 8,
        ACCELERATOR = 9,
        RCDATA = 10,
        MESSAGETABLE = 11,
        GROUP_CURSOR = 12,
        GROUP_ICON = 14,
        VERSION = 16,
        DLGINCLUDE = 17,
        PLUGPLAY = 19,
        VXD = 20,
        ANICURSOR = 21,
        ANIICON = 22,
        HTML = 23,
        MANIFEST = 24
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{Cx} x {Cy}, {BitCount}bit, {Size}bytes")]
    internal struct ICONRESINF
    {
        public byte Cx;
        public byte Cy;
        public byte ColorCount;
        public byte Reserved;
        public ushort Planes;
        public ushort BitCount;
        public uint Size;
        public ushort ID;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ICONRESHEAD
    {
        public ushort Reserved;
        public ushort Type;
        public ushort Count;
        // public ICONRESINF[] IconInf;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{Cx} x {Cy}, {BitCount}bit, {Size}bytes")]
    internal struct ICONFILEINF
    {
        public byte Cx;
        public byte Cy;
        public byte ColorCount;
        public byte Reserved;
        public ushort Planes;
        public ushort BitCount;
        public uint Size;
        public uint Address;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ICONFILEHEAD
    {
        public ushort Reserved;
        public ushort Type;
        public ushort Count;
        //ICONFILEINF	IconInf[];
    };

    internal delegate bool EnumResNameProcDelegate(IntPtr hModule, RT lpszType, IntPtr lpszName, IntPtr lParam);

    internal class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LOAD_LIBRARY dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool EnumResourceNames(IntPtr hModule, RT type, EnumResNameProcDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpszName, RT type);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr LockResource(IntPtr hResource);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);
    }
}