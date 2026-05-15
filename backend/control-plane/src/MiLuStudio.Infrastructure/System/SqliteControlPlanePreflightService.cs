namespace MiLuStudio.Infrastructure.System;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.System;
using MiLuStudio.Infrastructure.Configuration;
using MiLuStudio.Infrastructure.Persistence.Sqlite;

public sealed class SqliteControlPlanePreflightService : IControlPlanePreflightService
{
    private readonly MiLuStudioDbContext _db;
    private readonly IControlPlaneMigrationService _migrations;
    private readonly ControlPlaneOptions _options;

    public SqliteControlPlanePreflightService(
        MiLuStudioDbContext db,
        IControlPlaneMigrationService migrations,
        IOptions<ControlPlaneOptions> options)
    {
        _db = db;
        _migrations = migrations;
        _options = options.Value;
    }

    public async Task<ControlPlanePreflightDto> CheckAsync(CancellationToken cancellationToken)
    {
        var checks = new List<PreflightCheckDto>
        {
            new(
                "repository_provider",
                "ok",
                "Control API is configured for the local SQLite repository.",
                new Dictionary<string, string> { ["provider"] = RepositoryProviderNames.Sqlite })
        };
        var recommendations = new List<string>();
        var healthy = true;

        var connectionString = _db.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            healthy = false;
            checks.Add(new(
                "connection_string",
                "error",
                "ConnectionStrings:MiLuStudioControlPlane is missing and no SQLite fallback was resolved.",
                new Dictionary<string, string>()));
            recommendations.Add("Configure ConnectionStrings:MiLuStudioControlPlane or ControlPlane:StorageRoot so the backend can own SQLite initialization.");
        }
        else
        {
            var dataSource = ResolveSqliteDataSource(connectionString);
            checks.Add(new(
                "database_file",
                File.Exists(dataSource) || string.Equals(dataSource, ":memory:", StringComparison.OrdinalIgnoreCase) ? "ok" : "warning",
                File.Exists(dataSource) || string.Equals(dataSource, ":memory:", StringComparison.OrdinalIgnoreCase)
                    ? "SQLite database file is available."
                    : "SQLite database file has not been created yet.",
                new Dictionary<string, string> { ["dataSource"] = dataSource }));
        }

        var canConnect = false;
        try
        {
            canConnect = await _db.Database.CanConnectAsync(cancellationToken);
        }
        catch (Exception error) when (error is InvalidOperationException or TimeoutException or SqliteException)
        {
            checks.Add(new(
                "database_reachable",
                "error",
                $"SQLite is not reachable by the backend process: {error.Message}",
                new Dictionary<string, string>()));
        }

        if (canConnect)
        {
            checks.Add(new(
                "database_reachable",
                "ok",
                "SQLite is reachable from the backend process.",
                new Dictionary<string, string>()));
        }
        else
        {
            healthy = false;
            recommendations.Add("Let the backend initialize the local SQLite file and verify the storage directory is writable.");
        }

        if (canConnect)
        {
            var migrationStatus = await _migrations.GetStatusAsync(cancellationToken);
            var pendingCount = migrationStatus.Migrations.Count(migration => migration.Status == "pending");
            checks.Add(new(
                "sqlite_schema",
                pendingCount == 0 ? "ok" : "warning",
                pendingCount == 0 ? "SQLite schema is ready." : "SQLite schema has not been initialized yet.",
                new Dictionary<string, string>
                {
                    ["status"] = migrationStatus.Status,
                    ["pendingCount"] = pendingCount.ToString()
                }));

            if (pendingCount > 0)
            {
                healthy = false;
                recommendations.Add("Initialize SQLite through the backend migration endpoint or backend startup path before production work.");
            }
        }

        AddDirectoryCheck(checks, "storage_root", _options.StorageRoot, "Storage root");
        AddDirectoryCheck(checks, "uploads_root", _options.UploadsRoot, "Uploads root");

        var ffmpegPath = Path.Combine(_options.FfmpegBinPath, "ffmpeg.exe");
        var ffprobePath = Path.Combine(_options.FfmpegBinPath, "ffprobe.exe");
        checks.Add(new(
            "ffmpeg_runtime",
            File.Exists(ffmpegPath) && File.Exists(ffprobePath) ? "ok" : "warning",
            File.Exists(ffmpegPath) && File.Exists(ffprobePath)
                ? "Bundled FFmpeg runtime is available."
                : "Bundled FFmpeg runtime is not ready; video/audio technical parsing will fall back where possible.",
            new Dictionary<string, string>
            {
                ["ffmpegPath"] = ffmpegPath,
                ["ffprobePath"] = ffprobePath
            }));

        var ocrPath = ResolveOcrExecutablePath();
        var ocrTessdataPath = ResolveOcrTessdataPath(ocrPath);
        var ocrExecutableExists = File.Exists(ocrPath);
        var ocrTessdataExists = Directory.Exists(ocrTessdataPath);
        var ocrRuntimeReady = ocrExecutableExists && ocrTessdataExists;
        checks.Add(new(
            "ocr_runtime",
            ocrRuntimeReady ? "ok" : "warning",
            ocrRuntimeReady
                ? "OCR runtime and tessdata are available through the backend adapter."
                : ocrExecutableExists
                    ? "OCR executable exists, but tessdata is not ready; image OCR will use structured fallback if invocation fails."
                    : "OCR runtime is not ready; image OCR will use structured metadata fallback.",
            new Dictionary<string, string>
            {
                ["tesseractPath"] = ocrPath,
                ["tessdataPath"] = ocrTessdataPath,
                ["tessdataAvailable"] = ocrTessdataExists ? "true" : "false",
                ["languages"] = ResolveOcrLanguagesText(),
                ["installScript"] = "scripts\\windows\\Install-MiLuStudioTesseract.ps1"
            }));
        if (!ocrRuntimeReady)
        {
            recommendations.Add("Install or import a Tesseract-compatible OCR runtime with scripts\\windows\\Install-MiLuStudioTesseract.ps1, including tessdata for eng and chi_sim when needed.");
        }

