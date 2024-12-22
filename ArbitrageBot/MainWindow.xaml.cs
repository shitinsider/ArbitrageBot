using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ArbitrageBot
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        private ArbitrageLogic _arbitrageLogic;
        private CancellationTokenSource _cancellationTokenSource;


        public MainWindow()
        {
            int pollingInterval;
            if (!int.TryParse(txtPollingInterval.Text, out pollingInterval) || pollingInterval <= 0)
            {
                Log("Ошибка: Некорректное значение интервала проверки.");
                return;
            }
            pollingInterval *= 1000; // Преобразуем секунды в миллисекунды

            InitializeComponent();
            Instance = this;
            _arbitrageLogic = new ArbitrageLogic(pollingInterval);
        }

        public void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText($"{DateTime.Now:yyyy.MM.dd HH:mm:ss}: {message}\n");
                txtLog.ScrollToEnd();
            });
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseSettings(out double solValue, out double slippage, out double spreadThreshold, out int pollingInterval, out int retryDelay, out string privateKey))
            {
                Log("Ошибка: Некорректные параметры. Проверьте введённые значения.");
                return;
            }

            Log("Запуск арбитража...");
            Log($"Параметры: SOL={solValue}, Slippage={slippage}, Spread={spreadThreshold}, PollingInterval={pollingInterval}ms, RetryDelay={retryDelay}ms");

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await _arbitrageLogic.RunArbitrageAsync(spreadThreshold, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Log("Арбитраж остановлен.");
            }
            catch (Exception ex)
            {
                Log($"Ошибка: {ex.Message}");
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                Log("Арбитраж успешно остановлен.");
            }
            else
            {
                Log("Арбитраж уже остановлен.");
            }
        }

        private bool TryParseSettings(out double solValue, out double slippage, out double spreadThreshold, out int pollingInterval, out int retryDelay, out string privateKey)
        {
            try
            {
                solValue = double.TryParse(txtSol.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double tempSol) ? tempSol : 0.1;
                slippage = double.TryParse(txtSlippage.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double tempSlippage) ? tempSlippage : 0.5;
                spreadThreshold = double.TryParse(txtSpread.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double tempSpread) ? tempSpread : 1.0;
                pollingInterval = int.TryParse(txtPollingInterval.Text, out int tempPolling) ? tempPolling * 1000 : 5000;
                retryDelay = int.TryParse(txtRetryDelay.Text, out int tempRetry) ? tempRetry * 1000 : 10000;
                privateKey = txtPrivateKey.Text.Trim();

                if (string.IsNullOrEmpty(privateKey))
                {
                    Log("Ошибка: Приватный ключ не указан.");
                    return false;
                }

                return true;
            }
            catch
            {
                solValue = 0;
                slippage = 0;
                spreadThreshold = 0;
                pollingInterval = 0;
                retryDelay = 0;
                privateKey = null;
                return false;
            }
        }
    }
}
