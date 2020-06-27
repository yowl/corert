// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Internal.Runtime;

// Disable: Filter expression is a constant. We know. We just can't do an unfiltered catch.
#pragma warning disable 7095

namespace System.Runtime
{
    internal static unsafe partial class EH
    {
        internal struct RhEHClauseWasm
        {
            internal uint _tryStartOffset;
            internal EHClauseIterator.RhEHClauseKindWasm _clauseKind;
            internal uint _tryEndOffset;
            internal uint _typeSymbol;
            internal byte* _handlerAddress;
            internal byte* _filterAddress;

            public bool TryStartsAt(uint idxTryLandingStart)
            {
                return idxTryLandingStart == _tryStartOffset;
            }

            public bool ContainsCodeOffset(uint idxTryLandingStart)
            {
                return ((idxTryLandingStart >= _tryStartOffset) &&
                        (idxTryLandingStart < _tryEndOffset));
            }
        }

        // thie method and DispatchExSecondPass similar to src\Runtime.Base\src\System\Runtime\ExceptionHandling.cs but only iterates the current frame for handlers, if a handler is not found, the finally funclets are stored and the exception resumed
        // when a handler is found the stored finally funclets are run along with any in the current frame.
        private static byte* DispatchExFirstPass(object exception, byte* ehInfoStart, byte* ehInfoEnd,
            uint idxCurrentBlockStart, void* shadowStack)
        {
            var ehInfoIterator = new EHClauseIterator();
            ehInfoIterator.InitFromEhInfo(ehInfoStart, ehInfoEnd, 0);

            uint tryRegionIdx;

            return FindFirstPassHandlerWasm(exception, 0xFFFFFFFFu, idxCurrentBlockStart, shadowStack, ref ehInfoIterator,
                out tryRegionIdx); // LLVM LandingPad detects non zero and branches to LPFoundCatch, executing catch handler
        }


