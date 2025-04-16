using System.Linq;
using Microsoft.CodeAnalysis;

namespace NetCorePal.Extensions.Doc.SourceGenerators.Models;

public class AggregateRootMetaData(INamedTypeSymbol symbol)
{
    /// <summary>
    /// 全部名称
    /// </summary>
    public string FullName => symbol.ToDisplayString();

    /// <summary>
    /// 命名空间
    /// </summary>
    public string Namespace => symbol.ContainingNamespace.ToDisplayString();

    /// <summary>
    /// 名称
    /// </summary>
    public string Name => symbol.Name;

    /// <summary>
    /// 所有属性
    /// </summary>
    public PropertyMetaData[] Properties =>
        symbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Property)
            .OfType<IPropertySymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public)
            .Select(m => new PropertyMetaData(
                m.Name,
                m.Type.ToDisplayString()))
            .ToArray();

    /// <summary>
    /// 所有方法
    /// </summary>
    public MethodMetaData[] Methods =>
        symbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Method)
            .OfType<IMethodSymbol>()
            .Where(m =>
                m.DeclaredAccessibility == Accessibility.Public &&
                m.MethodKind != MethodKind.PropertyGet &&
                m.MethodKind != MethodKind.PropertySet &&
                m.MethodKind != MethodKind.Constructor)
            .Select(m => new MethodMetaData(
                m.Name,
                m.ReturnType.ToDisplayString(),
                m.Parameters.Select(p => p.Type.ToDisplayString()).ToArray()))
            .ToArray();
}

public class PropertyMetaData(
    string name,
    string type)
{
    public string Name { get; } = name;
    public string Type { get; } = type;
}

public class MethodMetaData(
    string name,
    string returnType,
    string[] parameters)
{
    public string Name { get; } = name;
    public string ReturnType { get; } = returnType;
    public string[] Parameters { get; } = parameters;
}