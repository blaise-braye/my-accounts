using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Operations.Classification.Tests.Steps
{
    [Binding]
    public class PostScenarioCleaner
    {
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
            foreach (var task in tasks)
                try
                {
                    await task();
                }
                catch (Exception exn)
                {
                    Trace.TraceError(exn.Message);
                }
        }
    }
}