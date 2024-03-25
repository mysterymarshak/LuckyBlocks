using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace LuckyBlocks.SourceGenerators.ExtendedEvents.Data;

[Generator]
public class ExtendedEventsDataGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var extendedEvents = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax,
            static (gsc, _) =>
            {
                var node = (ClassDeclarationSyntax)gsc.Node;
                var typeInfo =
                    (ITypeSymbol)gsc.SemanticModel.GetDeclaredSymbol(node, CancellationToken.None)!;
                return typeInfo.Interfaces.Any(x => x.Name == "IExtendedEvents") ? typeInfo : null;
            }).Where(static x => x is not null);

        var compilationsAndClassDeclarations = context.CompilationProvider.Combine(extendedEvents.Collect());

        context.RegisterSourceOutput(compilationsAndClassDeclarations,
            static (spc, source) => Execute(source.Right!, spc));
    }

    private static void Execute(ImmutableArray<ITypeSymbol> declarations, SourceProductionContext context)
    {
        if (declarations.IsDefaultOrEmpty)
            return;
        
        BuildExtendedEventsData(context);
    }
    
    private static void BuildExtendedEventsData(SourceProductionContext context)
    {
        const string folder = "ExtendedEvents.Data";
        const string file = "ExtendedEvents.Data.sbn-cs";
        const string @namespace = "LuckyBlocks.SourceGenerators.ExtendedEvents.Data";

        var model = new ExtendedEventsDataModel(@namespace);

        var path = Path.Combine(folder, "resources", file);
        var template = Template.Parse(EmbeddedResource.GetContent(path), path);

        AddSource(template, model, file, context);
    }
    
    private static void AddSource(Template template, object model, string templateFileName,
        SourceProductionContext context)
    {
        var output = template.Render(model, member => member.Name);
        context.AddSource(templateFileName.Replace("sbn-cs", "g.cs"), SourceText.From(output, Encoding.UTF8));
    }
}