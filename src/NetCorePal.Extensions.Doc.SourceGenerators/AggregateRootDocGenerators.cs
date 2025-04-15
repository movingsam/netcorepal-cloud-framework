using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NetCorePal.Extensions.Doc.SourceGenerators;

[Generator]
public class AggregateRootDocGenerators : IIncrementalGenerator
{
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
                GenerateAggregateRootDoc(spc, semanticModel, tds);
            }
        });
    }

    private static void GenerateAggregateRootDoc(SourceProductionContext spc,
        SemanticModel semanticModel, TypeDeclarationSyntax tds)
    {
        var symbol = semanticModel.GetDeclaredSymbol(tds);
        if (symbol is not INamedTypeSymbol namedTypeSymbol) return;

        var aggregateRoot = namedTypeSymbol.Interfaces
            .SingleOrDefault(t => t.Name.Equals("IAggregateRoot"));
        if (aggregateRoot == null) return;
        // 聚合根名称获取
        var aggregateRootName = namedTypeSymbol.Name;
        // 聚合根命名空间获取
        var aggregateRootNamespace = namedTypeSymbol.ContainingNamespace.ToDisplayString();
        // 聚合根属性获取
        var aggregateRootProperties = namedTypeSymbol.GetMembers()
            .Where(p => p.Kind == SymbolKind.Property)
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public)
            .Select(p => new AggregateRootPropGraphModel(p.Name, p.Type.Name.ToString())).ToArray();


        // 聚合根方法获取
        var aggregateRootMethods = namedTypeSymbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Method)
            .OfType<IMethodSymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public &&
                        m.MethodKind != MethodKind.PropertyGet &&
                        m.MethodKind != MethodKind.PropertySet &&
                        m.MethodKind != MethodKind.Constructor)
            .Select(m => new AggregateRootMethodGraphModel(m))
            .ToArray();

        // 生成Markdown输出模型
        var aggregateRootDto = new AggregateRootGraphModel(aggregateRootName,
            aggregateRootNamespace, aggregateRootProperties,
            aggregateRootMethods);
        foreach (var domainEvent in aggregateRootDto
                     .Methods.Where(x =>
                        !string.IsNullOrWhiteSpace(x.DomainEvent))
                     .Select(x => x.DomainEvent!))
        {
            var refHandler = EventHandlerDocGenerators.DomainEventGraphModels.TryGetValue(
                domainEvent,
                out var eventHandlerGraphModel);
            aggregateRootDto.AddDomainEventHandler(
                eventHandlerGraphModel?.Name ?? string.Empty,
                eventHandlerGraphModel?.Namespace ?? string.Empty);
        }

     
        spc.AddSource($"{aggregateRootDto.Name}MDOutput.g.cs",
            SourceText.From(aggregateRootDto.ToString(), Encoding.UTF8));
    }
}