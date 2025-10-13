using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameStore.Client.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly ApiService _apiService;

        public LoginPage()
        {
            InitializeComponent();
            _apiService = new ApiService();

            
            Loaded += (s, e) => LoginTextBox.Focus();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformLogin();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            
            // NavigationService.Navigate(new RegisterPage());
            
        }

        private async void TestApiButton_Click(object sender, RoutedEventArgs e)
        {
            await TestApiConnection();
        }

        private async void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformLogin();
            }
        }

        private async Task PerformLogin()
        {
            var login = LoginTextBox.Text.Trim();
            var password = PasswordBox.Password;

            // Валидация
            if (string.IsNullOrWhiteSpace(login))
            {
                ShowError("Введите логин");
                LoginTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите пароль");
                PasswordBox.Focus();
                return;
            }

            
            SetLoadingState(true);

            try
            {
                StatusText.Text = "🔐 Проверка учетных данных...";

                var user = await _apiService.AuthenticateAsync(login, password);

                if (user != null)
                {
                    
                    MainWindow.CurrentUser = user;
                    StatusText.Text = $"✅ Добро пожаловать, {user.Login}!";

                    
                    MessageBox.Show($"Успешный вход!\nБаланс: {user.Balance} ₽", "Добро пожаловать!",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    
                    if (NavigationService != null)
                    {
                        NavigationService.Navigate(new MainWindow());
                    }
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                    PasswordBox.Focus();
                    PasswordBox.SelectAll();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подключения: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async Task TestApiConnection()
        {
            SetLoadingState(true);
            StatusText.Text = "🔌 Тестирование подключения к API...";

            try
            {
                var games = await _apiService.GetGamesAsync();

                if (games != null)
                {
                    StatusText.Text = $"✅ API подключен! Доступно {games.Count} игр";
                    MessageBox.Show($"API работает корректно!\nЗагружено игр: {games.Count}",
                                  "Тест подключения", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ShowError("❌ Не удалось получить данные от API");
                }
            }
            catch (Exception ex)
            {
                ShowError($"❌ Ошибка подключения к API: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            LoginButton.IsEnabled = !isLoading;
            RegisterButton.IsEnabled = !isLoading;
            TestApiButton.IsEnabled = !isLoading;
            LoginTextBox.IsEnabled = !isLoading;
            PasswordBox.IsEnabled = !isLoading;

            if (isLoading)
            {
                LoginButton.Content = "⏳ Вход...";
            }
            else
            {
                LoginButton.Content = "Войти";
            }
        }

        private void ShowError(string message)
        {
            StatusText.Text = message;
            StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 100, 100));
        }

        
    }
}
