using Scheduler.Model.CpuAggregate;
using Scheduler.Model.SimulationAggregate;
using Scheduler.Infrastructure;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Util;

namespace Scheduler
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConsoleLogger logger = new ConsoleLogger();
            ICpu cpu = new CpuRepository(logger);
            IScheduler scheduler = new SchedulerRepository(cpu);
            var simulationRepository = new SimulationRepository(cpu, logger, scheduler);
            
            simulationRepository.SimulateScheduler();
        }
    }
}
