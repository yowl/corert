// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Threading
{
    public partial class EventWaitHandle
    {
        private void CreateEventCore(bool initialState, EventResetMode mode, string name, out bool createdNew)
        {
            if (name != null)
                throw new PlatformNotSupportedException(SR.PlatformNotSupported_NamedSynchronizationPrimitives);

            PrintLine("CreateEventCore");
            PrintLine(mode.ToString());

            Debug.Assert((mode == EventResetMode.AutoReset) || (mode == EventResetMode.ManualReset));
            SafeWaitHandle = WaitSubsystem.NewEvent(initialState, mode);
            createdNew = true;
        }

        [DllImport("*")]
        private static unsafe extern int printf(byte* str, byte* unused);

        private static unsafe void PrintString(string s)
        {
            int length = s.Length;
            fixed (char* curChar = s)
            {
                for (int i = 0; i < length; i++)
                {
                    TwoByteStr curCharStr = new TwoByteStr();
                    curCharStr.first = (byte)(*(curChar + i));
                    printf((byte*)&curCharStr, null);
                }
            }
        }

        public static void PrintLine(string s)
        {
            PrintString(s);
            PrintString("\n");
        }

        private static OpenExistingResult OpenExistingWorker(string name, out EventWaitHandle result)
        {
            throw new PlatformNotSupportedException(SR.PlatformNotSupported_NamedSynchronizationPrimitives);
        }

        public bool Reset()
        {
            SafeWaitHandle waitHandle = ValidateHandle();
            try
            {
                WaitSubsystem.ResetEvent(waitHandle.DangerousGetHandle());
                return true;
            }
            finally
            {
                waitHandle.DangerousRelease();
            }
        }

        public bool Set()
        {
            SafeWaitHandle waitHandle = ValidateHandle();
            try
            {
                WaitSubsystem.SetEvent(waitHandle.DangerousGetHandle());
                return true;
            }
            finally
            {
                waitHandle.DangerousRelease();
            }
        }

        private SafeWaitHandle ValidateHandle()
        {
            // The field value is modifiable via the public <see cref="WaitHandle.SafeWaitHandle"/> property, save it locally
            // to ensure that one instance is used in all places in this method
            SafeWaitHandle waitHandle = _waitHandle;
            if (waitHandle == null)
            {
                ThrowInvalidHandleException();
            }

            waitHandle.DangerousAddRef();
            return waitHandle;
        }
    }
}
