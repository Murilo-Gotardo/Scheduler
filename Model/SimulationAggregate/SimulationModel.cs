using Newtonsoft.Json;
using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SchedulerAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Model.SimulationAggregate
{
    public class SimulationModel
    {
        public int I { get; set; }

        public int TaskNumber { get; set; }

        public int Time { get; set; }
    }
}
