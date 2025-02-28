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
            base.OnStartup(e);
            ConfigureAppSettings();
            InitilizeDatabase();
            ConfigureServices();
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

            string path = Environment.GetEnvironmentVariable("dbPath")!;
            services.AddDbContext<AppDbContext>(options =>
               options.UseSqlite($"Data Source={path}"));
            
            services.AddSingleton<NetworkService>();

            ServiceProvider = services.BuildServiceProvider();
        }
        
        private void InitilizeDatabase()
        {
            string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dbPath = Path.Join(BasePath, "docstack.services.db");
            Environment.SetEnvironmentVariable("dbPath", dbPath, EnvironmentVariableTarget.Process);

            if (!File.Exists(dbPath)) 
            { 
                File.Create(dbPath);
            }

        }
    }

}
