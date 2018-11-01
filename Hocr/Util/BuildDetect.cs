using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Net.FairfieldTek.Hocr.Util
{
    public class BuildDetect
    {
        private static readonly bool Is64BitProcess = IntPtr.Size == 8;
        private static bool _is64BitOperatingSystem = Is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        public static bool InternalCheckIsWow64()
        {
            if (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1 ||
                Environment.OSVersion.Version.Major >= 6)
                using (Process p = Process.GetCurrentProcess())
                {
                    return IsWow64Process(p.Handle, out var retVal) && retVal;
                }

            return false;
        }
    }
}