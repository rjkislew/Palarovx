using System.Collections.ObjectModel;
using System.Text.Json;
using System.Net.Http.Json;

namespace Client.Palaro2026.Services
{
    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Author { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsUser { get; set; }
    }

    public class WebhookRequest
    {
        public string Message { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }

    public interface IChatService
    {
        event Action? OnMessagesChanged;
        Task<bool> SendMessageAsync(string userMessage);
        Task<List<ChatMessage>> GetMessagesAsync();
        Task ClearMessagesAsync();
        Task<string> GetResponseAsync(string userMessage);
    }

    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly APIService _apiService;
        private List<ChatMessage> _messages = new();
        private readonly string _webhookUrl = "https://workflow.pgas.ph/webhook/97125c35-98f6-4ca0-b1d9-665377cadf68/chat";
        private readonly string _sessionId = Guid.NewGuid().ToString();

        public event Action? OnMessagesChanged;

        public ChatService(HttpClient httpClient, APIService apiService)
        {
            _httpClient = httpClient;
            _apiService = apiService;
        }

        public async Task<bool> SendMessageAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return false;

            // Add user message
            var userMsg = new ChatMessage
            {
                Author = "You",
                Content = userMessage,
                IsUser = true,
                Timestamp = DateTime.UtcNow
            };
            _messages.Add(userMsg);

            // Get response from webhook
            var response = await GetResponseAsync(userMessage);

            // Add bot response
            var botMsg = new ChatMessage
            {
                Author = "Assistant",
                Content = response,
                IsUser = false,
                Timestamp = DateTime.UtcNow
            };
            _messages.Add(botMsg);

            OnMessagesChanged?.Invoke();
            return true;
        }

        public Task<List<ChatMessage>> GetMessagesAsync()
        {
            return Task.FromResult(_messages);
        }

        public Task ClearMessagesAsync()
        {
            _messages.Clear();
            OnMessagesChanged?.Invoke();
            return Task.CompletedTask;
        }

        public async Task<string> GetResponseAsync(string userMessage)
        {
            try
            {
                // Prepare the webhook request
                var request = new WebhookRequest
                {
                    Message = userMessage,
                    SessionId = _sessionId
                };

                // Send request to the webhook
                var response = await _httpClient.PostAsJsonAsync(_webhookUrl, request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Try to extract the response message
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(content);
                        var root = jsonDoc.RootElement;

                        // Check various possible response formats
                        if (root.TryGetProperty("message", out var messageElement))
                            return messageElement.GetString() ?? content;
                        if (root.TryGetProperty("response", out var responseElement))
                            return responseElement.GetString() ?? content;
                        if (root.TryGetProperty("text", out var textElement))
                            return textElement.GetString() ?? content;
                        if (root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Object)
                        {
                            if (dataElement.TryGetProperty("message", out var dataMessage))
                                return dataMessage.GetString() ?? content;
                            if (dataElement.TryGetProperty("response", out var dataResponse))
                                return dataResponse.GetString() ?? content;
                        }

                        // If no specific field found, return the whole content
                        return content;
                    }
                    catch
                    {
                        // If JSON parsing fails, return the raw content
                        return content;
                    }
                }
                else
                {
                    return $"Error from server: {response.StatusCode}";
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Connection error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
