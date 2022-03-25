
using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MappingGenerator.SourceGeneration
{
    using static SyntaxFactory;

    internal class MappingSyntaxFactory
    {
        public static InvocationExpressionSyntax CallMappingMethod(string methodName)
        {
            return InvocationExpression(IdentifierName(methodName))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("source")))));
        }

        public static InvocationExpressionSyntax CallConvertMethod(string methodName, ExpressionSyntax expression)
        {
            return InvocationExpression(IdentifierName(methodName))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(expression))));
        }

        public static FieldDeclarationSyntax InnerMapperField(ITypeSymbol sourceType, ITypeSymbol destinationType, string name)
        {
            return FieldDeclaration(VariableDeclaration(MapperInterface(sourceType, destinationType))
                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(name)))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
        }

        public static ParameterSyntax InnerMapperConstructorParameter(ITypeSymbol sourceType, ITypeSymbol destinationType, string name)
        {
            return Parameter(Identifier(name)).WithType(MapperInterface(sourceType, destinationType));
        }

        public static IEnumerable<StatementSyntax> InnerMapperConstructorStatement(string member)
        {
            return List(
                new []
                {
                    ArgumentNotNull(member),
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            ThisMemberAccess(member),
                            IdentifierName(member)
                            )
                        )
                });
        }

        public static InvocationExpressionSyntax CallInnerMapper(string member, string sourceProperty)
        {
            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisMemberAccess(member),
                    IdentifierName("Map")
                    )
                ).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(MemberAccess("source", sourceProperty)))));
        }

        public static ExpressionSyntax CallCopyToNew(
            ITypeSymbol helperType,
            ITypeSymbol elementsType,
            ITypeSymbol collectionType,
            string sourceProperty)
        {
            var helperFqn = CreateQualifiedName(helperType);
            var elementsFqn = CreateQualifiedName(elementsType);
            var collectionFqn = CreateQualifiedName(collectionType);

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    helperFqn,
                    GenericName(Identifier("CopyToNew"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SeparatedList<TypeSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    elementsFqn,
                                    Token(SyntaxKind.CommaToken),
                                    collectionFqn,
                                })
                            )
                        )
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

        public static ExpressionSyntax CallCopyToNew(
            ITypeSymbol helperType,
            ITypeSymbol sourceType,
            string sourceProperty,
            ITypeSymbol destinationType,
            ITypeSymbol collectionType,
            string mapperMember)
        {
            var helperFqn = CreateQualifiedName(helperType);
            var srcFqn = CreateQualifiedName(sourceType);
            var dstFqn = CreateQualifiedName(destinationType);
            var collectionFqn = CreateQualifiedName(collectionType);

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    helperFqn,
                    GenericName(Identifier("CopyToNew"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SeparatedList<TypeSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    srcFqn,
                                    Token(SyntaxKind.CommaToken),
                                    dstFqn,
                                    Token(SyntaxKind.CommaToken),
                                    collectionFqn
                                })
                            )
                        )
                    )
                ).WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(MemberAccess("source", sourceProperty)),
                                Token(SyntaxKind.CommaToken),
                                Argument(IdentifierName(mapperMember))
                            })
                        )
                    );
        }

        public static StatementSyntax CallCopyTo(
            ITypeSymbol helperType,
            ITypeSymbol type,
            string sourceProperty,
            string destinationProperty)
        {
            var helperFqn = CreateQualifiedName(helperType);
            var srcFqn = CreateQualifiedName(type);

            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        helperFqn,
                        GenericName(Identifier("CopyTo"))
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
                                Argument(MemberAccess("result", destinationProperty))
                            })
                        )
                    )
                );
        }

        public static StatementSyntax CallCopyTo(
            ITypeSymbol helperType,
            ITypeSymbol sourceType,
            string sourceProperty,
            ITypeSymbol destinationType,
            string destinationProperty,
            string mapperMember)
        {
            var helperFqn = CreateQualifiedName(helperType);
            var srcFqn = CreateQualifiedName(sourceType);
            var dstFqn = CreateQualifiedName(destinationType);

            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        helperFqn,
                        GenericName(Identifier("CopyTo"))
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
                                Argument(MemberAccess("result", destinationProperty)),
                                Token(SyntaxKind.CommaToken),
                                Argument(IdentifierName(mapperMember))
                            })
                        )
                    )
                );
        }

        public static InvocationExpressionSyntax CallInnerMapper(NameSyntax thisType, string member, string sourceProperty)
        {
            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ParenthesizedExpression(CastExpression(thisType, ThisMemberAccess(member))),
                    IdentifierName("Map")
                    )
                ).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(MemberAccess("source", sourceProperty)))));
        }

        public static ExpressionStatementSyntax PropertyMapping(
            string destinationProperty,
            ExpressionSyntax assignment)
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    MemberAccess("result", destinationProperty),
                    assignment
                    )
                );
        }

        public static ExpressionSyntax PropertyMappingExpression(string sourceProperty)
        {
            return MemberAccess("source", sourceProperty);
        }

        public static TypeSyntax GetTypeSyntax(ITypeSymbol destinationType)
        {
            TypeSyntax typeSyntax;

            if (destinationType.SpecialType == SpecialType.None)
                typeSyntax = ParseTypeName(destinationType.ToDisplayString());
            else
            {
                typeSyntax = PredefinedType(Token(GetPredefinedKeywordKind(destinationType.SpecialType)));

                if (destinationType.NullableAnnotation.HasFlag(NullableAnnotation.Annotated))
                    typeSyntax = NullableType(typeSyntax);
            }

            return typeSyntax;
        }

        private static SyntaxKind GetPredefinedKeywordKind(SpecialType specialType)
        {
            return specialType switch
            {
                SpecialType.System_Boolean => SyntaxKind.BoolKeyword,
                SpecialType.System_Byte => SyntaxKind.ByteKeyword,
                SpecialType.System_SByte => SyntaxKind.SByteKeyword,
                SpecialType.System_Int32 => SyntaxKind.IntKeyword,
                SpecialType.System_UInt32 => SyntaxKind.UIntKeyword,
                SpecialType.System_Int16 => SyntaxKind.ShortKeyword,
                SpecialType.System_UInt16 => SyntaxKind.UShortKeyword,
                SpecialType.System_Int64 => SyntaxKind.LongKeyword,
                SpecialType.System_UInt64 => SyntaxKind.ULongKeyword,
                SpecialType.System_Single => SyntaxKind.FloatKeyword,
                SpecialType.System_Double => SyntaxKind.DoubleKeyword,
                SpecialType.System_Decimal => SyntaxKind.DecimalKeyword,
                SpecialType.System_String => SyntaxKind.StringKeyword,
                SpecialType.System_Char => SyntaxKind.CharKeyword,
                SpecialType.System_Object => SyntaxKind.ObjectKeyword,
                SpecialType.System_Void => SyntaxKind.VoidKeyword,
                _ => SyntaxKind.None
            };
        }

        public SyntaxTree Build(MappingSyntaxModel model)
        {
            var body = new List<StatementSyntax>();

            body.Add(ArgumentNotNull("source"));
            body.Add(DeclareResultVar(model.Context.DestinationType, model.Context.DestinationConstructorMethodName));
            body.AddRange(model.MappingStatements);
            body.Add(CallAfterMapMethod(model.Context.AfterMapMethodName));
            body.Add(ReturnResult());

            var nsFqn = CreateQualifiedName(model.Namespace.ToDisplayString());
            var sourceFqn = CreateQualifiedName(model.SourceType);
            var destFqn = CreateQualifiedName(model.DestinationType);

            var classMembers = new List<MemberDeclarationSyntax>();
            classMembers.AddRange(model.Fields);
            classMembers.Add(
                Constructor(
                    model.MapperType.Name,
                    model.ConstructorParameters,
                    model.ConstructorBody
                    )
                );

            var enumerable = model.Context.KnownTypes.IEnumerableType;
            var list = model.Context.KnownTypes.ListType;
            var hashSet = model.Context.KnownTypes.HashSetType;
            var collection = model.Context.KnownTypes.CollectionType;

            var sourceEnumerable = enumerable.Construct(model.SourceType);
            var listInterface = MapperInterface(sourceEnumerable, list.Construct(model.DestinationType));

            var baseTypes = new SyntaxNodeOrToken[]
            {
                SimpleBaseType(MapperInterface(model.SourceType, model.DestinationType)),
                Token(SyntaxKind.CommaToken),
                SimpleBaseType(listInterface),
                Token(SyntaxKind.CommaToken),
                SimpleBaseType(MapperInterface(sourceEnumerable, hashSet.Construct(model.DestinationType))),
                Token(SyntaxKind.CommaToken),
                SimpleBaseType(MapperInterface(sourceEnumerable, collection.Construct(model.DestinationType))),
                Token(SyntaxKind.CommaToken),
                SimpleBaseType(MapperInterface(sourceEnumerable, model.DestinationType, isDestinationArray: true)),
            };

            if (model.DestinationTypeConstructor != null)
                classMembers.Add(model.DestinationTypeConstructor);

            classMembers.Add(MapMethod(sourceFqn, destFqn, body));

            var mapListBody = MapCollectionMethodBody(sourceFqn, CreateQualifiedName(list.Construct(model.DestinationType)));
            var mapHashSetBody = MapCollectionMethodBody(sourceFqn, CreateQualifiedName(hashSet.Construct(model.DestinationType)));
            var mapToArrayBody = MapToTargetEnumerable(sourceFqn, "ToArray");
            var mapCollectionBody = MapToTargetCollection(listInterface, destFqn);

            classMembers.Add(MapManyMethodDeclarationSyntax(sourceEnumerable, list.Construct(model.DestinationType), mapListBody));
            classMembers.Add(MapManyMethodDeclarationSyntax(sourceEnumerable, hashSet.Construct(model.DestinationType), mapHashSetBody));
            classMembers.Add(MapManyMethodDeclarationSyntax(sourceEnumerable, collection.Construct(model.DestinationType), mapCollectionBody));
            classMembers.Add(MapManyMethodDeclarationSyntax(sourceEnumerable, model.DestinationType, mapToArrayBody, true));

            classMembers.Add(AfterMapMethod(sourceFqn, destFqn, model.Context.AfterMapMethodName));

            var typeParameters = new List<SyntaxNodeOrToken>();

            foreach (var tp in model.MapperType.TypeArguments)
            {
                typeParameters.Add(TypeParameter(Identifier(tp.Name)));
                typeParameters.Add(Token(SyntaxKind.CommaToken));
            }

            TypeParameterListSyntax? typeParametersList = null;

            if (typeParameters.Count > 0)
            {
                typeParameters.RemoveAt(typeParameters.Count - 1);
                typeParametersList = TypeParameterList(SeparatedList<TypeParameterSyntax>(typeParameters));
            }

            var usings = new[]
            {
                UsingDirective(IdentifierName("System")),
                UsingDirective(
                    QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Collections")), IdentifierName("Generic"))
                    ),
                UsingDirective(
                    QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Collections")), IdentifierName("ObjectModel"))
                    ),
                UsingDirective(QualifiedName(IdentifierName("System"), IdentifierName("Linq"))),
                UsingDirective(QualifiedName(IdentifierName("MappingGenerator"), IdentifierName("Abstractions"))),
            };

            return CompilationUnit()
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(nsFqn)
                            //.WithNamespaceKeyword(NullableKeyword(true))
                            .WithUsings(List(usings))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(
                                    ClassDeclaration(model.MapperType.Name)
                                        .WithModifiers(
                                            TokenList(new[] { Token(SyntaxKind.PartialKeyword) })
                                            )
                                        .WithBaseList(
                                            BaseList(
                                                SeparatedList<BaseTypeSyntax>(baseTypes)
                                                )
                                            )
                                        .WithTypeParameterList(typeParametersList)
                                        .WithMembers(List(classMembers))
                                        )
                                )
                            )
                    )
                //.WithEndOfFileToken(NullableKeyword(false))
                .NormalizeWhitespace()
                .SyntaxTree;
        }

        private static NameSyntax CreateQualifiedName(ITypeSymbol typeSymbol)
        {
            return ParseName(typeSymbol.ToDisplayString());
        }

        private static NameSyntax CreateQualifiedName(string fqn)
        {
            return ParseName(fqn);
        }

        private static SyntaxToken NullableKeyword(bool set)
        {
            var nullableDirective = set ? SyntaxKind.EnableKeyword : SyntaxKind.RestoreKeyword;
            var tokenDirective = set ? SyntaxKind.NamespaceKeyword : SyntaxKind.EndOfFileToken;

            return Token(
                TriviaList(Trivia(NullableDirectiveTrivia(Token(nullableDirective), true))),
                tokenDirective,
                TriviaList()
                );
        }

        private static TypeSyntax SimpleArray(TypeSyntax type)
        {
            return ArrayType(type)
                .WithRankSpecifiers(
                    SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))
                    )
                );
        }

        public static GenericNameSyntax MapperInterface(
            ITypeSymbol sourceType, 
            ITypeSymbol destinationType, 
            bool isDestinationArray = false)
        {
            var srcFqn = CreateQualifiedName(sourceType);
            var dstFqn = CreateQualifiedName(destinationType);

            SyntaxNode destTypeSyntax = !isDestinationArray
                ? dstFqn
                : SimpleArray(dstFqn);

            return GenericName(Identifier("IMapper"))
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SeparatedList<TypeSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                srcFqn,
                                Token(SyntaxKind.CommaToken),
                                destTypeSyntax
                            })
                        )
                    );
        }

        private static StatementSyntax ReturnResult()
        {
            return ReturnStatement(IdentifierName("result"));
        }

        private static StatementSyntax DeclareResultVar(INamedTypeSymbol destinationType, string createMethod)
        {
            return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier("result"))
                                .WithInitializer(EqualsValueClause(CallCreateMethod(createMethod)))
                            )
                        )
                );
        }

        private static BlockSyntax MapToTargetCollection(
            NameSyntax mapListInterface, 
            TypeSyntax destinationTypeSyntax)
        {
            return Block(
                MapToListStatement(mapListInterface),
                ReturnStatement(
                    ObjectCreationExpression(
                        GenericName(Identifier("Collection"))
                        .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(destinationTypeSyntax)))
                        )
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    IdentifierName("result")
                                    )
                                )
                            )
                        )
                    )
                );
        }

        private static BlockSyntax MapToTargetEnumerable(TypeSyntax sourceType, string convertTo)
        {
            return Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.CoalesceAssignmentExpression,
                        IdentifierName("source"),
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("Enumerable"),
                                GenericName(Identifier("Empty"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(SingletonSeparatedList(sourceType))
                                    )
                                )
                            )
                        )
                    ),
                ReturnStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("source"),
                                    IdentifierName("Select")))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("Map"))))),
                            IdentifierName(convertTo)
                            )
                        )
                    )
                );
        }

        private static StatementSyntax MapToListStatement(TypeSyntax mapListInterface)
        {
            return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier("result"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    ParenthesizedExpression(CastExpression(mapListInterface, ThisExpression())),
                                                    IdentifierName("Map")
                                                    )
                                                )
                                            .WithArgumentList(
                                                ArgumentList(SingletonSeparatedList(Argument(IdentifierName("source"))))
                                                )
                                        )
                                    )
                            )
                        )
                );
        }

        private static BlockSyntax MapCollectionMethodBody(TypeSyntax sourceType, TypeSyntax collectionType)
        {
            return Block(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.CoalesceAssignmentExpression,
                        IdentifierName("source"),
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("Enumerable"),
                                GenericName(Identifier("Empty"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(SingletonSeparatedList(sourceType))
                                    )
                                )
                            )
                        )
                    ),
                ReturnStatement(
                    ObjectCreationExpression(collectionType)
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("source"),
                                            IdentifierName("Select")))
                                    .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("Map")))))
                                    )
                                )
                            )
                        )
                    )
                );
        }

        private static MemberDeclarationSyntax MapManyMethodDeclarationSyntax(
            ITypeSymbol sourceType,
            INamedTypeSymbol destinationType,
            BlockSyntax body,
            bool isDestinationArray = false)
        {
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));
            
            var srcFqn = CreateQualifiedName(sourceType);
            var dstFqn = isDestinationArray ? SimpleArray(CreateQualifiedName(destinationType)) : CreateQualifiedName(destinationType);

            return MethodDeclaration(dstFqn, Identifier("Map"))
                .WithExplicitInterfaceSpecifier(
                    ExplicitInterfaceSpecifier(MapperInterface(sourceType, destinationType, isDestinationArray))
                    )
                .WithParameterList(
                    ParameterList(SingletonSeparatedList(Parameter(Identifier("source")).WithType(srcFqn)))
                    )
                .WithBody(body);
        }

        private static StatementSyntax ArgumentNotNull(string name)
        {
            return IfStatement(
                BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    IdentifierName(name),
                    LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                ThrowStatement(
                    ObjectCreationExpression(IdentifierName("ArgumentNullException"))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        InvocationExpression(
                                            IdentifierName(
                                                Identifier(TriviaList(), SyntaxKind.NameOfKeyword, "nameof", "nameof", TriviaList())
                                                )
                                            ).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName(name))))))
                                    )
                                )
                        )
                    )
                );
        }

        private static MemberAccessExpressionSyntax MemberAccess(string expression, string name)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(expression),
                IdentifierName(name)
                );
        }

        private static MemberAccessExpressionSyntax ThisMemberAccess(string name)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                ThisExpression(),
                IdentifierName(name)
                );
        }

        private static ConstructorDeclarationSyntax Constructor(
            string typeName,
            IEnumerable<ParameterSyntax> parameters,
            IEnumerable<StatementSyntax> body)
        {
            return ConstructorDeclaration(Identifier(typeName))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(SeparatedList(parameters)))
                .WithBody(Block(body));
        }

        private static MethodDeclarationSyntax MapMethod(
            NameSyntax sourceFqn,
            NameSyntax destFqn,
            IEnumerable<StatementSyntax> body)
        {
            return MethodDeclaration(destFqn, Identifier("Map"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(ParameterList(SingletonSeparatedList(Parameter(Identifier("source")).WithType(sourceFqn))))
                .WithBody(Block(body));
        }

        private static MethodDeclarationSyntax AfterMapMethod(
            NameSyntax sourceType, 
            NameSyntax destinationMap,
            string name)
        {
            return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier(name))
                .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(Identifier("source")).WithType(sourceType),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier("result")).WithType(destinationMap)
                            })
                        )
                    )
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private static StatementSyntax CallAfterMapMethod(string name)
        {
            return ExpressionStatement(
                InvocationExpression(IdentifierName(name))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(IdentifierName("source")),
                                Token(SyntaxKind.CommaToken),
                                Argument(IdentifierName("result"))
                            })
                        )
                    )
                );
        }

        public static StatementSyntax CallConstructor(
            INamedTypeSymbol destinationType,
            IEnumerable<ArgumentSyntax> parameters,
            IEnumerable<ExpressionSyntax> initializers)
        {
            var destFqn = CreateQualifiedName(destinationType);

            return ReturnStatement(ObjectCreationExpression(destFqn)
                .WithArgumentList(ArgumentList(SeparatedList(parameters)))
                .WithInitializer(InitializerExpression(SyntaxKind.ObjectInitializerExpression, SeparatedList(initializers)))
                );
        }

        public static MethodDeclarationSyntax CreateMethod(
            INamedTypeSymbol sourceType,
            INamedTypeSymbol destinationType,
            string name,
            IEnumerable<StatementSyntax> destinationTypeConstructor)
        {
            var srcFqn = CreateQualifiedName(sourceType);
            var dstFqn = CreateQualifiedName(destinationType);

            return MethodDeclaration(dstFqn, Identifier(name))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(Parameter(Identifier("source")).WithType(srcFqn))
                        )
                    )
                .WithBody(Block(destinationTypeConstructor));
        }

        private static ExpressionSyntax CallCreateMethod(string name)
        {
            return InvocationExpression(IdentifierName(name))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(Argument(IdentifierName("source")))
                        )
                );
        }
    }
}
