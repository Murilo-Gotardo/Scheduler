using Scheduler.Controllers;

namespace Scheduler
{
    public class Worker(SimulationController simulationController)
    {
        public void Run()
        {
            simulationController.GetSimulation();
        }
    }
}
