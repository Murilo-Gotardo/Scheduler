using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;

namespace Scheduler.Repositories
{
    public class FcfsSchedulerRepository : IScheduler
    {
        public void Schedule(Queue<TaskSoModel> readyQueue, int time, int totalSimulationTime)
        {
            if (CpuModel.TaskSo != null && CpuModel.TaskSo.ExecutedTime % CpuModel.TaskSo.ComputationTime == 0)
            {
                CpuModel.TaskSo.CompletionTime = time;
                CpuModel.TaskSo = null;
            }

            if (readyQueue.Count != 0 && CpuModel.TaskSo == null && time < totalSimulationTime)
            {
                CpuModel.TaskSo = readyQueue.Dequeue();
            }

            if (CpuModel.TaskSo != null)
            {
                if (time != totalSimulationTime)
                {
                    CpuModel.Utilization += 1;
                    CpuModel.TaskSo.ExecutedTime += 1;
                    CpuModel.TaskSo.ExecutePoints.Add(time);
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
