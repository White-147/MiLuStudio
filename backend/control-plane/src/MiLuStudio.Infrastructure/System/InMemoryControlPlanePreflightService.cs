namespace MiLuStudio.Infrastructure.System;

using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Application.System;
using MiLuStudio.Infrastructure.Configuration;

public sealed class InMemoryControlPlanePreflightService : IControlPlanePreflightService, IControlPlaneMigrationService
{
    private readonly ControlPlaneOptions _options;

    public InMemoryControlPlanePreflightService(IOptions<ControlPlaneOptions> options)
    {
        _options = options.Value;
    }

    public Task<ControlPlanePreflightDto> CheckAsync(CancellationToken cancellationToken)
    {
        var storageStatus = Directory.Exists(_options.StorageRoot) ? "ok" : "warning";
        var ffmpegPath = Path.Combine(_options.FfmpegBinPath, "ffmpeg.exe");
        var ffprobePath = Path.Combine(_options.FfmpegBinPath, "ffprobe.exe");
        var ocrPath = ResolveOcrExecutablePath();
        var ocrTessdataPath = ResolveOcrTessdataPath(ocrPath);
        var ocrExecutableExists = File.Exists(ocrPath);
        var ocrTessdataExists = Directory.Exists(ocrTessdataPath);
        var ocrRuntimeReady = ocrExecutableExists && ocrTessdataExists;
        var pdfRasterizerPath = ResolvePdfRasterizerPath();
        var pdfRasterizerExists = File.Exists(pdfRasterizerPath);
        var recommendations = new List<string>
        {
            "Switch ControlPlane:RepositoryProvider to SQLite only after local database configuration is ready."
        };
        if (!ocrRuntimeReady)
        {
            recommendations.Add("Install or import a Tesseract-compatible OCR runtime with scripts\\windows\\Install-MiLuStudioTesseract.ps1, including tessdata for eng and chi_sim when needed.");
        }
        if (!pdfRasterizerExists)
        {
            recommendations.Add("Install or import a Poppler pdftoppm runtime with scripts\\windows\\Install-MiLuStudioPdfRasterizer.ps1 before validating scanned PDF OCR.");
        }

        var checks = new List<PreflightCheckDto>
        {
            new(
                "repository_provider",
                "ok",
                "Control API is using the in-memory repository provider.",
                new Dictionary<string, string> { ["provider"] = RepositoryProviderNames.InMemory }),
            new(
                "database",
                "skipped",
                "SQLite is not required while RepositoryProvider=InMemory.",
                new Dictionary<string, string>()),
            new(
                "migrations",
                "skipped",
                "SQLite schema initialization is checked only when RepositoryProvider=SQLite.",
                new Dictionary<string, string> { ["migrationsPath"] = _options.MigrationsPath }),
            new(
                "storage_root",
                storageStatus,
                Directory.Exists(_options.StorageRoot) ? "Storage root exists." : "Storage root does not exist yet; backend setup should create it before real asset writes.",
                new Dictionary<string, string> { ["storageRoot"] = _options.StorageRoot }),
            new(
                "ffmpeg_runtime",
                File.Exists(ffmpegPath) && File.Exists(ffprobePath) ? "ok" : "warning",
                File.Exists(ffmpegPath) && File.Exists(ffprobePath)
                    ? "Bundled FFmpeg runtime is available."
                    : "Bundled FFmpeg runtime is not ready; video/audio technical parsing will fall back where possible.",
                new Dictionary<string, string>
                {
                    ["ffmpegPath"] = ffmpegPath,
                    ["ffprobePath"] = ffprobePath
                }),
            new(
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
                }),
            new(
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
                }),
            new(
                "python_runtime",
                File.Exists(_options.PythonExecutablePath) ? "ok" : "warning",
                File.Exists(_options.PythonExecutablePath) ? "Python executable exists for Worker skill sidecar calls." : "Python executable was not found.",
                new Dictionary<string, string> { ["pythonExecutablePath"] = _options.PythonExecutablePath }),
            new(
                "python_skills_root",
                Directory.Exists(_options.PythonSkillsRoot) ? "ok" : "warning",
                Directory.Exists(_options.PythonSkillsRoot) ? "Python skills root exists." : "Python skills root was not found.",
                new Dictionary<string, string> { ["pythonSkillsRoot"] = _options.PythonSkillsRoot })
        };

        return Task.FromResult(new ControlPlanePreflightDto(
            RepositoryProviderNames.InMemory,
            Healthy: true,
            checks,
            recommendations));
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

    public Task<MigrationStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new MigrationStatusDto(
            RepositoryProviderNames.InMemory,
            "skipped",
            []));
    }

    public Task<MigrationApplyResultDto> ApplyPendingAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new MigrationApplyResultDto(
            RepositoryProviderNames.InMemory,
            "skipped",
            [],
            []));
    }
}
