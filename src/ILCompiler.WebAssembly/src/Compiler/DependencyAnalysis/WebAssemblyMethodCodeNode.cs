// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ILCompiler.DependencyAnalysisFramework;

using Internal.Text;
using Internal.TypeSystem;
using LLVMSharp;

namespace ILCompiler.DependencyAnalysis
{
    internal abstract class WebAssemblyMethodCodeNode : DependencyNodeCore<NodeFactory>
    {
        protected readonly MethodDesc _method;
        protected IEnumerable<Object> _dependencies = Enumerable.Empty<Object>();

        protected WebAssemblyMethodCodeNode(MethodDesc method)
        {
            Debug.Assert(!method.IsAbstract);
            _method = method;
        }

        public void SetDependencies(IEnumerable<Object> dependencies)
        {
            Debug.Assert(dependencies != null);
            _dependencies = dependencies;
        }
        
        public MethodDesc Method
        {
            get
            {
                return _method;
            }
        }

        public override bool StaticDependenciesAreComputed => CompilationCompleted;

        public bool CompilationCompleted { get; set; }

        public void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append(nameMangler.GetMangledMethodName(_method));
        }
        public int Offset => 0;
        public bool RepresentsIndirectionCell => false;

        public override bool InterestingForDynamicDependencyAnalysis => false;
        public override bool HasDynamicDependencies => false;
        public override bool HasConditionalStaticDependencies => false;

        public override IEnumerable<CombinedDependencyListEntry> GetConditionalStaticDependencies(NodeFactory factory) => null;
        public override IEnumerable<CombinedDependencyListEntry> SearchDynamicDependencies(List<DependencyNodeCore<NodeFactory>> markedNodes, int firstNode, NodeFactory factory) => null;
    }

    internal class WebAssemblyMethodBodyNode : WebAssemblyMethodCodeNode, IMethodBodyNode
    {
        public WebAssemblyMethodBodyNode(MethodDesc method)
            : base(method)
        {
        }

        protected override string GetName(NodeFactory factory) => this.GetMangledName(factory.NameMangler);

        public override IEnumerable<DependencyListEntry> GetStaticDependencies(NodeFactory factory)
        {
            var dependencies = new DependencyList();

            foreach (Object node in _dependencies)
                dependencies.Add(node, "Wasm code ");

            CodeBasedDependencyAlgorithm.AddDependenciesDueToMethodCodePresence(ref dependencies, factory, _method);

            return dependencies;
        }

        int ISortableNode.ClassCode => -1502960727;

        int ISortableNode.CompareToImpl(ISortableNode other, CompilerComparer comparer)
        {
            return comparer.Compare(_method, ((WebAssemblyMethodBodyNode)other)._method);
        }
    }

    internal class WebAssemblyUnboxingThunkNode : WebAssemblyMethodCodeNode, IMethodNode
    {
        public WebAssemblyUnboxingThunkNode(MethodDesc method)
            : base(method)
        {
        }

        protected override string GetName(NodeFactory factory) => this.GetMangledName(factory.NameMangler);

        public override IEnumerable<DependencyListEntry> GetStaticDependencies(NodeFactory factory)
        {
            var dependencies = new DependencyList();

            foreach (Object node in _dependencies)
                dependencies.Add(node, "Wasm code ");

            return dependencies;
        }

        int ISortableNode.ClassCode => -18942467;

        int ISortableNode.CompareToImpl(ISortableNode other, CompilerComparer comparer)
        {
            return comparer.Compare(_method, ((WebAssemblyUnboxingThunkNode)other)._method);
        }
    }

    internal class WebAssemblyBlockRefNode : DependencyNodeCore<NodeFactory>, ISymbolNode
    {
        readonly LLVMValueRef catchFuncletRef;
        readonly string mangledName;

        public WebAssemblyBlockRefNode(LLVMValueRef catchFuncletRef, string mangledName)
        {
            this.catchFuncletRef = catchFuncletRef;
            this.mangledName = mangledName;
        }

        public override bool HasConditionalStaticDependencies => false;
        public override bool HasDynamicDependencies => false;
        public override bool InterestingForDynamicDependencyAnalysis => false;
        public override bool StaticDependenciesAreComputed => true;
        public override IEnumerable<DependencyListEntry> GetStaticDependencies(NodeFactory context)
        {
            return Enumerable.Empty<DependencyListEntry>();
        }

        public override IEnumerable<CombinedDependencyListEntry> GetConditionalStaticDependencies(NodeFactory context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<CombinedDependencyListEntry> SearchDynamicDependencies(List<DependencyNodeCore<NodeFactory>> markedNodes, int firstNode, NodeFactory context)
        {
            throw new NotImplementedException();
        }

        protected override string GetName(NodeFactory context)
        {
            throw new NotImplementedException();
        }

//        public override int ClassCode => 1019664187;

        

//        public override ObjectNodeSection Section { get; }
//        public override bool IsShareable { get; }
        public void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append(mangledName);
        }

        public int Offset { get; }
        public bool RepresentsIndirectionCell { get; }
    }
}
