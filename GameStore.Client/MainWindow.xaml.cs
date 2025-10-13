using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GameStore.Database.Models;
using System.Windows.Controls;

namespace GameStore.Client
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Game> Games { get; set; }
        public static User CurrentUser { get; set; }
        private readonly ApiService _apiService;

        public MainWindow()
        {
            InitializeComponent();

            _apiService = new ApiService();
            Games = new ObservableCollection<Game>();
            GamesItemsControl.ItemsSource = Games;

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGamesAsync();
            UpdateUserInfo();
        }

        // 🔹 Загрузка игр из API
        private async Task LoadGamesAsync()
        {
            StatusText.Text = "🔄 Загрузка игр...";

            try
            {
                var games = await _apiService.GetGamesAsync();

                Games.Clear();
                foreach (var game in games)
                {
                    Games.Add(game);
                }

                StatusText.Text = $"✅ Загружено {games.Count} игр";
            }
            catch (Exception ex)
            {
                StatusText.Text = "❌ Ошибка загрузки игр";
                MessageBox.Show($"Ошибка загрузки игр: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 🔹 Загрузка библиотеки пользователя
        private async Task LoadUserLibraryAsync()
        {
            if (CurrentUser == null) return;

            StatusText.Text = "🔄 Загрузка библиотеки...";

            try
            {
                var library = await _apiService.GetUserLibraryAsync(CurrentUser.UserId);

                // Здесь можно обновить интерфейс библиотеки
                StatusText.Text = $"✅ В библиотеке {library.Count} игр";
            }
            catch (Exception ex)
            {
                StatusText.Text = "❌ Ошибка загрузки библиотеки";
                MessageBox.Show($"Ошибка загрузки библиотеки: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUserInfo()
        {
            if (CurrentUser != null)
            {
                UserNameText.Text = CurrentUser.Login;
                BalanceText.Text = $"Баланс: {CurrentUser.Balance} ₽";
            }
            else
            {
                UserNameText.Text = "Не авторизован";
                BalanceText.Text = "Баланс: 0 ₽";
            }
        }

        // Перетаскивание окна
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        // Кнопки управления окном
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Навигация
        private async void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            ContentTitle.Text = "🏠 Главная";
            StatusText.Text = "Переход на главную страницу";
            await LoadGamesAsync();
        }

        private async void GamesButton_Click(object sender, RoutedEventArgs e)
        {
            ContentTitle.Text = "🎮 Все игры";
            StatusText.Text = "Загрузка списка игр...";
            await LoadGamesAsync();
        }

        private async void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            ContentTitle.Text = "📚 Моя библиотека";
            if (CurrentUser != null)
            {
                await LoadUserLibraryAsync();
            }
            else
            {
                StatusText.Text = "❌ Войдите в систему для просмотра библиотеки";
                MessageBox.Show("Пожалуйста, войдите в систему", "Авторизация",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ContentTitle.Text = "👤 Профиль";
            if (CurrentUser != null)
            {
                StatusText.Text = $"Профиль пользователя: {CurrentUser.Login}";
                // Здесь можно открыть окно редактирования профиля
            }
            else
            {
                StatusText.Text = "❌ Войдите в систему";
            }
        }

        // Поиск
        private async void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await LoadGamesAsync();
                return;
            }

            // Фильтрация на клиенте (можно сделать на сервере)
            try
            {
                var allGames = await _apiService.GetGamesAsync();
                var filteredGames = allGames.FindAll(g =>
                    g.GameName.ToLower().Contains(searchText) ||
                    g.Genre.ToLower().Contains(searchText));

                Games.Clear();
                foreach (var game in filteredGames)
                {
                    Games.Add(game);
                }

                StatusText.Text = $"🔍 Найдено {filteredGames.Count} игр по запросу: {searchText}";
            }
            catch (Exception ex)
            {
                StatusText.Text = "❌ Ошибка поиска";
            }
        }

        // Покупка игры
        private async void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser == null)
            {
                MessageBox.Show("Пожалуйста, войдите в систему для покупки игр", "Авторизация",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as Button;
            var game = button?.DataContext as Game;

            if (game != null)
            {
                try
                {
                    // Создаем запись в библиотеке
                    var userLibrary = new UserLibrary
                    {
                        UserId = CurrentUser.UserId,
                        GameId = game.GameId,
                        PurchaseDate = DateTime.Now
                    };

                    var success = await _apiService.AddToLibraryAsync(userLibrary);

                    if (success)
                    {
                        MessageBox.Show($"Игра '{game.GameName}' успешно добавлена в библиотеку!", "Покупка",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        StatusText.Text = $"✅ Игра '{game.GameName}' добавлена в библиотеку";
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при покупке игры", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при покупке: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 🔐 Кнопка входа
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //var loginWindow = new LoginWindow(_apiService);
            //loginWindow.UserAuthenticated += (user) =>
            //{
            //    CurrentUser = user;
            //    UpdateUserInfo();
            //    StatusText.Text = $"✅ Добро пожаловать, {user.Login}!";
            //};
            //loginWindow.ShowDialog();
        }
    }
}