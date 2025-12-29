using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PhoenixLogisticPrintLabel.Avalonia;

public partial class MainWindow : Window
{
    // Base URL cho API
    // private const string API_BASE_URL = "http://127.0.0.1:8000";
    private const string API_BASE_URL = "https://phoenixlogistics.vn";
    private static readonly HttpClient SharedHttpClient = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };
        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(8)
        };
        client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip");
        client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("deflate");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("PhoenixLogisticPrintLabel/1.0");
        return client;
    }

    private UserEntity? _currentUser;

    public MainWindow()
    {
        InitializeComponent();
        PopulatePrinters();
        TryLoadSavedCredentials();
        
        // Force fixed credentials
        txtEmail!.Text = "staff01@phoenixlogistics.vn";
        txtPassword!.Text = "123456aA@";
        txtEmail.IsEnabled = false;
        txtPassword.IsEnabled = false;
        if (chkRemember != null)
        {
            chkRemember.IsChecked = false;
            chkRemember.IsVisible = false;
        }
    }

    private void PopulatePrinters()
    {
        try
        {
            // Always add Save as PDF option first
            cboPrinters!.Items.Add("💾 Save as PDF");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS: Get printers from CUPS using lpstat
                var printers = GetMacOSPrinters();
                foreach (var printer in printers)
                {
                    cboPrinters.Items.Add(printer);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: Get printers from WMI
                var printers = GetWindowsPrinters();
                foreach (var printer in printers)
                {
                    cboPrinters.Items.Add(printer);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux: Also use CUPS
                var printers = GetMacOSPrinters(); // Same command works on Linux
                foreach (var printer in printers)
                {
                    cboPrinters.Items.Add(printer);
                }
            }

            if (cboPrinters.ItemCount > 0)
            {
                cboPrinters.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            lblStatus!.Text = $"Không thể lấy danh sách máy in: {ex.Message}";
        }
    }

    private string[] GetMacOSPrinters()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "lpstat",
                    Arguments = "-p -d",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse output: "printer HP_LaserJet is idle..."
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var printers = lines
                .Where(l => l.StartsWith("printer"))
                .Select(l => l.Split(' ')[1])
                .ToArray();

            return printers.Length > 0 ? printers : new[] { "Default Printer" };
        }
        catch
        {
            return new[] { "Default Printer" };
        }
    }

    private string[] GetWindowsPrinters()
    {
        try
        {
            var printers = new System.Collections.Generic.List<string>();
            
            // Use PowerShell to get printers
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Get-Printer | Select-Object -ExpandProperty Name\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    printers.Add(trimmed);
                }
            }

            return printers.Count > 0 ? printers.ToArray() : new[] { "Microsoft Print to PDF" };
        }
        catch
        {
            // Fallback to default Windows PDF printer
            return new[] { "Microsoft Print to PDF" };
        }
    }

    private async void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        // Logout when already logged in
        if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.access_token))
        {
            _currentUser = null;
            txtEmail!.IsEnabled = true;
            txtPassword!.IsEnabled = true;
            txtEmail.Focus();
            lblStatus!.Text = "Đã đăng xuất";
            btnLogin!.Content = "Đăng nhập";
            return;
        }

        var email = txtEmail!.Text?.Trim() ?? "";
        var password = txtPassword!.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            lblStatus!.Text = "Vui lòng nhập email và mật khẩu";
            return;
        }

        btnLogin!.IsEnabled = false;
        lblStatus!.Text = "Đang đăng nhập...";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{API_BASE_URL}/api/auth/login");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var payload = new { email, password };
            var json = JsonConvert.SerializeObject(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await SharedHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var body = await response.Content.ReadAsStringAsync();
            
            // Kiểm tra nếu response là HTML thay vì JSON
            if (body.TrimStart().StartsWith("<"))
            {
                await ShowMessageBox("Server trả về dữ liệu không hợp lệ. Vui lòng kiểm tra kết nối hoặc liên hệ hỗ trợ.", "Lỗi đăng nhập");
                lblStatus.Text = "Lỗi server";
                return;
            }
            
            response.EnsureSuccessStatusCode();
            var user = JsonConvert.DeserializeObject<UserEntity>(body);

            if (user != null && !string.IsNullOrEmpty(user.access_token))
            {
                await ShowMessageBox("Đăng nhập thành công", "Thông báo");
                txtPassword.IsEnabled = false;
                txtEmail.IsEnabled = false;
                txtBarcode!.Focus();
                lblStatus.Text = "Đã đăng nhập";
                _currentUser = user;
                btnLogin.Content = "Đăng xuất";
                btnLogin.IsEnabled = true;

                if (chkRemember!.IsChecked == true)
                {
                    SaveCredentials(email, password);
                }
                else
                {
                    ClearSavedCredentials();
                }
            }
            else
            {
                await ShowMessageBox("Đăng nhập không thành công", "Thông báo");
                lblStatus.Text = "Đăng nhập thất bại";
            }
        }
        catch (HttpRequestException httpEx)
        {
            await ShowMessageBox($"Lỗi kết nối: {httpEx.Message}", "Lỗi đăng nhập");
            lblStatus!.Text = "Lỗi kết nối";
        }
        catch (JsonException jsonEx)
        {
            await ShowMessageBox($"Lỗi xử lý dữ liệu từ server: {jsonEx.Message}", "Lỗi đăng nhập");
            lblStatus!.Text = "Lỗi dữ liệu";
        }
        catch (Exception ex)
        {
            await ShowMessageBox(ex.Message, "Lỗi đăng nhập");
            lblStatus!.Text = "Lỗi khi đăng nhập";
        }
        finally
        {
            btnLogin!.IsEnabled = true;
        }
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        txtBarcode!.Text = string.Empty;
        txtBarcode.Focus();
    }

    private async void OnBarcodeKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var code = txtBarcode!.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(code))
            {
                lblStatus!.Text = "Vui lòng nhập mã vạch";
                return;
            }

            if (_currentUser == null || string.IsNullOrEmpty(_currentUser.access_token))
            {
                lblStatus!.Text = "Vui lòng đăng nhập trước";
                return;
            }

            lblStatus!.Text = "Đang lấy dữ liệu đơn hàng";
            OrderEntity? orderEntity = null;
            string? apiError = null;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{API_BASE_URL}/api/orders/package?order_id=" + Uri.EscapeDataString(code));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentUser.access_token);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                var response = await SharedHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                var body = await response.Content.ReadAsStringAsync(cts.Token);
                
                Debug.WriteLine($"API Response ({response.StatusCode}): {body}");
                
                if (!response.IsSuccessStatusCode)
                {
                    apiError = $"API trả về lỗi {(int)response.StatusCode}: {body}";
                }
                else if (body.TrimStart().StartsWith("<"))
                {
                    apiError = "Server trả về HTML thay vì JSON";
                }
                else
                {
                    orderEntity = JsonConvert.DeserializeObject<OrderEntity>(body);
                }
            }
            catch (TaskCanceledException)
            {
                apiError = "Hết thời gian chờ phản hồi từ server";
            }
            catch (HttpRequestException httpEx)
            {
                apiError = $"Lỗi kết nối: {httpEx.Message}";
            }
            catch (Exception ex)
            {
                apiError = $"Lỗi: {ex.Message}";
            }

            if (apiError != null)
            {
                lblStatus.Text = apiError;
                await ShowMessageBox(apiError, "Lỗi tìm đơn");
                return;
            }

            if (orderEntity == null)
            {
                lblStatus!.Text = "Không có dữ liệu để in";
                return;
            }
            if (orderEntity.package == null || string.IsNullOrEmpty(orderEntity.package.label_print_url))
            {
                lblStatus!.Text = "Không có dữ liệu để in";
                return;
            }

            lblOrderInfo!.Text = $"ID: {orderEntity.package.id} Order ID: {orderEntity.package.order_id}";
            var printerName = cboPrinters!.SelectedItem as string ?? string.Empty;
            var error = string.Empty;
            if (await PrintLabel(orderEntity.package.label_print_url, orderEntity.package.id?.ToString() ?? code, printerName))
            {
                txtBarcode.Text = string.Empty;
                lblOrderInfo.Text = "--";
                txtBarcode.Focus();
                lblStatus.Text = "Đã in label";
            }
            else
            {
                await ShowMessageBox("Lỗi khi in", "Thông báo");
            }
        }
    }

    private void SaveCredentials(string email, string password)
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhoenixLogisticPrintLabel");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, "creds.bin");
            var plain = Encoding.UTF8.GetBytes(email + "\n" + password);
            
            // ProtectedData is Windows-only, use simple encryption for cross-platform
            var encrypted = Convert.ToBase64String(plain);
            File.WriteAllText(file, encrypted);
        }
        catch { }
    }

    private void TryLoadSavedCredentials()
    {
        try
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhoenixLogisticPrintLabel", "creds.bin");
            if (File.Exists(file))
            {
                var encrypted = File.ReadAllText(file);
                var plain = Convert.FromBase64String(encrypted);
                var text = Encoding.UTF8.GetString(plain);
                var parts = text.Split('\n');
                if (parts.Length >= 2)
                {
                    txtEmail!.Text = parts[0];
                    txtPassword!.Text = parts[1];
                    chkRemember!.IsChecked = true;
                }
            }
        }
        catch { }
    }

    private void ClearSavedCredentials()
    {
        try
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhoenixLogisticPrintLabel", "creds.bin");
            if (File.Exists(file)) File.Delete(file);
        }
        catch { }
    }

    private async Task<bool> PrintLabel(string labelUrl, string filePrefix, string printerName)
    {
        try
        {
            // Download PDF
            var tempDir = Path.Combine(Path.GetTempPath(), "PhoenixPrintTemp");
            Directory.CreateDirectory(tempDir);
            var tempPath = Path.Combine(tempDir, $"{filePrefix}_{Guid.NewGuid():N}.pdf");

            using (var http = new HttpClient())
            {
                var bytes = await http.GetByteArrayAsync(labelUrl);
                await File.WriteAllBytesAsync(tempPath, bytes);
            }

            // Check if user wants to save as PDF
            bool success = false;
            if (printerName.Contains("Save as PDF"))
            {
                success = await SaveAsPDF(tempPath, filePrefix);
                try { File.Delete(tempPath); } catch { }
                return success;
            }

            // Print using platform-specific method
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                success = await PrintOnMacOS(tempPath, printerName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                success = await PrintOnLinux(tempPath, printerName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                success = await PrintOnWindows(tempPath, printerName);
            }
            else
            {
                await ShowMessageBox("Printing on this platform is not implemented", "Error");
            }

            try { File.Delete(tempPath); } catch { }
            return success;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Print error: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> PrintOnMacOS(string pdfPath, string printerName)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "lpr",
                    Arguments = string.IsNullOrWhiteSpace(printerName) 
                        ? $"\"{pdfPath}\""
                        : $"-P \"{printerName}\" \"{pdfPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> PrintOnLinux(string pdfPath, string printerName)
    {
        // Same as macOS - uses CUPS
        return await PrintOnMacOS(pdfPath, printerName);
    }

    private async Task<bool> PrintOnWindows(string pdfPath, string printerName)
    {
        try
        {
            // Use default PDF viewer to print
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pdfPath,
                    Verb = "print",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                }
            };
            
            if (!string.IsNullOrWhiteSpace(printerName) && !printerName.Contains("Save as PDF"))
            {
                // Try to print with specific printer using PDFtoPrinter or similar
                // For now, use default behavior
                process.Start();
                await Task.Delay(3000); // Wait for print job to be sent
                
                try 
                { 
                    if (!process.HasExited)
                    {
                        process.CloseMainWindow();
                        process.Close();
                    }
                } 
                catch { }
                
                return true;
            }
            else
            {
                process.Start();
                await Task.Delay(3000);
                
                try 
                { 
                    if (!process.HasExited)
                    {
                        process.CloseMainWindow();
                        process.Close();
                    }
                } 
                catch { }
                
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Windows print error: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> SaveAsPDF(string sourcePdfPath, string filePrefix)
    {
        try
        {
            // Create Downloads folder path
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );
            
            // Generate filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"PhoenixLabel_{filePrefix}_{timestamp}.pdf";
            var destinationPath = Path.Combine(downloadsPath, fileName);
            
            // Copy file
            File.Copy(sourcePdfPath, destinationPath, true);
            
            await ShowMessageBox($"PDF đã được lưu vào:\n{destinationPath}", "Lưu thành công");
            
            // Open the folder
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", $"/select,\"{destinationPath}\"");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", $"-R \"{destinationPath}\"");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            await ShowMessageBox($"Lỗi khi lưu PDF: {ex.Message}", "Lỗi");
            return false;
        }
    }

    private async Task ShowMessageBox(string message, string title)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap
        });

        var btnOk = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = 100
        };
        btnOk.Click += (s, e) => dialog.Close();
        panel.Children.Add(btnOk);

        dialog.Content = panel;
        await dialog.ShowDialog(this);
    }
}

public class UserEntity
{
    public string? access_token { get; set; }
}

public class OrderEntity
{
    public PackageEntity? package { get; set; }
}

public class PackageEntity
{
    public long? id { get; set; }
    public string? order_id { get; set; }
    public string? label_print_url { get; set; }
}
