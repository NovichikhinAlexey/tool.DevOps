using System.Diagnostics;

namespace ApplicationInsight
{
    public static class ShellHelper
    {
        public static string AZFileName = @"C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\wbin\az.cmd";

        public static string AZ(this string args)
        {
            //var escapedArgs = cmd.Replace("\"", "\\\"");
             

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = AZFileName,
                    //Arguments = $"-c \"{args}\"",
                    Arguments = $"{args}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}