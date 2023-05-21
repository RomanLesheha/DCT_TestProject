using DCT_TestProject.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DCT_TestProject.Models
{
    class CoinCapApiParser:IApiParser
    {
        private readonly HttpClient _client;

        public CoinCapApiParser()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://api.coincap.io");
        }

        public async Task<T> ParseAsync<T>(string endpoint)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                // Handle the exception here or throw a custom exception
                Console.WriteLine($"An error occurred while parsing the data: {ex.Message}");
                return default; // or throw an exception if appropriate for your use case
            }
        }
    }
}
