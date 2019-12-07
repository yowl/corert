﻿using System;
using System.Runtime.CompilerServices;

namespace CoreRT.WebAssembly.Interop
{
    internal static class InternalCalls
    {
        // Copied from Mono
        // We're passing asyncHandle by ref not because we want it to be writable, but so it gets
        // passed as a pointer (4 bytes). We can pass 4-byte values, but not 8-byte ones.
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string InvokeJSMarshalled(out string exception, ref long asyncHandle, string functionIdentifier, string argsJson);

//        [MethodImpl(MethodImplOptions.InternalCall)]
//        public static extern TRes InvokeJSUnmarshalled<T0, T1, T2, TRes>(out string exception, string functionIdentifier, T0 arg0, T1 arg1, T2 arg2);

    }
}
