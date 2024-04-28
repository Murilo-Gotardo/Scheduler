using Scheduler.Controllers;

namespace Scheduler
{
    public class Worker(SimulationController simulationController)
    {
        private readonly SimulationController _simulationController = simulationController;

        public void Run()
        {
            _simulationController.GetSimulation();
        }
    }
}
