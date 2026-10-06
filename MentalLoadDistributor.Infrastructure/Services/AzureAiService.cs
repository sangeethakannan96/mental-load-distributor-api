using MentalLoadDistributor.Core.Ports;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace MentalLoadDistributor.Infrastructure.Services
{
    public class AzureAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AzureAiService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> AskAsync(string prompt)
        {
            var endpoint = GetSetting("AI:ProjectEndpoint");
            var apiKey = GetSetting("AI:ApiKey");
            var model = GetSetting("AI:Model");

            var requestBody = new
            {
                model = model,
                input = prompt
            };

            return await SendRequestAsync(endpoint, apiKey, requestBody);
        }

        public async Task<string> AskStructuredAsync(
            string prompt,
            object schema,
            string schemaName)
        {
            var endpoint = GetSetting("AI:ProjectEndpoint");
            var apiKey = GetSetting("AI:ApiKey");
            var model = GetSetting("AI:Model");

            var requestBody = new
            {
                model = model,
                input = prompt,

                text = new
                {
                    format = new
                    {
                        type = "json_schema",
                        name = schemaName,
                        schema = schema,
                        strict = true
                    }
                }
            };

            return await SendRequestAsync(
                endpoint,
                apiKey,
                requestBody);
        }

        private async Task<string> SendRequestAsync(
     string endpoint,
     string apiKey,
     object requestBody)
        {
            var requestUrl = $"{endpoint.TrimEnd('/')}/openai/v1/responses";

            var requestJson = JsonSerializer.Serialize(
                requestBody,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                requestUrl);

            request.Headers.Add("api-key", apiKey);

            request.Content = new StringContent(
                requestJson,
                Encoding.UTF8,
                "application/json");

            Console.WriteLine("======================================");
            Console.WriteLine("Azure AI Request");
            Console.WriteLine($"URL: {requestUrl}");
            Console.WriteLine("Body:");
            Console.WriteLine(requestJson);
            Console.WriteLine("======================================");

            try
            {
                var response =
                    await _httpClient.SendAsync(request);

                var responseContent =
                    await response.Content.ReadAsStringAsync();

                Console.WriteLine("======================================");
                Console.WriteLine("Azure AI Response");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine("Body:");
                Console.WriteLine(responseContent);
                Console.WriteLine("======================================");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Azure AI request failed. " +
                        $"Status: {response.StatusCode}. " +
                        $"Response: {responseContent}");
                }

                using var document =
                    JsonDocument.Parse(responseContent);

                if (!document.RootElement.TryGetProperty(
                        "output",
                        out var output))
                {
                    throw new InvalidOperationException(
                        "Azure AI response did not contain an output property.");
                }

                foreach (var outputItem in output.EnumerateArray())
                {
                    if (!outputItem.TryGetProperty(
                            "content",
                            out var content))
                        continue;

                    foreach (var contentItem in content.EnumerateArray())
                    {
                        if (contentItem.TryGetProperty(
                                "text",
                                out var text))
                        {
                            return text.GetString()
                                   ?? string.Empty;
                        }
                    }
                }

                throw new InvalidOperationException(
                    "Azure AI response did not contain text output.");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Error while processing Azure AI response.",
                    ex);
            }
        }

        private string GetSetting(string key)
        {
            var value = _configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"{key} is not configured.");
            }

            return value;
        }
    }
}