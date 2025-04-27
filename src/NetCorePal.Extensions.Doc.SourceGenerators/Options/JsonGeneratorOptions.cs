using System.IO;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetCorePal.Extensions.Doc.SourceGenerators.Options;

internal class JsonGeneratorOptions(AnalyzerConfigOptionsProvider config, string assemblyName)
{
    public string AssemblyName { get; } = assemblyName;
    public string OutputDir { get; } = GetOutputDir(config);

    private static string GetOutputDir(AnalyzerConfigOptionsProvider config)
    {
        return config.GlobalOptions.TryGetValue("build_property.NetCorePalDocGenerator_OutputDir", out var path)
            ? path
            : config.GlobalOptions.TryGetValue("build_property.projectDir", out var dir)
                ? Path.Combine(dir, "generated-doc-json")
                : "";
    }
}