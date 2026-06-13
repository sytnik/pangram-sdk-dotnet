using System.Diagnostics;
using System.Text;

namespace Pangram.Sdk.WinForms.Services;

public static class PandocExtractor
{
    public static bool IsPandocAvailable { get; private set; }

    static PandocExtractor() => CheckAvailability();

    private static void CheckAvailability()
    {
        try
        {
            using var proc = Process.Start(new ProcessStartInfo("pandoc", "--version")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            IsPandocAvailable = proc?.WaitForExit(4000) == true && proc.ExitCode == 0;
        }
        catch { IsPandocAvailable = false; }
    }

    public static async Task<string> ExtractTextAsync(string filePath)
    {
        if (!IsPandocAvailable)
            throw new InvalidOperationException(
                "Pandoc is not installed or not on PATH.\nInstall from https://pandoc.org/installing.html");

        var psi = new ProcessStartInfo("pandoc", $"--to plain \"{filePath}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };

        using var proc = new Process { StartInfo = psi };
        var output = new StringBuilder();
        var error = new StringBuilder();

        proc.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        await Task.Run(() => proc.WaitForExit()).ConfigureAwait(false);

        if (proc.ExitCode != 0)
            throw new InvalidOperationException($"Pandoc exited with code {proc.ExitCode}:\n{error}");

        return output.ToString();
    }
}