        // private static byte* DispatchExSecondPass(object exception, byte* ehInfoStart, byte* ehInfoEnd,
        //     uint idxCurrentBlockStart, void* shadowStack)
        // {
        //     int leaveDestination = 0;
        //     var foundCatchBlock = _currentFunclet.AppendBasicBlock("LPFoundCatch");
        //     // If it didn't find a catch block, we can rethrow (resume in LLVM) the C++ exception to continue the stack walk.
        //     var noCatch = landingPadBuilder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false),
        //         handler.ValueAsInt32(landingPadBuilder, false), "testCatch");
        //     var secondPassBlock = _currentFunclet.AppendBasicBlock("SecondPass");
        //     landingPadBuilder.BuildCondBr(noCatch, secondPassBlock, foundCatchBlock);
        //
        //     landingPadBuilder.PositionAtEnd(foundCatchBlock);
        //
        //     LLVMValueRef[] callCatchArgs = new LLVMValueRef[]
        //                           {
        //                               LLVMValueRef.CreateConstPointerNull(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0)),
        //                               CastIfNecessary(landingPadBuilder, handlerFunc, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0)), /* catch funclet address */
        //                               _currentFunclet.GetParam(0),
        //                               LLVMValueRef.CreateConstPointerNull(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0))
        //                           };
        //     LLVMValueRef leaveReturnValue = landingPadBuilder.BuildCall(RhpCallCatchFunclet, callCatchArgs, "");
        //
        //     landingPadBuilder.BuildStore(leaveReturnValue, leaveDestination);
        //     landingPadBuilder.BuildBr(secondPassBlock);
        //
        //     landingPadBuilder.PositionAtEnd(secondPassBlock);
        //
        //     // reinitialise the iterator
        //     CallRuntime(_compilation.TypeSystemContext, "EHClauseIterator", "InitFromEhInfo", iteratorInitArgs, null, fromLandingPad: true, builder: landingPadBuilder);
        //
        //     var secondPassArgs = new StackEntry[] { new ExpressionEntry(StackValueKind.Int32, "idxStart", LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0xFFFFFFFFu, false)),
        //                                               new ExpressionEntry(StackValueKind.Int32, "idxTryLandingStart", LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)tryRegion.ILRegion.TryOffset, false)),
        //                                               new ExpressionEntry(StackValueKind.ByRef, "refFrameIter", ehInfoIterator),
        //                                               new ExpressionEntry(StackValueKind.Int32, "idxLimit", LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0xFFFFFFFFu, false)),
        //                                               new ExpressionEntry(StackValueKind.NativeInt, "shadowStack", _currentFunclet.GetParam(0))
        //                                           };
        //     CallRuntime(_compilation.TypeSystemContext, "EH", "InvokeSecondPassWasm", secondPassArgs, null, true, builder: landingPadBuilder);
        //
        //     var catchLeaveBlock = _currentFunclet.AppendBasicBlock("CatchLeave");
        //     landingPadBuilder.BuildCondBr(noCatch, GetOrCreateResumeBlock(pad, tryRegion.ILRegion.TryOffset.ToString()), catchLeaveBlock);
        //     landingPadBuilder.PositionAtEnd(catchLeaveBlock);
        //
        //     // Use the else as the path for no exception handler found for this exception
        //     LLVMValueRef @switch = landingPadBuilder.BuildSwitch(landingPadBuilder.BuildLoad(leaveDestination, "loadLeaveDest"), GetOrCreateUnreachableBlock(), 1 /* number of cases, but fortunately this doesn't seem to make much difference */);
        //
        //     if (_leaveTargets != null)
        //     {
        //         LLVMBasicBlockRef switchReturnBlock = default;
        //         foreach (var leaveTarget in _leaveTargets)
        //         {
        //             var targetBlock = _basicBlocks[leaveTarget];
        //             var funcletForBlock = GetFuncletForBlock(targetBlock);
        //             if (funcletForBlock.Handle.Equals(_currentFunclet.Handle))
        //             {
        //                 @switch.AddCase(BuildConstInt32(targetBlock.StartOffset), GetLLVMBasicBlockForBlock(targetBlock));
        //             }
        //             else
        //             {
        //
        //                 // leave destination is in a different funclet, this happens when an exception is thrown/rethrown from inside a catch handler and the throw is not directly in a try handler
        //                 // In this case we need to return out of this funclet to get back to the containing funclet.  Logic checks we are actually in a catch funclet as opposed to a finally or the main function funclet
        //                 ExceptionRegion currentRegion = GetTryRegion(_currentBasicBlock.StartOffset);
        //                 if (currentRegion != null && _currentBasicBlock.StartOffset >= currentRegion.ILRegion.HandlerOffset && _currentBasicBlock.StartOffset < currentRegion.ILRegion.HandlerOffset + currentRegion.ILRegion.HandlerLength
        //                     && currentRegion.ILRegion.Kind == ILExceptionRegionKind.Catch)
        //                 {
        //                     if (switchReturnBlock == default)
        //                     {
        //                         switchReturnBlock = _currentFunclet.AppendBasicBlock("SwitchReturn");
        //                     }
        //                     @switch.AddCase(BuildConstInt32(targetBlock.StartOffset), switchReturnBlock);
        //                 }
        //             }
        //         }
        //         if (switchReturnBlock != default)
        //         {
        //             landingPadBuilder.PositionAtEnd(switchReturnBlock);
        //             landingPadBuilder.BuildRet(landingPadBuilder.BuildLoad(leaveDestination, "loadLeaveDest"));
        //         }
        //     }
        //
        //     landingPadBuilder.Dispose();
        //
        //     return landingPad;
        // }

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
        public struct TwoByteStr
        {
            public byte first;
            public byte second;
        }
        [DllImport("*")]
        private static unsafe extern int printf(byte* str, byte* unused);
        public static void PrintLine(string s)
        {
            PrintString(s);
            PrintString("\n");
        }

