using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace md.stdl.Windows
{
    [Flags]
    internal enum ErrorModes : uint
    {
        SYSTEM_DEFAULT = 0x0,
        SEM_FAILCRITICALERRORS = 0x0001,
        SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
        SEM_NOGPFAULTERRORBOX = 0x0002,
        SEM_NOOPENFILEERRORBOX = 0x8000
    }

    /// <summary>
    /// Utilities for error message mode in case current application crashes
    /// </summary>
    public static class AppCrashErrorMessageSuppression
    {
        [DllImport("kernel32.dll")]
        internal static extern ErrorModes SetErrorMode(ErrorModes mode);

        /// <summary>
        /// Suppress AppCrash message box for current process only (and in theory child processes)
        /// </summary>
        public static void SuppressForProcess()
        {
            var desirederrormode = ErrorModes.SEM_NOGPFAULTERRORBOX | ErrorModes.SEM_NOOPENFILEERRORBOX;
            var preverrormode = SetErrorMode(desirederrormode);
            SetErrorMode(preverrormode | desirederrormode);
        }

        /// <summary>
        /// Suppress AppCrash message box for the entire machine. Caution this uses registry
        /// therefore it's persistent and requires admin rights.
        /// </summary>
        /// <param name="entireMachine"></param>
        public static void SuppressInRegistry(bool entireMachine)
        {
            try
            {
                var root = entireMachine ? Registry.LocalMachine : Registry.CurrentUser;
                var key = root.OpenSubKey(@"Software\Microsoft\Windows\Windows Error Reporting");
                key?.SetValue("Disabled", 1, RegistryValueKind.DWord);
            }
            catch (Exception e) { }
        }
    }
}
