using ComX.Infrastructure.Distributed.Workertimer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ExtensionsWorkerProgramabilityTimer
    {
        #region [ Fields ]
        #endregion

        #region [ Properties ]
        #endregion

        #region [ Constructor ]
        #endregion

        #region [ Methods ]
        public static void AddWorkerProgramabilityTimer<TConfiguration, TWorkerProcess>(this IServiceCollection services)
          where TConfiguration : class, IConfigurationTimer
          where TWorkerProcess : class, IWorkerProcess
        {
            services.TryAddTransient<TWorkerProcess>();
            services.TryAddScoped<IConfigurationTimer, TConfiguration>();
            services.AddHostedService<BackgroundWorker<TWorkerProcess>>(sp =>
            {
                // we can register multiple times AddWorkerPRogramabilityTimer<> with different processes
                // because the IConfiguration is specific to each IBAckgrondWorker, we cannot regsiter it in services
                IConfigurationTimer configurationTimer = sp.CreateScope().ServiceProvider.GetService<IConfigurationTimer>();
                if (configurationTimer is null)
                {
                    configurationTimer = ActivatorUtilities.CreateInstance<TConfiguration>(sp);
                    if (configurationTimer is null)
                    {
                        throw new ApplicationException($"Could not activate or load from services the configuration " +
                            $"{typeof(TConfiguration).FullName}");
                    }
                }
                // not required. If it does not exist, it will not log
                ILoggerFactory loggerFactory = sp.CreateScope().ServiceProvider.GetService<ILoggerFactory>();

                IBackgroundProcessor backgroundWorker = new BackgroundTimerProcessor(configurationTimer);

                return new BackgroundWorker<TWorkerProcess>(
                    backgroundWorker: backgroundWorker,
                    loggerFactory: loggerFactory,
                    processFactory: () => sp.CreateScope().ServiceProvider.GetRequiredService<TWorkerProcess>());
            });
        }

        public static void AddWorkerProgramabilityTimer<TWorkerProcess>(
            this IServiceCollection services,
            Func<IServiceProvider, IConfigurationTimer> configurationProvider)
                where TWorkerProcess : class, IWorkerProcess
        {
            services.TryAddTransient<TWorkerProcess>();
            services.AddHostedService<BackgroundWorker<TWorkerProcess>>(sp =>
            {
                // we can register multiple times AddWorkerPRogramabilityTimer<> with different processes
                // because the IConfiguration is specific to each IBAckgrondWorker, we cannot regsiter it in services

                IConfigurationTimer configurationTimer = configurationProvider(sp);
                if (configurationTimer is null)
                {
                    throw new ApplicationException($"Could create the configuration from the provider method " +
                        $"{typeof(IConfigurationTimer).FullName}");
                }
                // not required. If it does not exist, it will not log
                ILoggerFactory loggerFactory = sp.GetService<ILoggerFactory>();

                IBackgroundProcessor backgroundWorker = new BackgroundTimerProcessor(configurationTimer);

                return new BackgroundWorker<TWorkerProcess>(
                    backgroundWorker: backgroundWorker,
                    loggerFactory: loggerFactory,
                    processFactory: () => sp.CreateScope().ServiceProvider.GetRequiredService<TWorkerProcess>());
            });
        }

        public static void AddWorkerProgramabilityTimer<TWorkerProcess>(
           this IServiceCollection services,
           IConfigurationTimer configuration)
           where TWorkerProcess : class, IWorkerProcess
        {
            services.TryAddTransient<TWorkerProcess>();
            services.AddHostedService<BackgroundWorker<TWorkerProcess>>(sp =>
            {
                // we can register multiple times AddWorkerPRogramabilityTimer<> with different processes
                // because the IConfiguration is specific to each IBAckgrondWorker, we cannot regsiter it in services

                if (configuration is null)
                {
                    throw new ApplicationException($"The configuration {typeof(IConfigurationTimer).FullName} cannot be null");
                }
                // not required. If it does not exist, it will not log
                ILoggerFactory loggerFactory = sp.GetService<ILoggerFactory>();

                IBackgroundProcessor backgroundWorker = new BackgroundTimerProcessor(configuration);

                return new BackgroundWorker<TWorkerProcess>(
                    backgroundWorker: backgroundWorker,
                    loggerFactory: loggerFactory,
                    processFactory: () => sp.CreateScope().ServiceProvider.GetRequiredService<TWorkerProcess>());
            });
        }
        #endregion
    }
}
