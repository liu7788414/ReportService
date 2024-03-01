using Microsoft.Extensions.Logging;
using Services;

namespace ReportService
{

    public class Program
    {
        public static void Main(string[] args)
        {
            logHelper.LogInfo("Program Start!!!");

            IHost host = Host.CreateDefaultBuilder(args)
                         .ConfigureAppConfiguration((hostContext, config) =>
                         {
                             config.AddJsonFile("appsettings.json", optional: true);
                             config.AddEnvironmentVariables();
                         })
                         .UseWindowsService(options =>
                         {
                             //Naming the service,it can be shown in the Windows service list.
                             options.ServiceName = "PowerReportService";
                         })
                         .ConfigureServices(services =>
                         {
                             services.AddHostedService<ServiceWorker>();
                             services.AddSingleton<IPowerService, PowerService>();
                         })
                         .Build();

            host.Run();
        }

        //public static async Task Main(string[] args)
        //{
        //    await CreateHostBuilder(args).Build().RunAsync();
        //}

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureAppConfiguration((hostContext, config) =>
        //        {
        //            config.AddJsonFile("appsettings.json", optional: true);
        //            config.AddEnvironmentVariables();
        //        })
        //        .ConfigureServices((hostContext, services) =>
        //        {
        //            services.AddHostedService<ServiceWorker>();
        //            //services.AddSingleton<IPowerServiceClient, PowerServiceClient>();
        //        });
    }

}