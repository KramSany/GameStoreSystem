using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GameStore.Database.Models;
using GameStore.Client.Pages;
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
            
            LoginPage loginPage = new LoginPage();
            loginPage.ShowDialog();
            IsUser(loginPage);

            _apiService = new ApiService();
            Games = new ObservableCollection<Game>();
            GamesItemsControl.ItemsSource = Games;


            MainMenuBtn.Checked += MainMenuBtn_Checked;
            AllGamesBtn.Checked += AllGamesBtn_Checked;
            LibraryBtn.Checked += LibraryBtn_Checked;
            ProfileBtn.Checked += ProfileBtn_Checked;

            Loaded += MainWindow_Loaded;
        }

        private void IsUser(LoginPage loginPage)
        {
            if (loginPage.AuthUser == null)
                this.Close();
            else
                CurrentUser = loginPage.AuthUser;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadGamesAsync();
            UpdateUserInfo();
            UpdateContent();
        }

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

        private async Task LoadUserLibraryAsync()
        {
            if (CurrentUser == null) return;

            StatusText.Text = "🔄 Загрузка библиотеки...";

            try
            {
                var library = await _apiService.GetUserLibraryAsync(CurrentUser.UserId);

                Games.Clear();
                foreach (var obj in library)
                {
                    Games.Add(obj.Game);
                }

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

        private void UpdateContent()
        {
            if (ProfileBtn.IsChecked == true)
            {
                ProfileSection.Visibility = Visibility.Visible;
                ContentTitle.Text = "👤 Профиль";
                StatusText.Text = "Просмотр профиля пользователя";

                Games.Clear();
            }
            else
            {
                ProfileSection.Visibility = Visibility.Collapsed;

                if (MainMenuBtn.IsChecked == true)
                {
                    ContentTitle.Text = "🏠 Главная";
                    StatusText.Text = "Добро пожаловать в GameStore!";
                }
                else if (AllGamesBtn.IsChecked == true)
                {
                    ContentTitle.Text = "🎮 Все игры";
                    StatusText.Text = "Просмотр каталога игр";
                }
                else if (LibraryBtn.IsChecked == true)
                {
                    ContentTitle.Text = "📚 Моя библиотека";
                    StatusText.Text = "Просмотр вашей игровой библиотеки";
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private async void MainMenuBtn_Checked(object sender, RoutedEventArgs e)
        {
            UpdateContent();
            await LoadGamesAsync();
        }

        private async void AllGamesBtn_Checked(object sender, RoutedEventArgs e)
        {
            UpdateContent();
            await LoadGamesAsync();
        }

        private async void LibraryBtn_Checked(object sender, RoutedEventArgs e)
        {
            UpdateContent();

            if (CurrentUser != null)
            {
                await LoadUserLibraryAsync();
            }
            else
            {
                StatusText.Text = "❌ Войдите в систему для просмотра библиотеки";
                MessageBox.Show("Пожалуйста, войдите в систему", "Авторизация",
                              MessageBoxButton.OK, MessageBoxImage.Warning);

                AllGamesBtn.IsChecked = true;
            }
        }

        private void ProfileBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (CurrentUser == null)
            {
                StatusText.Text = "❌ Войдите в систему для просмотра профиля";
                MessageBox.Show("Пожалуйста, войдите в систему", "Авторизация",
                              MessageBoxButton.OK, MessageBoxImage.Warning);

                AllGamesBtn.IsChecked = true;
                return;
            }

            UpdateContent();
        }

        private async void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                if (AllGamesBtn.IsChecked == true || MainMenuBtn.IsChecked == true)
                {
                    await LoadGamesAsync();
                }
                else if (LibraryBtn.IsChecked == true && CurrentUser != null)
                {
                    await LoadUserLibraryAsync();
                }
                return;
            }

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
                    
                    if (CurrentUser.Balance < game.Price)
                    {
                        MessageBox.Show($"Недостаточно средств на балансе!\nТребуется: {game.Price} ₽\nНа балансе: {CurrentUser.Balance} ₽",
                                      "Недостаточно средств",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var userLibrary = new UserLibrary
                    {
                        UserId = CurrentUser.UserId,
                        GameId = game.GameId,
                        PurchaseDate = DateTime.Now
                    };

                    var success = await _apiService.AddToLibraryAsync(userLibrary);

                    if (success)
                    {
                        
                        CurrentUser.Balance -= (decimal)game.Price;
                        UpdateUserInfo();

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
    }
}