// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// This is where we group together all the runtime export calls.
//

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Internal.Runtime;
using Internal.Runtime.CompilerServices;

namespace System.Runtime
{
    internal static class RuntimeExports
    {
        //
        // internal calls for allocation
        //
        [RuntimeExport("RhNewObject")]
        public static unsafe object RhNewObject(EETypePtr pEEType)
        {
            EEType* ptrEEType = (EEType*)pEEType.ToPointer();

            // This is structured in a funny way because at the present state of things in CoreRT, the Debug.Assert
            // below will call into the assert defined in the class library (and not the MRT version of it). The one
            // in the class library is not low level enough to be callable when GC statics are not initialized yet.
            // Feel free to restructure once that's not a problem.
#if DEBUG
            bool isValid = !ptrEEType->IsGenericTypeDefinition &&
                !ptrEEType->IsInterface &&
                !ptrEEType->IsArray &&
                !ptrEEType->IsString &&
                !ptrEEType->IsByRefLike;
            if (!isValid)
                Debug.Assert(false);
#endif

#if FEATURE_64BIT_ALIGNMENT
            if (ptrEEType->RequiresAlign8)
            {
                if (ptrEEType->IsValueType)
                    return InternalCalls.RhpNewFastMisalign(ptrEEType);
                if (ptrEEType->IsFinalizable)
                    return InternalCalls.RhpNewFinalizableAlign8(ptrEEType);
                return InternalCalls.RhpNewFastAlign8(ptrEEType);
            }
            else
#endif // FEATURE_64BIT_ALIGNMENT
            {
                if (ptrEEType->IsFinalizable)
                    return InternalCalls.RhpNewFinalizable(ptrEEType);
                return InternalCalls.RhpNewFast(ptrEEType);
            }
        }

        [RuntimeExport("RhNewArray")]
        public static unsafe object RhNewArray(EETypePtr pEEType, int length)
        {
            EEType* ptrEEType = (EEType*)pEEType.ToPointer();

            Debug.Assert(ptrEEType->IsArray || ptrEEType->IsString);

#if FEATURE_64BIT_ALIGNMENT
            if (ptrEEType->RequiresAlign8)
            {
                return InternalCalls.RhpNewArrayAlign8(ptrEEType, length);
            }
            else
#endif // FEATURE_64BIT_ALIGNMENT
            {
                return InternalCalls.RhpNewArray(ptrEEType, length);
            }
        }

        [RuntimeExport("RhBox")]
        public static unsafe object RhBox(EETypePtr pEEType, ref byte data)
        {
            EEType* ptrEEType = (EEType*)pEEType.ToPointer();
            int dataOffset = 0;
            object result;

            // If we're boxing a Nullable<T> then either box the underlying T or return null (if the
            // nullable's value is empty).
            if (ptrEEType->IsNullable)
            {
                // The boolean which indicates whether the value is null comes first in the Nullable struct.
                if (data == 0)
                    return null;

                // Switch type we're going to box to the Nullable<T> target type and advance the data pointer
                // to the value embedded within the nullable.
                dataOffset = ptrEEType->NullableValueOffset;
                ptrEEType = ptrEEType->NullableType;
            }

#if FEATURE_64BIT_ALIGNMENT
            if (ptrEEType->RequiresAlign8)
            {
                result = InternalCalls.RhpNewFastMisalign(ptrEEType);
            }
            else
#endif // FEATURE_64BIT_ALIGNMENT
            {
                result = InternalCalls.RhpNewFast(ptrEEType);
            }
            InternalCalls.RhpBox(result, ref Unsafe.Add(ref data, dataOffset));
            return result;
        }

        public struct TwoByteStr
        {
            internal byte first;
            public byte second;
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

        internal static void PrintLine(string s)
        {
            PrintString(s);
            PrintString("\n");
        }

        [RuntimeExport("RhBoxAny")]
        public static unsafe object RhBoxAny(ref byte data, EETypePtr pEEType)
        {
            EEType* ptrEEType = (EEType*)pEEType.ToPointer();
            if (ptrEEType->IsValueType)
            {
//                PrintLine("value type");
                return RhBox(pEEType, ref data);
            }
            else
            {
                PrintLine("!value type");
//                if ((byte*)data == null)
//                {
//                    PrintLine("data is null");
//                }
                var o = Unsafe.As<byte, object>(ref data);
//                if (o == null)
//                {
//                    PrintLine("o is null");
//                }

                return o;
            }
        }

