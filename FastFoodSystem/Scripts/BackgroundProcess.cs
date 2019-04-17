using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastFoodSystem.Scripts
{
    public class BackgroundProcess
    {
        private bool state;
        private Queue<Task<object>> tasks;
        private Thread thread;

        public BackgroundProcess()
        {
            state = false;
        }

        public void Start()
        {
            state = true;
            tasks = new Queue<Task<object>>();
            thread = new Thread(new ThreadStart(StartTask));
            thread.Start();
        }

        public void AddTask(Task<object> task)
        {
            if(state)
                tasks.Enqueue(task);
        }

        public void Kill()
        {
            state = false;
        }

        private void StartTask()
        {
            while (state || tasks.Count > 0)
            {
                if(tasks.Count > 0)
                {
                    var task = tasks.Dequeue();
                    task.Start();
                    task.Wait();
                    Thread.Sleep(10);
                }
            }
        }
    }
}
