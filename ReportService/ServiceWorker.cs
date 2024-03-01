
using Services;
using System.Configuration;

namespace ReportService
{
    public class ServiceWorker : BackgroundService
    {
        private readonly IPowerService _powerServiceClient;
        private readonly string _csvFilePath;
        private readonly int _scheduledIntervalSeconds;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="powerService"></param>
        /// <param name="configuration"></param>
        public ServiceWorker(IPowerService powerService, IConfiguration configuration)
        {
            _powerServiceClient = powerService;
            _csvFilePath = configuration.GetValue<string>("CsvFilePath");
            _scheduledIntervalSeconds = configuration.GetValue<int>("ScheduledIntervalSeconds");
        }

        /// <summary>
        /// Execute Task
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ExtractPowerPosition();

            while (!stoppingToken.IsCancellationRequested)
            {
                logHelper.LogInfo($"{DateTimeOffset.Now},Task Executed!");
                await Task.Delay(GetDelayUntilNextInterval(), stoppingToken);
                ExtractPowerPosition();
            }
        }

        /// <summary>
        /// Extract Power Postion Data
        /// </summary>
        private void ExtractPowerPosition()
        {
            try
            {
                logHelper.LogInfo($"Extracting Power Position...");
                var currentDateTime = DateTime.Now;
                var trades = _powerServiceClient.GetTradesAsync(currentDateTime).Result;
                var aggregatedVolumes = AggregateVolumes(trades);
                WriteToCsv(currentDateTime, aggregatedVolumes);
            }
            catch (Exception ex)
            {
                logHelper.LogError("Error occurred while extracting power position: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Aggregate Volumes of a period
        /// </summary>
        /// <param name="trades"></param>
        /// <returns></returns>
        private Dictionary<int, double> AggregateVolumes(IEnumerable<PowerTrade> trades)
        {
            var aggregatedVolumes = new Dictionary<int, double>();
            foreach (var trade in trades)
            {
                foreach (var period in trade.Periods)
                {
                    logHelper.LogInfo($"tradeDate:{trade.Date}, Period:{period.Period}, Volume:{period.Volume}");
                    var hour = period.Period - 2; // Period starts from 1
                    if (hour < 0)
                    {
                        hour += 24;
                    }
                    if (aggregatedVolumes.ContainsKey(hour))
                        aggregatedVolumes[hour] += period.Volume;
                    else
                        aggregatedVolumes[hour] = period.Volume;
                }
            }
            return aggregatedVolumes;
        }


        /// <summary>
        /// Write the aggregated volumes to csv file
        /// </summary>
        /// <param name="currentDate"></param>
        /// <param name="aggregatedVolumes"></param>
        private void WriteToCsv(DateTimeOffset currentDate, Dictionary<int, double> aggregatedVolumes)
        {
            try
            {
                using (var writer = new StreamWriter(GetCsvFileName(currentDate), false))
                {
                    writer.WriteLine("Local Time, Volume");
                    foreach (var kvp in aggregatedVolumes)
                    {
                        var localTime = kvp.Key.ToString("00") + ":00";
                        writer.WriteLine($"{localTime}, {kvp.Value}");
                    }
                }
            }
            catch (Exception ex) 
            {
                logHelper.LogError($"Failed to write to csv file!", ex);
            }
        }

        /// <summary>
        /// Generate the file name of csv file
        /// </summary>
        /// <param name="currentDateTime"></param>
        /// <returns></returns>
        private string GetCsvFileName(DateTimeOffset currentDateTime)
        {
            if (!Directory.Exists(_csvFilePath))
            {
                Directory.CreateDirectory(_csvFilePath);
            }

            var fullPath = _csvFilePath + $"\\PowerPosition_{currentDateTime:yyyyMMdd_HHmm}.csv";
            logHelper.LogInfo($"CSV file path:{fullPath}");
            return fullPath;
        }

        /// <summary>
        /// Get the delay interval base on current time and interval setting.
        /// </summary>
        /// <returns></returns>
        private int GetDelayUntilNextInterval()
        {
            var now = DateTimeOffset.Now;
            var nextInterval = now.AddSeconds(_scheduledIntervalSeconds);
            var delay = nextInterval - now;
            logHelper.LogInfo($"Next Interval: {delay.TotalMilliseconds}");
            return (int)delay.TotalMilliseconds;
        }

        /// <summary>
        /// Start Service
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logHelper.LogInfo($"{DateTimeOffset.Now},Service Started!");

            return base.StartAsync(cancellationToken);
        }
        /// <summary>
        /// Stop Service
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logHelper.LogInfo($"{DateTimeOffset.Now},Service Stopped!");

            return base.StopAsync(cancellationToken);
        }
    }
}
