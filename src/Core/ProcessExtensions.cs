using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RimDev.Automation.WebHosting
{
    internal static class ProcessExtensions
    {
        public static void SendStopMessage(this Process process)
        {
            Debug.Assert(process != null);

            try
            {
                if (!process.HasExited)
                {
                    var processId = process.Id;

                    for (var ptr = NativeMethods.GetTopWindow(IntPtr.Zero);
                        ptr != IntPtr.Zero;
                        ptr = NativeMethods.GetWindow(ptr, 2))
                    {
                        uint num;
                        NativeMethods.GetWindowThreadProcessId(ptr, out num);
                        if (processId == num)
                        {
                            var hWnd = new HandleRef(null, ptr);
                            NativeMethods.PostMessage(hWnd, 0x12, IntPtr.Zero, IntPtr.Zero);
                            return;
                        }
                    }
                }
            }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
        }
    }
}
