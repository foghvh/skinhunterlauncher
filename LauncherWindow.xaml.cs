using SkinHunterLauncher.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System;

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
                await vm.InitializeAsync();
                vm.PropertyChanged += ViewModel_PropertyChanged;

                if (this.WindowStartupLocation == WindowStartupLocation.Manual)
                {
                    CenterWindowOnScreen();
                }

                if (vm.CurrentViewModel != null)
                {
                    ApplyWindowStateForViewModel(vm.CurrentViewModel, true);
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
                        ApplyWindowStateForViewModel(vm.CurrentViewModel, true);
                        this.Title = vm.CurrentViewModel.Title;
                    });
                }
            }
        }

        private void ApplyWindowStateForViewModel(LauncherBaseViewModel? viewModel, bool recenter)
        {
            double currentLeft = this.Left;
            double currentTop = this.Top;
            double currentWidth = this.Width;
            double currentHeight = this.Height;

            double newWidth = currentWidth;
            double newHeight = currentHeight;
            ResizeMode newResizeMode = this.ResizeMode;

            if (viewModel is WelcomeViewModel || viewModel is SignInViewModel || viewModel is LoadingViewModel)
            {
                newWidth = 400;
                newHeight = 330;
                this.MinHeight = 330;
                this.MinWidth = 400;
                newResizeMode = ResizeMode.NoResize;
            }
            else if (viewModel is MainLauncherViewModel)
            {
                newWidth = 860;
                newHeight = 520;
                this.MinHeight = 500;
                this.MinWidth = 700;
                newResizeMode = ResizeMode.CanResizeWithGrip;
            }

            bool sizeChanged = (this.Width != newWidth || this.Height != newHeight);

            this.ResizeMode = newResizeMode;
            this.Width = newWidth;
            this.Height = newHeight;

            if (recenter || sizeChanged)
            {
                if (WindowStartupLocation == WindowStartupLocation.Manual || Left == 0 && Top == 0 || recenter && sizeChanged)
                {
                    CenterWindowOnScreen();
                }
            }
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = SystemParameters.WorkArea.Width; // Usar WorkArea para evitar la barra de tareas
            double screenHeight = SystemParameters.WorkArea.Height;
            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
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