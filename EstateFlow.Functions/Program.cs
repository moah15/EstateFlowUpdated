using EstateFlow.Data.Context;
using EstateFlow.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host =
    new HostBuilder()
        .ConfigureFunctionsWebApplication()
        .ConfigureServices(
            (context, services) =>
            {
                var configuration =
                    context.Configuration;

                var estateFlowConnectionString =
                    configuration["EstateFlowDb"];

                if (string.IsNullOrWhiteSpace(
                        estateFlowConnectionString))
                {
                    throw new InvalidOperationException(
                        "The EstateFlow database connection string is missing. " +
                        "Set 'EstateFlowDb' in the Function App environment variables.");
                }

                services.AddDbContext<EstateFlowDbContext>(
                    options =>
                        options.UseSqlServer(
                            estateFlowConnectionString,
                            sqlOptions =>
                            {
                                sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 5,
                                    maxRetryDelay:
                                        TimeSpan.FromSeconds(10),
                                    errorNumbersToAdd: null);
                            }));

                // Typed HttpClient used by MonthlyReportFunction
                // and DiaryReminderFunction.
                services.AddHttpClient<
                    EmailNotificationService>();

                // Application Insights logging for deployed Functions.
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
            })
        .Build();

host.Run();
