using Newtonsoft.Json;
using PM.Client;
using PM.Extensions;
using PM.Models;
using PM.Models.Enums;
using PM.Models.Manifests;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PM.Services
{
    public static class PackageManagerService
    {
        private static async Task<PackageManifest> GetManifestFromResource(string addr)
        {

            PackageManifest? manifest = null;

            if (File.Exists(addr))
            {
                manifest = JsonConvert.DeserializeObject<PackageManifest>(File.ReadAllText(addr));
            }
            else
            {
                HttpClient httpClient = new HttpClient();
                manifest = await httpClient.DownloadToJson<PackageManifest>(new Uri(addr));
            }
            if (manifest == null) throw new Exception($"unable to convert object at {addr} to valid package object");
            return manifest;
        }

        public static async Task ListAllPackages()
        {
            try
            {
                LoggerService.Log($".fetching package descriptors from remote server");
                var ls = await PackageManagerClient.GetAllAsync();
                LoggerService.Log("   Name  |  Tag  |  Description", force: true);
                foreach(var manifest in ls)
                {
                    LoggerService.Log($" - {manifest.Name}    {manifest.Tag}    {manifest.Description}", force: true);
                }
                return;     
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Unexpected error occurred", LogSeverity.ERROR, true);
                LoggerService.Log($"ERROR -- {ex.Message}", LogSeverity.ERROR, true);
            }
        }

        public static async Task ListAllTags(string name)
        {
            try
            {
                LoggerService.Log($".fetching tags for package {name} from remote server");
                var ls = await PackageManagerClient.GetAllTagsAsync(name);
                LoggerService.Log("  Tag  ", force: true);
                foreach (var tag in ls)
                {
                    LoggerService.Log($" - {tag}", force: true);
                }
                return;
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Unexpected error occurred", LogSeverity.ERROR, true);
                LoggerService.Log($"ERROR -- {ex.Message}", LogSeverity.ERROR, true);
            }
        }

        public static async Task PushManifestToRemote(string uri)
        {
            try
            {
                var manifest = await GetManifestFromResource(uri);
                LoggerService.Log($".retrieved manifest from {uri}");
                LoggerService.Log($".posting manifest to {ConfigService.GetConfig("baseUrl")}");
                await PackageManagerClient.PostAsync(manifest);
                LoggerService.Log($".pushed package {manifest.Name}:{manifest.Tag} to remote", LogSeverity.SUCCESS);
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Unexpected error occurred", LogSeverity.ERROR, true);
                LoggerService.Log($"ERROR -- {ex.Message}", LogSeverity.ERROR, true);
            }
        }

        public static async Task GetPackageWithLogging(string name, string tag)
        {
            try
            {
                LoggerService.Log($".fetching manifest from remote server");
                var manifest = await PackageManagerClient.GetAsync(name, tag);
                LoggerService.Log($".package manifest fetched from remote {manifest.Name}:{manifest.Tag}");
                await GetPackageWithLogging(manifest);
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Unexpected error occurred retrieving package JSON", LogSeverity.ERROR, true);
                LoggerService.Log($"ERROR -- {ex.Message}", LogSeverity.ERROR, true);
            }
        }

        public static async Task GetPackageWithLogging(string addr)
        {
            try
            {
                var manifest = await GetManifestFromResource(addr);
                await GetPackageWithLogging(manifest);
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Unexpected error occurred retrieving package JSON", LogSeverity.ERROR, true);
                LoggerService.Log($"ERROR -- {ex.Message}", LogSeverity.ERROR, true);
            }
        }

        public static async Task GetPackageWithLogging(PackageManifest manifest)
        {
            bool loggingEnabled = LoggerService.LoggingEnabled;
            LoggerService.LoggingEnabled = true;
            try
            {
                await GetPackage(manifest);
            } catch (Exception ex)
            {
                LoggerService.Log($"Unexpected error occured fetching package {manifest.Name}:{manifest.Tag}", LogSeverity.ERROR);
                LoggerService.Log($"Error -- {ex.Message}", LogSeverity.ERROR, true);
            }
            LoggerService.LoggingEnabled = loggingEnabled;
        }

        public static async Task GetPackage(PackageManifest manifest)
        {
            if (manifest == null) throw new ArgumentNullException(nameof(manifest));
            if (manifest.DownloadManifest == null) throw new ArgumentNullException(nameof(manifest.DownloadManifest));
            if (manifest.UnzipManifests == null) throw new ArgumentNullException(nameof(manifest.UnzipManifests));
            if (manifest.RunManifests == null) throw new ArgumentNullException(nameof(manifest.RunManifests));

            LoggerService.Log($".getting package {manifest.Name}:{manifest.Tag}");
            await Download(manifest.DownloadManifest);
            LoggerService.Log("..finshed downloading");
            LoggerService.Log(".unzipping files");
            if (!manifest.UnzipManifests.Any())
                LoggerService.Log("..found no files to unzip\n..skipping", LogSeverity.WARNING);
            else
            {
                foreach (var unzip in manifest.UnzipManifests)
                {
                    Unzip(unzip);
                }
                LoggerService.Log("..finished unzipping");
            }
            LoggerService.Log(".running install commands");
            var orderedRunManifests = manifest.RunManifests.OrderBy(r => r.Stage);
            foreach (var runManifest in orderedRunManifests)
            {
                LoggerService.Log($"..({runManifest.Stage}/{orderedRunManifests.Count()}) run ~$ {runManifest.Cmd}");
                var rc = Run(runManifest);
                if (rc.Status == RunStatus.SUCCESS || rc.Status == RunStatus.IGNORED)
                {
                    LoggerService.Log($".. SUCCESS ({(rc.ProducedUsableExitCode ? $"{rc.ExitCode}" : "")})", LogSeverity.SUCCESS);
                    continue;
                }
                LoggerService.Log($"..FAIL -- {(rc.ProducedUsableExitCode ? $"exit code ({rc.ExitCode})" : "")}.", LogSeverity.ERROR, true);
                LoggerService.Log($"..FAILURE REASON -- {rc.ErrorTrace}", LogSeverity.ERROR, true);
                return;
            }
            LoggerService.Log($".package {manifest.Name}:{manifest.Tag} installed", LogSeverity.SUCCESS);
        }

        public static async Task Download(DownloadManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(manifest.Uri)) throw new ArgumentNullException(manifest.Uri);
            if (!Directory.Exists(Path.GetDirectoryName(manifest.Dest))) {
                Directory.CreateDirectory(Path.GetDirectoryName(manifest.Dest));
            }
            if (!manifest.Remote)
            {
                if (!File.Exists(manifest.Uri)) throw new FileNotFoundException(manifest.Uri);
                File.Copy(manifest.Uri, manifest.Dest, true);
            } else
            {
                HttpClient httpClient = new HttpClient();
                await httpClient.DownloadFileTaskAsync(new Uri(manifest.Uri), manifest.Dest);
            }
        }
        public static void Unzip(UnzipManifest manifest)
        {
            if (!File.Exists(manifest.Src)) throw new FileNotFoundException(manifest.Src);
            if (!Directory.Exists(manifest.Dest)) {
                if (!manifest.CreateDestIfNotExists) throw new DirectoryNotFoundException(manifest.Dest);
                Directory.CreateDirectory(manifest.Dest);
            } 
            ZipFile.ExtractToDirectory(manifest.Src, manifest.Dest, true);
        }

        public static RunResult Run(RunManifest manifest)
        {
            try
            {
                var argsPrepend = "";
                var shellName = "/bin/bash";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    shellName = "cmd";
                    argsPrepend = "/c ";
                }
                ProcessStartInfo startInfo = new ProcessStartInfo() 
                { 
                    FileName =shellName,
                    Arguments = $"{argsPrepend} {manifest.Cmd}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };
                Process proc = new Process() 
                {
                    StartInfo = startInfo,
                };
                proc.Start();
                proc.WaitForExit();
                if (manifest.ShowLogs)
                {
                    var lines = proc.StandardOutput.ReadToEnd().Split('\n');
                    foreach (var line in lines)
                    {
                        LoggerService.Log($"....{line}", LogSeverity.LOGS, true);
                    }
                }
                
                return new RunResult
                {
                    ExitCode = proc.ExitCode,
                    ProducedUsableExitCode = true,
                    Status = manifest.ExitCodeSuccess.Contains(proc.ExitCode) ? RunStatus.SUCCESS : (manifest.IgnoreOnFail ? RunStatus.IGNORED : RunStatus.FAIL),
                    HadError = false,
                };
            } catch(Exception ex)
            {
                return new RunResult
                {
                    ProducedUsableExitCode = false,
                    Status = manifest.IgnoreOnFail? RunStatus.IGNORED : RunStatus.FAIL,
                    HadError = true,
                    ErrorTrace = ex.ToString(),
                };
            }
        }
    }
}
