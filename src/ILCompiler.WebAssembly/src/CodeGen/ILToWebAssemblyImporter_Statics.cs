// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Internal.TypeSystem;
using Internal.TypeSystem.Ecma;

using ILCompiler;
using ILCompiler.CodeGen;

using ILCompiler.DependencyAnalysis;
using LLVMSharp;

namespace Internal.IL
{
    internal partial class ILImporter
    {
        public static void CompileMethod(WebAssemblyCodegenCompilation compilation, WebAssemblyMethodCodeNode methodCodeNodeNeedingCode)
        {
            MethodDesc method = methodCodeNodeNeedingCode.Method;

            if (compilation.Logger.IsVerbose)
            {
                string methodName = method.ToString();
                compilation.Logger.Writer.WriteLine("Compiling " + methodName);
            }

            if (method.HasCustomAttribute("System.Runtime", "RuntimeImportAttribute"))
            {
                methodCodeNodeNeedingCode.CompilationCompleted = true;
                //throw new NotImplementedException();
                //CompileExternMethod(methodCodeNodeNeedingCode, ((EcmaMethod)method).GetRuntimeImportName());
                //return;
            }

            if (method.IsRawPInvoke())
            {
                //CompileExternMethod(methodCodeNodeNeedingCode, method.GetPInvokeMethodMetadata().Name ?? method.Name);
                //return;
            }

            var methodIL = compilation.GetMethodIL(method);
            if (methodIL == null)
                return;

            ILImporter ilImporter = null;
            try
            {
                string mangledName;

                // TODO: Better detection of the StartupCodeMain method
                if (methodCodeNodeNeedingCode.Method.Signature.IsStatic && methodCodeNodeNeedingCode.Method.Name == "StartupCodeMain")
                {
                    mangledName = "StartupCodeMain";
                }
                else
                {
                    mangledName = compilation.NameMangler.GetMangledMethodName(methodCodeNodeNeedingCode.Method).ToString();
                }

                ilImporter = new ILImporter(compilation, method, methodIL, mangledName);

                CompilerTypeSystemContext typeSystemContext = compilation.TypeSystemContext;

                //MethodDebugInformation debugInfo = compilation.GetDebugInfo(methodIL);

               /* if (!compilation.Options.HasOption(CppCodegenConfigProvider.NoLineNumbersString))*/
                {
                    //IEnumerable<ILSequencePoint> sequencePoints = debugInfo.GetSequencePoints();
                    /*if (sequencePoints != null)
                        ilImporter.SetSequencePoints(sequencePoints);*/
                }

                //IEnumerable<ILLocalVariable> localVariables = debugInfo.GetLocalVariables();
                /*if (localVariables != null)
                    ilImporter.SetLocalVariables(localVariables);*/

                IEnumerable<string> parameters = GetParameterNamesForMethod(method);
                /*if (parameters != null)
                    ilImporter.SetParameterNames(parameters);*/

                ilImporter.Import();
                ilImporter.CreateEHData(methodCodeNodeNeedingCode);
                methodCodeNodeNeedingCode.CompilationCompleted = true;
            }
            catch (Exception e)
            {
                compilation.Logger.Writer.WriteLine(e.Message + " (" + method + ")");

                methodCodeNodeNeedingCode.CompilationCompleted = true;
//                methodCodeNodeNeedingCode.SetDependencies(ilImporter.GetDependencies());
                //throw new NotImplementedException();
                //methodCodeNodeNeedingCode.SetCode(sb.ToString(), Array.Empty<Object>());
            }

            // Uncomment the block below to get specific method failures when LLVM fails for cryptic reasons
#if false
            LLVMBool result = LLVM.VerifyFunction(ilImporter._llvmFunction, LLVMVerifierFailureAction.LLVMPrintMessageAction);
            if (result.Value != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error compliling {method.OwningType}.{method}");
                Console.ResetColor();
            }
#endif // false

            // Ensure dependencies show up regardless of exceptions to avoid breaking LLVM
            methodCodeNodeNeedingCode.SetDependencies(ilImporter.GetDependencies());
        }

        static LLVMValueRef DebugtrapFunction = default(LLVMValueRef);
        static LLVMValueRef TrapFunction = default(LLVMValueRef);
        static LLVMValueRef DoNothingFunction = default(LLVMValueRef);
        static LLVMValueRef RhpThrowEx = default(LLVMValueRef);
        static LLVMValueRef RhpCallCatchFunclet = default(LLVMValueRef);
        static LLVMValueRef LlvmCatchFunclet = default(LLVMValueRef);
        static LLVMValueRef LlvmFinallyFunclet = default(LLVMValueRef);
        static LLVMValueRef NullRefFunction = default(LLVMValueRef);
        public static LLVMValueRef GxxPersonality = default(LLVMValueRef);
        public static LLVMTypeRef GxxPersonalityType = default(LLVMTypeRef);

