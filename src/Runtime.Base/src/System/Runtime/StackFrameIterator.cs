// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Internal.NativeFormat;

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
        NativeParser _nativeParser;

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

        internal void InitFromEhInfo(byte* ehInfoStart, byte* ehInfoEnd, int idxStart)
        {
            NativeReader reader =  new NativeReader(ehInfoStart, (uint)(ehInfoEnd - ehInfoStart));
            _nativeParser = new NativeParser(reader, 0);
            _totalClauses = _nativeParser.GetUnsigned();
            PrintString("read offset now ");
            PrintLine(_nativeParser.Offset.ToString());
            PrintString("read _totalClauses ");
            PrintLine(_totalClauses.ToString());
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
            if (_nativeParser.Offset >= _nativeParser.Reader.Size) return false;

            // read next EHInfo
//                RhEHClauseKind clauseKind;
//
//                if (exceptionRegion.ILRegion.Kind == ILExceptionRegionKind.Fault ||
//                    exceptionRegion.ILRegion.Kind == ILExceptionRegionKind.Finally)
//                {
//                    clauseKind = RhEHClauseKind.RH_EH_CLAUSE_FAULT;
//                }
//                else
//                if (exceptionRegion.ILRegion.Kind == ILExceptionRegionKind.Filter)
//                {
//                    clauseKind = RhEHClauseKind.RH_EH_CLAUSE_FILTER;
//                }
//                else
//                {
//                    clauseKind = RhEHClauseKind.RH_EH_CLAUSE_TYPED;
//                }

                pEHClause._tryOffset = _nativeParser.GetUnsigned();

                // clause.TryLength returned by the JIT is actually end offset... // TODO: does this apply to Wasm ExceptionRegion?
                // https://github.com/dotnet/coreclr/issues/3585
                uint tryLengthAndKind = _nativeParser.GetUnsigned();
                pEHClause._clauseKind = tryLengthAndKind & 3;
                pEHClause._tryLength = tryLengthAndKind >> 2;

                switch (clauseKind)
                {
                    case RhEHClauseKind.RH_EH_CLAUSE_TYPED:
                        {
                            builder.EmitCompressedUInt((uint)exceptionRegion.ILRegion.HandlerOffset);

                            var type = (TypeDesc)_methodIL.GetObject((int)exceptionRegion.ILRegion.ClassToken);

                            Debug.Assert(!type.IsCanonicalSubtype(CanonicalFormKind.Any));

                            var typeSymbol = _compilation.NodeFactory.NecessaryTypeSymbol(type);

                            RelocType rel = (_compilation.NodeFactory.Target.IsWindows) ?
                                RelocType.IMAGE_REL_BASED_ABSOLUTE :
                                RelocType.IMAGE_REL_BASED_REL32;

                            if (_compilation.NodeFactory.Target.Abi == TargetAbi.Jit)
                                rel = RelocType.IMAGE_REL_BASED_REL32;

                            builder.EmitReloc(typeSymbol, rel);
                        }
                        break;
                    case RhEHClauseKind.RH_EH_CLAUSE_FAULT:
                        builder.EmitCompressedUInt((uint)exceptionRegion.ILRegion.HandlerOffset);
                        break;
                    case RhEHClauseKind.RH_EH_CLAUSE_FILTER:
                        builder.EmitCompressedUInt((uint)exceptionRegion.ILRegion.HandlerOffset);
                        builder.EmitCompressedUInt((uint)exceptionRegion.ILRegion.FilterOffset);
                        break;
                }
            }

            uint uExCollideClauseIdx;
            bool fUnwoundReversePInvoke;
            return Next(out uExCollideClauseIdx, out fUnwoundReversePInvoke);
        }
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
