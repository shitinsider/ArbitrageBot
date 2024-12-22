using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ArbitrageBot
{
    public class DFlowApi
    {
        private const string ApiUrl = "https://quote-api.dflow.net/quote";

        public async Task<double> GetPriceAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Формируем строку запроса
                    string url = $"{ApiUrl}?inputMint=So11111111111111111111111111111111111111112" +
                                 $"&outputMint=EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v" +
                                 $"&amount=1000000000&slippageBps=50"; // 1 SOL, проскальзывание 0.5%

                    MainWindow.Instance.Log($"Отправка GET-запроса на DFlow: {url}");

                    // Выполняем GET-запрос
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"DFlow API Error: {response.StatusCode}, {errorContent}");
                    }

                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Логирование полного ответа
                    MainWindow.Instance.Log($"Ответ DFlow: {responseBody}");

                    // Десериализация ответа
                    var responseData = JsonConvert.DeserializeObject<DFlowQuoteResponse>(responseBody);

                    if (responseData == null || string.IsNullOrEmpty(responseData.OutAmount))
                    {
                        throw new Exception("Некорректный ответ от DFlow API: отсутствуют необходимые данные.");
                    }

                    // Возвращаем цену в формате USDC
                    return Convert.ToDouble(responseData.OutAmount) / 1000000.0; // USDC имеет 6 десятичных знаков
                }
            }
            catch (Exception ex)
            {
                MainWindow.Instance.Log($"Ошибка получения цены с DFlow: {ex.Message}");
                throw;
            }
        }
    }

    // Класс для десериализации ответа
    public class DFlowQuoteResponse
    {
        [JsonProperty("outAmount")]
        public string OutAmount { get; set; }
    }
}
