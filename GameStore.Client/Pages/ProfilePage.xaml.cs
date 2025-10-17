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
using GameStore.Database.Models;

namespace GameStore.Client.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        private readonly ApiService _apiService;
        private User _currentUser;

        public ProfilePage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            Loaded += ProfilePage_Loaded;
        }

        private async void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProfileData();
        }

        private async Task LoadProfileData()
        {
            if (MainWindow.CurrentUser == null) return;

            _currentUser = MainWindow.CurrentUser;
            
            ProfileUserName.Text = _currentUser.Login;
            ProfileLogin.Text = _currentUser.Login;
            ProfileUserId.Text = _currentUser.UserId.ToString();
            ProfileBalance.Text = $"Баланс: {_currentUser.Balance} ₽";
           
            await LoadUserStatistics();
        }

        private async Task LoadUserStatistics()
        {
            try
            {
                var userLibrary = await _apiService.GetUserLibraryAsync(_currentUser.UserId);
                GamesCount.Text = userLibrary?.Count.ToString() ?? "0";

                TotalSpent.Text = $"{_currentUser.Balance * 2} ₽"; // Пример расчета

                LastActivity.Text = "Сегодня";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddBalanceButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция пополнения баланса в разработке", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PurchaseHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("История покупок в разработке", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки в разработке", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