        private static unsafe bool UnboxAnyTypeCompare(EEType* pEEType, EEType* ptrUnboxToEEType)
        {
            if (TypeCast.AreTypesEquivalentInternal(pEEType, ptrUnboxToEEType))
                return true;

            if (pEEType->CorElementType == ptrUnboxToEEType->CorElementType)
            {
                // Enum's and primitive types should pass the UnboxAny exception cases
                // if they have an exactly matching cor element type.
                switch (ptrUnboxToEEType->CorElementType)
                {
                    case CorElementType.ELEMENT_TYPE_I1:
                    case CorElementType.ELEMENT_TYPE_U1:
                    case CorElementType.ELEMENT_TYPE_I2:
                    case CorElementType.ELEMENT_TYPE_U2:
                    case CorElementType.ELEMENT_TYPE_I4:
                    case CorElementType.ELEMENT_TYPE_U4:
                    case CorElementType.ELEMENT_TYPE_I8:
                    case CorElementType.ELEMENT_TYPE_U8:
                    case CorElementType.ELEMENT_TYPE_I:
                    case CorElementType.ELEMENT_TYPE_U:
                        return true;
                }
            }

            return false;
        }

        [RuntimeExport("RhUnboxAny")]
        public static unsafe void RhUnboxAny(object o, ref byte data, EETypePtr pUnboxToEEType)
        {
            EEType* ptrUnboxToEEType = (EEType*)pUnboxToEEType.ToPointer();
            if (ptrUnboxToEEType->IsValueType)
            {
                bool isValid = false;

                if (ptrUnboxToEEType->IsNullable)
                {
                    isValid = (o == null) || TypeCast.AreTypesEquivalentInternal(o.EEType, ptrUnboxToEEType->NullableType);
                }
                else
                {
                    isValid = (o != null) && UnboxAnyTypeCompare(o.EEType, ptrUnboxToEEType);
                }

                if (!isValid)
                {
                    // Throw the invalid cast exception defined by the classlib, using the input unbox EEType* 
                    // to find the correct classlib.

                    ExceptionIDs exID = o == null ? ExceptionIDs.NullReference : ExceptionIDs.InvalidCast;

                    throw ptrUnboxToEEType->GetClasslibException(exID);
                }

                InternalCalls.RhUnbox(o, ref data, ptrUnboxToEEType);
            }
            else
            {
                if (o != null && (TypeCast.IsInstanceOf(o, ptrUnboxToEEType) == null))
                {
                    throw ptrUnboxToEEType->GetClasslibException(ExceptionIDs.InvalidCast);
                }

                Unsafe.As<byte, object>(ref data) = o;
            }
        }

        //
        // Unbox helpers with RyuJIT conventions
        //
        [RuntimeExport("RhUnbox2")]
        public static unsafe ref byte RhUnbox2(EETypePtr pUnboxToEEType, object obj)
        {
            EEType* ptrUnboxToEEType = (EEType*)pUnboxToEEType.ToPointer();
            if ((obj == null) || !UnboxAnyTypeCompare(obj.EEType, ptrUnboxToEEType))
            {
                ExceptionIDs exID = obj == null ? ExceptionIDs.NullReference : ExceptionIDs.InvalidCast;
                throw ptrUnboxToEEType->GetClasslibException(exID);
            }
            return ref obj.GetRawData();
        }

        [RuntimeExport("RhUnboxNullable")]
        public static unsafe void RhUnboxNullable(ref byte data, EETypePtr pUnboxToEEType, object obj)
        {
            EEType* ptrUnboxToEEType = (EEType*)pUnboxToEEType.ToPointer();
            if ((obj != null) && !TypeCast.AreTypesEquivalentInternal(obj.EEType, ptrUnboxToEEType->NullableType))
            {
                throw ptrUnboxToEEType->GetClasslibException(ExceptionIDs.InvalidCast);
            }
            InternalCalls.RhUnbox(obj, ref data, ptrUnboxToEEType);
        }

        [RuntimeExport("RhArrayStoreCheckAny")]
        public static unsafe void RhArrayStoreCheckAny(object array, ref byte data)
        {
            if (array == null)
            {
                return;
            }

            Debug.Assert(array.EEType->IsArray, "first argument must be an array");

            EEType* arrayElemType = array.EEType->RelatedParameterType;
            if (arrayElemType->IsValueType)
            {
                return;
            }

            TypeCast.CheckArrayStore(array, Unsafe.As<byte, object>(ref data));
        }

        [RuntimeExport("RhBoxAndNullCheck")]
        public static unsafe bool RhBoxAndNullCheck(ref byte data, EETypePtr pEEType)
        {
            EEType* ptrEEType = (EEType*)pEEType.ToPointer();
            if (ptrEEType->IsValueType)
                return true;
            else
                return Unsafe.As<byte, object>(ref data) != null;
        }

#pragma warning disable 169 // The field 'System.Runtime.RuntimeExports.Wrapper.o' is never used. 
        private class Wrapper
        {
            private object _o;
        }
#pragma warning restore 169

