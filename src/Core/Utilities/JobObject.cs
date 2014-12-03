using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RimDev.Automation.WebHosting.Utilities
{
    internal class JobObject
    {
        private const int KillOnJobClose = 0x2000;

        private IntPtr handle;
        private bool disposed;

        public JobObject()
        {
            handle = NativeMethods.CreateJobObject(IntPtr.Zero, null);

            var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = KillOnJobClose
            };

            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = info
            };

            var length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            var extendedInfoPtr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

            if (!NativeMethods.SetInformationJobObject(handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
            {
                throw new Exception(string.Format("Unable to set information.  Error: {0}", Marshal.GetLastWin32Error()));
            }
        }

        public bool AddProcess(IntPtr processHandle)
        {
            return NativeMethods.AssignProcessToJobObject(handle, processHandle);
        }

        public bool AddProcess(int processId)
        {
            return AddProcess(Process.GetProcessById(processId).Handle);
        }

        public void Close()
        {
            NativeMethods.CloseHandle(handle);
            handle = IntPtr.Zero;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Close();
            }
        }
    }
}
