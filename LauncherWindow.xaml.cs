using SkinHunterLauncher.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media.Animation; // Para animaciones si se decide
using System; // Para EventArgs

namespace SkinHunterLauncher
{
    public partial class LauncherWindow : Window
    {
        public LauncherWindow(LauncherMainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += LauncherWindow_Loaded;
            // SizeChanged += LauncherWindow_SizeChanged; // Considerar si el centrado solo se hace al cambiar de VM
        }

        private async void LauncherWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LauncherMainViewModel vm)
            {
                await vm.InitializeAsync();
                vm.PropertyChanged += ViewModel_PropertyChanged;
                if (vm.CurrentViewModel != null)
                {
                    ApplyWindowStateForViewModel(vm.CurrentViewModel, false); // Aplicar estado inicial sin animación/recentrado agresivo
                    this.Title = vm.CurrentViewModel.Title;
                }
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LauncherMainViewModel.CurrentViewModel) && sender is LauncherMainViewModel vm)
            {
                if (vm.CurrentViewModel != null)
                {
                    Dispatcher.Invoke(() => {
                        ApplyWindowStateForViewModel(vm.CurrentViewModel, true); // Aplicar con posible recentrado
                        this.Title = vm.CurrentViewModel.Title;
                    });
                }
            }
        }

        private void ApplyWindowStateForViewModel(LauncherBaseViewModel? viewModel, bool recenter)
        {
            double newWidth = this.Width;
            double newHeight = this.Height;
            ResizeMode newResizeMode = this.ResizeMode;

            if (viewModel is WelcomeViewModel || viewModel is SignInViewModel || viewModel is LoadingViewModel)
            {
                newWidth = 420;
                newHeight = 600;
                newResizeMode = ResizeMode.CanMinimize;
            }
            else if (viewModel is MainLauncherViewModel)
            {
                newWidth = 800;
                newHeight = 550;
                newResizeMode = ResizeMode.CanResizeWithGrip;
            }

            this.ResizeMode = newResizeMode; // Aplicar ResizeMode primero
            this.Width = newWidth;
            this.Height = newHeight;

            if (recenter)
            {
                CenterWindowOnScreen();
            }
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            this.Left = (screenWidth / 2) - (this.Width / 2);
            this.Top = (screenHeight / 2) - (this.Height / 2);
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