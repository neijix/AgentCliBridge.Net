using System.Text.RegularExpressions;

namespace D3sk.AgentCli.Specs.Claude;

/// <summary>A composer's Effort and Ultracode choices as Claude Code's own flags, per model.</summary>
public static partial class ClaudeEffort
{
    /// <summary>Levels Claude takes, weakest first.</summary>
    public static IReadOnlyList<string> Levels { get; } = ["low", "medium", "high", "xhigh", "max"];

    /// <summary>The <c>--settings</c> value that turns Ultracode on.</summary>
    public const string UltracodeSettings = "{\"ultracode\":true}";

    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["opus"] = "claude-opus-5-5",
        ["sonnet"] = "claude-sonnet-5-5",
        ["haiku"] = "claude-haiku-5-5",
        ["fable"] = "claude-fable-5-1",
    };

    private static readonly Dictionary<string, string[]> Caps = new()
    {
        ["claude-opus-4-5"] = ["low", "medium", "high"],
        ["claude-opus-4-6"] = ["low", "medium", "high", "max"],
        ["claude-sonnet-4-6"] = ["low", "medium", "high", "max"],
    };

    [GeneratedRegex(@"^claude-(3-|haiku-4|sonnet-4(-[05])?$|opus-4(-[01])?$|mythos-5$)")]
    private static partial Regex NoEffort();

    [GeneratedRegex(@"^(?:[a-z]+\.)?anthropic\.")]
    private static partial Regex Provider();

    [GeneratedRegex(@"-v\d+(?::\d+)?$")]
    private static partial Regex Version();

    [GeneratedRegex(@"[-@]\d{8}$")]
    private static partial Regex Date();

    /// <summary>The table's id for a model: lower case, without 1M tag, provider prefix, version or date.</summary>
    public static string ModelKey(string? model)
    {
        var text = (model ?? "").Trim().ToLowerInvariant();
        if (text.EndsWith("[1m]", StringComparison.Ordinal)) text = text[..^4];
        text = text[(text.LastIndexOf('/') + 1)..];
        text = Provider().Replace(text, "");
        text = Version().Replace(text, "");
        text = Date().Replace(text, "");
        return Aliases.GetValueOrDefault(text, text);
    }

    /// <summary>The levels this model takes, or null when it takes no effort at all.</summary>
    public static IReadOnlyList<string>? LevelsFor(string? model)
    {
        var key = ModelKey(model);
        if (NoEffort().IsMatch(key)) return null;
        return Caps.TryGetValue(key, out var caps) ? caps : Levels;
    }

    /// <summary>The effort when the model takes it, else the strongest level it takes below it.</summary>
    public static string Clamp(IReadOnlyList<string> levels, string effort)
    {
        if (levels.Contains(effort)) return effort;
        var rank = Rank(effort);
        var below = levels.Where(l => Rank(l) <= rank).ToList();
        return below.Count > 0 ? below[^1] : levels[0];
    }

    /// <summary>The extra argv for this turn; only a known level reaches it, and Haiku never gets Ultracode.</summary>
    public static List<string> Args(string? effort, bool ultracode, string? model)
    {
        var args = new List<string>();
        var levels = LevelsFor(model);
        if (levels is null) return args;
        if (effort is not null && Levels.Contains(effort)) args.AddRange(["--effort", Clamp(levels, effort)]);
        if (ultracode && !ModelKey(model).Contains("haiku", StringComparison.Ordinal))
            args.AddRange(["--settings", UltracodeSettings]);
        return args;
    }

    private static int Rank(string level) => ((List<string>)[.. Levels]).IndexOf(level);
}
