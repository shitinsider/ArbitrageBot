using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArbitrageBot
{
    public class ArbitrageLogic
    {
        private readonly DFlowApi _dFlowApi;
        private readonly JupiterApi _jupiterApi;
        private readonly int _pollingInterval; // Интервал между проверками в миллисекундах

        public ArbitrageLogic(int pollingInterval)
        {
            _dFlowApi = new DFlowApi();
            _jupiterApi = new JupiterApi();
            _pollingInterval = pollingInterval;
        }

        public async Task RunArbitrageAsync(double spreadThreshold, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Получаем цену с DFlow
                    double dflowPrice = await _dFlowApi.GetPriceAsync();
                    MainWindow.Instance.Log($"Цена с DFlow: {dflowPrice} USDC за 1 SOL");

                    // Получаем цену с Jupiter
                    double jupiterPrice = await _jupiterApi.GetPriceAsync();
                    MainWindow.Instance.Log($"Цена с Jupiter: {jupiterPrice} USDC за 1 SOL");

                    // Вычисляем спред
                    double spread = jupiterPrice - dflowPrice;
                    MainWindow.Instance.Log($"Текущий спред: {spread} USDC");

                    // Проверяем спред
                    if (spread >= spreadThreshold)
                    {
                        MainWindow.Instance.Log($"Спред подходит для арбитража. Начинаем сделку...");
                        await ExecuteArbitrageAsync(dflowPrice, jupiterPrice, token);
                    }
                    else
                    {
                        MainWindow.Instance.Log($"Спред не подходит для арбитража. Значение спреда: {spread} USDC");
                    }

                    // Пауза между проверками
                    await Task.Delay(_pollingInterval, token);
                }
                catch (Exception ex)
                {
                    MainWindow.Instance.Log($"Ошибка: {ex.Message}");
                }
            }

            MainWindow.Instance.Log("Арбитраж успешно остановлен.");
        }

        private async Task ExecuteArbitrageAsync(double dflowPrice, double jupiterPrice, CancellationToken token)
        {
            try
            {
                MainWindow.Instance.Log($"Исполнение сделки: покупка по цене {dflowPrice}, продажа по цене {jupiterPrice}.");
                // Здесь добавьте логику выполнения транзакций через DFlow и Jupiter API.
                await Task.Delay(1000, token); // Заглушка вместо реальной операции
                MainWindow.Instance.Log("Сделка успешно выполнена.");
            }
            catch (Exception ex)
            {
                MainWindow.Instance.Log($"Ошибка при исполнении сделки: {ex.Message}");
            }
        }
    }
}
