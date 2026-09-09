using Microsoft.Extensions.Configuration;

namespace ProductApp.Api.Infrastructure;

public static class DotEnvConfiguration
{
    public static IConfigurationBuilder AddOptionalDotEnv(
        this IConfigurationBuilder configuration,
        string contentRootPath)
    {
        var candidates = new[]
        {
            Path.Combine(contentRootPath, ".env"),
            Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", ".env"))
        };
        var path = candidates.FirstOrDefault(File.Exists);
        if (path is null) return configuration;

        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                line = line[7..].TrimStart();

            var separator = line.IndexOf('=');
            if (separator <= 0) continue;

            var key = line[..separator].Trim();
            if (key.Length == 0 || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
                continue;

            var value = line[(separator + 1)..].Trim();
            if (value.Length >= 2
                && ((value[0] == '"' && value[^1] == '"')
                    || (value[0] == '\'' && value[^1] == '\'')))
                value = value[1..^1];

            values[key] = value;
        }

        return configuration.AddInMemoryCollection(values);
    }
}
