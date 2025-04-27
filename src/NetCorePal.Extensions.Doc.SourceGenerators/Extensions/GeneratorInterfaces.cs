using System.Linq;
using Microsoft.CodeAnalysis;

namespace NetCorePal.Extensions.Doc.SourceGenerators.Extensions;

internal static class GeneratorInterfaces
{
    
    public static bool IsPartial(this INamedTypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility == Accessibility.Public &&
               symbol.IsPartial() &&
               symbol.TypeKind == TypeKind.Class;
    }
    
    
    public static bool IsAggregateRoot(this INamedTypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility == Accessibility.Public &&
               symbol.TypeKind == TypeKind.Class &&
               symbol.Interfaces.Any(x => x.Name.Equals("IAggregateRoot"));
    }
    
}