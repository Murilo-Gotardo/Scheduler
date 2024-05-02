using Newtonsoft.Json;
using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.SimulationAggregate;
using Scheduler.Model.TaskSOAggregate;
using Scheduler.Util;

namespace Scheduler.Repositories
{
    public class SimulationRepository(IConsoleLogger consoleLogger, ICpu cpu) : ISimulation
    {
        public void SimulateScheduler()
        {
            SchedulerModel schedulerJson = GetSchedulerJson();
            SimulationModel simulationModel = new();
            IScheduler scheduler = SelectScheduler(schedulerJson.SchedulerName);

            List<TaskSoModel> allTasksThroughSystem = [];
            Queue<TaskSoModel> readyQueue = [];

            var series = new double[schedulerJson.SimulationTime + 1];

            while (schedulerJson.SimulationTime >= simulationModel.Time)
            {
                CalculateReadyQueue(ref readyQueue, allTasksThroughSystem, schedulerJson, simulationModel);

                series[simulationModel.Time] = CpuModel.Utilization / schedulerJson.SimulationTime * 100;

                scheduler.Schedule(readyQueue, simulationModel.Time, schedulerJson.SimulationTime);

                if (simulationModel.I < schedulerJson.TasksNumber) simulationModel.I++;

                simulationModel.Time++;
                simulationModel.TaskNumber = 0;
            }

            ShowSystemTasksOnTable(schedulerJson.TasksNumber, schedulerJson.SimulationTime, allTasksThroughSystem);
            ShowStarvedAndHalfExecTasks(schedulerJson, allTasksThroughSystem);
            ShowSystemMetricsAndStatistics(allTasksThroughSystem, schedulerJson.SimulationTime, series);
        }

        private static void CalculateReadyQueue(ref Queue<TaskSoModel> readyQueue, List<TaskSoModel> allTasksThroughSystem, SchedulerModel schedulerJson, SimulationModel simulationModel)
        {
            foreach (var task in schedulerJson.Tasks)
            {
                var idToUse = "T" + (simulationModel.TaskNumber + 1);
                
                if (!allTasksThroughSystem.Exists(t => t.Id.Equals(idToUse)))
                {
                    TaskSoModel newTask = new(
                        task.Offset,
                        task.ComputationTime,
                        task.PeriodTime,
                        idToUse
                    );

                    allTasksThroughSystem.Add(newTask);
                }
                
                var taskToUse = allTasksThroughSystem.Find(t => t.Id.Equals(idToUse));

                if (task.Quantum != null)
                {
                    taskToUse.Quantum = task.Quantum;
                }

                if (task.Deadline != null)
                {
                    taskToUse.Deadline = task.Deadline;
                }

                if (taskToUse.Cicle * task.PeriodTime + task.Offset == simulationModel.Time)
                {
                    taskToUse.Cicle++;
                    readyQueue.Enqueue(taskToUse);
                    readyQueue = new Queue<TaskSoModel>([.. readyQueue.OrderBy(t => t.Priority)]);
                }
                
                if (simulationModel.TaskNumber < schedulerJson.TasksNumber) simulationModel.TaskNumber++;
            }
        }

        private void ShowSystemTasksOnTable(int taskNumber, int totalSimulationTime, List<TaskSoModel> allTasksThroughSystem)
        {
            consoleLogger.LogMetrics("Mostrando tabela de execução");
            cpu.CalculateTableOfTasks(taskNumber, totalSimulationTime, allTasksThroughSystem);
        }

        private void ShowStarvedAndHalfExecTasks(SchedulerModel scheduler, List<TaskSoModel> allTasksThroughSystem)
        {
            cpu.CalculateStarvedAndHalfExecTasks(scheduler, allTasksThroughSystem);

            foreach (var task in scheduler.StarvedTasks)
            {
                consoleLogger.LogInfo(task.Id + " sofreu starvation");
            }

            foreach (var task in scheduler.HalfExecTasks)
            {
                consoleLogger.LogInfo(task.Id + " não executou totalmente");
            }
        }

        private void ShowSystemMetricsAndStatistics(List<TaskSoModel> tasks, int simulationTime, double[] series)
        {
            cpu.ShowTurnAroundTime(tasks);
            cpu.ShowWaitTime(tasks);
            cpu.ShowCpuUtilization(simulationTime, series);
        }

        public SchedulerModel GetSchedulerJson()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var p = Directory.GetParent(currentDirectory)?.FullName;
            var a = Directory.GetParent(p!)?.FullName;
            var b = Directory.GetParent(a!)?.FullName;
            var schedulerJsonDirectory = Path.Combine(b!, "SchedulerJson");;

            // Verifica se o diretório existe
            if (Directory.Exists(schedulerJsonDirectory))
            {
                // Obtém uma matriz com o nome de todos os arquivos no diretório
                var files = Directory.GetFiles(schedulerJsonDirectory);

                Console.WriteLine("Schedulers disponíveis:");

                // Itera sobre cada arquivo e imprime seu nome
                var i = 0;
                foreach (var file in files)
                {
                    i++;
                    Console.WriteLine(i + " - " + Path.GetFileName(file));
                }

                var fileChoice = Console.ReadLine();

                if (int.TryParse(fileChoice, out int choice) && choice >= 1 && choice <= files.Length)
                {
                    var chosenFile = files[choice - 1];
                    
                    var json = JsonConvert.DeserializeObject<SchedulerModel>(File.ReadAllText(chosenFile));

                    return json;
                }
                else
                {
                    throw new FileNotFoundException("Arquivo não encontrado");
                }
            }
            else
            {
                Console.WriteLine("O diretório não existe");
                throw new DirectoryNotFoundException();
            }
        }

        private static IScheduler SelectScheduler(string schedulerName)
        {
            return schedulerName.ToUpper() switch
            {
                "FCFS" => new FcfsSchedulerRepository(),
                "RR" => new RrSchedulerRepository(),
                "RM" => new RmSchedulerRepository(),
                _ => throw new NotImplementedException("Escalonador não suportado")
            };
        }
    }
}
