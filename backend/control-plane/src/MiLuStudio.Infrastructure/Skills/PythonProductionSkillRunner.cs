namespace MiLuStudio.Infrastructure.Skills;

using Process = global::System.Diagnostics.Process;
using ProcessStartInfo = global::System.Diagnostics.ProcessStartInfo;
using Microsoft.Extensions.Options;
using MiLuStudio.Application.Abstractions;
using MiLuStudio.Infrastructure.Configuration;

public sealed class PythonProductionSkillRunner : IProductionSkillRunner
{
    private readonly ControlPlaneOptions _options;

    public PythonProductionSkillRunner(IOptions<ControlPlaneOptions> options)
    {
        _options = options.Value;
    }

    public async Task<ProductionSkillRunResult> RunAsync(
        string skillName,
        string inputJson,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(_options.PythonExecutablePath))
        {
            throw new FileNotFoundException("MiLuStudio Python executable was not found.", _options.PythonExecutablePath);
        }

        if (!Directory.Exists(_options.PythonSkillsRoot))
        {
            throw new DirectoryNotFoundException($"MiLuStudio Python skills root was not found: {_options.PythonSkillsRoot}");
        }

        PruneOldRunDirectories();

        var runRoot = Path.Combine(_options.SkillRunTempRoot, $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}");
        Directory.CreateDirectory(runRoot);

        var inputPath = Path.Combine(runRoot, "input.json");
        var outputPath = Path.Combine(runRoot, "output.json");
        await File.WriteAllTextAsync(inputPath, inputJson, cancellationToken);

        using var process = StartProcess(skillName, inputPath, outputPath);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, _options.SkillRunTimeoutSeconds)));

        string stdout;
        string stderr;

        try
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
            var stderrTask = process.StandardError.ReadToEndAsync(timeout.Token);
            await process.WaitForExitAsync(timeout.Token);
            stdout = await stdoutTask;
            stderr = await stderrTask;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            throw new TimeoutException($"Python skill '{skillName}' exceeded {_options.SkillRunTimeoutSeconds} seconds.");
        }

        if (!File.Exists(outputPath))
        {
            throw new InvalidOperationException(
                $"Python skill '{skillName}' did not write an output envelope. ExitCode={process.ExitCode}; stderr={stderr}");
        }

        var outputJson = await File.ReadAllTextAsync(outputPath, cancellationToken);
        return new ProductionSkillRunResult(outputJson, process.ExitCode, stdout, stderr);
    }

    private Process StartProcess(string skillName, string inputPath, string outputPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _options.PythonExecutablePath,
            WorkingDirectory = _options.PythonSkillsRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-m");
        startInfo.ArgumentList.Add("milu_studio_skills");
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--skill");
        startInfo.ArgumentList.Add(skillName);
        startInfo.ArgumentList.Add("--input");
        startInfo.ArgumentList.Add(inputPath);
        startInfo.ArgumentList.Add("--output");
        startInfo.ArgumentList.Add(outputPath);

        startInfo.Environment["PYTHONIOENCODING"] = "utf-8";

        return Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start Python skill process for '{skillName}'.");
    }

    private void PruneOldRunDirectories()
    {
        Directory.CreateDirectory(_options.SkillRunTempRoot);

        var retentionCount = Math.Max(1, _options.SkillRunRetentionCount);
        var oldRuns = Directory
            .GetDirectories(_options.SkillRunTempRoot)
            .Select(path => new DirectoryInfo(path))
            .OrderByDescending(directory => directory.CreationTimeUtc)
            .Skip(retentionCount)
            .ToList();

        foreach (var oldRun in oldRuns)
        {
            try
            {
                oldRun.Delete(recursive: true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }
}
