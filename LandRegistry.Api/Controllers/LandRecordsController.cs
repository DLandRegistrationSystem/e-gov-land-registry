using LandRegistry.Api.Data;
using LandRegistry.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.Json;

namespace LandRegistry.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LandController : ControllerBase
    {
        private readonly LandDbContext _context;
        private readonly string packageId = "0x69373aab510d125280e775c4c99a826bf6b7be29a98747a9e8c95ebc5fbf4ad2";
        private readonly string adminCapId = "0x3ab049e510886e46608305a605de10f6c541d60b5194e4c43494947e50cc0630";

        public LandController(LandDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LandRecord>>> GetRecords()
        {
            return await _context.LandRecords.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<LandRecord>> AddRecord(LandRecord record)
        {
            _context.LandRecords.Add(record);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRecords), new { id = record.Id }, record);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterLand([FromBody] RegisterLandDto dto)
        {
            // Basic validation: ensure hex-prefixed IDs/addresses and required fields
            dto.LandId = dto.LandId?.Trim();
            dto.OwnerAddress = dto.OwnerAddress?.Trim();
            dto.Location = dto.Location?.Trim();

            if (string.IsNullOrWhiteSpace(dto.LandId))
                return BadRequest(new { Error = "LandId is required." });

            if (string.IsNullOrWhiteSpace(dto.OwnerAddress) || !Regex.IsMatch(dto.OwnerAddress, "^0x[0-9a-fA-F]{64}$"))
                return BadRequest(new { Error = "OwnerAddress must be 0x followed by 64 hex digits (e.g., 0x...64 hex...)." });

            if (string.IsNullOrWhiteSpace(adminCapId) || !Regex.IsMatch(adminCapId, "^0x[0-9a-fA-F]{64}$"))
                return BadRequest(new { Error = "Server adminCapId is not a valid 0x + 64-hex Sui object ID. Contact admin." });

            if (string.IsNullOrWhiteSpace(dto.Location))
                return BadRequest(new { Error = "Location is required." });

            if (dto.AreaRopani == 0)
                return BadRequest(new { Error = "AreaRopani must be greater than 0." });

            // Prepare CLI arguments (UNtyped to match your current CLI behavior)
            // Order matters: &AdminCap first, then land_id (string), owner (address), location (string), area (u64)
            string args = $"client call " +
                          $"--package {packageId} " +
                          $"--module land_registry " +
                          $"--function register_land " +
                          $"--args {adminCapId} \"{dto.LandId}\" {dto.OwnerAddress} \"{dto.Location}\" {dto.AreaRopani} " +
                          $"--gas-budget 20000000 " +
                          $"--json";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sui",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            // Try to extract transaction hash (digest) from JSON output, fallback to text parser
            string txHash = ExtractTransactionHashFromJson(output) ?? ExtractTransactionHash(output);

            if (!string.IsNullOrEmpty(txHash))
            {
                return Ok(new { TransactionHash = txHash, Output = output });
            }
            else
            {
                return BadRequest(new { Error = error, Output = output });
            }
        }

        // Simple extractor for transaction hash (adjust regex as needed)
        private string ExtractTransactionHash(string output)
        {
            // Example: look for "digest: <hash>"
            var marker = "digest: ";
            var idx = output.IndexOf(marker);
            if (idx >= 0)
            {
                var start = idx + marker.Length;
                var end = output.IndexOf('\n', start);
                if (end > start)
                    return output.Substring(start, end - start).Trim();
                else
                    return output.Substring(start).Trim();
            }
            return null;
        }

        // Robust JSON extractor: find the first string property named "digest" anywhere in the JSON tree
        private string ExtractTransactionHashFromJson(string output)
        {
            try
            {
                using var doc = JsonDocument.Parse(output);
                if (TryFindDigest(doc.RootElement, out var digest))
                {
                    return digest;
                }
            }
            catch
            {
                // ignore parse errors and fallback to text extraction
            }
            return null;
        }

        private bool TryFindDigest(JsonElement element, out string? digest)
        {
            digest = null;
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        if (prop.NameEquals("digest") && prop.Value.ValueKind == JsonValueKind.String)
                        {
                            digest = prop.Value.GetString();
                            return true;
                        }
                        if (TryFindDigest(prop.Value, out digest))
                            return true;
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        if (TryFindDigest(item, out digest))
                            return true;
                    }
                    break;
            }
            return false;
        }
    }

    // DTO for land registration
    public class RegisterLandDto
    {
        public string? LandId { get; set; }
        public string? OwnerAddress { get; set; }
        public string? Location { get; set; }
        public ulong AreaRopani { get; set; }
    }
}
