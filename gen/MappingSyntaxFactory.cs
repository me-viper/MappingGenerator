
using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Talk2Bits.MappingGenerator.Abstractions;

namespace Talk2Bits.MappingGenerator.SourceGeneration
{
    using static SyntaxFactory;

    internal class MappingSyntaxFactory
    {
        private MappingSyntaxFactoryWithContext _syntaxFactoryWithContext = default!;

        private readonly IReadOnlyCollection<UsingDirectiveSyntax> _knownUsings;

        private readonly NameSyntax _compilerGenerated;

        private static readonly NameSyntax _collectionsHelper = IdentifierName("CollectionsHelper");

        public MappingSyntaxFactory()
        {
            _knownUsings = new[]
            {
                UsingDirective(IdentifierName("System")),
                UsingDirective(
                    QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Collections")), IdentifierName("Generic"))
                    ),
                UsingDirective(
                    QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Collections")), IdentifierName("ObjectModel"))
                    ),
                UsingDirective(QualifiedName(IdentifierName("System"), IdentifierName("Linq"))),
                UsingDirective(
                    QualifiedName(QualifiedName(IdentifierName("Talk2Bits"), IdentifierName("MappingGenerator")), IdentifierName("Abstractions"))
                    ),
            };

            _compilerGenerated = QualifiedName(
                QualifiedName(
                    QualifiedName(
                        IdentifierName("System"), 
                        IdentifierName("Runtime")
                        ),
                        IdentifierName("CompilerServices")
                    ),
                    IdentifierName("CompilerGeneratedAttribute")
                );
        }

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

