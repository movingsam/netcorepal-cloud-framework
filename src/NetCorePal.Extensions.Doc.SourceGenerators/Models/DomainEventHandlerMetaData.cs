using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetCorePal.Extensions.Doc.SourceGenerators.Abstractions;

namespace NetCorePal.Extensions.Doc.SourceGenerators.Models;

public class DomainEventHandlerMetaData:ISgMetaData
{
    public DomainEventHandlerMetaData(INamedTypeSymbol namedTypeSymbol, SemanticModel semanticModel)
    {
        Name = namedTypeSymbol.Name;
        Namespace = namedTypeSymbol.ContainingNamespace.ToDisplayString();
        FullName = namedTypeSymbol.ToDisplayString();
        EventType = namedTypeSymbol.Interfaces.SingleOrDefault(x => x.Name.Equals("IDomainEventHandler"))
            ?.TypeArguments[0].ToDisplayString() ?? string.Empty;
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
            // 检查是否是成员访问表达式（例如：_mediator.Send）
            if (invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Send" })
                // 获取成员访问的父级类型
            {
                var arguments = invocation.ArgumentList.Arguments;
                foreach (var arugment in arguments)
                {
                    var argumentType = semanticModel.GetTypeInfo(arugment.Expression);
                    if (argumentType.Type == null) continue;
                    CommandTypes.Add(argumentType.Type!.ToDisplayString());
                }
            }
        }
    }
}