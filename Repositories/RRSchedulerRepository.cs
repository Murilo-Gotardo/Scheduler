using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Repositories
{
    public class RRSchedulerRepository : IScheduler
    {
        public void Schedule(Queue<TaskSOModel> readyQueue, int time, int totalSimulationTime)
        {
            throw new NotImplementedException();
        }
    }
}
