using Scheduler.Model.SimulationAggregate;

namespace Scheduler.Controllers
{
    public class SimulationController (ISimulation simulation)
    {
        public void GetSimulation()
        {
            simulation.SimulateScheduler();
        }
    }
}
