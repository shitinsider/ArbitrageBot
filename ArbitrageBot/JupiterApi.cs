using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ArbitrageBot
{
    public class JupiterApi
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string QuoteApiUrl = "https://quote-api.jup.ag/v1/quote";

        public async Task<double> GetPriceAsync()
        {
            try
            {
                string inputMint = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"; // USDC
                string outputMint = "So11111111111111111111111111111111111111112"; // SOL
                ulong amount = 1000000; // 1 USDC в минимальных единицах

                string requestUrl = $"{QuoteApiUrl}?inputMint={inputMint}&outputMint={outputMint}&amount={amount}";

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Jupiter API Error: {response.StatusCode}, {errorContent}");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(jsonResponse);

                if (data == null || data.quote == null || data.quote.outputAmount == null)
                {
                    throw new Exception($"Некорректный ответ от Jupiter API: {jsonResponse}");
                }

                return (double)data.quote.outputAmount / 1000000000;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения цены с Jupiter: {ex.Message}");
            }
        }
    }
}
