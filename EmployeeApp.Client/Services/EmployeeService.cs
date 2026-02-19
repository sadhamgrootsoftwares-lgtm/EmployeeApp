using System.Net.Http.Json;
using EmployeeApp.Shared;

namespace EmployeeApp.Client.Services
{
    public class EmployeeService
    {
        private readonly HttpClient _httpClient;

        public EmployeeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Employee>> GetEmployees()
        {
            return await _httpClient.GetFromJsonAsync<List<Employee>>("api/Employees") ?? new List<Employee>();
        }

        public async Task<Employee?> GetEmployee(int id)
        {
            return await _httpClient.GetFromJsonAsync<Employee>($"api/Employees/{id}");
        }

        public async Task CreateEmployee(Employee employee)
        {
            await _httpClient.PostAsJsonAsync("api/Employees", employee);
        }

        public async Task UpdateEmployee(int id, Employee employee)
        {
            await _httpClient.PutAsJsonAsync($"api/Employees/{id}", employee);
        }

        public async Task DeleteEmployee(int id)
        {
            await _httpClient.DeleteAsync($"api/Employees/{id}");
        }
    }
}
