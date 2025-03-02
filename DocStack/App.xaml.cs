using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;

namespace DocStack
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!IsRunAsAdministrator())
            {
                RestartAsAdmin();
                return;
            }

            base.OnStartup(e);
            ConfigureAppSettings();
            InitilizeDatabase();
            ConfigureServices();
            ApplyMigrations();
        }

        private void ConfigureAppSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // load env variabels
            string CoreApiToken = Configuration["SecretSection:CoreApiKey"]!;
            Environment.SetEnvironmentVariable("CoreApiToken", CoreApiToken, EnvironmentVariableTarget.Process);
        }

        private void ConfigureServices()
        {
            ServiceCollection services = new();

            services.AddSingleton<AppDbContext>();
            services.AddSingleton<NetworkService>();
            services.AddSingleton<PaperService>();
            services.AddSingleton<StarredService>();

            ServiceProvider = services.BuildServiceProvider();
        }
        
        private void InitilizeDatabase()
        {
            string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dbPath = Path.Join(BasePath, "docstack.services.db");
            Environment.SetEnvironmentVariable("dbPath", dbPath, EnvironmentVariableTarget.Process);

            if (!File.Exists(dbPath))
            {
                using (File.Create(dbPath)) { } // Ensures the file stream is closed immediately
            }


        }

        private bool IsRunAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        private void RestartAsAdmin()
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(psi);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to restart as administrator: " + ex.Message);
            }
        }

        private void ApplyMigrations()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate(); // Apply pending migrations
            }
        }
    }

}
