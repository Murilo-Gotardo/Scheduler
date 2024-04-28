using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;

namespace Scheduler.Model.SimulationAggregate
{
    public interface ISimulation
    {
        void SimulateScheduler();

        SchedulerModel GetSchedulerJson();
    }
}
