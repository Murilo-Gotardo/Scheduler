using Newtonsoft.Json;
using Scheduler.Model.TaskAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Scheduler.Model.SchedulerAggregate
{
    public interface IScheduler
    {
        void FirstComeFirstServe(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime);

        void RoundRobin(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime);

        void RateMonotonic(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime);

        void EarliestDeadlineFirst(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime);
    }
}
