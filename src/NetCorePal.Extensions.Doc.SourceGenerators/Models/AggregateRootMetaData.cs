using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetCorePal.Extensions.Doc.SourceGenerators.Abstractions;

namespace NetCorePal.Extensions.Doc.SourceGenerators.Models;

public record AggregateRootMetaData(INamedTypeSymbol Symbol,SourceProductionContext Context) : ISgMetaData
{
    [JsonIgnore] private INamedTypeSymbol Symbol { get; } = Symbol;
    [JsonIgnore] private SourceProductionContext Context { get; } = Context;

    /// <summary>
    /// 全部名称
    /// </summary>
    public string FullName => Symbol.ToDisplayString();

    /// <summary>
    /// 命名空间
    /// </summary>
    public string Namespace => Symbol.ContainingNamespace.ToDisplayString();

    /// <summary>
    /// 名称
    /// </summary>
    public string Name => Symbol.Name;

    /// <summary>
    /// 所有属性
    /// </summary>
    public PropertyMetaData[] Properties =>
        Symbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Property)
            .OfType<IPropertySymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public)
            .Select(m => new PropertyMetaData(m))
            .ToArray();

    /// <summary>
    /// 所有方法
    /// </summary>
    public MethodMetaData[] Methods =>
        Symbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Method)
            .OfType<IMethodSymbol>()
            .Where(m =>
                m.DeclaredAccessibility == Accessibility.Public &&
                m.MethodKind != MethodKind.PropertyGet &&
                m.MethodKind != MethodKind.PropertySet &&
                m.MethodKind != MethodKind.Constructor)
            .Select(m => new MethodMetaData(m,Context))
            .ToArray();
}

public record PropertyMetaData
{
    public PropertyMetaData(IPropertySymbol symbol)
    {
        this.FullName = symbol.ToDisplayString();
        this.Name = symbol.Name;
        this.Type = symbol.Type.ToDisplayString();
    }

    public string FullName { get; }
    public string Name { get; }
    public string Type { get; }
}

public class MethodMetaData
{
    public MethodMetaData(IMethodSymbol symbol,SourceProductionContext context)
    {
        this.Name = symbol.MetadataName;
        this.ReturnType = symbol.ReturnType.ToDisplayString();
        this.Parameters = symbol.Parameters
            .Select(p => p.Type.ToDisplayString())
            .ToArray();
        this.HasDomainEvent = CheckDomainEvent(symbol,context);
    }

    /// <summary>
    /// 通过语法树检查是否是领域事件
    /// 便利每一行代码 看下是否拥有AddDomainEvent这个函数的调用
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private bool CheckDomainEvent(IMethodSymbol symbol,SourceProductionContext context)
    {
        var invocations = symbol.DeclaringSyntaxReferences
            .FirstOrDefault()?.GetSyntax()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>();
        if (invocations == null) return false;
        HashSet<string> domainEvents = [];
        // 遍历方法体内的调用表达式
        foreach (var invocation in invocations)
        {
            if (invocation.Expression is IdentifierNameSyntax { Identifier.ValueText: "AddDomainEvent" })
            {
                var domainEvent = invocation.ArgumentList.Arguments
                    .Select(p => p)
                    .FirstOrDefault();
                domainEvents.Add(domainEvent?.ToString() ?? string.Empty);
            }
        }
        if (!domainEvents.Any()) return false;
        this.DomainEvents = domainEvents.ToArray();
        return true;

    }


    public string Name { get; }
    public string ReturnType { get; }
    public string[] Parameters { get; }
    public string[]? DomainEvents { get; private set; } 
    public bool HasDomainEvent { get; }
}