using DirectoryService.Infrastructure.Identity.Configuration;

public sealed class DatabaseConfig
{
    public const string HostNameKey = "DB_HOST";
    public const string PortKey = "DB_PORT";
    public const string UserNameKey = "DB_USERNAME";
    public const string PasswordKey = "DB_PASSWORD";
    public const string DatabaseNameKey = "DB_DATABASE";
    public string HostName { get; }
    public int Port { get; }
    public string UserName { get; }
    public string Password { get; }
    public string DatabaseName { get; }

    public DatabaseConfig(string hostName, int port, string userName, string password, string databaseName)
    {
        HostName = hostName;
        Port = port;
        UserName = userName;
        Password = password;
        DatabaseName = databaseName;
    }
}

public sealed class SeqConfig
{
    public const string HostKey = "SEQ_HOST";
    public string Host { get; }
    public SeqConfig(string host)
    {
        Host = host;
    }
}

public sealed class SeedConfig
{
    public const string Key = "USE_SEED";
    public bool UseSeed { get; }
    public SeedConfig(bool useSeed)
    {
        UseSeed = useSeed;
    }
}

public sealed class ApplicationConfig
{
    public DatabaseConfig Database { get; }
    public SeqConfig Seq { get; }

    public SeedConfig Seed { get; }

    public IdentityConfig Identity { get; }

    private ApplicationConfig(
        DatabaseConfig database,
        SeqConfig seq,
        SeedConfig seed,
        IdentityConfig identity)
    {
        Database = database;
        Seq = seq;
        Seed = seed;
        Identity = identity;
    }

    public static ApplicationConfig CreateFromEnvironment()
    {
        string? hostName = Environment.GetEnvironmentVariable(DatabaseConfig.HostNameKey);
        if (string.IsNullOrWhiteSpace(hostName))
        {
            throw new ApplicationException(string.Format("{0} is not specified in environment variables", DatabaseConfig.HostNameKey));
        }

        int port = int.Parse(Environment.GetEnvironmentVariable(DatabaseConfig.PortKey) ?? "");
        if (port == 0)
        {
            throw new ApplicationException(string.Format("{0} is not specified in environment variables", DatabaseConfig.PortKey));
        }

        string? userName = Environment.GetEnvironmentVariable(DatabaseConfig.UserNameKey);
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ApplicationException(string.Format("{0} is not specified in environment variables", DatabaseConfig.UserNameKey));
        }

