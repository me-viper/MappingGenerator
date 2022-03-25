using MappingGenerator.Abstractions;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Text;

namespace MappingGenerator.SourceGeneration
{
    using static SyntaxFactory;

    internal class MappingSyntaxFactoryWithContext
    {
        private NameSyntax _mapInterface;

        private ImplementationType _implementationType;

        public MappingSyntaxFactoryWithContext(NameSyntax mapInterface, ImplementationType implementationType)
        {
            _mapInterface = mapInterface;
            _implementationType = implementationType;
         }

        public MethodDeclarationSyntax MapMethod(
            NameSyntax sourceFqn,
            NameSyntax destFqn,
            IEnumerable<StatementSyntax> body)
        {
            return _implementationType == ImplementationType.Explicit 
                ? MapMethodExplicit(sourceFqn, destFqn, body) 
                : MapMethodImplicit(sourceFqn, destFqn, body);
        }

        private MethodDeclarationSyntax MapMethodExplicit(
            NameSyntax sourceFqn,
            NameSyntax destFqn,
            IEnumerable<StatementSyntax> body)
        {
            return MethodDeclaration(destFqn, Identifier("Map"))
                .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(_mapInterface))
                .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("source")).WithType(sourceFqn))))
                .WithBody(Block(body));
        }

        private MethodDeclarationSyntax MapMethodImplicit(
            NameSyntax sourceFqn,
            NameSyntax destFqn,
            IEnumerable<StatementSyntax> body)
        {
            return MethodDeclaration(destFqn, Identifier("Map"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("source")).WithType(sourceFqn))))
                .WithBody(Block(body));
        }

        public SimpleLambdaExpressionSyntax CallMapMethodLambda()
        {
            return SimpleLambdaExpression(Parameter(Identifier("p")))
                .WithExpressionBody(
                    CallMapMethod()
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("p")))))
                    );
        }

        public InvocationExpressionSyntax CallMapMethod(TypeSyntax? interfaceToCastTo = null)
        {
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(CastExpression(interfaceToCastTo ?? _mapInterface, ThisExpression())),
                        IdentifierName("Map")
                        )
                    )
                .WithArgumentList(
                    ArgumentList(SingletonSeparatedList(Argument(IdentifierName("source"))))
                    );
        }
    }
}
