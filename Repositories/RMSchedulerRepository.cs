using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Repositories
{
    public class RMSchedulerRepository : IScheduler
    {
        public void Schedule(Queue<TaskSOModel> readyQueue, int time, int totalSimulationTime)
        {
            if (CPUModel.TaskSO != null)
            {
                if (CPUModel.TaskSO.ComputationTime == CPUModel.TaskSO.ExecutedTime)
                {
                    CPUModel.TaskSO.CompletionTime = time;
                    CPUModel.TaskSO = null;
                }
                else if (CPUModel.TaskSO.Priority < readyQueue.Peek().Priority)
                {
                    readyQueue.Enqueue(CPUModel.TaskSO);
                    CPUModel.TaskSO = null;
                }
            }

            if (readyQueue.Count != 0 && CPUModel.TaskSO == null && time < totalSimulationTime)
            {
                CPUModel.TaskSO = readyQueue.Dequeue();
            }

            if (CPUModel.TaskSO != null)
            {
                if (time != totalSimulationTime)
                {
                    CPUModel.Utilization += 1;
                    CPUModel.TaskSO.ExecutedTime += 1;
                    CPUModel.TaskSO.ExecutePoints.Add(time);
                }

                if (readyQueue.Count != 0)
                {
                    foreach (var task in readyQueue)
                    {
                        task.WaitPoints.Add(time);
                        task.WaitedTime++;
                    }
                }
            }
        }
    }
}
