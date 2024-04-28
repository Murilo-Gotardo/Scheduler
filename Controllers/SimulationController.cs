using Scheduler.Model.SimulationAggregate;

namespace Scheduler.Controllers
{
    public class SimulationController (ISimulation simulation)
    {
        private readonly ISimulation _simulation = simulation;

        public void GetSimulation()
        {
            _simulation.SimulateScheduler();
        }
    }
}