        public static SyntaxList<ExpressionStatementSyntax> InnerMapperConstructorThisStatement(
            ITypeSymbol sourceType, 
            ITypeSymbol destinationType,
            string member)
        {
            return List(
                new[]
                {
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            ThisMemberAccess(member),
                            CastExpression(MapperInterface(sourceType, destinationType), ThisExpression())
                            )
                        )
                });
        }

        public static SyntaxList<StatementSyntax> InnerMapperConstructorStatement(string member)
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

        public static SimpleLambdaExpressionSyntax ExplicitCastConverter(ITypeSymbol targetType)
        {
            var type = CreateQualifiedName(targetType);

            return SimpleLambdaExpression(Parameter(Identifier("p")))
                .WithModifiers(TokenList(Token(SyntaxKind.StaticKeyword)))
                .WithExpressionBody(CastExpression(type, IdentifierName("p")));
        }

        public static ExpressionSyntax CallCopyToNew(
            ITypeSymbol elementsType,
            ITypeSymbol collectionType,
            string sourceProperty)
        {
            var elementsFqn = CreateQualifiedName(elementsType);
            var collectionFqn = CreateQualifiedName(collectionType);

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    _collectionsHelper,
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
            ITypeSymbol sourceType,
            string sourceProperty,
            ITypeSymbol destinationType,
            ITypeSymbol collectionType,
            ExpressionSyntax converter)
        {
            var srcFqn = CreateQualifiedName(sourceType);
            var dstFqn = CreateQualifiedName(destinationType);
            var collectionFqn = CreateQualifiedName(collectionType);

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    _collectionsHelper,
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
                                Argument(converter)
                            })
                        )
                    );
        }

        public static ExpressionSyntax CallCopyToNew(
            ITypeSymbol sourceType,
            string sourceProperty,
            ITypeSymbol destinationType,
            ITypeSymbol collectionType,
            string mapperMember)
        {
            return CallCopyToNew(
                sourceType,
                sourceProperty,
                destinationType,
                collectionType,
                IdentifierName(mapperMember)
                );
        }

        public static StatementSyntax CallCopyTo(
            ITypeSymbol type,
            string sourceProperty,
            string destinationProperty)
        {
            var srcFqn = CreateQualifiedName(type);

            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        _collectionsHelper,
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
            ITypeSymbol sourceType,
            string sourceProperty,
            ITypeSymbol destinationType,
            string destinationProperty,
            string mapperMember)
        {
            return CallCopyTo(
                sourceType,
                sourceProperty,
                destinationType,
                destinationProperty,
                IdentifierName(mapperMember)
                );
        }

        public static StatementSyntax CallCopyTo(
            ITypeSymbol sourceType,
            string sourceProperty,
            ITypeSymbol destinationType,
            string destinationProperty,
            ExpressionSyntax converter)
        {
            var srcFqn = CreateQualifiedName(sourceType);
            var dstFqn = CreateQualifiedName(destinationType);

            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        _collectionsHelper,
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
                                Argument(converter)
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

        public static IEnumerable<SyntaxToken> GetAccessibilityModifier(ConstructorAccessibility accessibility)
        {
            return accessibility switch
            {
                ConstructorAccessibility.Public => new[] { Token(SyntaxKind.PublicKeyword) },
                ConstructorAccessibility.Private => new[] { Token(SyntaxKind.PrivateKeyword) },
                ConstructorAccessibility.PrivateProtected => new[] { Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ProtectedKeyword) },
                ConstructorAccessibility.Protected => new[] { Token(SyntaxKind.ProtectedKeyword) },
                ConstructorAccessibility.Internal => new[] { Token(SyntaxKind.InternalKeyword) },
                ConstructorAccessibility.InternalProtected => new[] { Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.InternalKeyword) },
                _ => throw new NotSupportedException($"Invalid value {accessibility} for {typeof(ConstructorAccessibility)}"),
            };
        }

        public SyntaxTree BuildConstructorOnly(MapperAnchorSyntaxModel model)
        {
            var nsFqn = CreateQualifiedName(model.Namespace.ToDisplayString());

            var typeParameters = new SeparatedSyntaxList<TypeParameterSyntax>()
                .AddRange(model.MapperType.TypeArguments.Select(p => TypeParameter(Identifier(p.Name))));

            TypeParameterListSyntax? typeParametersList = null;

            if (typeParameters.Count > 0)
            {
                typeParametersList = TypeParameterList(typeParameters);
            }

            var classMembers = new List<MemberDeclarationSyntax>();
            classMembers.AddRange(model.Fields);

            classMembers.Add(
                Constructor(
                    model.MapperType.Name,
                    model.ConstructorAccessibility,
                    model.ConstructorParameters,
                    model.ConstructorBody
                    )
                );

            return CompilationUnit()
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(nsFqn)
                            //.WithNamespaceKeyword(NullableKeyword(true))
                            .WithUsings(List(_knownUsings))
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(
                                    ClassDeclaration(model.MapperType.Name)
                                        // .WithAttributeLists(
                                        //     SingletonList(AttributeList(SingletonSeparatedList(Attribute(_compilerGenerated))))
                                        //     )
                                        .WithModifiers(
                                            TokenList(new[] { Token(SyntaxKind.PartialKeyword) })
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

        public SyntaxTree Build(MapperInstanceSyntaxModel model)
        {
            var mapperInterface = MapperInterface(model.SourceType, model.DestinationType);
            
            _syntaxFactoryWithContext = new MappingSyntaxFactoryWithContext(mapperInterface, model.ImplementationType);

            var body = new List<StatementSyntax>();

            body.Add(ArgumentNotNull("source"));

            if (model.DestinationTypeConstructor == null)
                body.Add(DeclareResultVar(model.DestinationConstructorMethodName, false));
            else
            {
                body.Add(model.DestinationTypeConstructor);
                body.Add(DeclareResultVar(model.DestinationConstructorMethodName, true));
            }
            
            body.AddRange(model.MappingStatements);
            body.Add(CallAfterMapMethod(model.AfterMapMethodName));
            body.Add(ReturnResult());

            var nsFqn = CreateQualifiedName(model.Namespace.ToDisplayString());
            var sourceFqn = CreateQualifiedName(model.SourceType);
            var destFqn = CreateQualifiedName(model.DestinationType);

            var classMembers = new List<MemberDeclarationSyntax>();
            classMembers.AddRange(model.Fields);

            if (model.ConstructorBody.Count > 0 || model.ConstructorAccessibility != default)
            {
                classMembers.Add(
                    Constructor(
                        model.MapperType.Name,
                        model.ConstructorAccessibility,
                        model.ConstructorParameters,
                        model.ConstructorBody
                        )
                    ); 
            }

            var enumerable = model.KnownTypes.IEnumerableType;
            var list = model.KnownTypes.ListType;
            var hashSet = model.KnownTypes.HashSetType;
            var collection = model.KnownTypes.CollectionType;

            var sourceEnumerable = enumerable.Construct(model.SourceType);
            var listInterface = MapperInterface(sourceEnumerable, list.Construct(model.DestinationType));

            var baseTypes = new SyntaxNodeOrToken[]
            {
                SimpleBaseType(mapperInterface),
                Token(SyntaxKind.CommaToken),
                SimpleBaseType(listInterface),
                Token(SyntaxKind.CommaToken),
                SimpleBaseType(MapperInterface(sourceEnumerable, hashSet.Construct(model.DestinationType))),
                Token(SyntaxKind.CommaToken),
                SimpleBaseType(MapperInterface(sourceEnumerable, collection.Construct(model.DestinationType))),
                Token(SyntaxKind.CommaToken),
                SimpleBaseType(MapperInterface(sourceEnumerable, model.DestinationType, isDestinationArray: true)),
            };

            classMembers.Add(_syntaxFactoryWithContext.MapMethod(sourceFqn, destFqn, body));

            var mapListBody = MapCollectionMethodBody(sourceFqn, CreateQualifiedName(list.Construct(model.DestinationType)));
            var mapHashSetBody = MapCollectionMethodBody(sourceFqn, CreateQualifiedName(hashSet.Construct(model.DestinationType)));
            var mapToArrayBody = MapToTargetEnumerable(sourceFqn, "ToArray");
            var mapCollectionBody = MapToTargetCollection(listInterface, destFqn);

            classMembers.Add(MapManyMethodDeclarationSyntax(sourceEnumerable, list.Construct(model.DestinationType), mapListBody));
            classMembers.Add(MapManyMethodDeclarationSyntax(sourceEnumerable, hashSet.Construct(model.DestinationType), mapHashSetBody));
            classMembers.Add(MapManyMethodDeclarationSyntax(sourceEnumerable, collection.Construct(model.DestinationType), mapCollectionBody));
            classMembers.Add(MapManyMethodDeclarationSyntax(sourceEnumerable, model.DestinationType, mapToArrayBody, true));

            classMembers.Add(AfterMapMethod(sourceFqn, destFqn, model.AfterMapMethodName));

            var typeParameters = new SeparatedSyntaxList<TypeParameterSyntax>()
                .AddRange(model.MapperType.TypeArguments.Select(p => TypeParameter(Identifier(p.Name))));

            TypeParameterListSyntax? typeParametersList = null;

            if (typeParameters.Count > 0)
            {
                typeParametersList = TypeParameterList(typeParameters);
            }

            return CompilationUnit()
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(nsFqn)
                            //.WithNamespaceKeyword(NullableKeyword(true))
                            .WithUsings(List(_knownUsings))
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

        private SimpleLambdaExpressionSyntax CallMapMethodLambda(TypeSyntax mapInterface)
        {
            return SimpleLambdaExpression(Parameter(Identifier("p")))
                .WithExpressionBody(
                    CallMapMethod(mapInterface)
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("p")))))
                    );
        }

        private static InvocationExpressionSyntax CallMapMethod(TypeSyntax mapInterface)
        {
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(CastExpression(mapInterface, ThisExpression())),
                        IdentifierName("Map")
                        )
                    )
                .WithArgumentList(
                    ArgumentList(SingletonSeparatedList(Argument(IdentifierName("source"))))
                    );
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

        private static StatementSyntax DeclareResultVar(string createMethod, bool isLocal)
        {
            return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier("result"))
                                .WithInitializer(EqualsValueClause(CallCreateMethod(createMethod, isLocal)))
                            )
                        )
                );
        }

        private BlockSyntax MapToTargetCollection(
            NameSyntax mapListInterface, 
            TypeSyntax destinationTypeSyntax)
        {
            return Block(
                MapToListStatement(mapListInterface),
                ReturnStatement(
                    ObjectCreationExpression(
                        GenericName(Identifier("Collection"))
                        .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(destinationTypeSyntax)))
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

        private BlockSyntax MapToTargetEnumerable(TypeSyntax sourceType, string convertTo)
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
                                .WithArgumentList(
                                    ArgumentList(SingletonSeparatedList(Argument(_syntaxFactoryWithContext.CallMapMethodLambda())))
                                    ),
                            IdentifierName(convertTo)
                            )
                        )
                    )
                );
        }

        private StatementSyntax MapToListStatement(TypeSyntax mapListInterface)
        {
            return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(Identifier(TriviaList(), SyntaxKind.VarKeyword, "var", "var", TriviaList())))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier("result"))
                                .WithInitializer(
                                    EqualsValueClause(_syntaxFactoryWithContext.CallMapMethod(mapListInterface))
                                    )
                            )
                        )
                );
        }

        private BlockSyntax MapCollectionMethodBody(TypeSyntax sourceType, TypeSyntax collectionType)
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
                                    .WithArgumentList(
                                        ArgumentList(SingletonSeparatedList(Argument(_syntaxFactoryWithContext.CallMapMethodLambda())))
                                        )
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
            ConstructorAccessibility accessibility,
            IEnumerable<ParameterSyntax> parameters,
            IEnumerable<StatementSyntax> body)
        {
            var modifiers = GetAccessibilityModifier(accessibility);

            return ConstructorDeclaration(Identifier(typeName))
                .WithModifiers(TokenList(modifiers))
                .WithParameterList(ParameterList(SeparatedList(parameters)))
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

        public static LocalFunctionStatementSyntax CreateMethod(
            INamedTypeSymbol destinationType,
            string name,
            IEnumerable<StatementSyntax> destinationTypeConstructor)
        {
            var dstFqn = CreateQualifiedName(destinationType);

            return LocalFunctionStatement(dstFqn, Identifier(name))
                .WithBody(Block(destinationTypeConstructor));
        }

        private static ExpressionSyntax CallCreateMethod(string name, bool isLocal)
        {
            if (isLocal)
                return InvocationExpression(IdentifierName(name));

            return InvocationExpression(IdentifierName(name))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(Argument(IdentifierName("source")))
                        )
                );
        }
    }
}
