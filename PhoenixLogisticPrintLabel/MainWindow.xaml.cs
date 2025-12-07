using System;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using Spire.Pdf;
using System.Security.Cryptography;
using System.Threading;

namespace PhoenixLogisticPrintLabel;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Base URL cho API - đổi để test local
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
    public MainWindow()
    {
        InitializeComponent();
        PopulatePrinters();
        TryLoadSavedCredentials();
        // Force fixed credentials
        txtEmail.Text = "staff01@phoenixlogistics.vn";
        pwdPassword.Password = "123456aA@";
        txtEmail.IsEnabled = false;
        pwdPassword.IsEnabled = false;
        if (chkRemember != null)
        {
            chkRemember.IsChecked = false;
            chkRemember.Visibility = Visibility.Collapsed;
        }
    }

    private void PopulatePrinters()
    {
        try
        {
            var server = new LocalPrintServer();
            var queues = server.GetPrintQueues();
            foreach (var q in queues)
            {
                cboPrinters.Items.Add(q.FullName);
            }
            if (cboPrinters.Items.Count > 0)
            {
                cboPrinters.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Không thể lấy danh sách máy in: {ex.Message}";
        }
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        // Logout when already logged in
        if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.access_token))
        {
            _currentUser = null;
            txtEmail.IsEnabled = true;
            pwdPassword.IsEnabled = true;
            txtEmail.Focus();
            lblStatus.Text = "Đã đăng xuất";
            btnLogin.Content = "Đăng nhập";
            return;
        }

        var email = txtEmail.Text.Trim();
        var password = pwdPassword.Password.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            lblStatus.Text = "Vui lòng nhập email và mật khẩu";
            return;
        }

        btnLogin.IsEnabled = false;
        lblStatus.Text = "Đang đăng nhập...";

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
                MessageBox.Show(this, "Server trả về dữ liệu không hợp lệ. Vui lòng kiểm tra kết nối hoặc liên hệ hỗ trợ.", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Lỗi server";
                return;
            }
            
            response.EnsureSuccessStatusCode();
            var user = JsonConvert.DeserializeObject<UserEntity>(body);

            if (user != null && !string.IsNullOrEmpty(user.access_token))
            {
                MessageBox.Show(this, "Đăng nhập thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                pwdPassword.IsEnabled = false;
                txtEmail.IsEnabled = false;
                txtBarcode.Focus();
                lblStatus.Text = "Đã đăng nhập";
                _currentUser = user;
                btnLogin.Content = "Đăng xuất";
                btnLogin.IsEnabled = true;

                if (chkRemember.IsChecked == true)
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
                MessageBox.Show(this, "Đăng nhập không thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Đăng nhập thất bại";
            }
        }
        catch (HttpRequestException httpEx)
        {
            MessageBox.Show(this, $"Lỗi kết nối: {httpEx.Message}", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Lỗi kết nối";
        }
        catch (JsonException jsonEx)
        {
            MessageBox.Show(this, $"Lỗi xử lý dữ liệu từ server: {jsonEx.Message}", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Lỗi dữ liệu";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Lỗi khi đăng nhập";
        }
        finally
        {
            // Bật lại nút để có thể Đăng xuất hoặc thử lại
            btnLogin.IsEnabled = true;
        }
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        txtBarcode.Clear();
        txtBarcode.Focus();
    }

    private async void OnBarcodeKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var code = txtBarcode.Text.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                lblStatus.Text = "Vui lòng nhập mã vạch";
                return;
            }

            if (_currentUser == null || string.IsNullOrEmpty(_currentUser.access_token))
            {
                lblStatus.Text = "Vui lòng đăng nhập trước";
                return;
            }

            lblStatus.Text = "Đang lấy dữ liệu đơn hàng";
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
                
                // Debug: hiển thị response để kiểm tra
                System.Diagnostics.Debug.WriteLine($"API Response ({response.StatusCode}): {body}");
                
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
                MessageBox.Show(this, apiError, "Lỗi tìm đơn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (orderEntity == null)
            {
                lblStatus.Text = "Không có dữ liệu để in";
                return;
            }
            if (orderEntity.package == null || string.IsNullOrEmpty(orderEntity.package.label_print_url))
            {
                lblStatus.Text = "Không có dữ liệu để in";
                return;
            }

            lblOrderInfo.Text = $"ID: {orderEntity.package.id} Order ID: {orderEntity.package.order_id}";
            var printerName = cboPrinters.SelectedItem as string ?? string.Empty;
            var error = string.Empty;
            if (PrintLabel(orderEntity.package.label_print_url, orderEntity.package.id?.ToString() ?? code, printerName, out error))
            {
                txtBarcode.Text = string.Empty;
                lblOrderInfo.Text = "--";
                txtBarcode.Focus();
                lblStatus.Text = "Đã in label";
            }
            else
            {
                MessageBox.Show(this, error, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private UserEntity? _currentUser;

    private void SaveCredentials(string email, string password)
    {
        try
        {
            var dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhoenixLogisticPrintLabel");
            Directory.CreateDirectory(dir);
            var file = System.IO.Path.Combine(dir, "creds.bin");
            var plain = Encoding.UTF8.GetBytes(email + "\n" + password);
            var protectedBytes = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(file, protectedBytes);
        }
        catch { }
    }

    private void TryLoadSavedCredentials()
    {
        try
        {
            var file = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhoenixLogisticPrintLabel", "creds.bin");
            if (File.Exists(file))
            {
                var data = File.ReadAllBytes(file);
                var plain = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                var text = Encoding.UTF8.GetString(plain);
                var parts = text.Split('\n');
                if (parts.Length >= 2)
                {
                    txtEmail.Text = parts[0];
                    pwdPassword.Password = parts[1];
                    chkRemember.IsChecked = true;
                }
            }
        }
        catch { }
    }

    private void ClearSavedCredentials()
    {
        try
        {
            var file = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PhoenixLogisticPrintLabel", "creds.bin");
            if (File.Exists(file)) File.Delete(file);
        }
        catch { }
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

public partial class MainWindow
{
    private bool PrintLabel(string labelUrl, string filePrefix, string printerName, out string error)
    {
        error = string.Empty;
        try
        {
            // Ensure temp folder
            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "PhoenixPrintTemp");
            Directory.CreateDirectory(tempDir);
            var tempPath = System.IO.Path.Combine(tempDir, $"{filePrefix}_{Guid.NewGuid():N}.pdf");

            using (var http = new HttpClient())
            {
                var bytes = http.GetByteArrayAsync(labelUrl).GetAwaiter().GetResult();
                File.WriteAllBytes(tempPath, bytes);
            }

            // Print directly with Spire.PDF
            using var pdf = new PdfDocument();
            pdf.LoadFromFile(tempPath);
            pdf.PrintSettings.Copies = 1;
            if (!string.IsNullOrWhiteSpace(printerName))
            {
                pdf.PrintSettings.PrinterName = printerName;
            }
            pdf.Print();

            try { File.Delete(tempPath); } catch { }
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}