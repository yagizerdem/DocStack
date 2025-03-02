using Models;
using Models.ApiResponse;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    
        public async Task<ServiceResponse<SearchPapersResponse?>> GetWorks(string q, int offset = 0, int limit = 10)
        {
            string uri = $"search/works?q={q}&offset={offset}&limit={limit}";

            SearchPapersResponse? searchWorksResponse = null;
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

                    searchWorksResponse = JsonConvert.DeserializeObject<SearchPapersResponse>(json_string, settings);

                    if (searchWorksResponse == null || searchWorksResponse.results == null)
                    {
                        return ServiceResponse<SearchPapersResponse?>.Fail("Deserialization failed: No data returned.");
                    }
                }
                else
                {
                    return ServiceResponse<SearchPapersResponse?>.Fail("Empty response received.");
                }

                return ServiceResponse<SearchPapersResponse?>.Success(searchWorksResponse);
            }
            catch (JsonException ex)
            {
                return ServiceResponse<SearchPapersResponse?>.Fail("JSON Deserialization Error", ex);
            }
            catch (Exception ex)
            {
                return ServiceResponse<SearchPapersResponse?>.Fail("Unexpected error occurred.", ex);
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

        public async Task<ServiceResponse<object>> DownloadFromInternet(string url, string destinationPath, IProgress<double> progress = null)
        {
            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode(); 

                    long? totalBytes = response.Content.Headers.ContentLength;
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                  fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        byte[] buffer = new byte[8192];
                        long totalRead = 0;
                        int read;

                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;

                            if (totalBytes.HasValue)
                            {
                                double percentage = (double)totalRead / totalBytes.Value * 100;
                                progress?.Report(percentage);
                            }
                        }
                    }
                }

                return ServiceResponse<object>.Success(null, $"Download complete: {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading file: " + ex.Message);
                return ServiceResponse<object>.Fail($"Error downloading file : {ex.Message}" ,ex);
            }
        }

        public async Task<bool> CheckDOIExists(string doi)
        {
            string url = $"https://api.crossref.org/works/{doi}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

    }
}
