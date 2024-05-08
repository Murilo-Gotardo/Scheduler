using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Model.CpuAggregate
{
    public interface ICpu
    {
        void AddTaskToCpu(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime);
        
        void VerifyTaskCompletion(int time);

        void VerifyLostDeadLine(TaskModel task, int time);
        
        void ShowCpuUtilization(int simulationTime, IEnumerable<double> series);

        void ProgressTask(int time, int totalSimulationTime);

        void MakeTasksWait(Queue<TaskModel> readyQueue, int time, int totalSimulationTime);

        void MakeTasksWaitDeadline(Queue<TaskModel> readyQueue, int time, int totalSimulationTime);
    }
}
