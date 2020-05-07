using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WindowsService1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IDITestClass _diTestClass;

        public Worker(ILogger<Worker> logger, IDITestClass diTestClass)
        {
            _diTestClass = diTestClass;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Service Start: {DateTimeOffset.Now}");

            _diTestClass.TestCall();

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(new TimeSpan(0, 0, 10), stoppingToken);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.LogInformation($"Canceled: {e.Message}");
            }

            _logger.LogInformation($"Service Stop: {DateTimeOffset.Now}");
        }
    }
}
