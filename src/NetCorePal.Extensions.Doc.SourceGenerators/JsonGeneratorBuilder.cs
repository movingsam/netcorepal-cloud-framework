
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using NetCorePal.Extensions.Doc.SourceGenerators.Abstractions;
using NetCorePal.Extensions.Doc.SourceGenerators.Options;

namespace NetCorePal.Extensions.Doc.SourceGenerators;

internal class JsonGeneratorBuilder(
    INamedTypeSymbol symbol,
    JsonGeneratorOptions options
    )
{
    private JsonGeneratorOptions Options { get; } = options;
    private INamedTypeSymbol Symbol { get; } = symbol;

    private static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        WriteIndented = true,
        IncludeFields = true
        , PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    public void WriteJson<T>(T metaData) where T: ISgMetaData
    {
        var json = JsonSerializer.Serialize(metaData, JsonSerializerOptions);
        var fileName = metaData.Name + ".json";
        var file = GetOutputFilePath(Symbol, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(file) ?? string.Empty);
        File.WriteAllText(file, json);
    }
    
    private string GetOutputFilePath(ITypeSymbol symbol,string fileName)
    {
        return Path.Combine([Options.OutputDir,
            symbol.ContainingAssembly.Name,
            .. symbol.ContainingNamespace.ToString().Replace("<", "").Replace(">", "").Split('.'),
            fileName]);
    }
}