using DummyDataDog.Data;
using DummyDataDog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DummyDataDog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public ChatController(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
            _httpClient = new HttpClient();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var endpoint = _config["Gemini:Endpoint"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
                return BadRequest("Gemini API key or endpoint not configured.");

            string userMessage = request.Message.ToLower();
            string botReply;

            var recentChats = await _context.ChatLogs
                .Where(c => c.UserId == request.UserId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(20)
                .ToListAsync();

            int? projectId = recentChats.FirstOrDefault(c => c.ProjectId.HasValue)?.ProjectId;
            string dataType = recentChats.FirstOrDefault(c => !string.IsNullOrEmpty(c.DataType))?.DataType;
            string lastBotResponse = recentChats.FirstOrDefault(c => !string.IsNullOrEmpty(c.Response))?.Response;

            bool hasContext = projectId.HasValue && !string.IsNullOrEmpty(dataType) && !string.IsNullOrEmpty(lastBotResponse);

            if (userMessage.Contains("hi") || userMessage.Contains("hello"))
            {
                botReply = "👋 Hello! I am BGTAI. Please tell me the Project Name (e.g., ANZ, HSBC, XYZ)?";
            }
            else if (await _context.Projects.AnyAsync(p => p.Name.ToLower() == userMessage.ToLower()))
            {
                var project = await _context.Projects.FirstAsync(p => p.Name.ToLower() == userMessage.ToLower());
                projectId = project.Id;
                botReply = $"✅ Project '{project.Name}' selected. Now, which data type do you want me to analyze? (alerts, logs, metrics, services)";
            }
            else if (userMessage.Contains("alerts") || userMessage.Contains("logs") || userMessage.Contains("metrics") || userMessage.Contains("services"))
            {
                dataType = userMessage;
                botReply = "✅ Got it! For what time range should I check? (Example: 'From yesterday 6PM to today 8AM')";
            }
            else if (userMessage.Contains("from") && userMessage.Contains("to"))
            {
                var timeRange = ParseTimeRange(userMessage);

                if (timeRange == null)
                {
                    botReply = "❌ Sorry, I couldn't parse the time range. Please rephrase like: 'From yesterday 6PM to today 8AM'";
                }
                else if (projectId == null || string.IsNullOrEmpty(dataType))
                {
                    botReply = "❌ Missing project or data type. Please select a project and data type first.";
                }
                else
                {
                    var (fromTime, toTime) = timeRange.Value;
                    string dataSummary = await FetchDataForLLM(projectId.Value, dataType, fromTime, toTime);

                    string finalPrompt = $"{dataSummary}\n\nPlease analyze this data and provide root cause analysis, impact, and fix recommendation.";
                    botReply = await CallLLM_Gemini(finalPrompt, apiKey, endpoint);
                }
            }
            else if (hasContext)
            {
                string followUpPrompt = $"User says: '{request.Message}'. Respond like a helpful human assistant, no need to restate earlier information.";
                botReply = await CallLLM_Gemini(followUpPrompt, apiKey, endpoint);
            }
            else
            {
                botReply = "🤖 Sorry, I didn't understand that. Can you clarify?";
            }

            var chatLog = new ChatLog
            {
                UserId = request.UserId,
                Prompt = request.Message,
                Response = botReply,
                CreatedAt = DateTime.UtcNow,
                ProjectId = projectId,
                DataType = dataType,
                TimeRange = userMessage.Contains("from") && userMessage.Contains("to")
                    ? userMessage.Substring(userMessage.IndexOf("from"))
                    : null
            };

            _context.ChatLogs.Add(chatLog);
            await _context.SaveChangesAsync();

            return Ok(new { reply = botReply });
        }

        private async Task<string> CallLLM_Gemini(string prompt, string apiKey, string endpoint)
        {
            var url = $"{endpoint}?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ParseGeminiResponse(responseString);
            }
            else
            {
                return "❌ Gemini API Error: " + responseString;
            }
        }

        private string ParseGeminiResponse(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? "No text returned from Gemini.";
            }
            catch
            {
                return "❌ Failed to parse Gemini API response: " + jsonResponse;
            }
        }

        private (DateTime, DateTime)? ParseTimeRange(string message)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                message = message.ToLower();

                var durationRegex = new Regex(@"last\s+(\d+)\s*(hour|hours|minute|minutes)");
                var durationMatch = durationRegex.Match(message);
                if (durationMatch.Success)
                {
                    int value = int.Parse(durationMatch.Groups[1].Value);
                    string unit = durationMatch.Groups[2].Value;

                    DateTime fromTime = unit.Contains("hour") ? now.AddHours(-value) : now.AddMinutes(-value);
                    return (fromTime, now);
                }

                var rangeRegex = new Regex(@"from\s+(.*?)\s+to\s+(.*)", RegexOptions.IgnoreCase);
                var rangeMatch = rangeRegex.Match(message);
                if (rangeMatch.Success)
                {
                    string fromPart = rangeMatch.Groups[1].Value.Trim();
                    string toPart = rangeMatch.Groups[2].Value.Trim();

                    var fromTime = ParseNaturalDateTime(fromPart);
                    var toTime = ParseNaturalDateTime(toPart);

                    if (fromTime != null && toTime != null)
                        return (fromTime.Value, toTime.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Time range parsing error: {ex.Message}");
            }

            return null;
        }

        private DateTime? ParseNaturalDateTime(string text)
        {
            DateTime now = DateTime.UtcNow;
            text = text.ToLower();

            if (text.Contains("now")) return now;
            if (text.Contains("today morning")) return now.Date.AddHours(6);
            if (text.Contains("today")) return now.Date;
            if (text.Contains("yesterday morning")) return now.Date.AddDays(-1).AddHours(6);
            if (text.Contains("yesterday evening")) return now.Date.AddDays(-1).AddHours(18);
            if (text.Contains("yesterday")) return now.Date.AddDays(-1);

            if (DateTime.TryParse(text, out DateTime absolute))
                return absolute.ToUniversalTime();

            if (text.Contains("6pm")) return now.Date.AddHours(18);
            if (text.Contains("8am")) return now.Date.AddHours(8);

            return null;
        }

        private async Task<string> FetchDataForLLM(int projectId, string dataType, DateTime from, DateTime to)
        {
            var sb = new StringBuilder();

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            string projectName = project != null ? project.Name : $"ProjectId: {projectId}";

            sb.AppendLine("===== PROJECT DETAILS =====");
            sb.AppendLine($"Project: {projectName}");
            sb.AppendLine($"Data Type: {dataType}");
            sb.AppendLine($"Time Range: From {from} To {to}");
            sb.AppendLine();

            sb.AppendLine("===== DATA SNAPSHOT =====");

            switch (dataType.ToLower())
            {
                case "alerts":
                    var alerts = await _context.Alerts
                        .Where(a => a.ProjectId == projectId && a.CreatedAt >= from && a.CreatedAt <= to)
                        .ToListAsync();

                    foreach (var alert in alerts)
                        sb.AppendLine($"[{alert.CreatedAt}] [{alert.Severity}] {alert.Message}");

                    if (!alerts.Any()) sb.AppendLine("No alerts found for the selected time range.");
                    break;

                case "logs":
                    var logs = await _context.Logs
                        .Where(l => l.ProjectId == projectId && l.LoggedAt >= from && l.LoggedAt <= to)
                        .ToListAsync();

                    foreach (var log in logs)
                        sb.AppendLine($"[{log.LoggedAt}] [{log.LogLevel}] {log.Message}");

                    if (!logs.Any()) sb.AppendLine("No logs found for the selected time range.");
                    break;

                case "metrics":
                    var metrics = await _context.Metrics
                        .Where(m => m.ProjectId == projectId && m.CollectedAt >= from && m.CollectedAt <= to)
                        .ToListAsync();

                    foreach (var metric in metrics)
                        sb.AppendLine($"[{metric.CollectedAt}] [{metric.MetricType}] Value: {metric.Value}");

                    if (!metrics.Any()) sb.AppendLine("No metrics found for the selected time range.");
                    break;

                case "services":
                    var services = await _context.Services
                        .Where(s => s.ProjectId == projectId && s.CheckedAt >= from && s.CheckedAt <= to)
                        .ToListAsync();

                    foreach (var service in services)
                        sb.AppendLine($"[{service.CheckedAt}] Service: {service.ServiceName}, Status: {service.Status}");

                    if (!services.Any()) sb.AppendLine("No service status records found for the selected time range.");
                    break;

                default:
                    sb.AppendLine("Invalid data type provided.");
                    break;
            }

            sb.AppendLine();
            sb.AppendLine("===== ANALYSIS REQUEST =====");
            sb.AppendLine("Please analyze the above data and provide:");
            sb.AppendLine("- Root Cause Analysis");
            sb.AppendLine("- Business Impact");
            sb.AppendLine("- Fix Recommendations");
            sb.AppendLine();
            sb.AppendLine("If no significant issues are found, please mention that explicitly.");

            return sb.ToString();
        }
    }

    public class ChatRequest
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public int ProjectId { get; set; }
        public string DataType { get; set; }
    }
}
