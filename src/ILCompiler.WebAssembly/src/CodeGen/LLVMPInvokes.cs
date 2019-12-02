// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Text;
using LLVMSharp;

namespace ILCompiler.WebAssembly
{
    internal unsafe class LLVMUnsafeDIFunctions
    {
        public static LLVMDIBuilderRef CreateDIBuilder(LLVMModuleRef module)
        {
            return new LLVMDIBuilderRef((IntPtr)LLVM.CreateDIBuilder((LLVMOpaqueModule*)module.Pointer));
        }

        public static LLVMMetadataRef DIBuilderCreateFile(LLVMDIBuilderRef builder, string filename, string directory)
        {
            byte[] filenameBytes = Encoding.ASCII.GetBytes(filename);
            byte[] directoryBytes = Encoding.ASCII.GetBytes(directory);
            fixed (byte* pFilename = filenameBytes)
            fixed (byte* pDirectory = directoryBytes)
            {
                sbyte* filenameSBytePtr = (sbyte*)pFilename;
                sbyte* directorySBytePtr = (sbyte*)pDirectory;
                LLVMOpaqueMetadata* metadataPtr = LLVM.DIBuilderCreateFile((LLVMOpaqueDIBuilder*)builder.Pointer, filenameSBytePtr,
                    (UIntPtr)filenameBytes.Length, directorySBytePtr, (UIntPtr)directoryBytes.Length);
                return new LLVMMetadataRef((IntPtr)metadataPtr);
            }
        }

        public static LLVMMetadataRef DIBuilderCreateCompileUnit(LLVMDIBuilderRef builder, LLVMDWARFSourceLanguage lang,
            LLVMMetadataRef fileMetadataRef, string producer, int isOptimized, string flags, uint runtimeVersion,
            string splitName, LLVMDWARFEmissionKind dwarfEmissionKind, uint dWOld, int splitDebugInlining,
            int debugInfoForProfiling)
        {
            byte[] producerBytes = Encoding.ASCII.GetBytes(producer);
            byte[] flagsBytes = Encoding.ASCII.GetBytes(flags);
            byte[] splitNameBytes = Encoding.ASCII.GetBytes(splitName);

            fixed (byte* pProducer = producerBytes)
            fixed (byte* pFlags = flagsBytes)
            fixed (byte* pSplitName = splitNameBytes)
            {
                sbyte* producerSBytePtr = (sbyte*)pProducer;
                sbyte* flagsSBytePtr = (sbyte*)pFlags;
                sbyte* splitNameSBytePtr = (sbyte*)pSplitName;
                LLVMOpaqueMetadata* metadataPtr = LLVM.DIBuilderCreateCompileUnit((LLVMOpaqueDIBuilder*)builder.Pointer,
                    lang,
                    (LLVMOpaqueMetadata*)fileMetadataRef.Pointer, producerSBytePtr, (UIntPtr)producerBytes.Length,
                    isOptimized, flagsSBytePtr, (UIntPtr)flagsBytes.Length, runtimeVersion,
                    splitNameSBytePtr, (UIntPtr)splitNameBytes.Length, dwarfEmissionKind, dWOld, splitDebugInlining,
                    debugInfoForProfiling);
                return new LLVMMetadataRef((IntPtr)metadataPtr);
            }
        }

        public static void AddNamedMetadataOperand(LLVMContextRef context, LLVMModuleRef module, string name, LLVMMetadataRef compileUnitMetadata)
        {
            module.AddNamedMetadataOperand(name, MetadataAsOpaqueValue(context, compileUnitMetadata));
        }

        static LLVMOpaqueValue* MetadataAsOpaqueValue(LLVMContextRef context, LLVMMetadataRef metadata)
        {
            return LLVM.MetadataAsValue(context, metadata);
        }

        public static LLVMValueRef MetadataAsValue(LLVMContextRef context, LLVMMetadataRef metadata)
        {
            return new LLVMValueRef((IntPtr)MetadataAsOpaqueValue(context, metadata));
        }

        public static LLVMMetadataRef DIBuilderCreateFunction(LLVMDIBuilderRef builder, LLVMMetadataRef debugMetadataCompileUnit, string methodName, string linkageName, LLVMMetadataRef debugMetadataFile, uint lineNumber, LLVMMetadataRef typeMetadata, int isLocalToUnit, 
            int isDefinition, uint scopeLine, LLVMDIFlags llvmDiFlags, int IsOptimized)
        {
            byte[] methodNameBytes = Encoding.ASCII.GetBytes(methodName);
            byte[] linkageNameBytes = Encoding.ASCII.GetBytes(linkageName);
            fixed (byte* pMethodName = methodNameBytes)
            fixed (byte* pLinkageNameBytes = linkageNameBytes)
            {
                sbyte* methodNameSBytePtr = (sbyte*)pMethodName;
                sbyte* linkageNameSBytePtr = (sbyte*)pLinkageNameBytes;

                return LLVM.DIBuilderCreateFunction((LLVMOpaqueDIBuilder*) builder.Pointer, (LLVMOpaqueMetadata*) debugMetadataCompileUnit.Pointer, methodNameSBytePtr, (UIntPtr)methodNameBytes.Length, linkageNameSBytePtr, (UIntPtr)linkageNameBytes.Length,
                    (LLVMOpaqueMetadata*) debugMetadataFile.Pointer, lineNumber, typeMetadata, isLocalToUnit, isDefinition, scopeLine, llvmDiFlags, IsOptimized);
            }
        }

        public static LLVMMetadataRef CreateDebugLocation(LLVMContextRef context, uint lineNumber, uint column, LLVMMetadataRef debugFunction, LLVMMetadataRef inlinedAt)
        {
            return LLVM.DIBuilderCreateDebugLocation((LLVMOpaqueContext*) context.Pointer, lineNumber, column, debugFunction, inlinedAt);
        }

        public static void DIBuilderFinalize(LLVMDIBuilderRef builder)
        {
            LLVM.DIBuilderFinalize(builder);
        }
    }
}
