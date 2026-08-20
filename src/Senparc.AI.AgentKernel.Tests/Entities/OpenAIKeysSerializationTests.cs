using System.Text.Json;
using Senparc.CO2NET.Extensions;

namespace Senparc.AI.AgentKernel.Tests.Entities;

[TestClass]
public class OpenAIKeysSerializationTests
{
    private static readonly string[] ProductionProjectDirectories =
    [
        "Senparc.AI",
        "Senparc.AI.Kernel",
        "Senparc.AI.Agents",
        "Senparc.AI.AgentKernel",
        "Senparc.AI.AgentKernel.Providers.FastAPI",
        "Senparc.AI.AgentKernel.Providers.HuggingFace"
    ];

    [TestMethod]
    public void SenparcAiAssembly_ShouldNotReferenceNewtonsoftJson()
    {
        var referencedAssemblyNames = typeof(OpenAIKeys).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        CollectionAssert.DoesNotContain(referencedAssemblyNames, "Newtonsoft.Json");
    }

    [TestMethod]
    public void ProductionDependencyGraphs_ShouldNotContainNewtonsoftJson()
    {
        var repositoryDirectory = FindRepositoryDirectory();
        var projectsWithNewtonsoftJson = new List<string>();

        foreach (var projectDirectory in ProductionProjectDirectories)
        {
            var assetsPath = Path.Combine(
                repositoryDirectory.FullName,
                "src",
                projectDirectory,
                "obj",
                "project.assets.json");

            Assert.IsTrue(File.Exists(assetsPath), $"Restore output was not found: {assetsPath}");

            using var assets = JsonDocument.Parse(File.ReadAllText(assetsPath));
            var hasNewtonsoftJson = assets.RootElement
                .GetProperty("libraries")
                .EnumerateObject()
                .Any(library => library.Name.StartsWith("Newtonsoft.Json/", StringComparison.OrdinalIgnoreCase));

            if (hasNewtonsoftJson)
            {
                projectsWithNewtonsoftJson.Add(projectDirectory);
            }
        }

        Assert.AreEqual(
            0,
            projectsWithNewtonsoftJson.Count,
            $"Production dependency graphs contain Newtonsoft.Json: {string.Join(", ", projectsWithNewtonsoftJson)}");
    }

    [TestMethod]
    public void CO2NetToJson_ShouldOmitOnlyNullOpenAIEndpoint()
    {
        var keys = new OpenAIKeys
        {
            ApiKey = "test-key",
            OrganizationId = "test-organization"
        };

        var jsonWithoutEndpoint = keys.ToJson();
        using var documentWithoutEndpoint = JsonDocument.Parse(jsonWithoutEndpoint);
        Assert.IsFalse(documentWithoutEndpoint.RootElement.TryGetProperty(nameof(OpenAIKeys.OpenAIEndpoint), out _));

        keys.OpenAIEndpoint = "https://api.example.com";

        var jsonWithEndpoint = keys.ToJson();
        using var documentWithEndpoint = JsonDocument.Parse(jsonWithEndpoint);
        Assert.AreEqual(
            keys.OpenAIEndpoint,
            documentWithEndpoint.RootElement.GetProperty(nameof(OpenAIKeys.OpenAIEndpoint)).GetString());
    }

    private static DirectoryInfo FindRepositoryDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "Senparc.AI.sln")))
            {
                return directory;
            }

            directory = directory.Parent;
        }

        Assert.Fail($"Could not locate the repository from {AppContext.BaseDirectory}.");
        throw new InvalidOperationException();
    }
}