        private static IEnumerable<string> GetParameterNamesForMethod(MethodDesc method)
        {
            // TODO: The uses of this method need revision. The right way to get to this info is from
            //       a MethodIL. For declarations, we don't need names.

            method = method.GetTypicalMethodDefinition();
            var ecmaMethod = method as EcmaMethod;
            if (ecmaMethod != null && ecmaMethod.Module.PdbReader != null)
            {
                return (new EcmaMethodDebugInformation(ecmaMethod)).GetParameterNames();
            }

            return null;
        }

        void BuildCatchFunclet()
        {
            LlvmCatchFunclet = LLVM.AddFunction(Module, "LlvmCatchFunclet", LLVM.FunctionType(LLVM.Int32Type(),
                new LLVMTypeRef[]
                {
                    LLVM.PointerType(LLVM.Int8Type(), 0),
                    LLVM.PointerType(LLVM.FunctionType(LLVMTypeRef.Int32Type(), new LLVMTypeRef[] { LLVM.PointerType(LLVM.Int8Type(), 0)}, false), 0), // pHandlerIP - catch funcletAddress
                    LLVM.PointerType(LLVM.Int8Type(), 0), // shadow stack
                    LLVM.PointerType(LLVM.Int8Type(), 0)
                }, false));
            var block = LLVM.AppendBasicBlock(LlvmCatchFunclet, "GenericCatch");
            LLVMBuilderRef funcletBuilder = LLVM.CreateBuilder();
            LLVM.PositionBuilderAtEnd(funcletBuilder, block);
            //            EmitTrapCall(funcletBuilder);

            var exceptionParam = LLVM.GetParam(LlvmCatchFunclet, 0);  
            var catchFunclet = LLVM.GetParam(LlvmCatchFunclet, 1);
            var castShadowStack = LLVM.GetParam(LlvmCatchFunclet, 2);  
            var debugArgs = new StackEntry[] {new ExpressionEntry(StackValueKind.ObjRef, "managedPtr", catchFunclet) };
            var mainBuilder = _builder;  // if not doing the CallRuntime and remove this
            var currentFunclet = _currentFunclet;
            _currentFunclet = LlvmCatchFunclet;
            _builder = funcletBuilder;
            CallRuntime(_compilation.TypeSystemContext, "EH", "DebugPointer", debugArgs, GetWellKnownType(WellKnownType.Int32), true);

            List<LLVMValueRef> llvmArgs = new List<LLVMValueRef>();
            llvmArgs.Add(castShadowStack);

            LLVMValueRef leaveToILOffset = LLVM.BuildCall(_builder, catchFunclet, llvmArgs.ToArray(), string.Empty);
            _builder = mainBuilder;
            _currentFunclet = currentFunclet;
            LLVM.BuildRet(funcletBuilder, leaveToILOffset);
            LLVM.DisposeBuilder(funcletBuilder);
        }

        void BuildFinallyFunclet()
        {
            LlvmFinallyFunclet = LLVM.AddFunction(Module, "LlvmFinallyFunclet", LLVM.FunctionType(LLVM.VoidType(),
                new LLVMTypeRef[]
                {
                    LLVM.PointerType(LLVM.FunctionType(LLVMTypeRef.VoidType(), new LLVMTypeRef[] { LLVM.PointerType(LLVM.Int8Type(), 0)}, false), 0), // finallyHandler
                    LLVM.PointerType(LLVM.Int8Type(), 0), // shadow stack
                }, false));
            var block = LLVM.AppendBasicBlock(LlvmFinallyFunclet, "GenericFinally");
            LLVMBuilderRef funcletBuilder = LLVM.CreateBuilder();
            LLVM.PositionBuilderAtEnd(funcletBuilder, block);
            //            EmitTrapCall(funcletBuilder);

            var finallyFunclet = LLVM.GetParam(LlvmFinallyFunclet, 0);
            var castShadowStack = LLVM.GetParam(LlvmFinallyFunclet, 1);
            var debugArgs = new StackEntry[] { new ExpressionEntry(StackValueKind.ObjRef, "managedPtr", finallyFunclet) };
            var mainBuilder = _builder;  // if not doing the CallRuntime and remove this
            var currentFunclet = _currentFunclet;
            _currentFunclet = LlvmFinallyFunclet;
            _builder = funcletBuilder;
//            CallRuntime(_compilation.TypeSystemContext, "EH", "DebugPointer", debugArgs, GetWellKnownType(WellKnownType.Int32), true);

            List<LLVMValueRef> llvmArgs = new List<LLVMValueRef>();
            llvmArgs.Add(castShadowStack);

            LLVM.BuildCall(_builder, finallyFunclet, llvmArgs.ToArray(), string.Empty);
            _builder = mainBuilder;
            _currentFunclet = currentFunclet;
            LLVM.BuildRetVoid(funcletBuilder);
            LLVM.DisposeBuilder(funcletBuilder);
        }
    }
}
