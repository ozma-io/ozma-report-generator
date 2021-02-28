using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ReportGenerator
{
    public static class FormatConverter
    {
        public static async Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<int>();
            EventHandler exitHandler = (s, e) =>
            {
                tcs.TrySetResult(process.ExitCode);
            };
            try
            {
                process.EnableRaisingEvents = true;
                process.Exited += exitHandler;
                if (process.HasExited)
                {
                    tcs.TrySetResult(process.ExitCode);
                }

                using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                process.Exited -= exitHandler;
            }
        }

        public static async Task<string> ConvertOdtByUnoconv(IConfiguration configuration, string odtFilePath, string newFormat)
        {
            var workingDirectory = configuration["UnoConvSettings:WorkingDirectory"];
            var fileName = configuration["UnoConvSettings:FileName"];
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "unoconv";
            }
            var arguments = configuration["UnoConvSettings:Arguments"];
            var unoconv = new ProcessStartInfo(workingDirectory + fileName)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = arguments + " -f " + newFormat + " " + odtFilePath,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            var process = Process.Start(unoconv);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            await process.WaitForExitAsync();
            if (!string.IsNullOrEmpty(error))  return error;
            return output;
        }
    }
}
