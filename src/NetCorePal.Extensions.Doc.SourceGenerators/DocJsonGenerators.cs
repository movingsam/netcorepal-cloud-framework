using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NetCorePal.Extensions.Doc.SourceGenerators.Models;

namespace NetCorePal.Extensions.Doc.SourceGenerators;

[Generator(LanguageNames.CSharp)]
public partial class DocJsonGenerators : IIncrementalGenerator
{ 

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if Debug
        Debugger.Launch();
#endif
        RegisterAggregateRootDocJsonGenerator(context);
    }

   
}