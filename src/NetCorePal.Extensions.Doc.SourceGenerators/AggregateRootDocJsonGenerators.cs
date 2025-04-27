using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetCorePal.Extensions.Doc.SourceGenerators.Extensions;
using NetCorePal.Extensions.Doc.SourceGenerators.Models;
using NetCorePal.Extensions.Doc.SourceGenerators.Options;

namespace NetCorePal.Extensions.Doc.SourceGenerators;

public partial class DocJsonGenerators
{
    private static void RegisterAggregateRootDocJsonGenerator(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider
            .Select((compilation, _) => compilation.AssemblyName);

        var options = context.AnalyzerConfigOptionsProvider
            .Combine(assemblyName)
            .Select(static (config, _) => new JsonGeneratorOptions(config.Left, config.Right ?? ""))
            .Combine(context.ParseOptionsProvider
                .Select(static (options, _) => options.PreprocessorSymbolNames));

        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is BaseTypeDeclarationSyntax,
                transform: static (context, _) => context.SemanticModel.GetDeclaredSymbol(context.Node))
            .Collect();

        var generateSource = options.Combine(typeDeclarations);

        context.RegisterImplementationSourceOutput(generateSource, static (context, source) =>
        {
            var ((options, preprocessors), targetSymbols) = source;

            var symbols = targetSymbols
                .OfType<INamedTypeSymbol>()
                .Where(predicate: GeneratorInterfaces.IsAggregateRoot);

            foreach (var symbol in symbols)
            {
                var aggregateRoot = new AggregateRootMetaData(symbol, context);
                context.CancellationToken.ThrowIfCancellationRequested();
                var builder = new JsonGeneratorBuilder(symbol, options);
                builder.WriteJson(aggregateRoot);
                context.CancellationToken.ThrowIfCancellationRequested();
            }
        });
    }
}