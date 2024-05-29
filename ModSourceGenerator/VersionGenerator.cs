using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModSourceGenerator;

[Generator]
public class VersionGenerator: IIncrementalGenerator
{
    private const string AttributeCode = @"// <auto-generated />
namespace BepInEx
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    [System.Diagnostics.Conditional(""CodeGeneration"")]
    internal sealed class AutoVersionAttribute : System.Attribute
    {
        public AutoVersionAttribute(string id = null) {}
    }
}
";
    
    private class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<ClassDeclarationSyntax> CandidateTypes { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Any())
            {
                CandidateTypes.Add(classDeclarationSyntax);
            }
        }
    }
    /*
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(i => i.AddSource("AutoVersionAttribute", AttributeCode));
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
    
    private static string? GetAssemblyAttribute(GeneratorExecutionContext context, string name)
    {
        var attribute = context.Compilation.Assembly.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Name == name);
        return (string?)attribute?.ConstructorArguments.Single().Value;
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                return;

            var pluginAttributeType = context.Compilation.GetTypeByMetadataName("BepInEx.AutoVersionAttribute");
            if (pluginAttributeType == null) return;

            foreach (var classDeclarationSyntax in receiver.CandidateTypes)
            {
                var model = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                var typeSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDeclarationSyntax)!;

                AttributeData? attribute = null;

                foreach (var attributeData in typeSymbol.GetAttributes())
                {
                    if (attributeData.AttributeClass == null) continue;

                    if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, pluginAttributeType))
                    {
                        attribute = attributeData;
                    }

                    if (attribute == null)
                    {
                        continue;
                    }

                    var arguments = attribute.ConstructorArguments.Select(x => x.IsNull ? null : (string)x.Value!).ToArray();
                    var id = arguments[0];
                    var name =  GetAssemblyAttribute(context, nameof(AssemblyTitleAttribute)) ?? context.Compilation.AssemblyName;
                    var version =GetAssemblyAttribute(context, nameof(AssemblyInformationalVersionAttribute)) ?? GetAssemblyAttribute(context, nameof(AssemblyVersionAttribute));

                    var attributeName = type switch
                    {
                        AutoType.Plugin => "BepInEx.BepInPlugin",
                        AutoType.Patcher => "BepInEx.Preloader.Core.Patching.PatcherPluginInfo",
                        _ => throw new ArgumentOutOfRangeException(),
                    };

                    var source = SourceText.From($@"// <auto-generated />
namespace {typeSymbol.ContainingNamespace.ToDisplayString()}
{{
    [{attributeName}({typeSymbol.Name}.Id, ""{name}"", ""{version}"")]
    public partial class {typeSymbol.Name}
    {{
        /// <summary>
        /// Id of the <see cref=""{typeSymbol.Name}""/>.
        /// </summary>
        public const string Id = ""{id}"";

        /// <summary>
        /// Gets the name of the <see cref=""{typeSymbol.Name}""/>.
        /// </summary>
        public static string Name => ""{name}"";

        /// <summary>
        /// Gets the version of the <see cref=""{typeSymbol.Name}""/>.
        /// </summary>
        public static string Version => ""{version}"";
    }}
}}
", Encoding.UTF8);

                    context.AddSource($"{typeSymbol.Name}_Auto{type}.cs", source);
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "ERROR",
                        $"An exception was thrown by the {nameof(AutoPluginGenerator)}",
                        $"An exception was thrown by the {nameof(AutoPluginGenerator)} generator: {e.ToString().Replace("\n", ",")}",
                        nameof(AutoPluginGenerator),
                        DiagnosticSeverity.Error,
                        true),
                    Location.None));
            }
        }
    }*/

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.AnalyzerConfigOptionsProvider, (spc, source) =>
        {
        });
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}