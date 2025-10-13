using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GameStore.Database.Models;
using Newtonsoft.Json;

namespace GameStore.Client
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "https://localhost:44346/api/";

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);

            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<Game>> GetGamesAsync()
        {
            var respone = await _httpClient.GetAsync("Games");

            return JsonConvert.DeserializeObject<List<Game>>(await respone.Content.ReadAsStringAsync());
        }

        public async Task<Game> GetGameAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Games/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Game>(json);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting game: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateGameAsync(Game game)
        {
            try
            {
                var json = JsonConvert.SerializeObject(game);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("Games", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating game: {ex.Message}");
                return false;
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("Users");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<User>>(json);
                }
                return new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting users: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<User> AuthenticateAsync(string login, string password)
        {
            try
            {
                var users = await GetUsersAsync();
                return users?.Find(u => u.Login == login && u.PasswordHash == password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UserLibrary>> GetUserLibraryAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"UserLibrary/user/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<UserLibrary>>(json);
                }
                return new List<UserLibrary>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user library: {ex.Message}");
                return new List<UserLibrary>();
            }
        }

        public async Task<bool> AddToLibraryAsync(UserLibrary userLibrary)
        {
            try
            {
                var json = JsonConvert.SerializeObject(userLibrary);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("UserLibrary", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to library: {ex.Message}");
                return false;
            }
        }




    }
}
