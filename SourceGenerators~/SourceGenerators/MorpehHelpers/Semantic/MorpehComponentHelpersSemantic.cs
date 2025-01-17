﻿namespace SourceGenerators.MorpehHelpers.Semantic {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using NonSemantic;
    using SourceGenerators.Utils.NonSemantic;
    using Utils.Caches;
    using Utils.Collections;
    using Utils.Pools;

    public static class MorpehComponentHelpersSemantic {
        public static StashSpecialization GetStashSpecialization(ITypeSymbol? typeSymbol, string componentDecl) {
            return GetStashSpecialization(GetStashVariation(typeSymbol), componentDecl);
        }
        
        public static StashSpecialization GetStashSpecialization(StashVariation variation, string componentDecl) {
            return new StashSpecialization(
                Type: GetStashSpecializationType(variation, componentDecl),
                GetStashMethod: GetStashSpecializationGetStashMethod(variation, componentDecl),
                ConstraintInterface: GetStashSpecializationConstraintInterface(variation)
            );
        }
        
        public static string GetStashSpecializationType(StashVariation variation, string componentDecl) {
            switch (variation) {
                case StashVariation.Tag:
                    return "Scellecs.Morpeh.TagStash";
                case StashVariation.Disposable:
                    return StringBuilderPool.Get().Append("Scellecs.Morpeh.DisposableStash<").Append(componentDecl).Append(">").ToStringAndReturn();
                default:
                    return StringBuilderPool.Get().Append("Scellecs.Morpeh.Stash<").Append(componentDecl).Append(">").ToStringAndReturn();
            }
        }
        
        public static string GetStashSpecializationGetStashMethod(StashVariation variation, string componentDecl) {
            switch (variation) {
                case StashVariation.Tag:
                    return StringBuilderPool.Get().Append("GetTagStash<").Append(componentDecl).Append(">").ToStringAndReturn();
                case StashVariation.Disposable:
                    return StringBuilderPool.Get().Append("GetDisposableStash<").Append(componentDecl).Append(">").ToStringAndReturn();
                default:
                    return StringBuilderPool.Get().Append("GetStash<").Append(componentDecl).Append(">").ToStringAndReturn();
            }
        }
        
        public static string GetStashSpecializationConstraintInterface(StashVariation variation) {
            return variation switch {
                StashVariation.Tag => "Scellecs.Morpeh.ITagComponent",
                StashVariation.Disposable => "Scellecs.Morpeh.IDisposableComponent",
                _ => "Scellecs.Morpeh.IDataComponent"
            };
        }
        
        public static StashVariation GetStashVariation(ITypeSymbol? typeSymbol) {
            if (typeSymbol is not { TypeKind: TypeKind.Struct }) {
                return StashVariation.Unknown;
            }

#if !MORPEH_SOURCEGEN_NO_STASH_SPECIALIZATION
            var members = typeSymbol.GetMembers();
            
            var isTag = members
                .Where(m => m.Kind is SymbolKind.Field or SymbolKind.Property)
                .All(f => f.IsStatic);
            
            var isDisposable = members
                .Where(m => m.Kind is SymbolKind.Method)
                .Any(m => m.Name == "Dispose" && m is IMethodSymbol { Parameters: { Length: 0 } });

            if (isTag) {
                return StashVariation.Tag;
            }

            if (isDisposable) {
                return StashVariation.Disposable;
            }
#endif

            return StashVariation.Data;
        }
        
        public static EquatableArray<StashRequirement> GetStashRequirements(INamedTypeSymbol typeDeclaration) {
            var stashes = ThreadStaticListCache<StashRequirement>.GetClear();
            
            var attributes = typeDeclaration.GetAttributes();
            
            for (int i = 0, length = attributes.Length; i < length; i++) {
                var attribute = attributes[i];
                    
                if (attribute.AttributeClass?.Name != MorpehAttributes.INCLUDE_STASH_NAME) {
                    continue;
                }

                var args = attribute.ConstructorArguments;
                if (args.Length == 0) {
                    continue;
                }
                
                if (args[0].Kind != TypedConstantKind.Type) {
                    continue;
                }
                
                if (args[0].Value is not INamedTypeSymbol { TypeKind: TypeKind.Struct } componentTypeSymbol) {
                    continue;
                }

                var symbolChars = componentTypeSymbol.Name.AsSpan();
                
                var sb = StringBuilderPool.Get()
                    .Append('_')
                    .Append(char.ToLower(symbolChars[0]));

                for (int j = 1, jlength = symbolChars.Length; j < jlength; j++) {
                    sb.Append(symbolChars[j]);
                }
                
                RecursiveGenericWalker
                    .Create(sb, '_')
                    .Walk(componentTypeSymbol);
                
                stashes.Add(new StashRequirement(
                    FieldName: sb.ToStringAndReturn(),
                    MetadataClassName: componentTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    StashVariation: GetStashVariation(componentTypeSymbol)));
            }
            
            return new EquatableArray<StashRequirement>(stashes);
        }

        private readonly struct RecursiveGenericWalker {
            private readonly StringBuilder sb;
            private readonly char separator;
            
            private RecursiveGenericWalker(StringBuilder sb, char separator) {
                this.sb = sb;
                this.separator = separator;
            }
            
            public static RecursiveGenericWalker Create(StringBuilder sb, char separator) => new(sb, separator);

            public void Walk(INamedTypeSymbol typeSymbol) {
                if (typeSymbol is not { IsGenericType: true }) {
                    return;
                }

                var typeArguments = typeSymbol.TypeArguments;
                    
                for (int j = 0, jlength = typeArguments.Length; j < jlength; j++) {
                    var typeArgument = typeArguments[j];
                        
                    this.sb.Append(this.separator);
                    this.sb.Append(typeArgument.Name);
                    
                    if (typeArgument is INamedTypeSymbol namedTypeSymbol) {
                        this.Walk(namedTypeSymbol);
                    }
                }
            }
        }
    }
}