        var pdfRasterizerPath = ResolvePdfRasterizerPath();
        var pdfRasterizerExists = File.Exists(pdfRasterizerPath);
        checks.Add(new(
            "pdf_rasterizer_runtime",
            pdfRasterizerExists ? "ok" : "warning",
            pdfRasterizerExists
                ? "PDF rasterizer runtime is available through the backend adapter."
                : "PDF rasterizer runtime is not ready; scanned PDF uploads will use structured fallback metadata.",
            new Dictionary<string, string>
            {
                ["pdftoppmPath"] = pdfRasterizerPath,
                ["dpi"] = _options.PdfRasterizerDpi.ToString(),
                ["pageLimit"] = _options.PdfRasterizerPageLimit.ToString(),
                ["installScript"] = "scripts\\windows\\Install-MiLuStudioPdfRasterizer.ps1"
            }));
        if (!pdfRasterizerExists)
        {
            recommendations.Add("Install or import a Poppler pdftoppm runtime with scripts\\windows\\Install-MiLuStudioPdfRasterizer.ps1 before validating scanned PDF OCR.");
        }

        var pythonExists = File.Exists(_options.PythonExecutablePath);
        checks.Add(new(
            "python_runtime",
            pythonExists ? "ok" : "error",
            pythonExists ? "Python executable exists for Worker skill sidecar calls." : "Python executable was not found.",
            new Dictionary<string, string> { ["pythonExecutablePath"] = _options.PythonExecutablePath }));
        if (!pythonExists)
        {
            healthy = false;
            recommendations.Add("Configure ControlPlane:PythonExecutablePath to a valid Python runtime packaged with or selected by the dependency center.");
        }

        var skillsRootExists = Directory.Exists(_options.PythonSkillsRoot);
        checks.Add(new(
            "python_skills_root",
            skillsRootExists ? "ok" : "error",
            skillsRootExists ? "Python skills root exists." : "Python skills root was not found.",
            new Dictionary<string, string> { ["pythonSkillsRoot"] = _options.PythonSkillsRoot }));
        if (!skillsRootExists)
        {
            healthy = false;
            recommendations.Add("Configure ControlPlane:PythonSkillsRoot to the packaged or repository Python skills root.");
        }

        return new ControlPlanePreflightDto(
            RepositoryProviderNames.Sqlite,
            healthy,
            checks,
            recommendations);
    }

    private static string ResolveSqliteDataSource(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource) ||
            string.Equals(builder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return builder.DataSource;
        }

        return Path.GetFullPath(builder.DataSource);
    }

    private static void AddDirectoryCheck(
        List<PreflightCheckDto> checks,
        string name,
        string path,
        string label)
    {
        checks.Add(new(
            name,
            Directory.Exists(path) ? "ok" : "warning",
            Directory.Exists(path) ? $"{label} exists." : $"{label} does not exist yet; backend setup should create it before writes.",
            new Dictionary<string, string> { ["path"] = path }));
    }

    private string ResolveOcrExecutablePath()
    {
        if (!string.IsNullOrWhiteSpace(_options.OcrTesseractPath))
        {
            return Path.GetFullPath(_options.OcrTesseractPath.Trim());
        }

        return Path.Combine("D:\\code\\MiLuStudio", "runtime", "tesseract", "tesseract.exe");
    }

    private string ResolveOcrTessdataPath(string ocrExecutablePath)
    {
        if (!string.IsNullOrWhiteSpace(_options.OcrTessdataPath))
        {
            return Path.GetFullPath(_options.OcrTessdataPath.Trim());
        }

        return Path.Combine(Path.GetDirectoryName(ocrExecutablePath) ?? ".", "tessdata");
    }

    private string ResolveOcrLanguagesText()
    {
        return string.IsNullOrWhiteSpace(_options.OcrLanguages) ? "chi_sim+eng;eng" : _options.OcrLanguages;
    }

    private string ResolvePdfRasterizerPath()
    {
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(_options.PdfRasterizerPath))
        {
            candidates.Add(_options.PdfRasterizerPath);
        }

        candidates.AddRange(
        [
            Path.Combine("D:\\code\\MiLuStudio", "runtime", "poppler", "Library", "bin", "pdftoppm.exe"),
            Path.Combine("D:\\code\\MiLuStudio", "runtime", "poppler", "bin", "pdftoppm.exe"),
            Path.Combine("D:\\tools", "poppler", "Library", "bin", "pdftoppm.exe"),
            Path.Combine("D:\\tools", "poppler", "bin", "pdftoppm.exe")
        ]);

        var fullPaths = candidates
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.GetFullPath(path.Trim()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return fullPaths.FirstOrDefault(File.Exists) ?? fullPaths[0];
    }
}
