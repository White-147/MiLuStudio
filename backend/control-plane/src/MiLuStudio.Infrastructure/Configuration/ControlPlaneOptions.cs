namespace MiLuStudio.Infrastructure.Configuration;

public sealed class ControlPlaneOptions
{
    public const string SectionName = "ControlPlane";

    public string RepositoryProvider { get; set; } = RepositoryProviderNames.Sqlite;

    public string MigrationsPath { get; set; } = "backend/control-plane/db/sqlite";

    public string StorageRoot { get; set; } = "D:\\code\\MiLuStudio\\storage";

    public string UploadsRoot { get; set; } = "D:\\code\\MiLuStudio\\uploads";

    public string FfmpegBinPath { get; set; } = "D:\\code\\MiLuStudio\\runtime\\ffmpeg\\bin";

    public string OcrTesseractPath { get; set; } = "D:\\code\\MiLuStudio\\runtime\\tesseract\\tesseract.exe";

    public string OcrTessdataPath { get; set; } = string.Empty;

    public string OcrLanguages { get; set; } = "chi_sim+eng;eng";

    public string PdfRasterizerPath { get; set; } = "D:\\code\\MiLuStudio\\runtime\\poppler\\Library\\bin\\pdftoppm.exe";

    public int PdfRasterizerDpi { get; set; } = 180;

    public int PdfRasterizerPageLimit { get; set; } = 3;

    public int AssetParseTimeoutSeconds { get; set; } = 60;

    public int AssetTranscodeTimeoutSeconds { get; set; } = 180;

    public int AssetVideoFrameLimit { get; set; } = 8;

    public int OcrTimeoutSeconds { get; set; } = 45;

    public string ProviderSettingsPath { get; set; } = string.Empty;

    public string ProviderSecretStorePath { get; set; } = string.Empty;

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
