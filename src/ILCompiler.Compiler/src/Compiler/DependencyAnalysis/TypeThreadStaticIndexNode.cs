﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Internal.Text;
using Internal.TypeSystem;

namespace ILCompiler.DependencyAnalysis
{
    /// <summary>
    /// Represents a node containing information necessary at runtime to locate type's thread static base.
    /// </summary>
    public class TypeThreadStaticIndexNode : ObjectNode, ISymbolDefinitionNode
    {
        private MetadataType _type;

        public TypeThreadStaticIndexNode(MetadataType type)
        {
            _type = type;
            foreach (TypeDesc r in type.Instantiation)
            {
                if (r is RuntimeDeterminedType)
                {

                }
            }
        }

        public void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append("__TypeThreadStaticIndex_")
              .Append(nameMangler.GetMangledTypeName(_type));
        }
        public int Offset => 0;
        protected override string GetName(NodeFactory factory) => this.GetMangledName(factory.NameMangler);
        public override ObjectNodeSection Section
        {
            get
            {
                if (_type.Context.Target.IsWindows)
                    return ObjectNodeSection.ReadOnlyDataSection;
                else
                    return ObjectNodeSection.DataSection;
            }
        }
        public override bool IsShareable => true;
        public override bool StaticDependenciesAreComputed => true;

        protected override DependencyList ComputeNonRelocationBasedDependencies(NodeFactory factory)
        {
            return new DependencyList
            {
                new DependencyListEntry(factory.TypeThreadStaticsSymbol(_type), "Thread static storage")
            };
        }

        public override ObjectData GetData(NodeFactory factory, bool relocsOnly = false)
        {
            ObjectDataBuilder objData = new ObjectDataBuilder(factory, relocsOnly);

            objData.RequireInitialPointerAlignment();
            objData.AddSymbol(this);

            int typeTlsIndex = 0;
            if (!relocsOnly)
            {
                var node = factory.TypeThreadStaticsSymbol(_type);
                typeTlsIndex = ((ThreadStaticsNode)node).IndexFromBeginningOfArray;
            }

            objData.EmitPointerReloc(factory.TypeManagerIndirection);
            objData.EmitNaturalInt(typeTlsIndex);

            return objData.ToObjectData();
        }

        public MetadataType Type => _type;

        public override int ClassCode => -149601250;

        public override int CompareToImpl(ISortableNode other, CompilerComparer comparer)
        {
            return comparer.Compare(_type, ((TypeThreadStaticIndexNode)other)._type);
        }
    }
}
