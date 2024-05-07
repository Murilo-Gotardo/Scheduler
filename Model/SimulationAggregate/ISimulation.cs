using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskAggregate;

namespace Scheduler.Model.SimulationAggregate
{
    public interface ISimulation
    {
        void SimulateScheduler();
    }
}
