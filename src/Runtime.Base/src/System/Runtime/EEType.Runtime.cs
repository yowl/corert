// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

namespace Internal.Runtime
{
    // Extensions to EEType that are specific to the use in Runtime.Base.
    internal unsafe partial struct EEType
    {
        internal class X
        {
            [DllImport("*")]
            internal static unsafe extern int printf(byte* str, byte* unused);
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
                //            PrintString(s);
                //            PrintString("\n");
            }
            public static byte[] GetBytes(int value)
            {
                byte[] bytes = new byte[sizeof(int)];
                Unsafe.As<byte, int>(ref bytes[0]) = value;
                return bytes;
            }
            public unsafe static void PrintUint(int s)
            {
                byte[] intBytes = GetBytes(s);
                for (var i = 0; i < 4; i++)
                {
                    TwoByteStr curCharStr = new TwoByteStr();
                    var nib = (intBytes[3 - i] & 0xf0) >> 4;
                    curCharStr.first = (byte)((nib <= 9 ? '0' : 'A') + (nib <= 9 ? nib : nib - 10));
                    printf((byte*)&curCharStr, null);
                    nib = (intBytes[3 - i] & 0xf);
                    curCharStr.first = (byte)((nib <= 9 ? '0' : 'A') + (nib <= 9 ? nib : nib - 10));
                    printf((byte*)&curCharStr, null);
                }
                PrintString("\n");
            }

            public struct TwoByteStr
            {
                public byte first;
                public byte second;
            }

        }

        internal DispatchResolve.DispatchMap* DispatchMap
        {
            get
            {
                fixed (EEType* pThis = &this)
                    return InternalCalls.RhpGetDispatchMap(pThis);
            }
        }

        internal EEType* GetArrayEEType()
        {
            fixed (EEType* pThis = &this)
            {
                IntPtr pGetArrayEEType = (IntPtr)InternalCalls.RhpGetClasslibFunctionFromEEType(new IntPtr(pThis), ClassLibFunctionId.GetSystemArrayEEType);
                X.PrintUint(8);
                X.PrintUint((int)new IntPtr(pThis));
                X.PrintUint((int)pGetArrayEEType);
                return (EEType*)CalliIntrinsics.Call<IntPtr>(pGetArrayEEType);
            }
        }

        internal Exception GetClasslibException(ExceptionIDs id)
        {
#if INPLACE_RUNTIME
            return RuntimeExceptionHelpers.GetRuntimeException(id);
#else
            DynamicModule* dynamicModule = this.DynamicModule;
            if (dynamicModule != null)
            {
                IntPtr getRuntimeException = dynamicModule->GetRuntimeException;
                if (getRuntimeException != IntPtr.Zero)
                {
                    return CalliIntrinsics.Call<Exception>(getRuntimeException, id);
                }
            }
            if (IsParameterizedType)
            {
                return RelatedParameterType->GetClasslibException(id);
            }

            return EH.GetClasslibExceptionFromEEType(id, GetAssociatedModuleAddress());
#endif
        }

        internal void SetToCloneOf(EEType* pOrigType)
        {
            Debug.Assert((_usFlags & (ushort)EETypeFlags.EETypeKindMask) == 0, "should be a canonical type");
            _usFlags |= (ushort)EETypeKind.ClonedEEType;
            _relatedType._pCanonicalType = pOrigType;
        }

        // Returns an address in the module most closely associated with this EEType that can be handed to
        // EH.GetClasslibException and use to locate the compute the correct exception type. In most cases
        // this is just the EEType pointer itself, but when this type represents a generic that has been
        // unified at runtime (and thus the EEType pointer resides in the process heap rather than a specific
        // module) we need to do some work.
        internal unsafe IntPtr GetAssociatedModuleAddress()
        {
            fixed (EEType* pThis = &this)
            {
                if (!IsDynamicType)
                    return (IntPtr)pThis;

                // There are currently four types of runtime allocated EETypes, arrays, pointers, byrefs, and generic types.
                // Arrays/Pointers/ByRefs can be handled by looking at their element type.
                if (IsParameterizedType)
                    return pThis->RelatedParameterType->GetAssociatedModuleAddress();

                if (!IsGeneric)
                {
                    // No way to resolve module information for a non-generic dynamic type.
                    return IntPtr.Zero;
                }

                // Generic types are trickier. Often we could look at the parent type (since eventually it
                // would derive from the class library's System.Object which is definitely not runtime
                // allocated). But this breaks down for generic interfaces. Instead we fetch the generic
                // instantiation information and use the generic type definition, which will always be module
                // local. We know this lookup will succeed since we're dealing with a unified generic type
                // and the unification process requires this metadata.
                EEType* pGenericType = pThis->GenericDefinition;

                Debug.Assert(pGenericType != null, "Generic type expected");

                return (IntPtr)pGenericType;
            }
        }

        /// <summary>
        /// Return true if type is good for simple casting : canonical, no related type via IAT, no generic variance
        /// </summary>
        internal bool SimpleCasting()
        {
            return (_usFlags & (ushort)EETypeFlags.ComplexCastingMask) == (ushort)EETypeKind.CanonicalEEType;
        }

        /// <summary>
        /// Return true if both types are good for simple casting: canonical, no related type via IAT, no generic variance
        /// </summary>
        internal static bool BothSimpleCasting(EEType* pThis, EEType* pOther)
        {
            return ((pThis->_usFlags | pOther->_usFlags) & (ushort)EETypeFlags.ComplexCastingMask) == (ushort)EETypeKind.CanonicalEEType;
        }

        internal bool IsEquivalentTo(EEType* pOtherEEType)
        {
            fixed (EEType* pThis = &this)
            {
                if (pThis == pOtherEEType)
                    return true;

                EEType* pThisEEType = pThis;

                if (pThisEEType->IsCloned)
                    pThisEEType = pThisEEType->CanonicalEEType;

                if (pOtherEEType->IsCloned)
                    pOtherEEType = pOtherEEType->CanonicalEEType;

                if (pThisEEType == pOtherEEType)
                    return true;

                if (pThisEEType->IsParameterizedType && pOtherEEType->IsParameterizedType)
                {
                    return pThisEEType->RelatedParameterType->IsEquivalentTo(pOtherEEType->RelatedParameterType) &&
                        pThisEEType->ParameterizedTypeShape == pOtherEEType->ParameterizedTypeShape;
                }
            }

            return false;
        }
    }

    internal static class WellKnownEETypes
    {
        // Returns true if the passed in EEType is the EEType for System.Object
        // This is recognized by the fact that System.Object and interfaces are the only ones without a base type
        internal static unsafe bool IsSystemObject(EEType* pEEType)
        {
            if (pEEType->IsArray)
                return false;
            return (pEEType->NonArrayBaseType == null) && !pEEType->IsInterface;
        }

        // Returns true if the passed in EEType is the EEType for System.Array.
        // The binder sets a special CorElementType for this well known type
        internal static unsafe bool IsSystemArray(EEType* pEEType)
        {
            return (pEEType->CorElementType == CorElementType.ELEMENT_TYPE_ARRAY);
        }
    }
}
