using System.Linq;
using Microsoft.CodeAnalysis;

namespace NetCorePal.Extensions.Doc.SourceGenerators;

public class DomainEventHandlerGraphModel
{
    public DomainEventHandlerGraphModel(IMethodSymbol methodSymbol)
    {
        EventName = methodSymbol.Parameters.FirstOrDefault(x => x.Name.EndsWith("Event"))?.Name ?? string.Empty;
        Namespace = methodSymbol.ContainingNamespace.ToDisplayString();
        EventType = methodSymbol.Parameters.FirstOrDefault(x => x.Name.EndsWith("Event"))?.Type?.Name ?? string.Empty;
    }

    public string EventName { get; }
    public string Namespace { get; }
    public string EventType { get; }
}