using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;

namespace Scheduler.Repository
{
    public class FCFSSchedulerRepository : IScheduler
    {
        public void Schedule(Queue<TaskSOModel> readyQueue, int time, int totalSimulationTime)
        {
            if (CPUModel.TaskSO != null && CPUModel.TaskSO.ExecutedTime % CPUModel.TaskSO.ComputationTime == 0)
            {
                CPUModel.TaskSO.CompletionTime = time;
                CPUModel.TaskSO = null;
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
