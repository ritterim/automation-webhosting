using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RimDev.Automation.WebHosting
{
    public class IisExpress : IDisposable
    {
        private const string ReadyMsg = @"Registration completed for site";

        public static readonly string DefaultIisExpressPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "IIS Express");

        public static readonly string DefaultAppConfigPath =
            Path.Combine(DefaultIisExpressPath, @"config\templates\PersonalWebServer\applicationhost.config");

        private readonly ProcessStartInfo startInfo;
        private Process process;

        private IisExpress(string siteName, int httpPort, int httpsPort, string appConfigPath)
        {
            AppConfigPath = appConfigPath;
            HttpPort = httpPort;
            HttpsPort = httpsPort;

            var arguments = string.Format("/site:\"{0}\" /config:\"{1}\"", siteName, appConfigPath);
            startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(DefaultIisExpressPath, "iisexpress.exe"),
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
        }

        public static async Task<IisExpress> CreateServer(
            string physicalPath,
            string siteName,
            string configOutputPath = null)
        {
            var tmpPath = configOutputPath ?? Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var hostConfig = await HostConfiguration.LoadAsync(DefaultAppConfigPath).ConfigureAwait(false);

            var httpPort = NetworkUtilities.GetHttpPort();
            var httpsPort = NetworkUtilities.GetHttpsPort();

            hostConfig.CreateSite(siteName, physicalPath, httpPort, httpsPort);
            await hostConfig.SaveChanges(tmpPath);
            return new IisExpress(siteName, httpPort, httpsPort, tmpPath);
        }

        public string AppConfigPath { get; private set; }

        public int HttpPort { get; private set; }

        public int HttpsPort { get; private set; }

        public int ProcessId { get; private set; }

        public Task Start(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<object>();

            if (cancellationToken.IsCancellationRequested)
            {
                tcs.SetCanceled();
                return tcs.Task;
            }

            Trace.WriteLine("Starting IIS Express on ports " + HttpPort + " and " + HttpsPort);

            try
            {
                var proc = new Process { EnableRaisingEvents = true, StartInfo = startInfo };

                DataReceivedEventHandler onOutput = null;
                onOutput =
                    (sender, e) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            tcs.TrySetCanceled();
                        }

                        try
                        {
                            Debug.WriteLine("  [StdOut]\t{0}", (object)e.Data);

                            if (e.Data.StartsWith(ReadyMsg, StringComparison.OrdinalIgnoreCase))
                            {
                                proc.OutputDataReceived -= onOutput;
                                process = proc;
                                tcs.TrySetResult(null);
                            }
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                            proc.Dispose();
                        }
                    };

                proc.OutputDataReceived += onOutput;
                proc.ErrorDataReceived += (sender, e) => Debug.WriteLine("  [StdOut]\t{0}", (object)e.Data);
                proc.Exited += (sender, e) =>
                {
                    Debug.WriteLine("  IIS Express exited.");

                    try { File.Delete(AppConfigPath); } catch { }
                };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                ProcessId = proc.Id;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        public Task Stop()
        {
            var tcs = new TaskCompletionSource<object>(null);
            try
            {
                process.Exited += (sender, e) => tcs.TrySetResult(null);

                process.SendStopMessage();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        public void Quit()
        {
            Process proc;
            if ((proc = Interlocked.Exchange(ref process, null)) != null)
            {
                proc.Kill();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Quit();
            }
        }
    }
}
