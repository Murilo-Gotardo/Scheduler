using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Scheduler.Model.CpuAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.SimulationAggregate;
using Scheduler.Model.TaskAggregate;
using Scheduler.Util;

namespace Scheduler.Infrastructure
{
    public class SimulationRepository(ICpu cpu, IConsoleLogger consoleLogger, IScheduler schedulerRepository) : ISimulation
    {
        private delegate void SchedulerMethodGroup(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime);
        
        public void SimulateScheduler()
        {
            var schedulerJson = GetSchedulerJson();
            SimulationModel simulationModel = new();
            var scheduler = SelectScheduler(schedulerJson.SchedulerName);

            switch (schedulerJson.SchedulerName.ToUpper())
            {
                case "RM" when !CalculateRateMonotonicScalability(schedulerJson):
                    WarnNotScalableScheduler();
                    return;
                case "EDF" when !CalculateEarliestDeadlineFirstScalability(schedulerJson):
                    WarnNotScalableScheduler();
                    return;
            }
            
            consoleLogger.LogInfo("O conjunto é escalonável");
            consoleLogger.LogInfo("Seguindo em frente");
            
            List<TaskModel> allTasksThroughSystem = [];
            Queue<TaskModel> readyQueue = [];

            var series = new double[schedulerJson.SimulationTime + 1];

            while (schedulerJson.SimulationTime >= simulationModel.Time)
            {
                CalculateReadyQueue(ref readyQueue, allTasksThroughSystem, schedulerJson, simulationModel);

                series[simulationModel.Time] = CpuModel.Utilization / schedulerJson.SimulationTime * 100;

                scheduler(ref readyQueue, simulationModel.Time, schedulerJson.SimulationTime);

                if (simulationModel.I < schedulerJson.TasksNumber) simulationModel.I++;

                simulationModel.Time++;
                simulationModel.TaskNumber = 0;
            }
            
            ShowSystemMetricsAndStatistics(allTasksThroughSystem, schedulerJson, series);
        }
        
        private void ShowSystemMetricsAndStatistics(List<TaskModel> allTasksThroughSystem, SchedulerModel schedulerJson, IEnumerable<double> series)
        {
            consoleLogger.LogMetrics("Tabela de execução: ");
            ShowTableOfTasks(schedulerJson.TasksNumber, schedulerJson.SimulationTime, allTasksThroughSystem);
            ShowStarvedTasks(schedulerJson, allTasksThroughSystem);
            ShowPriorityInvertedTasks(allTasksThroughSystem);
            ShowTurnAroundTime(allTasksThroughSystem);
            ShowWaitTime(allTasksThroughSystem);
            ShowLostDeadline(allTasksThroughSystem);
            cpu.ShowCpuUtilization(schedulerJson.SimulationTime, series);
        }

        private static bool CalculateRateMonotonicScalability(SchedulerModel schedulerModel)
        {
            var sum = schedulerModel.Tasks.Sum(t => (double)t.ComputationTime / t.PeriodTime);
            return sum <= schedulerModel.TasksNumber * Math.Pow(2, 1 / (double)schedulerModel.TasksNumber) - 1;
        }
        
        private static bool CalculateEarliestDeadlineFirstScalability(SchedulerModel schedulerModel)
        {
            var sum = schedulerModel.Tasks.Sum(t => (double)t.ComputationTime / t.PeriodTime);
            return sum <= 1;
        }

        private void WarnNotScalableScheduler()
        {
            consoleLogger.LogInfo("O conjunto não é escalonável");
            consoleLogger.LogInfo("Terminando a execução");
        }

