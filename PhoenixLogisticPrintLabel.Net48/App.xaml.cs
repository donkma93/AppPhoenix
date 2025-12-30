using System;
using System.Windows;

namespace PhoenixLogisticPrintLabel.Net48
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // *** QUAN TRỌNG: Fix SSL/TLS cho Windows 7 ***
            // Bật hỗ trợ TLS 1.2 để kết nối HTTPS
            System.Net.ServicePointManager.SecurityProtocol = 
                System.Net.SecurityProtocolType.Tls12 | 
                System.Net.SecurityProtocolType.Tls11 | 
                System.Net.SecurityProtocolType.Tls;

            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                MessageBox.Show(ex.ExceptionObject.ToString(), "Lỗi không xử lý", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            base.OnStartup(e);
        }
    }
}