        [RuntimeExport("RhAllocLocal")]
        public static unsafe object RhAllocLocal(EETypePtr pEEType)
        {
            EEType* ptrEEType = (EEType*)pEEType.ToPointer();
            if (ptrEEType->IsValueType)
            {
#if FEATURE_64BIT_ALIGNMENT
                if (ptrEEType->RequiresAlign8)
                    return InternalCalls.RhpNewFastMisalign(ptrEEType);
#endif
                return InternalCalls.RhpNewFast(ptrEEType);
            }
            else
                return new Wrapper();
        }

        // RhAllocLocal2 helper returns the pointer to the data region directly
        // instead of relying on the code generator to offset the local by the 
        // size of a pointer to get the data region.
        [RuntimeExport("RhAllocLocal2")]
        public static unsafe ref byte RhAllocLocal2(EETypePtr pEEType)
        {
            EEType* ptrEEType = (EEType*)pEEType.ToPointer();
            if (ptrEEType->IsValueType)
            {
#if FEATURE_64BIT_ALIGNMENT
                if (ptrEEType->RequiresAlign8)
                    return ref InternalCalls.RhpNewFastMisalign(ptrEEType).GetRawData();
#endif
                return ref InternalCalls.RhpNewFast(ptrEEType).GetRawData();
            }
            else
                return ref new Wrapper().GetRawData();
        }

        [RuntimeExport("RhMemberwiseClone")]
        public static unsafe object RhMemberwiseClone(object src)
        {
            object objClone;

            if (src.EEType->IsArray)
                objClone = RhNewArray(new EETypePtr((IntPtr)src.EEType), Unsafe.As<Array>(src).Length);
            else
                objClone = RhNewObject(new EETypePtr((IntPtr)src.EEType));

            InternalCalls.RhpCopyObjectContents(objClone, src);

            return objClone;
        }

        [RuntimeExport("RhpReversePInvokeBadTransition")]
        public static void RhpReversePInvokeBadTransition(IntPtr returnAddress)
        {
            EH.FailFastViaClasslib(
                RhFailFastReason.IllegalNativeCallableEntry,
                null,
                returnAddress);
        }

        [RuntimeExport("RhGetCurrentThreadStackTrace")]
        [MethodImpl(MethodImplOptions.NoInlining)] // Ensures that the RhGetCurrentThreadStackTrace frame is always present
        public static unsafe int RhGetCurrentThreadStackTrace(IntPtr[] outputBuffer)
        {
            fixed (IntPtr* pOutputBuffer = outputBuffer)
                return RhpGetCurrentThreadStackTrace(pOutputBuffer, (uint)((outputBuffer != null) ? outputBuffer.Length : 0));
        }

