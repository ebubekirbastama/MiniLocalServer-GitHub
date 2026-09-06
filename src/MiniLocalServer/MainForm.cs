using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MiniLocalServer;

public class MainForm : Form
{
    private readonly TextBox txtFolder = new() { Dock = DockStyle.Fill };
    private readonly TextBox txtPhp = new() { Dock = DockStyle.Fill };
    private readonly NumericUpDown numPort = new() { Minimum = 1024, Maximum = 65535, Value = 8080, Width = 100 };
    private readonly Button btnBrowseFolder = new() { Text = "Klasör Seç", AutoSize = true };
    private readonly Button btnBrowsePhp = new() { Text = "PHP Seç", AutoSize = true };
    private readonly Button btnStart = new() { Text = "▶ Başlat", AutoSize = true };
    private readonly Button btnStop = new() { Text = "■ Durdur", AutoSize = true, Enabled = false };
    private readonly Button btnOpen = new() { Text = "Tarayıcıda Aç", AutoSize = true, Enabled = false };
    private readonly TextBox txtLog = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Font = new Font("Consolas", 10)
    };
    private readonly Label lblStatus = new() { Text = "Hazır", AutoSize = true };

    private Process? phpProcess;
    private string? phpRouterPath;
    private TcpListener? staticListener;
    private CancellationTokenSource? staticServerCts;
    private string? currentUrl;

    public MainForm()
    {
        Text = "Mini Local Server - PHP / HTML";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 560);
        Size = new Size(900, 620);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 6,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Mini Local Server",
            Font = new Font(Font.FontFamily, 18, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        var folderRow = CreateRow("Proje klasörü:", txtFolder, btnBrowseFolder);
        var phpRow = CreateRow("PHP yolu:", txtPhp, btnBrowsePhp);

        var controlRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 8, 0, 8)
        };
        controlRow.Controls.Add(new Label { Text = "Port:", AutoSize = true, Margin = new Padding(0, 7, 6, 0) });
        controlRow.Controls.Add(numPort);
        controlRow.Controls.Add(btnStart);
        controlRow.Controls.Add(btnStop);
        controlRow.Controls.Add(btnOpen);

        root.Controls.Add(title);
        root.Controls.Add(folderRow);
        root.Controls.Add(phpRow);
        root.Controls.Add(controlRow);
        root.Controls.Add(txtLog);
        root.Controls.Add(lblStatus);
        Controls.Add(root);

        btnBrowseFolder.Click += (_, _) => SelectProjectFolder();
        btnBrowsePhp.Click += (_, _) => SelectPhpExe();
        btnStart.Click += async (_, _) => await StartServerAsync();
        btnStop.Click += async (_, _) => await StopServerAsync();
        btnOpen.Click += (_, _) => OpenBrowser();
        FormClosing += async (_, _) => await StopServerAsync();

        txtPhp.Text = FindPhpExe() ?? string.Empty;
        txtFolder.Text = AppContext.BaseDirectory;
        Log("Hazır. Proje klasörünü seçip Başlat'a basın.");
        if (!string.IsNullOrWhiteSpace(txtPhp.Text))
            Log($"PHP bulundu: {txtPhp.Text}");
        else
            Log("PHP bulunamadı. Sadece HTML projeleri C# statik sunucusuyla çalıştırılabilir.");
    }

    private static Control CreateRow(string labelText, Control input, Control button)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            Margin = new Padding(0, 4, 0, 4)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 7, 8, 0) }, 0, 0);
        panel.Controls.Add(input, 1, 0);
        panel.Controls.Add(button, 2, 0);
        return panel;
    }

    private void SelectProjectFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "PHP/HTML proje klasörünü seçin",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(txtFolder.Text) ? txtFolder.Text : string.Empty
        };

        if (dialog.ShowDialog() == DialogResult.OK)
            txtFolder.Text = dialog.SelectedPath;
    }

    private void SelectPhpExe()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "php.exe dosyasını seçin",
            Filter = "PHP executable (php.exe)|php.exe|Executable (*.exe)|*.exe",
            FileName = "php.exe"
        };
        if (dialog.ShowDialog() == DialogResult.OK)
            txtPhp.Text = dialog.FileName;
    }

    private async Task StartServerAsync()
    {
        if (phpProcess is not null || staticListener is not null)
            return;

        var folder = txtFolder.Text.Trim();
        var port = (int)numPort.Value;

        if (!Directory.Exists(folder))
        {
            MessageBox.Show("Geçerli bir proje klasörü seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!IsPortFree(port))
        {
            MessageBox.Show($"{port} portu kullanımda. Başka bir port seçin.", "Port kullanımda", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var hasPhpFiles = Directory.EnumerateFiles(folder, "*.php", SearchOption.AllDirectories).Any();
        var phpExe = ResolvePhpPath();

        try
        {
            SetRunningUi(true);
            currentUrl = $"http://127.0.0.1:{port}/";

            if (!string.IsNullOrWhiteSpace(phpExe) && File.Exists(phpExe))
            {
                await StartPhpServerAsync(phpExe, folder, port);
            }
            else if (hasPhpFiles)
            {
                SetRunningUi(false);
                MessageBox.Show(
                    "Projede PHP dosyaları var fakat php.exe bulunamadı.\n\n" +
                    "Çözüm: Uygulamanın yanına php\\php.exe koyun veya PHP Seç düğmesinden php.exe'yi gösterin.",
                    "PHP gerekli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                await StartStaticServerAsync(folder, port);
            }

            lblStatus.Text = $"Çalışıyor: {currentUrl}";
            btnOpen.Enabled = true;
            Log($"Sunucu başladı: {currentUrl}");
            OpenBrowser();
        }
        catch (Exception ex)
        {
            Log("HATA: " + ex.Message);
            await StopServerAsync();
            MessageBox.Show(ex.Message, "Sunucu başlatılamadı", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task StartPhpServerAsync(string phpExe, string folder, int port)
    {
        var routerArgument = string.Empty;
        var indexPhp = Path.Combine(folder, "index.php");
        if (File.Exists(indexPhp))
        {
            phpRouterPath = CreatePhpRouterFile();
            routerArgument = $" \"{phpRouterPath}\"";
            Log("index.php bulundu: temiz URL / front-controller yönlendirmesi etkin.");
        }

        var psi = new ProcessStartInfo
        {
            FileName = phpExe,
            Arguments = $"-S 127.0.0.1:{port} -t \"{folder}\"{routerArgument}",
            WorkingDirectory = folder,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        phpProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
        phpProcess.OutputDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Log("PHP: " + e.Data); };
        phpProcess.ErrorDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Log("PHP: " + e.Data); };
        phpProcess.Exited += (_, _) => BeginInvoke(new Action(async () => await StopServerAsync()));

        if (!phpProcess.Start())
            throw new InvalidOperationException("PHP sunucusu başlatılamadı.");

        phpProcess.BeginOutputReadLine();
        phpProcess.BeginErrorReadLine();
        Log("PHP built-in server kullanılıyor.");
        await Task.Delay(250);
    }


    private static string CreatePhpRouterFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "MiniLocalServer");
        Directory.CreateDirectory(tempDir);
        var path = Path.Combine(tempDir, $"router-{Guid.NewGuid():N}.php");
        var router = """
<?php
$root = getcwd();
$rootReal = realpath($root);
$uri = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH);
$uri = urldecode($uri ?: '/');
$relative = ltrim(str_replace('/', DIRECTORY_SEPARATOR, $uri), DIRECTORY_SEPARATOR);
$target = realpath($root . DIRECTORY_SEPARATOR . $relative);

if ($uri !== '/' && $target !== false && $rootReal !== false &&
    strncmp($target, $rootReal, strlen($rootReal)) === 0 &&
    (is_file($target) || is_dir($target))) {
    return false;
}

$index = $root . DIRECTORY_SEPARATOR . 'index.php';
if (is_file($index)) {
    require $index;
    return true;
}

return false;
""";
        File.WriteAllText(path, router, new UTF8Encoding(false));
        return path;
    }

    private Task StartStaticServerAsync(string folder, int port)
    {
        staticServerCts = new CancellationTokenSource();
        staticListener = new TcpListener(IPAddress.Loopback, port);
        staticListener.Start();
        Log("PHP bulunmadığı için C# statik HTML sunucusu kullanılıyor.");
        _ = Task.Run(() => StaticServerLoopAsync(folder, staticListener, staticServerCts.Token));
        return Task.CompletedTask;
    }

    private async Task StaticServerLoopAsync(string rootFolder, TcpListener listener, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync(token);
                _ = Task.Run(() => HandleStaticClientAsync(rootFolder, client, token), token);
            }
            catch (OperationCanceledException) { break; }
            catch (ObjectDisposedException) { break; }
            catch (SocketException) when (token.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                Log("Statik sunucu hatası: " + ex.Message);
            }
        }
    }

    private async Task HandleStaticClientAsync(string rootFolder, TcpClient client, CancellationToken token)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.ASCII, false, 4096, leaveOpen: true))
        {
            try
            {
                var requestLine = await reader.ReadLineAsync(token);
                if (string.IsNullOrWhiteSpace(requestLine)) return;

                string? line;
                do { line = await reader.ReadLineAsync(token); }
                while (!string.IsNullOrEmpty(line));

                var parts = requestLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    await WriteHttpResponseAsync(stream, 400, "text/plain; charset=utf-8", Encoding.UTF8.GetBytes("400 - Geçersiz istek"), token);
                    return;
                }

                var method = parts[0].ToUpperInvariant();
                if (method is not ("GET" or "HEAD"))
                {
                    await WriteHttpResponseAsync(stream, 405, "text/plain; charset=utf-8", Encoding.UTF8.GetBytes("405 - Method Not Allowed"), token);
                    return;
                }

                var rawTarget = parts[1];
                var pathOnly = rawTarget.Split('?', 2)[0];
                var relative = Uri.UnescapeDataString(pathOnly).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

                var rootFull = Path.GetFullPath(rootFolder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                var rootBase = rootFull.TrimEnd(Path.DirectorySeparatorChar);
                var candidate = Path.GetFullPath(Path.Combine(rootFolder, relative));

                if (!candidate.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) &&
                    !candidate.Equals(rootBase, StringComparison.OrdinalIgnoreCase))
                {
                    await WriteHttpResponseAsync(stream, 403, "text/plain; charset=utf-8", Encoding.UTF8.GetBytes("403 - Forbidden"), token);
                    return;
                }

                byte[] body;
                string contentType;
                var statusCode = 200;

                if (Directory.Exists(candidate))
                {
                    var indexHtml = Path.Combine(candidate, "index.html");
                    var indexHtm = Path.Combine(candidate, "index.htm");
                    if (File.Exists(indexHtml)) candidate = indexHtml;
                    else if (File.Exists(indexHtm)) candidate = indexHtm;
                    else
                    {
                        var html = BuildDirectoryListing(rootFolder, candidate, pathOnly);
                        body = Encoding.UTF8.GetBytes(html);
                        contentType = "text/html; charset=utf-8";
                        await WriteHttpResponseAsync(stream, statusCode, contentType, body, token, method == "HEAD");
                        return;
                    }
                }

                if (!File.Exists(candidate))
                {
                    statusCode = 404;
                    body = Encoding.UTF8.GetBytes("404 - Dosya bulunamadı");
                    contentType = "text/plain; charset=utf-8";
                }
                else
                {
                    body = await File.ReadAllBytesAsync(candidate, token);
                    contentType = GetMimeType(candidate);
                }

                await WriteHttpResponseAsync(stream, statusCode, contentType, body, token, method == "HEAD");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                try
                {
                    await WriteHttpResponseAsync(stream, 500, "text/plain; charset=utf-8", Encoding.UTF8.GetBytes("500 - " + ex.Message), token);
                }
                catch { }
            }
        }
    }

    private static async Task WriteHttpResponseAsync(NetworkStream stream, int statusCode, string contentType, byte[] body, CancellationToken token, bool headOnly = false)
    {
        var reason = statusCode switch
        {
            200 => "OK",
            400 => "Bad Request",
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            500 => "Internal Server Error",
            _ => "OK"
        };

        var header =
            $"HTTP/1.1 {statusCode} {reason}\r\n" +
            $"Content-Type: {contentType}\r\n" +
            $"Content-Length: {body.Length}\r\n" +
            "Connection: close\r\n" +
            "Cache-Control: no-cache\r\n\r\n";

        var headerBytes = Encoding.ASCII.GetBytes(header);
        await stream.WriteAsync(headerBytes, token);
        if (!headOnly && body.Length > 0)
            await stream.WriteAsync(body, token);
        await stream.FlushAsync(token);
    }

    private static string BuildDirectoryListing(string root, string directory, string urlPath)
    {
        var sb = new StringBuilder();
        sb.Append("<!doctype html><meta charset='utf-8'><title>Dizin</title>");
        sb.Append("<style>body{font-family:Segoe UI,Arial;margin:32px}a{display:block;padding:6px 0}</style>");
        sb.Append($"<h2>{WebUtility.HtmlEncode(urlPath)}</h2>");
        foreach (var dir in Directory.GetDirectories(directory).OrderBy(x => x))
        {
            var name = Path.GetFileName(dir) + "/";
            sb.Append($"<a href='{Uri.EscapeDataString(Path.GetFileName(dir))}/'>{WebUtility.HtmlEncode(name)}</a>");
        }
        foreach (var file in Directory.GetFiles(directory).OrderBy(x => x))
        {
            var name = Path.GetFileName(file);
            sb.Append($"<a href='{Uri.EscapeDataString(name)}'>{WebUtility.HtmlEncode(name)}</a>");
        }
        return sb.ToString();
    }

    private async Task StopServerAsync()
    {
        try
        {
            if (phpProcess is not null)
            {
                try
                {
                    if (!phpProcess.HasExited)
                    {
                        phpProcess.Kill(entireProcessTree: true);
                        await phpProcess.WaitForExitAsync();
                    }
                }
                catch { }
                phpProcess.Dispose();
                phpProcess = null;
            }

            if (!string.IsNullOrWhiteSpace(phpRouterPath))
            {
                try { File.Delete(phpRouterPath); } catch { }
                phpRouterPath = null;
            }

            if (staticServerCts is not null)
            {
                staticServerCts.Cancel();
                staticServerCts.Dispose();
                staticServerCts = null;
            }

            if (staticListener is not null)
            {
                try { staticListener.Stop(); } catch { }
                staticListener = null;
            }
        }
        finally
        {
            currentUrl = null;
            if (!IsDisposed)
            {
                SetRunningUi(false);
                lblStatus.Text = "Durduruldu";
                Log("Sunucu durduruldu.");
            }
        }
    }

    private void OpenBrowser()
    {
        if (string.IsNullOrWhiteSpace(currentUrl)) return;
        try
        {
            Process.Start(new ProcessStartInfo(currentUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log("Tarayıcı açılamadı: " + ex.Message);
        }
    }

    private string? ResolvePhpPath()
    {
        var typed = txtPhp.Text.Trim().Trim('"');
        if (File.Exists(typed)) return typed;

        var found = FindPhpExe();
        if (!string.IsNullOrWhiteSpace(found))
        {
            txtPhp.Text = found;
            return found;
        }
        return null;
    }

    private static string? FindPhpExe()
    {
        var bundled = Path.Combine(AppContext.BaseDirectory, "php", "php.exe");
        if (File.Exists(bundled)) return bundled;

        var beside = Path.Combine(AppContext.BaseDirectory, "php.exe");
        if (File.Exists(beside)) return beside;

        var xampp = @"C:\xampp\php\php.exe";
        if (File.Exists(xampp)) return xampp;

        var laragon = @"C:\laragon\bin\php";
        if (Directory.Exists(laragon))
        {
            var php = Directory.GetDirectories(laragon)
                .OrderByDescending(x => x)
                .Select(x => Path.Combine(x, "php.exe"))
                .FirstOrDefault(File.Exists);
            if (php is not null) return php;
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var item in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var php = Path.Combine(item.Trim(), "php.exe");
                if (File.Exists(php)) return php;
            }
            catch { }
        }
        return null;
    }

    private static bool IsPortFree(int port)
    {
        try
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void SetRunningUi(bool running)
    {
        btnStart.Enabled = !running;
        btnStop.Enabled = running;
        btnOpen.Enabled = running;
        btnBrowseFolder.Enabled = !running;
        btnBrowsePhp.Enabled = !running;
        txtFolder.Enabled = !running;
        txtPhp.Enabled = !running;
        numPort.Enabled = !running;
    }

    private void Log(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => Log(text)));
            return;
        }
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
    }

    private static string GetMimeType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".html" or ".htm" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "application/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".xml" => "application/xml; charset=utf-8",
            ".svg" => "image/svg+xml",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".ico" => "image/x-icon",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            ".ttf" => "font/ttf",
            ".pdf" => "application/pdf",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };
    }
}
