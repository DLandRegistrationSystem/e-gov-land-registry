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
            return await _http.GetFromJsonAsync<List<LandRecord>>("api/Land") ?? new List<LandRecord>();
        }

        public async Task AddRecordAsync(LandRecord record)
        {
            await _http.PostAsJsonAsync("api/Land", record);
        }

        public async Task AddRecord(LandRecord record)
        {
            await _http.PostAsJsonAsync("api/Land", record);
        }

        public async Task<List<LandRecord>> GetRecordsByOwner(string owner)
        {
            return await _http.GetFromJsonAsync<List<LandRecord>>($"api/Land/owner/{owner}") ?? new List<LandRecord>();
        }

        public async Task<List<LandRecord>> GetAllRecords()
        {
            return await _http.GetFromJsonAsync<List<LandRecord>>("api/Land") ?? new List<LandRecord>();
        }

        // Calls blockchain register endpoint and returns transaction digest
        public async Task<string?> RegisterLandAsync(string landId, string ownerAddress, string location, ulong areaRopani)
        {
            var payload = new { LandId = landId, OwnerAddress = ownerAddress, Location = location, AreaRopani = areaRopani };
            using var resp = await _http.PostAsJsonAsync("api/Land/register", payload);
            resp.EnsureSuccessStatusCode();
            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream);
            if (doc.RootElement.TryGetProperty("TransactionHash", out var digestEl) && digestEl.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                return digestEl.GetString();
            }
            return null;
        }
    }
}
