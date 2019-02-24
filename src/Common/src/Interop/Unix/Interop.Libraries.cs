// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

internal static partial class Interop
{
    internal static partial class Libraries
    {
        internal const string CoreLibNative = "System.Private.CoreLib.Native";
    }
}


namespace WebAssembly
{
    namespace JSInterop
    {
        internal static class InternalCalls
        {
            public static bool InvokeJSUnmarshalled(out string exception, string functionIdentifier, IntPtr arg0, IntPtr arg1, IntPtr arg2)
            {
                exception = null;
                return true;
            }
        }
    }
}
