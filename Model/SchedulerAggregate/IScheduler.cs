using Newtonsoft.Json;
using Scheduler.Model.TaskSOAggregate;
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
        void Schedule(Queue<TaskSOModel> readyQueue, int time, int totalSimulationTime);
    }
}
