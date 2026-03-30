using IncidentAgentDemo.AgentHost;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IncidentAgentDemo.Tests;

public class PromptAssetsTests : IDisposable
{
    private readonly List<string> _tempDirs = [];

    public void Dispose()
    {
        foreach (var dir in _tempDirs)
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    // === SkillResolver Tests ===

    [Theory]
    [InlineData("Show me open incidents for Payments", "incident-triage")]
    [InlineData("What is the severity of the issue?", "incident-triage")]
    [InlineData("What is the health of the Identity service?", "service-health")]
    [InlineData("Is the service degraded?", "service-health")]
    [InlineData("Summarise the risk for Notifications", "risk-summary")]
    [InlineData("What is the impact assessment?", "risk-summary")]
    [InlineData("Explain the architecture", "demo-runbook")]
    [InlineData("Show me the demo walkthrough", "demo-runbook")]
    public void SkillResolver_MatchesSingleSkill(string input, string expectedSkill)
    {
        var resolver = new SkillResolver();

        var result = resolver.ResolveSkills(input);

        Assert.Contains(expectedSkill, result);
    }

    [Fact]
    public void SkillResolver_MatchesMultipleSkills()
    {
        var resolver = new SkillResolver();

        var result = resolver.ResolveSkills("What is the risk and health of Payments?");

        Assert.Contains("risk-summary", result);
        Assert.Contains("service-health", result);
    }

    [Fact]
    public void SkillResolver_ReturnsEmpty_WhenNoMatch()
    {
        var resolver = new SkillResolver();

        var result = resolver.ResolveSkills("Hello world");

        Assert.Empty(result);
    }

    [Fact]
    public void SkillResolver_IsCaseInsensitive()
    {
        var resolver = new SkillResolver();

        var result = resolver.ResolveSkills("SHOW ME ALL INCIDENTS");

        Assert.Contains("incident-triage", result);
    }

    // === PromptAssetLoader Tests ===

    [Fact]
    public async Task LoadAgentsInstructions_ReturnsContent_WhenFileExists()
    {
        var tempDir = CreateTempDir();
        var agentsPath = Path.Combine(tempDir, "AGENTS.md");
        await File.WriteAllTextAsync(agentsPath, "# Test Agent Instructions\n\nSome rules here.");

        var loader = CreateLoader(agentsPath, tempDir);

        var result = await loader.LoadAgentsInstructionsAsync();

        Assert.NotNull(result);
        Assert.Contains("Test Agent Instructions", result);
    }

    [Fact]
    public async Task LoadAgentsInstructions_ReturnsNull_WhenFileMissing()
    {
        var loader = CreateLoader("/nonexistent/path/AGENTS.md", "/nonexistent/skills");

        var result = await loader.LoadAgentsInstructionsAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task LoadAgentsInstructions_CachesContent()
    {
        var tempDir = CreateTempDir();
        var agentsPath = Path.Combine(tempDir, "AGENTS.md");
        await File.WriteAllTextAsync(agentsPath, "Original content");

        var loader = CreateLoader(agentsPath, tempDir);

        var first = await loader.LoadAgentsInstructionsAsync();
        await File.WriteAllTextAsync(agentsPath, "Modified content");
        var second = await loader.LoadAgentsInstructionsAsync();

        Assert.Equal(first, second);
        Assert.Contains("Original", second);
    }

    [Fact]
    public async Task LoadAgentsInstructions_ReloadsAfterCacheInvalidation()
    {
        var tempDir = CreateTempDir();
        var agentsPath = Path.Combine(tempDir, "AGENTS.md");
        await File.WriteAllTextAsync(agentsPath, "Original content");

        var loader = CreateLoader(agentsPath, tempDir);

        await loader.LoadAgentsInstructionsAsync();
        await File.WriteAllTextAsync(agentsPath, "Updated content");
        loader.InvalidateCache();
        var result = await loader.LoadAgentsInstructionsAsync();

        Assert.Contains("Updated", result);
    }

    // === Front Matter Parsing Tests ===

    [Fact]
    public async Task LoadSkill_ParsesYamlFrontMatter()
    {
        var tempDir = CreateTempDir();
        var skillDir = Path.Combine(tempDir, "my-skill");
        Directory.CreateDirectory(skillDir);
        await File.WriteAllTextAsync(
            Path.Combine(skillDir, "SKILL.md"),
            "---\nname: my-skill\ndescription: A test skill for parsing.\n---\n\n# Skill Content\n\nInstructions here.");

        var loader = CreateLoader("/dummy/AGENTS.md", tempDir);

        var result = await loader.LoadSkillAsync("my-skill");

        Assert.NotNull(result);
        Assert.Equal("my-skill", result.Name);
        Assert.Equal("A test skill for parsing.", result.Description);
        Assert.Contains("Skill Content", result.Content);
        Assert.DoesNotContain("---", result.Content);
    }

    [Fact]
    public void ParseSkillDocument_ParsesBoldMarkdownFormat()
    {
        var content = """
            # Skill: Test Skill

            **Name:** bold-skill  
            **Description:** Parsed from bold markdown.

            ## Instructions
            Do things.
            """;

        var result = PromptAssetLoader.ParseSkillDocument("fallback", "/some/path", content);

        Assert.Equal("bold-skill", result.Name);
        Assert.Equal("Parsed from bold markdown.", result.Description);
    }

    [Fact]
    public void ParseSkillDocument_UsesFolderNameAsFallback()
    {
        var content = "# Just Content\n\nNo front matter or bold patterns here.";

        var result = PromptAssetLoader.ParseSkillDocument("folder-name", "/some/path", content);

        Assert.Equal("folder-name", result.Name);
        Assert.Null(result.Description);
    }

    [Fact]
    public async Task LoadSkill_ReturnsNull_WhenFileMissing()
    {
        var loader = CreateLoader("/dummy", "/nonexistent/skills");

        var result = await loader.LoadSkillAsync("nonexistent-skill");

        Assert.Null(result);
    }

    // === LoadRelevantSkills Tests ===

    [Fact]
    public async Task LoadRelevantSkills_ReturnsEmpty_WhenSkillsDisabled()
    {
        var loader = CreateLoader("/dummy", "/dummy", enableSkills: false);

        var result = await loader.LoadRelevantSkillsAsync("Show me incidents");

        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadRelevantSkills_LoadsMatchingSkills()
    {
        var tempDir = CreateTempDir();
        CreateSkillFile(tempDir, "incident-triage", "---\nname: incident-triage\ndescription: Triage.\n---\n\nTriage content.");
        CreateSkillFile(tempDir, "service-health", "---\nname: service-health\ndescription: Health.\n---\n\nHealth content.");

        var loader = CreateLoader("/dummy/AGENTS.md", tempDir);

        var result = await loader.LoadRelevantSkillsAsync("What is the health status?");

        Assert.Single(result);
        Assert.Equal("service-health", result[0].Name);
    }

    [Fact]
    public async Task LoadRelevantSkills_SkipsMissingSkillFiles()
    {
        var tempDir = CreateTempDir();
        // Only create incident-triage, not service-health
        CreateSkillFile(tempDir, "incident-triage", "Triage content.");

        var loader = CreateLoader("/dummy/AGENTS.md", tempDir);

        // This should match both incident-triage and service-health, but service-health file is missing
        var result = await loader.LoadRelevantSkillsAsync("Show incidents and health status");

        Assert.Single(result);
        Assert.Equal("incident-triage", result[0].Name);
    }

    // === PromptComposer Tests ===

    [Fact]
    public void Compose_IncludesAllSections()
    {
        var composer = new PromptComposer();
        var skills = new List<SkillDocument>
        {
            new() { Name = "test-skill", Description = "A test", FilePath = "/x", Content = "Skill content here" }
        };

        var result = composer.Compose("AGENTS content here", skills);

        Assert.Contains("# Global Agent Instructions", result);
        Assert.Contains("AGENTS content here", result);
        Assert.Contains("# Loaded Skills", result);
        Assert.Contains("## Skill: test-skill", result);
        Assert.Contains("Skill content here", result);
        Assert.Contains("# Runtime Instructions", result);
    }

    [Fact]
    public void Compose_OmitsGlobalSection_WhenAgentsNull()
    {
        var composer = new PromptComposer();

        var result = composer.Compose(null, []);

        Assert.DoesNotContain("# Global Agent Instructions", result);
        Assert.Contains("# Runtime Instructions", result);
    }

    [Fact]
    public void Compose_OmitsSkillsSection_WhenEmpty()
    {
        var composer = new PromptComposer();

        var result = composer.Compose("AGENTS content", []);

        Assert.DoesNotContain("# Loaded Skills", result);
        Assert.Contains("# Global Agent Instructions", result);
        Assert.Contains("# Runtime Instructions", result);
    }

    [Fact]
    public void Compose_IncludesMultipleSkills()
    {
        var composer = new PromptComposer();
        var skills = new List<SkillDocument>
        {
            new() { Name = "skill-a", Description = "First", FilePath = "/a", Content = "Content A" },
            new() { Name = "skill-b", Description = "Second", FilePath = "/b", Content = "Content B" }
        };

        var result = composer.Compose(null, skills);

        Assert.Contains("## Skill: skill-a", result);
        Assert.Contains("## Skill: skill-b", result);
        Assert.Contains("Content A", result);
        Assert.Contains("Content B", result);
    }

    // === Helpers ===

    private string CreateTempDir()
    {
        var path = Path.Combine(Path.GetTempPath(), $"IncidentAgentTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        _tempDirs.Add(path);
        return path;
    }

    private static void CreateSkillFile(string skillsRoot, string skillName, string content)
    {
        var skillDir = Path.Combine(skillsRoot, skillName);
        Directory.CreateDirectory(skillDir);
        File.WriteAllText(Path.Combine(skillDir, "SKILL.md"), content);
    }

    private static PromptAssetLoader CreateLoader(
        string agentsPath, string skillsRoot, bool enableSkills = true)
    {
        var options = Options.Create(new PromptAssetsOptions
        {
            AgentsFilePath = agentsPath,
            SkillsRootPath = skillsRoot,
            EnableSkills = enableSkills
        });
        var resolver = new SkillResolver();
        var logger = NullLogger<PromptAssetLoader>.Instance;
        return new PromptAssetLoader(options, resolver, logger);
    }
}
