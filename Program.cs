using Scheduler.Controllers;
using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SimulationAggregate;
using Scheduler.Repositories;
using Scheduler.Repository;
using Scheduler.Util;

namespace Scheduler
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConsoleLogger logger = new ConsoleLogger();
            ICPU cpuRepository = new CPURepository(logger);
            ISimulation simulationRepository = new SimulationRepository(logger, cpuRepository);
            SimulationController simulationController = new(simulationRepository);

            Worker app = new(simulationController);
            app.Run();
        }
    }
}
