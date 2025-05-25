using Microsoft.Extensions.DependencyInjection;
using SkinHunterLauncher.ViewModels;
using System;
using System.Windows;

namespace SkinHunterLauncher
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var launcherWindow = ServiceProvider.GetRequiredService<LauncherWindow>();
            launcherWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<LauncherMainViewModel>();
            services.AddTransient<WelcomeViewModel>();
            services.AddTransient<SignInViewModel>();
            services.AddTransient<LoadingViewModel>();
            services.AddTransient<MainLauncherViewModel>();

            services.AddSingleton<LauncherWindow>();
        }
    }
}