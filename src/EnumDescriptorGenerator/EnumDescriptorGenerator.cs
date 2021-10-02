using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EnumDescriptorGenerator
{
    [Generator]
    public sealed class EnumDescriptorGenerator : ISourceGenerator
    {
        private const string AttributeText = @"
            #nullable enable
            [System.Diagnostics.Conditional(""EnumToString_Attributes"")]
            [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = false)]
            internal sealed class GenerateEnumDescription : System.Attribute
            {
            }
            ";
        public void Initialize(GeneratorInitializationContext context)
        {
            // Attach the debugger if required.
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
            context.RegisterForPostInitialization(ctx => ctx.AddSource("GenerateEnumDescription.g.cs", SourceText.From(AttributeText, Encoding.UTF8)));
            context.RegisterForSyntaxNotifications(() => new EnumSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = context.SyntaxReceiver as EnumSyntaxReceiver;

            var compilation = context.Compilation;
            var attributeSymbol = compilation.GetTypeByMetadataName("GenerateEnumDescription");
            var descriptionAttrSymbol = compilation.GetTypeByMetadataName("System.ComponentModel.DescriptionAttribute");

            ITypeSymbol GetSymbol(EnumDeclarationSyntax declaration)
            {
                var model = compilation.GetSemanticModel(declaration.SyntaxTree);
                return model.GetDeclaredSymbol(declaration)!;
            }

            var values = receiver!.Candidates.Where(item =>
            {
                var typeSymbol = GetSymbol(item);

                return (typeSymbol.GetAttributes().Any(x => x.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)));
            })
            .Select(item => new EnumProcessor(GetSymbol(item), descriptionAttrSymbol!));

            StringBuilder sb = new();
            sb.Append(@"
                namespace System
                {
                    public static class EnumStringExtensions
                    {      
                ");
            foreach (var item in values)
            {

                sb.Append($@"
                    public static string GetDescription(this {item.FullName} value)
                    {{
                        return value switch
                        {{
                    ");

                foreach (var (originalName, declarativeName) in item.GetMembers())
                {
                    if (declarativeName is null)
                    {

                        sb.Append("            ").Append(item.FullName).Append('.').Append(originalName).Append(" => nameof(").Append(item.FullName).Append('.').Append(originalName).AppendLine("),");
                    }
                    else
                    {
                        sb.Append("            ").Append(item.FullName).Append('.').Append(originalName).Append(" => \"").Append(declarativeName).AppendLine("\",");
                    }

                }

                sb.Append(@"
                            _ => value.ToString()
                        };
                    }
            ");
            }

            sb.Append(@"
            }
        }");

            context.AddSource("EnumDescriptionExtensions.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private struct EnumMembersProcessor
        {

        }

        private readonly struct EnumProcessor
        {
            public EnumProcessor(ITypeSymbol symbol, INamedTypeSymbol descriptionAttributeSymbol)
            {
                Symbol = symbol;
                DescriptionAttributeSymbol = descriptionAttributeSymbol;
            }

            private INamedTypeSymbol DescriptionAttributeSymbol { get; }
            private ITypeSymbol Symbol { get; }

            public string FullName => Symbol.ToString()!;
            public string? FullNamespace => GetNamespace(Symbol);

            public IEnumerable<(string OriginalName, string? DeclarativeName)> GetMembers()
            {
                var attrSymbol = DescriptionAttributeSymbol;
                var members = Symbol.GetMembers();
                foreach (var member in members)
                {
                    if (member is not IFieldSymbol field)
                        continue;

                    if (field.ConstantValue is null)
                        continue;

                    var descriptionAttr = member.GetAttributes()
                                .FirstOrDefault(x => x.AttributeClass!.Equals(attrSymbol, SymbolEqualityComparer.Default));

                    if (descriptionAttr != null && descriptionAttr.ConstructorArguments.Any())
                    {
                        yield return (member.Name, (string)descriptionAttr.ConstructorArguments[0].Value!);
                    }
                    else
                    {
                        yield return (member.Name, null);
                    }
                }
            }

            private static string? GetNamespace(ITypeSymbol symbol)
            {
                string? result = null;
                var ns = symbol.ContainingNamespace;
                while (ns is {IsGlobalNamespace: false})
                {
                    if (result != null)
                    {
                        result = ns.Name + "." + result;
                    }
                    else
                    {
                        result = ns.Name;
                    }

                    ns = ns.ContainingNamespace;
                }

                return result;
            }
        }

        public class EnumSyntaxReceiver : ISyntaxReceiver
        {
            public List<EnumDeclarationSyntax> Candidates { get; } = new List<EnumDeclarationSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // any method with at least one attribute is a candidate for property generation
                if (syntaxNode is EnumDeclarationSyntax { AttributeLists: { Count: >= 0 } } enumDeclarationSyntax)
                {
                    Candidates.Add(enumDeclarationSyntax);
                }
            }
        }
    }
}