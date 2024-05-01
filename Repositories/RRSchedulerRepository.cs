﻿using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;

namespace Scheduler.Repositories
{
    public class RRSchedulerRepository : IScheduler
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
                else if (CPUModel.TaskSO.ExecutedTime % CPUModel.TaskSO.Quantum == 0)
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
