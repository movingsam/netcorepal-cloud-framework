using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using NetCorePal.Extensions.Doc.SourceGenerators;
using NetCorePal.Extensions.Domain;
using Xunit;

namespace NetCorePal.SourceGenerator.UnitTests;

public class DocGeneratorTests
{
    [Fact]
    public void TestDocGenerator()
    {
        // 模拟输入文件
        var additionalText = new TestAdditionalText("TestEntity.cs", @"
            using NetCorePal.Extensions.Domain;

            namespace TestNamespace;

            public partial record TestEntityId(Int64 Id) : IStronglyTypedId<Int64>;

            public class TestEntity : Entity<TestEntityId>, IAggregateRoot
            {
                public string Name { get; set; } = string.Empty;
                public DateTime CreatedTime { get; set; }
                public DateTime UpdatedTime { get; set; }
            }
        ");

        // 创建语法树
        var syntaxTree = CSharpSyntaxTree.ParseText(additionalText.GetText().ToString());

        // 创建编译对象
        var compilation = CSharpCompilation.Create("TestAssembly",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // 创建生成器
        // var generator = new AggregateRootDocGenerators();
        // CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        ImmutableArray<AdditionalText> additionalTexts = [additionalText];

        // 运行生成器
        // driver = (CSharpGeneratorDriver)driver.AddAdditionalTexts(additionalTexts);
        // driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // 验证生成的代码
        // var generatedCode = outputCompilation.SyntaxTrees.Last().ToString();
        // Assert.Contains("public record AggregateRoot", generatedCode);
    }

    [Fact]
    public void TestDocGeneratorWithRealFile()
    {
        // 获取当前目录
        var directory = Directory.GetCurrentDirectory();
        // 去掉最后的 "bin/Debug/netx.x"
        directory = directory[..directory.LastIndexOf("bin", StringComparison.Ordinal)];
        // 加载实际的 .cs 文件
        string filePath = Path.Combine(directory, "TestEntity.cs"); // 替换为实际文件路径
        string fileContent = File.ReadAllText(filePath);

        // 创建语法树
        var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);

        // 创建编译对象
        var compilation = CSharpCompilation.Create(this.GetType().Assembly.GetName().Name,
            new[] { syntaxTree },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IDomainEventHandler<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IAggregateRoot).Assembly.Location),
            },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


        // 创建生成器
        var generator = new EventHandlerDocGenerators();
        var driver = CSharpGeneratorDriver.Create(generator);

        // 运行生成器
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);


        // 验证生成的代码
        // var generatedCode = outputCompilation.SyntaxTrees.Last().ToString();
        // var output = new TestEntityMDOutput();
        // var markdown = output.MarkdownRender();
        // var generatedCode2 = outputCompilation2.SyntaxTrees.Last().ToString();
        // var render = new TestEntityRender();
        // var json = render.JsonRender();
        // Assert.NotEmpty(json);
    }


    private class TestAdditionalText : AdditionalText
    {
        private readonly string _path;
        private readonly SourceText _text;

        public TestAdditionalText(string path, string text)
        {
            _path = path;
            _text = SourceText.From(text);
        }

        public override string Path => _path;

        public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
    }
}