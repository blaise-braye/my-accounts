using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using TechTalk.SpecFlow;

namespace Operations.Classification.Gmc.IntegrationTests.Steps
{
    [Binding]
    public class PostScenarioCleaner
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(PostScenarioCleaner));

        private readonly List<Func<Task>> _tasks = new List<Func<Task>>();

        public void Add(Action task)
        {
            Add(() => Task.FromResult(task));
        }

        public void Add(Func<Task> task)
        {
            lock (_tasks)
            {
                _tasks.Add(task);
            }
        }

        [AfterScenario]
        public async Task Cleanup()
        {
            List<Func<Task>> tasks;
            lock (_tasks)
            {
                tasks = _tasks.ToList();
                _tasks.Clear();
            }

            tasks.Reverse();
            for (var index = 0; index < tasks.Count; index++)
            {
                var task = tasks[index];
                try
                {
                    await task();
                }
                catch (Exception exn)
                {
                    _logger.Error($"cleanup task [{index}] failed", exn);
                }
            }
        }
    }
}