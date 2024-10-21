using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using shared;
using Thread = shared.Thread;

namespace ThreadsClient.Service
{
    public class ApiService
    {
        private readonly HttpClient http;
        private readonly IConfiguration configuration;
        private readonly string baseAPI = "";

        public ApiService(HttpClient http, IConfiguration configuration)
        {
            this.http = http;
            this.configuration = configuration;
            this.baseAPI = configuration["Configuration:base_api"];
        }

        public async Task<List<Thread>> GetThreads()
        {
            string url = $"{baseAPI}/threads";
            var response = await http.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Thread>>();
            }
            else
            {
                throw new Exception($"Failed to fetch threads: {response.ReasonPhrase}");
            }
        }
        
        public async Task<Thread> CreateThread(string title, string content, string authorName)
        {
            string url = $"{baseAPI}/threads";
            HttpResponseMessage msg = await http.PostAsJsonAsync(url, new { title, content, authorName });
            string json = await msg.Content.ReadAsStringAsync();
            Thread? newThread = JsonSerializer.Deserialize<Thread>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return newThread;
        }

        public async Task<Thread> GetThread(string id)
        {
            string url = $"{baseAPI}/threads/{id}";
            return await http.GetFromJsonAsync<Thread>(url);
        }

        public async Task<Comment> CreateComment(string text, int threadId, string authorName)
        {
            string url = $"{baseAPI}/threads/{threadId}/comments";
            HttpResponseMessage msg = await http.PostAsJsonAsync(url, new { text, authorName });
            string json = await msg.Content.ReadAsStringAsync();
            Comment? newComment = JsonSerializer.Deserialize<Comment>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return newComment;
        }

        public async Task<bool> UpvoteThread(int threadId)
        {
            string url = $"{baseAPI}/threads/{threadId}/upvote";
            HttpResponseMessage response = await http.PutAsync(url, null);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DownvoteThread(int threadId)
        {
            string url = $"{baseAPI}/threads/{threadId}/downvote";
            HttpResponseMessage response = await http.PutAsync(url, null);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> UpvoteComment(int threadId, int commentId)
        {
            string url = $"{baseAPI}/threads/{threadId}/comments/{commentId}/upvote";
            HttpResponseMessage response = await http.PutAsync(url, null);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DownvoteComment(int threadId, int commentId)
        {
            string url = $"{baseAPI}/threads/{threadId}/comments/{commentId}/downvote";
            HttpResponseMessage response = await http.PutAsync(url, null);
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}
