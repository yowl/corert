using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CoreRT.WebAssembly.Interop
{
    internal static class InternalCalls
    {
        // Copied from Mono
        // We're passing asyncHandle by ref not because we want it to be writable, but so it gets
        // passed as a pointer (4 bytes). We can pass 4-byte values, but not 8-byte ones.
        [DllImport("*", EntryPoint = "mono_wasm_invoke_js_marshalled")]
        public static extern string InvokeJSMarshalled(out string exception, ref long asyncHandle, string functionIdentifier, string argsJson);

        //        [MethodImpl(MethodImplOptions.InternalCall)]
        //        public static extern TRes InvokeJSUnmarshalled<T0, T1, T2, TRes>(out string exception, string functionIdentifier, T0 arg0, T1 arg1, T2 arg2);



        //Uno compatibility
        [DllImport("*", EntryPoint = "mono_wasm_invoke_js_marshalled")] 
        public static extern string InvokeJS(out int exception, string js);

        [DllImport("*", EntryPoint = "mono_wasm_invoke_js_marshalled")]
        public static extern string InvokeJSUnmarshalled(out int exception, string js, int p1, int p2, int p3);

    }
}
