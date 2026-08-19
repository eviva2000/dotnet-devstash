using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DevStash.Api.Data;

public static class DevStashDbContextOptions
{
    public static void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        var normalizedConnectionString = NormalizeConnectionString(connectionString);

        options.UseNpgsql(
            normalizedConnectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable(
                    DevStashDbContext.MigrationsHistoryTable,
                    DevStashDbContext.Schema);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });
    }

    public static string NormalizeConnectionString(string connectionString)
    {
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
            || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return connectionString;
        }

        var credentials = uri.UserInfo.Split(':', 2);
        if (credentials.Length != 2)
        {
            throw new InvalidOperationException(
                "The PostgreSQL connection URI must contain both a username and password.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1])
        };

        foreach (var option in ParseQuery(uri.Query))
        {
            switch (option.Key.ToLowerInvariant())
            {
                case "sslmode":
                    builder.SslMode = Enum.Parse<SslMode>(option.Value, ignoreCase: true);
                    break;
                case "channel_binding":
                    builder.ChannelBinding = Enum.Parse<ChannelBinding>(option.Value, ignoreCase: true);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported PostgreSQL connection URI option '{option.Key}'.");
            }
        }

        return builder.ConnectionString;
    }

    private static IEnumerable<KeyValuePair<string, string>> ParseQuery(string query)
    {
        foreach (var segment in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = segment.IndexOf('=');
            if (separatorIndex < 1)
            {
                throw new InvalidOperationException("The PostgreSQL connection URI contains an invalid option.");
            }

            yield return new KeyValuePair<string, string>(
                Uri.UnescapeDataString(segment[..separatorIndex]),
                Uri.UnescapeDataString(segment[(separatorIndex + 1)..]));
        }
    }
}
