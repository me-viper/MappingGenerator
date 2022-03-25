using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MappingGenerator.SourceGeneration.Spec
{
    internal class MapperTypeSpec
    {
        private readonly List<MappingSpec> _appliedSpecs = new();
        
        private readonly List<StatementSyntax> _mappingStatements = new();
        
        private readonly HashSet<KnownTypeMappingSpec> _knownMappingSpecs = new(KnownTypeMappingSpecEqualityComparer.Default);
        
        private readonly List<ExpressionSyntax> _initStatements = new();
        
        private readonly List<ArgumentSyntax> _destinationConstructorArguments = new();
        
        public bool HasCustomConstructor { get; set; }

        public IReadOnlyCollection<ArgumentSyntax> DestinationConstructorArguments => _destinationConstructorArguments;

        public IReadOnlyCollection<ExpressionSyntax> InitStatements => _initStatements;

        public IReadOnlyCollection<KnownTypeMappingSpec> KnownMappingSpecs => _knownMappingSpecs;

        public IReadOnlyCollection<StatementSyntax> MappingStatements => _mappingStatements;

        public IReadOnlyCollection<MappingSpec> AppliedSpecs => _appliedSpecs;

        public void ApplySpec(
            MappingSpec spec,
            Func<ExpressionSyntax, StatementSyntax>? makeStatement = null)
        {
            if (spec.Destination.EntryType == MappingDestinationType.Property)
            {
                if (makeStatement != null)
                    _mappingStatements.AddRange(spec.MappingExpressions.Select(makeStatement));
                _mappingStatements.AddRange(spec.MappingStatements);
            }

            if (spec.Destination.EntryType == MappingDestinationType.Parameter)
            {
                _destinationConstructorArguments.AddRange(
                    spec.MappingExpressions.Select(SyntaxFactory.Argument)
                    );
            }

            if (spec.Destination.EntryType == MappingDestinationType.InitProperty)
                _initStatements.AddRange(spec.MappingExpressions);

            if (spec is KnownTypeMappingSpec kms)
                _knownMappingSpecs.Add(kms);

            _appliedSpecs.Add(spec);
        }

        public void Merge(MapperTypeSpec other)
        {
            HasCustomConstructor = other.HasCustomConstructor;

            _destinationConstructorArguments.AddRange(other.DestinationConstructorArguments);
            _initStatements.AddRange(other.InitStatements);

            foreach (var spec in other.KnownMappingSpecs)
                _knownMappingSpecs.Add(spec);

            _mappingStatements.AddRange(other.MappingStatements);
            _appliedSpecs.AddRange(other.AppliedSpecs);
        }
    }
}