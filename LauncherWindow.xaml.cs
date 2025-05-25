using SkinHunterLauncher.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel; // Para PropertyChangedEventHandler

namespace SkinHunterLauncher
{
    public partial class LauncherWindow : Window
    {
        public LauncherWindow(LauncherMainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += LauncherWindow_Loaded;
        }

        private async void LauncherWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LauncherMainViewModel vm)
            {
                // Llama a InitializeAsync del MainViewModel para que configure la vista inicial
                await vm.InitializeAsync();

                // Suscribirse a PropertyChanged para actualizar el tamaño de la ventana
                // y el título de la ventana cuando CurrentViewModel cambie.
                vm.PropertyChanged += ViewModel_PropertyChanged;

                // Actualizar estado inicial basado en el CurrentViewModel que se acaba de establecer
                if (vm.CurrentViewModel != null)
                {
                    UpdateWindowStateForViewModel(vm.CurrentViewModel);
                    this.Title = vm.CurrentViewModel.Title; // Establecer el título inicial de la ventana
                }
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LauncherMainViewModel.CurrentViewModel) && sender is LauncherMainViewModel vm)
            {
                if (vm.CurrentViewModel != null)
                {
                    // Asegurarse de que esto se ejecuta en el hilo de la UI
                    Dispatcher.Invoke(() => {
                        UpdateWindowStateForViewModel(vm.CurrentViewModel);
                        this.Title = vm.CurrentViewModel.Title; // Actualizar el título de la ventana
                    });
                }
            }
        }

        private void UpdateWindowStateForViewModel(LauncherBaseViewModel? viewModel)
        {
            if (viewModel is WelcomeViewModel || viewModel is SignInViewModel || viewModel is LoadingViewModel)
            {
                this.Width = 420;
                this.Height = 600;
                this.MinWidth = 380;
                this.MinHeight = 500;
                this.ResizeMode = ResizeMode.CanMinimize;
            }
            else if (viewModel is MainLauncherViewModel)
            {
                this.Width = 800;
                this.Height = 550;
                this.MinWidth = 700;
                this.MinHeight = 500;
                this.ResizeMode = ResizeMode.CanResizeWithGrip;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}