        private static void CalculateReadyQueue(ref Queue<TaskModel> readyQueue, List<TaskModel> allTasksThroughSystem, SchedulerModel schedulerJson, SimulationModel simulationModel)
        {
            foreach (var task in schedulerJson.Tasks)
            {
                var idToUse = "T" + (simulationModel.TaskNumber + 1);
                
                if (!allTasksThroughSystem.Exists(t => t.Id.Equals(idToUse)))
                {
                    TaskModel newTask = new(
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
                
                if (taskToUse.Cycle * task.PeriodTime + task.Offset == simulationModel.Time && !readyQueue.Contains(taskToUse))
                {
                    if ((CpuModel.TaskSo != null && !CpuModel.TaskSo.Equals(taskToUse)) || CpuModel.TaskSo == null)
                    {
                        taskToUse.Cycle++;
                        taskToUse.AbsoluteDeadline = simulationModel.Time + taskToUse.Deadline;
                        taskToUse.EntryPoints.Add(simulationModel.Time);
                        readyQueue.Enqueue(taskToUse);
                    }
                } 
                
                if (taskToUse.Cycle * task.PeriodTime + task.Offset == simulationModel.Time && (readyQueue.Contains(taskToUse) || (CpuModel.TaskSo != null && CpuModel.TaskSo.Equals(taskToUse))))
                {
                    taskToUse.Cycle++;
                }
                
                if (simulationModel.TaskNumber < schedulerJson.TasksNumber) simulationModel.TaskNumber++;
            }
        }

        private void ShowLostDeadline(IEnumerable<TaskModel> allTasksThroughSystem)
        {
            foreach (var task in allTasksThroughSystem.Where(task => task.LostDeadlinePoints.Count != 0))
            {
                consoleLogger.LogInfo($"{task.Id} perdeu o deadline nos ticks {string.Join(", ", task.LostDeadlinePoints)}");
                consoleLogger.LogInfo($"Com uma frequencia de perca de {(double)task.ExecutePoints.Count / task.LostDeadlinePoints.Count} (ativações/percas)");
            }
        }

        private void ShowStarvedTasks(SchedulerModel scheduler, IEnumerable<TaskModel> allTasksThroughSystem)
        {
            CalculateStarvedTasks(scheduler, allTasksThroughSystem);

            foreach (var task in scheduler.StarvedTasks)
            {
                consoleLogger.LogInfo(task.Id + " sofreu starvation");
            }
        }

        private void ShowPriorityInvertedTasks(IEnumerable<TaskModel> allTasksThroughSystem)
        {
            foreach (var inversion in allTasksThroughSystem.Where(task => task.ListOfInversions.Count != 0).SelectMany(task => task.ListOfInversions))
            {
                consoleLogger.LogInfo($"Inverção de prioridade entre {inversion.HigherPriorityTask.Id} ({inversion.HigherPriorityTask.Priority}) e {inversion.LowerPriorityTask.Id} ({inversion.LowerPriorityTask.Priority}) no tick {inversion.PointOfInversion}");
            }
        }
        
        private void ShowTurnAroundTime(List<TaskModel> tasks)
        {
            double sum = 0;
            foreach (var task in tasks)
            {
                CalculateTurnAroundTimeSum(ref sum, task.ExecutedTime, task.WaitedTime);
                var tat = CalculateTurnAroundTimeForTask(task.ExecutedTime, task.WaitedTime);
                consoleLogger.LogStatistics("Turnaround time de " + task.Id + ": " + tat);
            }

            consoleLogger.LogStatistics("Turnaround time médio do sistema: " + CalculateAvgTurnAroundTime(sum, tasks.Count));
        }

        private static double CalculateAvgTurnAroundTime(double sum, int tasksNumber) => sum / tasksNumber;

        private static void CalculateTurnAroundTimeSum(ref double sum, int totalComputationTime, int totalWaitTime) => sum += CalculateTurnAroundTimeForTask(totalComputationTime, totalWaitTime);
        
        private static int CalculateTurnAroundTimeForTask(int totalComputationTime, int totalWaitTime) => totalComputationTime + totalWaitTime;

        private void ShowWaitTime(List<TaskModel> tasks)
        {
            double sum = 0;
            TaskModel? taskMaxWaitTime = null;
            TaskModel? taskMinWaitTime = null;
            var maxWaitTime = int.MinValue;
            var minWaitTime = int.MaxValue;

            foreach (var task in tasks)
            {
                CalculateWaitTimeSum(ref sum, task.WaitedTime);
                
                if (task.ExecutedTime == 0) continue;
                consoleLogger.LogMetrics("WaitTime de " + task.Id + ": " + task.WaitedTime);

                if (task.WaitedTime > maxWaitTime)
                {
                    maxWaitTime = task.WaitedTime;
                    taskMaxWaitTime = task;
                }

                if (task.WaitedTime >= minWaitTime) continue;
                minWaitTime = task.WaitedTime;
                taskMinWaitTime = task;
            }

            consoleLogger.LogMetrics($"Menor WaitTime: {taskMinWaitTime?.Id} => {minWaitTime}");
            consoleLogger.LogMetrics($"Maior WaitTime: {taskMaxWaitTime?.Id} => {maxWaitTime}");
            consoleLogger.LogMetrics(("WaitTime médio do sistema: " + CalculateAvgWaitTime(sum, tasks.Count)));
        }

        private static double CalculateAvgWaitTime(double sum, int tasksNumber) => sum / tasksNumber;

        private static void CalculateWaitTimeSum(ref double sum, int waitedTime) => sum += waitedTime;

        private static void CalculateStarvedTasks(SchedulerModel scheduler, IEnumerable<TaskModel> allTasksThroughSystem)
        {
            foreach (var task in allTasksThroughSystem.Where(task => task.ExecutedTime == 0))
            {
                scheduler.StarvedTasks.Add(task);
            }
        }

        private static void ShowTableOfTasks(int taskNumber, int totalSimulationTime, List<TaskModel> allTasksThroughSystem)
        {
            var time = new int[totalSimulationTime];

            Console.OutputEncoding = Encoding.UTF8;

            for (var i = allTasksThroughSystem.Count; i > 0; i--)
            {
                var idToSearch = "T" + i;
                Console.Write(allTasksThroughSystem.Find(t => t.Id.Equals(idToSearch)).Id +  " |");
                for (var j = 0; j < time.Length; j++)
                {
                    if (allTasksThroughSystem.ElementAt(i - 1).WaitPoints.Contains(j))
                    {
                        Console.Write(j <= 9 ? " • " : " •• ");
                    }
                    else if (allTasksThroughSystem.ElementAt(i - 1).ExecutePoints.Contains(j))
                    {
                        Console.Write(j <= 9 ? " █ " : " ██ ");
                    } 
                    else
                    {
                        Console.Write(j <= 9 ? " - " : " -- ");
                    }

                }
                Console.WriteLine();
            }

            Console.Write("Time");

            for (var i = 0; i < time.Length; i++)
            {
                Console.Write($" {i} ");
            }
            Console.WriteLine();
        }

        private static SchedulerModel GetSchedulerJson()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var p = Directory.GetParent(currentDirectory)?.FullName;
            var a = Directory.GetParent(p!)?.FullName;
            var b = Directory.GetParent(a!)?.FullName;
            var schedulerJsonDirectory = Path.Combine(b!, "SchedulerJson");;
            
            if (Directory.Exists(schedulerJsonDirectory))
            {
                var files = Directory.GetFiles(schedulerJsonDirectory);

                Console.WriteLine("Schedulers disponíveis:");
                
                var i = 0;
                foreach (var file in files)
                {
                    i++;
                    Console.WriteLine(i + " - " + Path.GetFileName(file));
                }

                var fileChoice = Console.ReadLine();

                if (!int.TryParse(fileChoice, out var choice) || choice < 1 || choice > files.Length)
                    throw new FileNotFoundException("Arquivo não encontrado");
                
                var chosenFile = files[choice - 1];
                    
                var json = JsonConvert.DeserializeObject<SchedulerModel>(File.ReadAllText(chosenFile));

                if (json != null) return json;
            }

            Console.WriteLine("O diretório não existe");
            throw new DirectoryNotFoundException();
        }

        private SchedulerMethodGroup SelectScheduler(string schedulerName)
        {
            return schedulerName.ToUpper() switch
            {
                "FCFS" => schedulerRepository.FirstComeFirstServe,
                "RR" => schedulerRepository.RoundRobin,
                "RM" => schedulerRepository.RateMonotonic,
                "EDF" => schedulerRepository.EarliestDeadlineFirst,
                _ => throw new NotImplementedException("Escalonador não suportado")
            };
        }
    }
}