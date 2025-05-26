using SkinHunterLauncher.ViewModels;
using System.Windows.Controls;

namespace SkinHunterLauncher.Views
{
    public partial class SignInView : UserControl
    {
        public SignInView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SignInViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }
    }
}