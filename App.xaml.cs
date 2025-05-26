using Microsoft.Extensions.DependencyInjection;
using SkinHunterLauncher.ViewModels;
using System;
using System.Windows;
using SkinHunterLauncher.Services;

namespace SkinHunterLauncher
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var supabaseService = ServiceProvider.GetRequiredService<SupabaseService>();
            await supabaseService.InitializeAsync();

            // Intentar cargar usuario recordado al inicio
            var authService = ServiceProvider.GetRequiredService<AuthService>();
            var sessionService = ServiceProvider.GetRequiredService<CurrentUserSessionService>();
            var (token, username) = authService.GetRememberedUser();
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
            {
                var principal = authService.ValidateToken(token);
                if (principal?.Identity?.IsAuthenticated == true)
                {
                    // Si el token es válido, obtener los datos completos del usuario
                    // Podríamos necesitar un método en SupabaseService para GetUserById si el claim solo tiene el ID
                    Models.User? rememberedUser = await supabaseService.GetUserByLogin(username);
                    if (rememberedUser != null)
                    {
                        sessionService.SetCurrentUser(rememberedUser, token);
                    }
                }
                else
                {
                    authService.ClearRememberedUser(); // Token recordado inválido
                }
            }


            var launcherWindow = ServiceProvider.GetRequiredService<LauncherWindow>();
            launcherWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SupabaseService>();
            services.AddSingleton<AuthService>();
            services.AddSingleton<CurrentUserSessionService>(); // AÑADIDO

            services.AddSingleton<LauncherMainViewModel>();
            services.AddTransient<WelcomeViewModel>();
            services.AddTransient<SignInViewModel>();
            services.AddTransient<LoadingViewModel>();
            services.AddTransient<MainLauncherViewModel>();

            services.AddSingleton<LauncherWindow>();
        }
    }
}