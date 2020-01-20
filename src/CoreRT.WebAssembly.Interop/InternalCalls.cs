using System;
using System.Runtime.InteropServices;

namespace CoreRT.WebAssembly.Interop
{
    public static class InternalCalls
    {
        // Copied from Mono
        // We're passing asyncHandle by ref not because we want it to be writable, but so it gets
        // passed as a pointer (4 bytes). We can pass 4-byte values, but not 8-byte ones.
        [DllImport("*", EntryPoint = "mono_wasm_invoke_js_marshalled")]
        public static extern string InvokeJSMarshalled(out string exception, ref long asyncHandle, string functionIdentifier, string argsJson);

        //        [MethodImpl(MethodImplOptions.InternalCall)]
        //        public static extern TRes InvokeJSUnmarshalled<T0, T1, T2, TRes>(out string exception, string functionIdentifier, T0 arg0, T1 arg1, T2 arg2);



        //Uno compatibility
        [DllImport("*", EntryPoint = "corert_wasm_invoke_js")]
        private static extern string InvokeJSInternal(string js, int length, out int exception);
        
        public static string InvokeJS(string js, out int exception)
        {
            return InvokeJSInternal(js, js.Length, out exception);
        }

        //        [DllImport("*", EntryPoint = "mono_wasm_invoke_js_marshalled")]
        //        private static extern string InvokeJSUnmarshalledInternal(out int exception, string js, int p1, int p2, int p3);

        //Uno compatibility
        [DllImport("*", EntryPoint = "corert_wasm_invoke_js_unmarshalled")]
        private static extern IntPtr InvokeJSUnmarshalledInternal(string js, int length, IntPtr p1, IntPtr p2, IntPtr p3, out string exception);


        // to match https://github.com/unoplatform/uno/blob/024eebddd33ac0dfa6b6d8ea0871d5c6effc9f12/src/Uno.Foundation/Runtime.wasm.cs#L41
        // not https://github.com/mono/WebAssembly.JSInterop/blob/9e65a41cf1043ce29c7d722bd3885648a653e734/src/WebAssembly.JSInterop/InternalCalls.cs#L15
        public static IntPtr InvokeJSUnmarshalled(out string exception, string js, IntPtr p1, IntPtr p2, IntPtr p3)
        {
            return InvokeJSUnmarshalledInternal(js, js.Length, p1, p2 ,p3, out exception);
        }
    }
}
