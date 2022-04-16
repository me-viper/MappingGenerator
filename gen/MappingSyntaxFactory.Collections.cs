using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    using static SyntaxFactory;

    internal partial class MappingSyntaxFactory
    {
        private static readonly NameSyntax _collectionsHelper = IdentifierName("CollectionsHelper");

        public static InvocationExpressionSyntax CallInnerMapper(
            CollectionKind collectionKind,
            string member, 
            string sourceProperty)
        {
            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisMemberAccess(member),
                    IdentifierName(MapperMethod(collectionKind))
                    )
                ).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(MemberAccess("source", sourceProperty)))));
        }

        public static ExpressionSyntax MapperToConverter(ExpressionSyntax member)
        {
            var callMap = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    member,
                    IdentifierName("Map")
                    )
                );

            return SimpleLambdaExpression(Parameter(Identifier("p")))
                .WithExpressionBody(
                    callMap.WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("p")))))
                    );
        }

        public static ExpressionSyntax CallCopyToNew(
            ITypeSymbol elementsType,
            CollectionKind collectionKind,
            string sourceProperty)
        {
            TypeArgumentListSyntax typeArgs;

            if (collectionKind != CollectionKind.Dictionary)
            {
                var elementsFqn = CreateQualifiedName(elementsType);
                typeArgs = TypeArgumentList(SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { elementsFqn })); 
            }
            else
            {
                var keyFqn = CreateQualifiedName(((INamedTypeSymbol)elementsType).TypeArguments[0]);
                var valFqn = CreateQualifiedName(((INamedTypeSymbol)elementsType).TypeArguments[1]);
                typeArgs = TypeArgumentList(SeparatedList<TypeSyntax>(new[] { keyFqn, valFqn }));
            }

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    _collectionsHelper,
                    GenericName(Identifier(CopyToNewMethod(collectionKind)))
                    .WithTypeArgumentList(typeArgs)
                    )
                ).WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(MemberAccess("source", sourceProperty)),
                            })
                        )
                    );
        }

        public static ExpressionSyntax CallConvertAndCopyToNew(
            ITypeSymbol sourceType,
            string sourceProperty,
            ITypeSymbol destinationType,
            CollectionKind collectionKind,
            ExpressionSyntax converter)
        {

            //var srcFqn = CreateQualifiedName(sourceType);
            //var dstFqn = CreateQualifiedName(destinationType);

            TypeArgumentListSyntax typeArgs;

            if (collectionKind != CollectionKind.Dictionary)
            {
                var srcFqn = CreateQualifiedName(sourceType);
                var dstFqn = CreateQualifiedName(destinationType);
                typeArgs = TypeArgumentList(SeparatedList<TypeSyntax>(new [] { srcFqn, dstFqn }));
            }
            else
            {
                var skeyFqn = CreateQualifiedName(((INamedTypeSymbol)sourceType).TypeArguments[0]);
                var svalFqn = CreateQualifiedName(((INamedTypeSymbol)sourceType).TypeArguments[1]);
                var dkeyFqn = CreateQualifiedName(((INamedTypeSymbol)destinationType).TypeArguments[0]);
                var dvalFqn = CreateQualifiedName(((INamedTypeSymbol)destinationType).TypeArguments[1]);
                typeArgs = TypeArgumentList(SeparatedList<TypeSyntax>(new[] { skeyFqn, svalFqn, dkeyFqn, dvalFqn }));
            }

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    _collectionsHelper,
                    GenericName(Identifier(CopyToNewMethod(collectionKind)))
                    .WithTypeArgumentList(typeArgs)
                    )
                ).WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(MemberAccess("source", sourceProperty)),
                                Token(SyntaxKind.CommaToken),
                                Argument(converter)
                            })
                        )
                    );
        }

        public static StatementSyntax CallCopyTo(
            ITypeSymbol sourceType,
            string sourceProperty,
            ExpressionSyntax destinationSyntax,
            CollectionKind collectionKind,
            bool isWritable)
        {
            var copyToStmt = CallCopyToExisting(
                sourceType,
                sourceProperty,
                destinationSyntax
                );

            if (isWritable)
            {
                return CopyToExistingOrNew(
                    sourceType,
                    collectionKind,
                    sourceProperty,
                    destinationSyntax,
                    copyToStmt
                    );
            }

            return copyToStmt;
        }

        public static StatementSyntax CallConvertAndCopyTo(
            ITypeSymbol sourceType,
            string sourceProperty,
            ITypeSymbol destinationType,
            ExpressionSyntax destinationSyntax,
            CollectionKind collectionKind,
            ExpressionSyntax converter,
            bool isWritable)
        {
            var copyToStmt = CallConvertAndCopyToExisting(
                sourceType,
                sourceProperty,
                destinationType,
                destinationSyntax,
                converter
                );

            if (isWritable)
            {
                return ConvertAndCopyToExistingOrNew(
                    sourceType,
                    collectionKind,
                    sourceProperty,
                    destinationType,
                    destinationSyntax,
                    copyToStmt,
                    converter
                    );
            }

            return copyToStmt;
        }

        private static StatementSyntax CopyToExistingOrNew(
            ITypeSymbol elementsType,
            CollectionKind collectionKind,
            string sourceProperty,
            ExpressionSyntax destinationSyntax,
            StatementSyntax copyToSyntax)
        {
            return IfStatement(
                BinaryExpression(SyntaxKind.EqualsExpression, destinationSyntax, LiteralExpression(SyntaxKind.NullLiteralExpression)),
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        destinationSyntax,
                        CallCopyToNew(elementsType, collectionKind, sourceProperty)
                        )
                    )
                ).WithElse(ElseClause(copyToSyntax));
        }

        private static StatementSyntax ConvertAndCopyToExistingOrNew(
            ITypeSymbol sourceType,
            CollectionKind collectionKind,
            string sourceProperty,
            ITypeSymbol destinationType,
            ExpressionSyntax destinationSyntax,
            StatementSyntax copyToSyntax,
            ExpressionSyntax converter)
        {
            return IfStatement(
                BinaryExpression(SyntaxKind.EqualsExpression, destinationSyntax, LiteralExpression(SyntaxKind.NullLiteralExpression)),
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        destinationSyntax,
                        CallConvertAndCopyToNew(sourceType, sourceProperty, destinationType, collectionKind, converter)
                        )
                    )
                ).WithElse(ElseClause(copyToSyntax));
        }

        private static StatementSyntax CallCopyToExisting(
            ITypeSymbol type,
            string sourceProperty,
            ExpressionSyntax destinationSyntax)
        {
            var srcFqn = CreateQualifiedName(type);

            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        _collectionsHelper,
                        GenericName(Identifier(nameof(CollectionsHelper.CopyTo)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SeparatedList<TypeSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        srcFqn,
                                    })
                                )
                            )
                        )
                    )
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(MemberAccess("source", sourceProperty)),
                                Token(SyntaxKind.CommaToken),
                                Argument(destinationSyntax)
                            })
                        )
                    )
                );
        }

        private static StatementSyntax CallConvertAndCopyToExisting(
            ITypeSymbol sourceType,
            string sourceProperty,
            ITypeSymbol destinationType,
            ExpressionSyntax destinationSyntax,
            ExpressionSyntax converter)
        {
            var srcFqn = CreateQualifiedName(sourceType);
            var dstFqn = CreateQualifiedName(destinationType);

            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        _collectionsHelper,
                        GenericName(Identifier(nameof(CollectionsHelper.CopyTo)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SeparatedList<TypeSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        srcFqn,
                                        Token(SyntaxKind.CommaToken),
                                        dstFqn
                                    })
                                )
                            )
                        )
                    )
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(MemberAccess("source", sourceProperty)),
                                Token(SyntaxKind.CommaToken),
                                Argument(destinationSyntax),
                                Token(SyntaxKind.CommaToken),
                                Argument(converter)
                            })
                        )
                    )
                );
        }

        private static string CopyToNewMethod(CollectionKind type)
        {
            return type switch
            {
                CollectionKind.List => $"{nameof(CollectionsHelper.CopyToNew)}List",
                CollectionKind.Collection => $"{nameof(CollectionsHelper.CopyToNew)}Collection",
                CollectionKind.HashSet => $"{nameof(CollectionsHelper.CopyToNew)}HashSet",
                CollectionKind.Array => $"{nameof(CollectionsHelper.CopyToNew)}Array",
                CollectionKind.Dictionary => $"{nameof(CollectionsHelper.CopyToNew)}Dictionary",
                _ => throw new NotSupportedException($"'{type}' is not supported"),
            };
        }

        private static string MapperMethod(CollectionKind type)
        {
            return type switch
            {
                CollectionKind.List => "ToList",
                CollectionKind.Collection => "ToCollection",
                CollectionKind.HashSet => "ToHashSet",
                CollectionKind.Array => "ToArray",
                _ => throw new NotSupportedException($"'{type}' is not supported"),
            };
        }
    }
}
