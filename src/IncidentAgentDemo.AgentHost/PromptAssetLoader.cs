using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IncidentAgentDemo.AgentHost;

public sealed partial class PromptAssetLoader(
    IOptions<PromptAssetsOptions> options,
    ISkillResolver skillResolver,
    ILogger<PromptAssetLoader> logger) : IPromptAssetLoader
{
    private readonly PromptAssetsOptions _options = options.Value;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public async Task<string?> LoadAgentsInstructionsAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue("AGENTS", out var cached))
            return cached;

        var path = Path.GetFullPath(_options.AgentsFilePath);

        if (!File.Exists(path))
        {
            logger.LogWarning("[PromptAssets] AGENTS.md not found at {Path}", path);
            return null;
        }

        var content = await File.ReadAllTextAsync(path, ct);
        _cache.TryAdd("AGENTS", content);

        logger.LogInformation("[PromptAssets] AGENTS.md loaded ({Length} chars)", content.Length);
        return content;
    }

    public async Task<SkillDocument?> LoadSkillAsync(string skillName, CancellationToken ct = default)
    {
        var cacheKey = $"SKILL:{skillName}";

        if (_cache.TryGetValue(cacheKey, out var cached))
            return ParseSkillDocument(skillName, GetSkillFilePath(skillName), cached);

        var skillPath = GetSkillFilePath(skillName);

        if (!File.Exists(skillPath))
        {
            logger.LogWarning("[PromptAssets] SKILL.md not found for '{Skill}' at {Path}", skillName, skillPath);
            return null;
        }

        var rawContent = await File.ReadAllTextAsync(skillPath, ct);
        _cache.TryAdd(cacheKey, rawContent);

        var doc = ParseSkillDocument(skillName, skillPath, rawContent);
        logger.LogInformation("[PromptAssets] Skill '{Skill}' loaded ({Length} chars)", doc.Name, rawContent.Length);
        return doc;
    }

    public async Task<IReadOnlyList<SkillDocument>> LoadRelevantSkillsAsync(
        string userInput, CancellationToken ct = default)
    {
        if (!_options.EnableSkills)
            return [];

        var skillNames = skillResolver.ResolveSkills(userInput);
        var skills = new List<SkillDocument>();

        foreach (var name in skillNames)
        {
            var skill = await LoadSkillAsync(name, ct);
            if (skill is not null)
                skills.Add(skill);
        }

        return skills;
    }

    public void InvalidateCache()
    {
        _cache.Clear();
        logger.LogInformation("[PromptAssets] Cache invalidated");
    }

    private string GetSkillFilePath(string skillName)
        => Path.GetFullPath(Path.Combine(_options.SkillsRootPath, skillName, "SKILL.md"));

    internal static SkillDocument ParseSkillDocument(string fallbackName, string filePath, string rawContent)
    {
        var name = fallbackName;
        string? description = null;
        var content = rawContent;

        // Try YAML-style front matter (--- delimited)
        if (rawContent.StartsWith("---"))
        {
            var endIndex = rawContent.IndexOf("---", 3, StringComparison.Ordinal);
            if (endIndex > 0)
            {
                var frontMatter = rawContent[3..endIndex].Trim();
                content = rawContent[(endIndex + 3)..].TrimStart();

                foreach (var line in frontMatter.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("name:", StringComparison.OrdinalIgnoreCase))
                        name = trimmed["name:".Length..].Trim();
                    else if (trimmed.StartsWith("description:", StringComparison.OrdinalIgnoreCase))
                        description = trimmed["description:".Length..].Trim();
                }
            }
        }
        else
        {
            // Fallback: try bold markdown pattern (**Name:** value)
            var nameMatch = BoldNamePattern().Match(rawContent);
            if (nameMatch.Success)
                name = nameMatch.Groups[1].Value.Trim();

            var descMatch = BoldDescriptionPattern().Match(rawContent);
            if (descMatch.Success)
                description = descMatch.Groups[1].Value.Trim();
        }

        return new SkillDocument
        {
            Name = name,
            Description = description,
            FilePath = filePath,
            Content = content
        };
    }

    [GeneratedRegex(@"\*\*Name:\*\*\s*(.+)")]
    private static partial Regex BoldNamePattern();

    [GeneratedRegex(@"\*\*Description:\*\*\s*(.+)")]
    private static partial Regex BoldDescriptionPattern();
}
