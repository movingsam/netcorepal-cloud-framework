using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetCorePal.Extensions.Doc.SourceGenerators;

[Generator]
public class EventHandlerDocGenerators : IIncrementalGenerator
{
    public static readonly ConditionalWeakTable<string,DomainEventHandlerGraphModel> DomainEventGraphModels =
        new ConditionalWeakTable<string, DomainEventHandlerGraphModel>();
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if Debug
        Debugger.Launch();
#endif
        var syntaxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is TypeDeclarationSyntax,
                transform: (syntaxContext, _) => (TypeDeclarationSyntax)syntaxContext.Node)
            .Where(tds => tds != null);

        var compilationAndTypes = context.CompilationProvider.Combine(syntaxProvider.Collect());
        context.RegisterSourceOutput(compilationAndTypes, (spc, source) =>
        {
            var (compilation, typeDeclarations) = source;
            foreach (var tds in typeDeclarations)
            {
                var semanticModel = compilation.GetSemanticModel(tds.SyntaxTree);
                GeneratorDomainEventHandlerGraph(spc, semanticModel, tds);
            }
        });
    }
    
    private static void GeneratorDomainEventHandlerGraph(SourceProductionContext spc,
        SemanticModel semanticModel, TypeDeclarationSyntax tds)
    {
        var symbol = semanticModel.GetDeclaredSymbol(tds);
        if (symbol is not INamedTypeSymbol namedTypeSymbol) return;
        var domainEventHandler = namedTypeSymbol.Interfaces
            .SingleOrDefault(t => t.Name.StartsWith("IDomainEventHandler"));
        if (domainEventHandler == null) return;
        // 聚合根名称获取
        var domainEventHandlerName =  namedTypeSymbol.Name;
        // 聚合根属性获取
        var domainEventHandlerProperties = namedTypeSymbol.GetMembers()
            .Where(p => p.Kind == SymbolKind.Method)
            .OfType<IMethodSymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && p.Name.Equals("Handle"))
            .Select(p => new DomainEventHandlerGraphModel(p)).ToArray();
        foreach (var domainEventHandlerItem in domainEventHandlerProperties)
        {
            if (!DomainEventGraphModels.TryGetValue(domainEventHandlerItem.EventType,
                    out _))
            {
                DomainEventGraphModels.Add(domainEventHandlerName,domainEventHandlerItem);
                continue;
            }
            DomainEventGraphModels.Remove(domainEventHandlerItem.EventType);
            DomainEventGraphModels.Add(domainEventHandlerName,domainEventHandlerItem);

        }
    }
}