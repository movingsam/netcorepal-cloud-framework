using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetCorePal.Extensions.Doc.SourceGenerators.Models;

public class DomainEventHandlerMetaData
{
    public DomainEventHandlerMetaData(INamedTypeSymbol namedTypeSymbol, SemanticModel semanticModel)
    {
        Name = namedTypeSymbol.Name;
        Namespace = namedTypeSymbol.ContainingNamespace.ToDisplayString();
        FullName = namedTypeSymbol.ToDisplayString();
        EventType = namedTypeSymbol.Interfaces.SingleOrDefault(x=>x.Name.Equals("IDomainEventHandler"))?.TypeArguments[0].ToDisplayString() ?? string.Empty;
        CommandTypes = [];
        GenerateCommandTypes(namedTypeSymbol, semanticModel);
    }

    public string FullName { get; }

    public string Name { get; }
    public string Namespace { get; }

    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; }

    /// <summary>
    /// 
    /// </summary>
    public List<string> CommandTypes { get; } = new();

    private void GenerateCommandTypes(INamedTypeSymbol namedTypeSymbol, SemanticModel semanticModel)
    {
        var method = namedTypeSymbol.GetMembers()
            .Where(p => p.Kind == SymbolKind.Method)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(p => p.DeclaredAccessibility == Accessibility.Public && p.Name.Equals("Handle"));


        if (method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
            is not MethodDeclarationSyntax methodSyntax) return;
        // 遍历方法体内的调用表达式
        var invocations = methodSyntax.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();
        foreach (var invocation in invocations)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation.Expression.Parent!);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                var className = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var methodName = methodSymbol.Name;
                if (className.Contains("IMediator") && methodName == "Send")
                {
                    var eventArg = methodSymbol.TypeParameters[0].ToDisplayString();
                    CommandTypes.Add(eventArg);
                }
            }
        }
    }
}