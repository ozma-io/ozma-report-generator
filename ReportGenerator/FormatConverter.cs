using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace ReportGenerator
{
    public static class FormatConverter
    {
        public static string OdtToPdf(IConfiguration configuration, string odtFilePath)
        {
            var workingDirectory = configuration["UnoConvSettings:WorkingDirectory"];
            var fileName = configuration["UnoConvSettings:FileName"];
            var arguments = configuration["UnoConvSettings:Arguments"];
            var unoconv = new ProcessStartInfo(workingDirectory + fileName)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = arguments + " -f pdf " + odtFilePath,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            var process = Process.Start(unoconv);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (!string.IsNullOrEmpty(error))  return error;
            return output;
        }
    }
}
