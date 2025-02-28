using Models;
using Models.ApiResponse;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class NetworkService
    {
        private HttpClient _httpClient;
        private SemaphoreSlim _semaphoreSlim;
        public NetworkService()
        {
            string Secret = Environment.GetEnvironmentVariable("CoreApiToken")!;
            _httpClient = new();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Secret);
            _httpClient.BaseAddress = new Uri("https://api.core.ac.uk/v3/");

            _semaphoreSlim = new(2, 2);
        }
    
        public async Task<ServiceResponse<SearchWorksResponse?>> GetWorks(string q, int offset = 0, int limit = 10)
        {
            string uri = $"search/works?q={q}&offset={offset}&limit={limit}";

            SearchWorksResponse? searchWorksResponse = null;
            try
            {
                ServiceResponse<string> response = await SendRequestWithRetry(uri);
                var json_string = response.Data;

                if (!string.IsNullOrWhiteSpace(json_string))
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore 
                    };

                    searchWorksResponse = JsonConvert.DeserializeObject<SearchWorksResponse>(json_string, settings);

                    if (searchWorksResponse == null || searchWorksResponse.results == null)
                    {
                        return ServiceResponse<SearchWorksResponse?>.Fail("Deserialization failed: No data returned.");
                    }
                }
                else
                {
                    return ServiceResponse<SearchWorksResponse?>.Fail("Empty response received.");
                }

                return ServiceResponse<SearchWorksResponse?>.Success(searchWorksResponse);
            }
            catch (JsonException ex)
            {
                return ServiceResponse<SearchWorksResponse?>.Fail("JSON Deserialization Error", ex);
            }
            catch (Exception ex)
            {
                return ServiceResponse<SearchWorksResponse?>.Fail("Unexpected error occurred.", ex);
            }

        }

        private async Task<ServiceResponse<string>> SendRequestWithRetry(string uri, int maxRetry = 3)
        {
            var random = new Random();
            string errorMessage = null;

            await _semaphoreSlim.WaitAsync();
            try
            {
                for (int i = 0; i < maxRetry; i++)
                {
                    try
                    {
                        HttpResponseMessage result = await _httpClient.GetAsync(uri);

                        if (!result.IsSuccessStatusCode)
                        {
                            errorMessage = $"HTTP Error: {result.StatusCode}";
                            continue;
                        }

                        string content = await result.Content.ReadAsStringAsync();
                        return ServiceResponse<string>.Success(content);
                    }
                    catch (HttpRequestException ex) 
                    {
                        if (errorMessage == null) errorMessage = ex.Message;

                        int delay = (int)(200 * Math.Pow(2, i)) + random.Next(100);
                        await Task.Delay(delay);
                    }
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            return ServiceResponse<string>.Fail(errorMessage ?? "Request failed after retries.");
        }

        private async Task<ServiceResponse<string>> SendRequest(string uri)
        {
            try
            {
                HttpResponseMessage result = await _httpClient.GetAsync(uri);

                if (!result.IsSuccessStatusCode)
                {
                    return ServiceResponse<string>.Fail($"HTTP Error: {result.StatusCode}");
                }

                string content = await result.Content.ReadAsStringAsync();
                return ServiceResponse<string>.Success(content);
            }
            catch (HttpRequestException ex)
            {
                return ServiceResponse<string>.Fail("Network error occurred.", ex);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Fail("An unexpected error occurred.", ex);
            }
        }

    }
}
