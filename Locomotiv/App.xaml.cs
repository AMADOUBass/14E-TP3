using Locomotiv.Data;
using Locomotiv.Model;
using Locomotiv.Model.DAL;
using Locomotiv.Model.Interfaces;
using Locomotiv.Utils;
using Locomotiv.Utils.Services;
using Locomotiv.Utils.Services.Interfaces;
using Locomotiv.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace Locomotiv
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        public App()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            IConfiguration configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<LoginViewModel>();

            services.AddSingleton<AdminDashboardViewModel>();
            services.AddSingleton<EmployeDashboardViewModel>();

            services.AddSingleton<IUserDAL, UserDAL>();
            services.AddSingleton<ITrainDAL, TrainDAL>();
            services.AddSingleton<IVoieDAL, VoieDAL>();
            services.AddSingleton<IStationDAL, StationDAL>();
            services.AddSingleton<ISignalDAL, SignalDAL>();
            services.AddSingleton<IItineraireDAL, ItineraireDAL>();
            services.AddSingleton<IEtapeDAL, EtapeDAL>();
            services.AddSingleton<IBlockDAL, BlockDAL>();
            services.AddSingleton<IPointArretDAL, PointArretDAL>();


            services.AddSingleton<IDatabaseSeeder, DatabaseSeeder>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IUserSessionService, Service>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<Func<Type, BaseViewModel>>(serviceProvider =>
            {
                BaseViewModel ViewModelFactory(Type viewModelType)
                {
                    return (BaseViewModel)serviceProvider.GetRequiredService(viewModelType);
                }
                return ViewModelFactory;
            });

            services.AddDbContext<ApplicationDbContext>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var seed = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();

                dbContext.Database.Migrate();
                seed.Seed();
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //    dbContext.Database.Migrate(); // Applique les migrations si besoin
            //    dbContext.SeedData(force: true); // Force le seed même si des users existent
            //}

            //var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            //mainWindow.Show();
            //base.OnStartup(e);
        }
    }
}
