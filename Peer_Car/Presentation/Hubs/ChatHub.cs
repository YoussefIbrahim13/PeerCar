using Microsoft.AspNetCore.SignalR;
using Peer_Car.Application.Interfaces;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.IO;
using Microsoft.Extensions.Configuration; // 🚩 لازم تضيف دي عشان الـ IConfiguration

namespace Peer_Car.Presentation.Hubs
{
    public class ChatHub : Hub
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration; // 🚩 مكان تخزين الإعدادات

        // التعديل هنا: ضفنا IConfiguration في الـ Constructor
        public ChatHub(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(2);
            _configuration = configuration;
        }

        public async Task SendMessage(string message)
        {
            // 1. إرسال رسالة اليوزر فوراً للواجهة
            await Clients.Caller.SendAsync("ReceiveMessage", "You", message);

            try
            {
                // 🚩 سحب اللينك من ملف appsettings.json
                var aiServiceUrl = _configuration["AiServiceUrl"];

                // 2. تجهيز البيانات
                var requestData = new { message = message };
                var jsonPayload = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // 3. إرسال الطلب باستخدام اللينك اللي سحبناه
                var request = new HttpRequestMessage(HttpMethod.Post, aiServiceUrl)
                {
                    Content = content
                };

                // نطلب الرد كـ Stream عشان يظهر كلمة بكلمة (الروشنة المطلوبة 😉)
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);

                    // نبعت إشارة للمتصفح يجهز الـ Bubble بتاعة المساعد
                    await Clients.Caller.SendAsync("StartAssistantMessage");

                    char[] buffer = new char[10];
                    int bytesRead;

                    while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        string chunk = new string(buffer, 0, bytesRead);
                        // نبعت "الحتة" اللي قريناها فوراً للمتصفح (Streaming)
                        await Clients.Caller.SendAsync("ReceiveChunk", chunk);
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", "Assistant", "عذراً، المساعد الذكي غير متاح حالياً.");
                }
            }
            catch (Exception ex)
            {
                // لو حصلت مشكلة في الاتصال ببايثون
                await Clients.Caller.SendAsync("ReceiveMessage", "System", "حدث خطأ في الاتصال بالذكاء الاصطناعي.");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    public class ChatResponse { public string Reply { get; set; } }
}