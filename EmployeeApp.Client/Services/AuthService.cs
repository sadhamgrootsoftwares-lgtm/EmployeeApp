using System.Net.Http.Json;
using EmployeeApp.Shared;
using Microsoft.AspNetCore.Components.Authorization;

namespace EmployeeApp.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<AuthResponse> Register(RegisterModel model)
        {
            var result = await _httpClient.PostAsJsonAsync("api/Auth/register", model);
            return await result.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse { IsSuccess = false, Message = "Unknown error" };
        }

        public async Task<AuthResponse> Login(LoginModel model)
        {
            var result = await _httpClient.PostAsJsonAsync("api/Auth/login", model);
            var response = await result.Content.ReadFromJsonAsync<AuthResponse>();

            if (response != null && response.IsSuccess)
            {
                // In a real app, you'd save the token to LocalStorage/SessionStorage
                // For this demo, we'll notify the state provider
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(response.Token);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", response.Token);
                return response;
            }
            return response ?? new AuthResponse { IsSuccess = false, Message = "Invalid login attempt" };
        }

        public void Logout()
        {
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
