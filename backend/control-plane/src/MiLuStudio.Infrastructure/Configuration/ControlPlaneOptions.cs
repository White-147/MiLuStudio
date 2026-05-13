namespace MiLuStudio.Infrastructure.Configuration;

public sealed class ControlPlaneOptions
{
    public const string SectionName = "ControlPlane";

    public string RepositoryProvider { get; set; } = "PostgreSQL";

    public string MigrationsPath { get; set; } = "backend/control-plane/db/migrations";

    public string StorageRoot { get; set; } = "D:\\code\\MiLuStudio\\storage";

    public string WorkerId { get; set; } = Environment.MachineName;

    public string PythonExecutablePath { get; set; } = "D:\\soft\\program\\Python\\Python313\\python.exe";

    public string PythonSkillsRoot { get; set; } = "D:\\code\\MiLuStudio\\backend\\sidecars\\python-skills";

    public string SkillRunTempRoot { get; set; } = "D:\\code\\MiLuStudio\\.tmp\\skill-runs";

    public int SkillRunTimeoutSeconds { get; set; } = 120;

    public int SkillRunRetentionCount { get; set; } = 30;

    public string AuthTestActivationCode { get; set; } = "MILU-STAGE16-TEST";

    public int AuthLicenseValidDays { get; set; } = 30;

    public int AuthMaxDevices { get; set; } = 2;
}
