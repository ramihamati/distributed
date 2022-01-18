using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Workertimer.Tests
{
    public class TestsWorkerTimerDependencyInjection
    {
        [Test]
        public void DependencyInjection_ShouldInject_ExtensionType1()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ServiceInjected>();
            services.AddWorkerProgramabilityTimer<TestInjectionWorker>(new Configuration(TimeSpan.FromSeconds(100)));
            ServiceProvider sp = services.BuildServiceProvider();

            IHostedService worker = sp.GetService<IHostedService>();
            TestInjectionWorker workerProcess = sp.GetService<TestInjectionWorker>();

            Assert.IsNotNull(worker, "The worker was not found");
            Assert.IsNotNull(workerProcess, "THe worker process was not found");
            Assert.IsNotNull(workerProcess.Service, "The injection of a service in the worker process failed");
        }

        [Test]
        public void DependencyInjection_ShouldInject_ExtensionType2()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ServiceInjected>();
            services.AddWorkerProgramabilityTimer<TestInjectionWorker>(
                (sp) => new Configuration(TimeSpan.FromSeconds(100)));
            
            ServiceProvider sp = services.BuildServiceProvider();

            IHostedService worker = sp.GetService<IHostedService>();
            TestInjectionWorker workerProcess = sp.GetService<TestInjectionWorker>();

            Assert.IsNotNull(worker, "The worker was not found");
            Assert.IsNotNull(workerProcess, "THe worker process was not found");
            Assert.IsNotNull(workerProcess.Service, "The injection of a service in the worker process failed");
        }

        [Test]
        public void DependencyInjection_ShouldInject_ExtensionType3()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<ServiceInjected>();
            services.AddWorkerProgramabilityTimer<AsServiceConfiguration, TestInjectionWorker>();
            ServiceProvider sp = services.BuildServiceProvider();

            IHostedService worker = sp.GetService<IHostedService>();
            TestInjectionWorker workerProcess = sp.GetService<TestInjectionWorker>();

            Assert.IsNotNull(worker, "The worker was not found");
            Assert.IsNotNull(workerProcess, "THe worker process was not found");
            Assert.IsNotNull(workerProcess.Service, "The injection of a service in the worker process failed");
        }
    }
}