        // TODO: temporary to try things out, when working look to see how to refactor with FindFirstPassHandler
        private static byte* FindFirstPassHandlerWasm(object exception, uint idxStart, uint idxCurrentBlockStart /* the start IL idx of the current block for the landing pad, will use in place of PC */, 
            void* shadowStack, ref EHClauseIterator clauseIter, out uint tryRegionIdx)
        {
            byte * pHandler = (byte*)0;
            tryRegionIdx = MaxTryRegionIdx;
            uint lastTryStart = 0, lastTryEnd = 0;
            RhEHClauseWasm ehClause = new RhEHClauseWasm();
            for (uint curIdx = 0; clauseIter.Next(ref ehClause); curIdx++)
            {
                // 
                // Skip to the starting try region.  This is used by collided unwinds and rethrows to pickup where
                // the previous dispatch left off.
                //
                if (idxStart != MaxTryRegionIdx)
                {
                    if (curIdx <= idxStart)
                    {
                        lastTryStart = ehClause._tryStartOffset;
                        lastTryEnd = ehClause._tryEndOffset;
                        continue;
                    }

                    // Now, we continue skipping while the try region is identical to the one that invoked the 
                    // previous dispatch.
                    if ((ehClause._tryStartOffset == lastTryStart) && (ehClause._tryEndOffset == lastTryEnd))
                    {
                        continue;
                    }

                    // We are done skipping. This is required to handle empty finally block markers that are used
                    // to separate runs of different try blocks with same native code offsets.
                    idxStart = MaxTryRegionIdx;
                }

                EHClauseIterator.RhEHClauseKindWasm clauseKind = ehClause._clauseKind;
                if (((clauseKind != EHClauseIterator.RhEHClauseKindWasm.RH_EH_CLAUSE_TYPED) &&
                     (clauseKind != EHClauseIterator.RhEHClauseKindWasm.RH_EH_CLAUSE_FILTER))
                    || !ehClause.ContainsCodeOffset(idxCurrentBlockStart))
                {
                    continue;
                }

                // Found a containing clause. Because of the order of the clauses, we know this is the
                // most containing.
                if (clauseKind == EHClauseIterator.RhEHClauseKindWasm.RH_EH_CLAUSE_TYPED)
                {
                    PrintLine("ShoudCatch");
                    if (ShouldTypedClauseCatchThisException(exception, (EEType*)ehClause._typeSymbol))
                    {
                        PrintLine("ShoudCatch true");
                        pHandler = ehClause._handlerAddress;
                        tryRegionIdx = curIdx;
                        return pHandler;
                    }
                    PrintLine("ShoudCatch false");
                }
                else
                {
                    tryRegionIdx = 0;
                    bool shouldInvokeHandler = InternalCalls.RhpCallFilterFunclet(exception, ehClause._filterAddress, shadowStack);
                    if (shouldInvokeHandler)
                    {
                        pHandler = ehClause._handlerAddress;
                        tryRegionIdx = curIdx;
                        return pHandler;
                    }
                }
            }

            return pHandler;
        }

        private static void InvokeSecondPassWasm(uint idxStart, uint idxTryLandingStart, ref EHClauseIterator clauseIter, uint idxLimit, void* shadowStack)
        {
            uint lastTryStart = 0, lastTryEnd = 0;
            // Search the clauses for one that contains the current offset.
            RhEHClauseWasm ehClause = new RhEHClauseWasm();
            for (uint curIdx = 0; clauseIter.Next(ref ehClause) && curIdx < idxLimit; curIdx++)
            {
                // 
                // Skip to the starting try region.  This is used by collided unwinds and rethrows to pickup where
                // the previous dispatch left off.
                //
                if (idxStart != MaxTryRegionIdx)
                {
                    if (curIdx <= idxStart)
                    {
                        lastTryStart = ehClause._tryStartOffset;
                        lastTryEnd = ehClause._tryEndOffset;
                        continue;
                    }

                    // Now, we continue skipping while the try region is identical to the one that invoked the 
                    // previous dispatch.
                    if ((ehClause._tryStartOffset == lastTryStart) && (ehClause._tryEndOffset == lastTryEnd))
                        continue;

                    // We are done skipping. This is required to handle empty finally block markers that are used
                    // to separate runs of different try blocks with same native code offsets.
                    idxStart = MaxTryRegionIdx;
                }

                EHClauseIterator.RhEHClauseKindWasm clauseKind = ehClause._clauseKind;

                if ((clauseKind != EHClauseIterator.RhEHClauseKindWasm.RH_EH_CLAUSE_FAULT)
                    || !ehClause.TryStartsAt(idxTryLandingStart))
                {
                    continue;
                }

                // Found a containing clause. Because of the order of the clauses, we know this is the
                // most containing.

                // N.B. -- We need to suppress GC "in-between" calls to finallys in this loop because we do
                // not have the correct next-execution point live on the stack and, therefore, may cause a GC
                // hole if we allow a GC between invocation of finally funclets (i.e. after one has returned
                // here to the dispatcher, but before the next one is invoked).  Once they are running, it's 
                // fine for them to trigger a GC, obviously.
                // 
                // As a result, RhpCallFinallyFunclet will set this state in the runtime upon return from the
                // funclet, and we need to reset it if/when we fall out of the loop and we know that the 
                // method will no longer get any more GC callbacks.

                byte* pFinallyHandler = ehClause._handlerAddress;

                InternalCalls.RhpCallFinallyFunclet(pFinallyHandler, shadowStack);
            }
        }
    } // static class EH
}
