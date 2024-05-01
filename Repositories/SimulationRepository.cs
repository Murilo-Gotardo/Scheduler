﻿using Newtonsoft.Json;
using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.SimulationAggregate;
using Scheduler.Model.TaskSOAggregate;
using Scheduler.Repositories;
using Scheduler.Util;

namespace Scheduler.Repository
{
    public class SimulationRepository(IConsoleLogger consoleLogger, ICPU cpu) : ISimulation
    {
        private readonly IConsoleLogger _consoleLogger = consoleLogger;
        private readonly ICPU _cpu = cpu;

        public void SimulateScheduler()
        {
            SchedulerModel schedulerJson = GetSchedulerJson();
            SimulationModel simulationModel = new();
            IScheduler scheduler = SelectScheduler(schedulerJson.SchedulerName);

            List<TaskSOModel> allTasksThroughSystem = [];
            Queue<TaskSOModel> readyQueue = [];

            var series = new double[schedulerJson.SimulationTime + 1];

            while (schedulerJson.SimulationTime >= simulationModel.Time)
            {
                CalculateReadyQueue(ref readyQueue, allTasksThroughSystem, schedulerJson, simulationModel);

                series[simulationModel.Time] = CPUModel.Utilization / schedulerJson.SimulationTime * 100;

                scheduler.Schedule(readyQueue, simulationModel.Time, schedulerJson.SimulationTime);

                if (simulationModel.I < schedulerJson.TasksNumber) simulationModel.I++;

                simulationModel.Time++;
                simulationModel.TaskNumber = 0;
            }

            ShowSystemTasksOnTable(schedulerJson.TasksNumber, schedulerJson.SimulationTime, allTasksThroughSystem);
            ShowStarvedAndHalfExecTasks(schedulerJson, allTasksThroughSystem);
            ShowSystemMetricsAndStatistics(allTasksThroughSystem, schedulerJson.SimulationTime, series);
        }

        private static void CalculateReadyQueue(ref Queue<TaskSOModel> readyQueue, List<TaskSOModel> allTasksThroughSystem, SchedulerModel schedulerJson, SimulationModel simulationModel)
        {
            foreach (var task in schedulerJson.Tasks)
            {
                //TODO: melhora esse negocio aqui (task.Offset + simulationModel.Time) % task.PeriodTime == 0), que nao ta funfando
                if ((task.Offset == simulationModel.Time || (task.Offset + simulationModel.Time) % task.PeriodTime == 0) && simulationModel.Time != schedulerJson.SimulationTime)
                {
                    TaskSOModel? newTask = null;

                    string idToUse = "T" + (simulationModel.TaskNumber + 1);

                    if (!allTasksThroughSystem.Exists(t => t.Id.Equals(idToUse)))
                    {
                        newTask = new(
                            task.Offset,
                            task.ComputationTime,
                            task.PeriodTime,
                            idToUse
                        );

                        allTasksThroughSystem.Add(newTask);
                    }

                    if (task.Quantum != null)
                    {
                        allTasksThroughSystem.Find(t => t.Id.Equals(idToUse)).Quantum = task.Quantum;
                    }

                    if (task.Deadline != null)
                    {
                        allTasksThroughSystem.Find(t => t.Id.Equals(idToUse)).Deadline = task.Deadline;
                    }

                    

                    readyQueue.Enqueue(allTasksThroughSystem.Find(t => t.Id.Equals(idToUse)));
                    readyQueue = new Queue<TaskSOModel>([.. readyQueue.OrderBy(t => t.Priority)]);
                }

                if (simulationModel.TaskNumber < schedulerJson.TasksNumber) simulationModel.TaskNumber++;
            }
        }

        private void ShowSystemTasksOnTable(int taskNumber, int totalSimulationTime, List<TaskSOModel> allTasksThroughSystem)
        {
            _consoleLogger.LogMetrics("Mostrando tabela de execução");
            _cpu.CalculateTableOfTasks(taskNumber, totalSimulationTime, allTasksThroughSystem);
        }

        private void ShowStarvedAndHalfExecTasks(SchedulerModel scheduler, List<TaskSOModel> allTasksThroughSystem)
        {
            _cpu.CalculateStarvedAndHalfExecTasks(scheduler, allTasksThroughSystem);

            foreach (var task in scheduler.StarvedTasks)
            {
                _consoleLogger.LogInfo(task.Id + " sofreu starvation");
            }

            foreach (var task in scheduler.HalfExecTasks)
            {
                _consoleLogger.LogInfo(task.Id + " não executou totalmente");
            }
        }

        private void ShowSystemMetricsAndStatistics(List<TaskSOModel> tasks, int simulationTime, double[] series)
        {
            _cpu.ShowTurnAroundTime(tasks);
            _cpu.ShowWaitTime(tasks);
            _cpu.ShowCPUUtilization(simulationTime, series);
        }

        public SchedulerModel GetSchedulerJson()
        {
            string schedulerJsonDirectory = @"..\..\..\SchedulerJson\";

            // Verifica se o diretório existe
            if (Directory.Exists(schedulerJsonDirectory))
            {
                // Obtém uma matriz com o nome de todos os arquivos no diretório
                string[] files = Directory.GetFiles(schedulerJsonDirectory);

                Console.WriteLine("Schedulers disponíveis:");

                // Itera sobre cada arquivo e imprime seu nome
                int i = 0;
                foreach (string file in files)
                {
                    i++;
                    Console.WriteLine(i + " - " + Path.GetFileName(file));
                }

                string fileChoice = Console.ReadLine();

                if (int.TryParse(fileChoice, out int choice) && choice >= 1 && choice <= files.Length)
                {
                    string chosenFile = files[choice - 1];
                    
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
                "FCFS" => new FCFSSchedulerRepository(),
                "RR" => new RRSchedulerRepository(),
                "RM" => new RMSchedulerRepository(),
                _ => throw new NotImplementedException("Escalonador não suportado")
            };
        }
    }
}
