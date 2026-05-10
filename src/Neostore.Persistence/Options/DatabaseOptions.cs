namespace Neostore.Persistence.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string ToConnectionString() =>
        $"Server={Server};Port={Port};Database={Database};User={User};Password={Password};";
}