        string? password = Environment.GetEnvironmentVariable(DatabaseConfig.PasswordKey);
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ApplicationException(string.Format("{0} is not specified in environment variables", DatabaseConfig.PasswordKey));
        }

        string? databaseName = Environment.GetEnvironmentVariable(DatabaseConfig.DatabaseNameKey);
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ApplicationException(string.Format("{0} is not specified in environment variables", DatabaseConfig.DatabaseNameKey));
        }

        string? seqHost = Environment.GetEnvironmentVariable(SeqConfig.HostKey);
        if (string.IsNullOrWhiteSpace(seqHost))
        {
            throw new ApplicationException(string.Format("{0} is not specified in environment variables", SeqConfig.HostKey));
        }

        bool useSeed = Environment.GetEnvironmentVariable(SeedConfig.Key)?.ToLower() == "true";

        IdentityConfig identity = IdentityConfig.CreateFromEnvironment();

        DatabaseConfig config = new(hostName, port, userName, password, databaseName);
        SeqConfig seq = new(seqHost);
        SeedConfig seed = new(useSeed);
        return new ApplicationConfig(config, seq, seed, identity);
    }

    public static ApplicationConfig CreateFromEnvFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ApplicationException(string.Format(".env file path is not specified. Specified: {0}", path));
        }

        Dictionary<string, string> configuration = CreateConfigurationMap(path);
        DatabaseConfig database = CreateDbConfiguration(configuration);
        SeqConfig seq = CreateSeqConfiguration(configuration);
        SeedConfig seed = CreateSeedConfiguration(configuration);
        IdentityConfig identity = CreateIdentityConfiguration(configuration);
        return new ApplicationConfig(database, seq, seed, identity);
    }

    private static SeedConfig CreateSeedConfiguration(Dictionary<string, string> configuration)
    {
        bool useSeed = configuration.ContainsKey(SeedConfig.Key) && configuration[SeedConfig.Key].ToLower() == "true";
        return new SeedConfig(useSeed);
    }

    private static IdentityConfig CreateIdentityConfiguration(Dictionary<string, string> configuration)
    {
        if (!configuration.ContainsKey(IdentityConfig.JwtHashKeyKey))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", IdentityConfig.JwtHashKeyKey));
        }

        if (!configuration.ContainsKey(IdentityConfig.IdentityDbConnectionStringKey))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", IdentityConfig.IdentityDbConnectionStringKey));
        }

        string jwtHashkey = configuration[IdentityConfig.JwtHashKeyKey];
        if (string.IsNullOrWhiteSpace(jwtHashkey))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", IdentityConfig.JwtHashKeyKey));
        }

        string identityDbConnectionString = configuration[IdentityConfig.IdentityDbConnectionStringKey];
        if (string.IsNullOrWhiteSpace(identityDbConnectionString))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", IdentityConfig.IdentityDbConnectionStringKey));
        }

        return new IdentityConfig(jwtHashkey, identityDbConnectionString);
    }

    private static SeqConfig CreateSeqConfiguration(Dictionary<string, string> configuration)
    {
        if (!configuration.ContainsKey(SeqConfig.HostKey))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", SeqConfig.HostKey));
        }

        string seqHost = configuration[SeqConfig.HostKey];
        if (string.IsNullOrWhiteSpace(seqHost))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", SeqConfig.HostKey));
        }

        return new SeqConfig(seqHost);
    }

    private static DatabaseConfig CreateDbConfiguration(Dictionary<string, string> configuration)
    {
        if (!configuration.ContainsKey(DatabaseConfig.HostNameKey))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", DatabaseConfig.HostNameKey));
        }

        if (!configuration.ContainsKey(DatabaseConfig.UserNameKey))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", DatabaseConfig.UserNameKey));
        }

        if (!configuration.ContainsKey(DatabaseConfig.PasswordKey))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", DatabaseConfig.PasswordKey));
        }

        if (!configuration.ContainsKey(DatabaseConfig.PortKey))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", DatabaseConfig.PortKey));
        }

        string host = configuration[DatabaseConfig.HostNameKey];
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", DatabaseConfig.HostNameKey));
        }

        int port = int.Parse(configuration[DatabaseConfig.PortKey]);
        if (port <= 0)
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", DatabaseConfig.PortKey));
        }

        string username = configuration[DatabaseConfig.UserNameKey];
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", DatabaseConfig.UserNameKey));
        }

        string databaseName = configuration[DatabaseConfig.DatabaseNameKey];
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", DatabaseConfig.DatabaseNameKey));
        }

        string password = configuration[DatabaseConfig.PasswordKey];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ApplicationException(string.Format("{0} is not specified in .env file", DatabaseConfig.PasswordKey));
        }
        return new DatabaseConfig(host, port, username, password, databaseName);
    }

    private static Dictionary<string, string> CreateConfigurationMap(string path)
    {
        Dictionary<string, string> configuration = new Dictionary<string, string>();
        int linesRead = 1;
        using (FileStream stream = new FileStream(path, FileMode.Open))
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    linesRead += 1;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        throw new ApplicationException(string.Format(
                            ".env file: {0}. Invalid string at line: {1}",
                            path,
                            linesRead
                        ));
                    }

                    ReadOnlySpan<char> span = line.AsSpan();
                    int index = span.IndexOf('=');
                    if (index == -1)
                    {
                        throw new ApplicationException(string.Format(
                            ".env file: {0}. Invalid string at line: {1}",
                            path,
                            linesRead
                        ));
                    }

                    string key = span[0..index].ToString();
                    if (configuration.ContainsKey(key))
                    {
                        throw new ApplicationException(string.Format(
                            ".env file: {0}. Duplicate key at line: {1}",
                            path,
                            linesRead
                        ));
                    }

                    string value = span[(index + 1)..span.Length].ToString();
                    configuration[key] = value;
                }
            }
        }

        return configuration;
    }
}