        [DllImport(Redhawk.BaseName, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int RhpGetCurrentThreadStackTrace(IntPtr* pOutputBuffer, uint outputBufferLength);

        // Worker for RhGetCurrentThreadStackTrace.  RhGetCurrentThreadStackTrace just allocates a transition
        // frame that will be used to seed the stack trace and this method does all the real work.
        //
        // Input:           outputBuffer may be null or non-null
        // Return value:    positive: number of entries written to outputBuffer
        //                  negative: number of required entries in outputBuffer in case it's too small (or null)
        // Output:          outputBuffer is filled in with return address IPs, starting with placing the this
        //                  method's return address into index 0
        //
        // NOTE: We don't want to allocate the array on behalf of the caller because we don't know which class
        // library's objects the caller understands (we support multiple class libraries with multiple root
        // System.Object types).
        [NativeCallable(EntryPoint = "RhpCalculateStackTraceWorker", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe int RhpCalculateStackTraceWorker(IntPtr* pOutputBuffer, uint outputBufferLength)
        {
            uint nFrames = 0;
            bool success = true;

            StackFrameIterator frameIter = new StackFrameIterator();

            bool isValid = frameIter.Init(null);
            Debug.Assert(isValid, "Missing RhGetCurrentThreadStackTrace frame");

            // Note that the while loop will skip RhGetCurrentThreadStackTrace frame
            while (frameIter.Next())
            {
                if (nFrames < outputBufferLength)
                    pOutputBuffer[nFrames] = new IntPtr(frameIter.ControlPC);
                else
                    success = false;

                nFrames++;
            }

            return success ? (int)nFrames : -(int)nFrames;
        }

        // The GC conservative reporting descriptor is a special structure of data that the GC
        // parses to determine whether there are specific regions of memory that it should not
        // collect or move around.
        // During garbage collection, the GC will inspect the data in this structure, and verify that:
        //  1) _magic is set to the magic number (also hard coded on the GC side)
        //  2) The reported region is valid (checks alignments, size, within bounds of the thread memory, etc...)
        //  3) The ConservativelyReportedRegionDesc pointer must be reported by a frame which does not make a pinvoke transition.
        //  4) The value of the _hash field is the computed hash of _regionPointerLow with _regionPointerHigh
        //  5) The region must be IntPtr aligned, and have a size which is also IntPtr aligned
        // If all conditions are satisfied, the region of memory starting at _regionPointerLow and ending at
        // _regionPointerHigh will be conservatively reported.
        // This can only be used to report memory regions on the current stack and the structure must itself 
        // be located on the stack.
        public struct ConservativelyReportedRegionDesc
        {
            internal const ulong MagicNumber64 = 0x87DF7A104F09E0A9UL;
            internal const uint MagicNumber32 = 0x4F09E0A9;

            internal UIntPtr _magic;
            internal UIntPtr _regionPointerLow;
            internal UIntPtr _regionPointerHigh;
            internal UIntPtr _hash;
        }

        [RuntimeExport("RhInitializeConservativeReportingRegion")]
        public static unsafe void RhInitializeConservativeReportingRegion(ConservativelyReportedRegionDesc* regionDesc, void* bufferBegin, int cbBuffer)
        {
            Debug.Assert((((int)bufferBegin) & (sizeof(IntPtr) - 1)) == 0, "Buffer not IntPtr aligned");
            Debug.Assert((cbBuffer & (sizeof(IntPtr) - 1)) == 0, "Size of buffer not IntPtr aligned");

            UIntPtr regionPointerLow = (UIntPtr)bufferBegin;
            UIntPtr regionPointerHigh = (UIntPtr)(((byte*)bufferBegin) + cbBuffer);

            // Setup pointers to start and end of region
            regionDesc->_regionPointerLow = regionPointerLow;
            regionDesc->_regionPointerHigh = regionPointerHigh;

            // Activate the region for processing
#if BIT64
            ulong hash = ConservativelyReportedRegionDesc.MagicNumber64;
            hash = ((hash << 13) ^ hash) ^ (ulong)regionPointerLow;
            hash = ((hash << 13) ^ hash) ^ (ulong)regionPointerHigh;

            regionDesc->_hash = new UIntPtr(hash);
            regionDesc->_magic = new UIntPtr(ConservativelyReportedRegionDesc.MagicNumber64);
#else
            uint hash = ConservativelyReportedRegionDesc.MagicNumber32;
            hash = ((hash << 13) ^ hash) ^ (uint)regionPointerLow;
            hash = ((hash << 13) ^ hash) ^ (uint)regionPointerHigh;

            regionDesc->_hash = new UIntPtr(hash);
            regionDesc->_magic = new UIntPtr(ConservativelyReportedRegionDesc.MagicNumber32);
#endif
        }

        // Disable conservative reporting 
        [RuntimeExport("RhDisableConservativeReportingRegion")]
        public static unsafe void RhDisableConservativeReportingRegion(ConservativelyReportedRegionDesc* regionDesc)
        {
            regionDesc->_magic = default(UIntPtr);
        }

        [RuntimeExport("RhCreateThunksHeap")]
        public static object RhCreateThunksHeap(IntPtr commonStubAddress)
        {
            return ThunksHeap.CreateThunksHeap(commonStubAddress);
        }

        [RuntimeExport("RhAllocateThunk")]
        public static IntPtr RhAllocateThunk(object thunksHeap)
        {
            return ((ThunksHeap)thunksHeap).AllocateThunk();
        }

        [RuntimeExport("RhFreeThunk")]
        public static void RhFreeThunk(object thunksHeap, IntPtr thunkAddress)
        {
            ((ThunksHeap)thunksHeap).FreeThunk(thunkAddress);
        }

        [RuntimeExport("RhSetThunkData")]
        public static void RhSetThunkData(object thunksHeap, IntPtr thunkAddress, IntPtr context, IntPtr target)
        {
            ((ThunksHeap)thunksHeap).SetThunkData(thunkAddress, context, target);
        }

        [RuntimeExport("RhTryGetThunkData")]
        public static bool RhTryGetThunkData(object thunksHeap, IntPtr thunkAddress, out IntPtr context, out IntPtr target)
        {
            return ((ThunksHeap)thunksHeap).TryGetThunkData(thunkAddress, out context, out target);
        }

        [RuntimeExport("RhGetThunkSize")]
        public static int RhGetThunkSize()
        {
            return InternalCalls.RhpGetThunkSize();
        }
    }
}
