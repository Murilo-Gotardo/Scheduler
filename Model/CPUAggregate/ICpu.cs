using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Model.CPUAggregate
{
    public interface ICpu
    {
        void ShowTurnAroundTime(List<TaskSoModel> tasks);

        void ShowWaitTime(List<TaskSoModel> tasks);

        void CalculateStarvedAndHalfExecTasks(SchedulerModel scheduler, List<TaskSoModel> allTasksThroughSystem);

        void CalculateTableOfTasks(int taskNumber, int totalSimulationTime, List<TaskSoModel> allTasksThroughSystem);

        void ShowCpuUtilization(int simulationTime, double[] series);
    }
}
