using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Model.CPUAggregate
{
    public interface ICPU
    {
        void ShowTurnAroundTime(List<TaskSOModel> tasks);

        void ShowWaitTime(List<TaskSOModel> tasks);

        void CalculateStarvedAndHalfExecTasks(SchedulerModel scheduler, List<TaskSOModel> allTasksThroughSystem);

        void CalculateTableOfTasks(int taskNumber, int totalSimulationTime, List<TaskSOModel> allTasksThroughSystem);

        void ShowCPUUtilization(int simulationTime, double[] series);
    }
}
