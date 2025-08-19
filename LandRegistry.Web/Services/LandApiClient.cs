using System.Net.Http.Json;
// using LandRegistry.Api.Models;
using LandRegistry.Web.Models;

namespace LandRegistry.Web.Services
{
    public class LandApiClient
    {
        private readonly HttpClient _http;

        public LandApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<LandRecord>> GetRecordsAsync()
        {
            return await _http.GetFromJsonAsync<List<LandRecord>>("api/LandRecords") ?? new List<LandRecord>();
        }

        public async Task AddRecordAsync(LandRecord record)
        {
            await _http.PostAsJsonAsync("api/LandRecords", record);
        }

        public async Task AddRecord(LandRecord record)
        {
            await _http.PostAsJsonAsync("api/LandRecords", record);
        }

        public async Task<List<LandRecord>> GetRecordsByOwner(string owner)
        {
            return await _http.GetFromJsonAsync<List<LandRecord>>($"api/LandRecords/owner/{owner}") ?? new List<LandRecord>();
        }

        public async Task<List<LandRecord>> GetAllRecords()
        {
            return await _http.GetFromJsonAsync<List<LandRecord>>("api/LandRecords") ?? new List<LandRecord>();
        }
    }
}
