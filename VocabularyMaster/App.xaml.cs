using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VocabularyMaster.Core.Interfaces;
using VocabularyMaster.Infrastructure.Data;
using VocabularyMaster.Infrastructure.Repositories;
using VocabularyMaster.WPF.ViewModels;

namespace VocabularyMaster.WPF
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VocabularyMaster",
                "vocabulary.db");

            var directory = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            services.AddDbContext<VocabularyDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            services.AddScoped<IWordRepository, WordRepository>();
            services.AddScoped<IReviewHistoryRepository, ReviewHistoryRepository>();

            services.AddTransient<DashboardViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<WordListViewModel>();
            services.AddTransient<TestViewModel>();
            services.AddTransient<StatisticsViewModel>();
            services.AddTransient<FlashcardViewModel>();

            services.AddTransient<MainWindow>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var context = _serviceProvider.GetRequiredService<VocabularyDbContext>();

            context.Database.EnsureCreated();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnExit(e);
        }
    }
}