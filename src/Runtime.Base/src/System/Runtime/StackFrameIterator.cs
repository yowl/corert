// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Runtime
{
    [StructLayout(LayoutKind.Explicit, Size = AsmOffsets.SIZEOF__REGDISPLAY)]
    internal unsafe struct REGDISPLAY
    {
        [FieldOffset(AsmOffsets.OFFSETOF__REGDISPLAY__SP)]
        internal UIntPtr SP;
    }

    public struct TwoByteStr
    {
        public byte first;
        public byte second;
    }

    internal unsafe struct EHClauseIterator
    {
        //            [FieldOffset(AsmOffsets.OFFSETOF__StackFrameIterator__m_FramePointer)]
        //            private UIntPtr _framePointer;
        //            [FieldOffset(AsmOffsets.OFFSETOF__StackFrameIterator__m_ControlPC)]
        //            private IntPtr _controlPC;
        //            [FieldOffset(AsmOffsets.OFFSETOF__StackFrameIterator__m_RegDisplay)]
        //            private REGDISPLAY _regDisplay;
        //            [FieldOffset(AsmOffsets.OFFSETOF__StackFrameIterator__m_OriginalControlPC)]
        //            private IntPtr _originalControlPC;
        //
        //            internal byte* ControlPC { get { return (byte*)_controlPC; } }
        //            internal byte* OriginalControlPC { get { return (byte*)_originalControlPC; } }
        //            internal void* RegisterSet { get { fixed (void* pRegDisplay = &_regDisplay) { return pRegDisplay; } } }
        //            internal UIntPtr SP { get { return _regDisplay.SP; } }
        //            internal UIntPtr FramePointer { get { return _framePointer; } }
        //
        private uint _totalClauses;
        private byte *_currentPtr;
        private int _currentClause;

        [DllImport("*")]
        private static unsafe extern int printf(byte* str, byte* unused);

        public static unsafe void PrintString(string s)
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

        private static uint DecodeUnsigned(ref byte* stream)
        {
            uint value = 0;

            uint val = *stream;
            if ((val & 1) == 0)
            {
                value = (val >> 1);
                stream += 1;
            }
            else if ((val & 2) == 0)
            {
                value = (val >> 2) |
                        (((uint)*(stream + 1)) << 6);
                stream += 2;
            }
            else if ((val & 4) == 0)
            {
                value = (val >> 3) |
                        (((uint)*(stream + 1)) << 5) |
                        (((uint)*(stream + 2)) << 13);
                stream += 3;
            }
            else if ((val & 8) == 0)
            {
                value = (val >> 4) |
                        (((uint)*(stream + 1)) << 4) |
                        (((uint)*(stream + 2)) << 12) |
                        (((uint)*(stream + 3)) << 20);
                stream += 4;
            }
            else if ((val & 16) == 0)
            {
                stream += 1;
                value = ReadUInt32(ref stream);
            }

            // TODO : deleted all the error handling
            return value;
        }

        private static uint ReadUInt32(ref byte* stream)
        {
            uint result = *(uint*)(stream); // Assumes little endian and unaligned access
            stream += 4;
            return result;
        }

        uint GetUnsigned()
        {
            uint value;
            value = DecodeUnsigned(ref _currentPtr);
            return value;
        }

        internal void InitFromEhInfo(byte* ehInfoStart, byte* ehInfoEnd, int idxStart)
        {
            _currentPtr = ehInfoStart;
            _currentClause = 0;
            _totalClauses = GetUnsigned();
#if netcoreapp
            PrintString("read _currentPtr now ");
            PrintLine(((uint)_currentPtr).ToString());
            PrintString("read _totalClauses ");
            PrintLine(_totalClauses.ToString());
#endif
        }

        // TODO : copied from EH
        internal enum RhEHClauseKindWasm
        {
            RH_EH_CLAUSE_TYPED = 0,
            RH_EH_CLAUSE_FAULT = 1,
            RH_EH_CLAUSE_FILTER = 2,
            RH_EH_CLAUSE_UNUSED = 3,
        }

        internal bool Next(ref EH.RhEHClauseWasm pEHClause)
        {
#if netcoreapp
            PrintLine("EH Next");

            PrintString("Next _currentPtr ");
            PrintLine(((uint)_currentPtr).ToString());
            PrintString("Next _currentClause ");
            PrintLine(_currentClause.ToString());
#endif

            if (_currentClause >= _totalClauses) return false;

            _currentClause++;
            pEHClause._tryStartOffset = GetUnsigned();
            uint tryLengthAndKind = GetUnsigned();
            pEHClause._clauseKind = (RhEHClauseKindWasm)(tryLengthAndKind & 3);
            pEHClause._tryEndOffset = (tryLengthAndKind >> 2) + pEHClause._tryStartOffset;
            switch (pEHClause._clauseKind)
            {
                case RhEHClauseKindWasm.RH_EH_CLAUSE_TYPED:

                    pEHClause._handlerOffset = GetUnsigned();
                    pEHClause._typeSymbol = ReadUInt32(ref _currentPtr);
                    pEHClause._handlerFunctionPtr = ReadUInt32(ref _currentPtr);
#if netcoreapp
                    PrintString("Next _typeSymbol ");
                    PrintLine(pEHClause._typeSymbol.ToString());
#endif
                    break;
                case RhEHClauseKindWasm.RH_EH_CLAUSE_FAULT:
                    pEHClause._handlerOffset = GetUnsigned();
                    break;
                case RhEHClauseKindWasm.RH_EH_CLAUSE_FILTER:
                    pEHClause._handlerOffset = GetUnsigned();
                    pEHClause._filterOffset = GetUnsigned();
                    break;
            }
#if netcoreapp
            PrintString("Next _tryStartOffset ");
            PrintLine(pEHClause._tryStartOffset.ToString());
            PrintString("Next _tryEndOffset ");
            PrintLine(pEHClause._tryEndOffset.ToString());
//            PrintString("Next _clauseKind ");
//            PrintLine(pEHClause._clauseKind.ToString());
            PrintString("Next _handlerOffset ");
            PrintLine(pEHClause._handlerOffset.ToString());
            PrintString("Next tryLengthAndKind ");
            PrintLine(tryLengthAndKind.ToString());
#endif
            return true;
        }

//            uint uExCollideClauseIdx;
//            bool fUnwoundReversePInvoke;
//            return Next(out uExCollideClauseIdx, out fUnwoundReversePInvoke);
//        }
        //
        //            internal bool Next(out uint uExCollideClauseIdx)
        //            {
        //                bool fUnwoundReversePInvoke;
        //                return Next(out uExCollideClauseIdx, out fUnwoundReversePInvoke);
        //            }
        //
        //            internal bool Next(out uint uExCollideClauseIdx, out bool fUnwoundReversePInvoke)
        //            {
        //                return InternalCalls.RhpSfiNext(ref this, out uExCollideClauseIdx, out fUnwoundReversePInvoke);
        //            }
    }

    [StructLayout(LayoutKind.Explicit, Size = AsmOffsets.SIZEOF__StackFrameIterator)]
    internal unsafe struct StackFrameIterator
    {
        [FieldOffset(AsmOffsets.OFFSETOF__StackFrameIterator__m_FramePointer)]
        private UIntPtr _framePointer;
        [FieldOffset(AsmOffsets.OFFSETOF__StackFrameIterator__m_ControlPC)]
        private IntPtr _controlPC;
        [FieldOffset(AsmOffsets.OFFSETOF__StackFrameIterator__m_RegDisplay)]
        private REGDISPLAY _regDisplay;
        [FieldOffset(AsmOffsets.OFFSETOF__StackFrameIterator__m_OriginalControlPC)]
        private IntPtr _originalControlPC;

        internal byte* ControlPC { get { return (byte*)_controlPC; } }
        internal byte* OriginalControlPC { get { return (byte*)_originalControlPC; } }
        internal void* RegisterSet { get { fixed (void* pRegDisplay = &_regDisplay) { return pRegDisplay; } } }
        internal UIntPtr SP { get { return _regDisplay.SP; } }
        internal UIntPtr FramePointer { get { return _framePointer; } }

        internal bool Init(EH.PAL_LIMITED_CONTEXT* pStackwalkCtx, bool instructionFault = false)
        {
            return InternalCalls.RhpSfiInit(ref this, pStackwalkCtx, instructionFault);
        }

        internal bool Next()
        {
            uint uExCollideClauseIdx;
            bool fUnwoundReversePInvoke;
            return Next(out uExCollideClauseIdx, out fUnwoundReversePInvoke);
        }

        internal bool Next(out uint uExCollideClauseIdx)
        {
            bool fUnwoundReversePInvoke;
            return Next(out uExCollideClauseIdx, out fUnwoundReversePInvoke);
        }

        internal bool Next(out uint uExCollideClauseIdx, out bool fUnwoundReversePInvoke)
        {
            return InternalCalls.RhpSfiNext(ref this, out uExCollideClauseIdx, out fUnwoundReversePInvoke);
        }
    }
}
