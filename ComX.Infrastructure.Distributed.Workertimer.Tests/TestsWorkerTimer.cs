using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Workertimer.Tests
{
    public class TestsWorkerTimer
    {
        public async Task Setup()
        {
            // wait before every test to allow tests to exit any pending tasks
            // which may lead to unreal test results
            await Task.Delay(1000);
        }

        [Test]
        public async Task Worker_WillRunOnce_IfProcessTakesLessThenPeriod()
        {
            // keep value at least 100 for tests, or else
            // the timer will stop before the it will execute it's actual tasks
            int workerPeriodMS = 100;
            TimeSpan workerPeriond = TimeSpan.FromMilliseconds(workerPeriodMS);

            IConfigurationTimer config = new Configuration(workerPeriond);
            BackgroundTimerProcessor backgroundWorker = new BackgroundTimerProcessor(config);
            TestIncrementalWorker workerProcess = new TestIncrementalWorker();
            BackgroundWorker<TestIncrementalWorker> workerTimer = new BackgroundWorker<TestIncrementalWorker>(
                backgroundWorker,
                null,
                () => workerProcess);

            int runningTimes = 0;

            workerProcess.Increment += () =>
            {
                Interlocked.Increment(ref runningTimes);
            };

            await workerTimer.StartAsync(default);
            // give worker time to execute
            await Task.Delay(10);
            await workerTimer.StopAsync(default);

            // THe period is 100 ms 
            // Timer starts at T : 0
            Assert.AreEqual(runningTimes, 1);
        }

        [Test]
        public async Task Worker_WillRunMoreThenOnce_IfProcessTakesLessThenPeriod()
        {
            // keep value at least 100 for tests, or else
            // the timer will stop before the it will execute it's actual tasks
            const int workerPeriodMS = 100;
            TimeSpan workerPeriond = TimeSpan.FromMilliseconds(workerPeriodMS);

            IConfigurationTimer config = new Configuration(workerPeriond);
            BackgroundTimerProcessor backgroundWorker = new BackgroundTimerProcessor(config);
            TestIncrementalWorker workerProcess = new TestIncrementalWorker();
            BackgroundWorker<TestIncrementalWorker> workerTimer = new BackgroundWorker<TestIncrementalWorker>(
                backgroundWorker,
                null,
                () => workerProcess);

            int runningTimes = 0;

            workerProcess.Increment += () =>
            {
                Interlocked.Increment(ref runningTimes);
            };

            await workerTimer.StartAsync(default);
            // give worker time to execute
            await Task.Delay(210);
            await workerTimer.StopAsync(default);

            // THe period is 100 ms 
            // Timer starts at T : 0
            // Timer starts at T : 100
            // Timer starts at T : 200
            Assert.AreEqual(runningTimes, 3);
        }

        [Test]
        public async Task Worker_WillRunTwice_WhenProcessIsLongerThenPeriod()
        {
            // while the timer is executing, if the timer wants to start again because
            // the interval was meat, it will store the intent of starting again and 
            // return while waiting for the current process cu execute

            // this process happens to all requests to start again , there will be only one intent
            // stored

            //Reason:
            // If the worker is set to start daily, and for some reason the long running process
            // takes 1 day and 1 hour. We want that when it ends, to start again. But we don't want 
            // a second thread to start each day, because we might we working with a database that 
            // requires single thread access

            // keep value at least 100 for tests, or else
            // the timer will stop before the it will execute it's actual tasks
            const int workerPeriodMS = 1000;
            TimeSpan workerPeriond = TimeSpan.FromMilliseconds(workerPeriodMS);

            IConfigurationTimer config = new Configuration(workerPeriond);
            BackgroundTimerProcessor backgroundWorker = new BackgroundTimerProcessor(config);
            TestIncrementalWorker workerProcess = new TestIncrementalWorker(
                processDelay: TimeSpan.FromMilliseconds(1200));
            BackgroundWorker<TestIncrementalWorker> workerTimer = new BackgroundWorker<TestIncrementalWorker>(
                backgroundWorker,
                null,
                () => workerProcess);

            int runningTimes = 0;

            workerProcess.Increment += () =>
            {
                Interlocked.Increment(ref runningTimes);
            };

            await workerTimer.StartAsync(default);

            // give worker time to execute
            await Task.Delay(1210);
            await workerTimer.StopAsync(default);

            // THe period is 1000 ms 
            // Timer 0 : worker process
            // Timer 1000 : worker still processing, intent to start again stored
            // Timer 1200: worker finished processing. intent restarts the process
            Assert.AreEqual(runningTimes, 2);

        }
    }
}