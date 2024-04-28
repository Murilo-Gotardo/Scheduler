using AsciiChart.Sharp;
using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;
using Scheduler.Util;
using System.Text;

namespace Scheduler.Repositories
{
    public class CPURepository(IConsoleLogger consoleLogger) : ICPU
    {
        private readonly IConsoleLogger _consoleLogger = consoleLogger;

        public void ShowTurnAroundTime(List<TaskSOModel> tasks)
        {
            double sum = 0;
            foreach (var task in tasks)
            {
                CalculateTurnAroundTimeSum(ref sum, task.CompletionTime, task.Offset);
                if (!(CalculateTurnAroundTimeForTask(task.CompletionTime, task.Offset) <= 0))
                { 
                    _consoleLogger.LogStatistics("Turnaround time de " + task.Id + ": " + CalculateTurnAroundTimeForTask(task.CompletionTime, task.Offset));
                }
            }

            _consoleLogger.LogStatistics("Turnaround time médio do sistema: " + CalculateAVGTurnAroundTime(sum, tasks.Count));
        }

        private static double CalculateAVGTurnAroundTime(double sum, int tasksNumber) 
        {
            return sum / tasksNumber;
        }

        private static void CalculateTurnAroundTimeSum(ref double sum, int completionTime, int offset) => sum += (completionTime - offset);

        private static int CalculateTurnAroundTimeForTask(int completionTime, int offset) => completionTime - offset;

        public void ShowWaitTime(List<TaskSOModel> tasks)
        {
            double sum = 0;
            TaskSOModel? taskMaxWaitTime = null;
            TaskSOModel? taskMinWaitTime = null;
            int maxWaitTime = int.MinValue;
            int minWaitTime = int.MaxValue;

            foreach (var task in tasks)
            {
                CalculateWaitTimeSum(ref sum, task.WaitedTime);
                if (task.ExecutedTime != 0)
                {
                    _consoleLogger.LogMetrics("WaitTime de " + task.Id + ": " + task.WaitedTime);

                    if (task.WaitedTime > maxWaitTime)
                    {
                        maxWaitTime = task.WaitedTime;
                        taskMaxWaitTime = task;
                    }

                    if (task.WaitedTime < minWaitTime)
                    {
                        minWaitTime = task.WaitedTime;
                        taskMinWaitTime = task;
                    }
                }
            }

            _consoleLogger.LogMetrics($"Menor WaitTime: {taskMaxWaitTime.Id} => {minWaitTime}");
            _consoleLogger.LogMetrics($"Maior WaitTime: {taskMinWaitTime.Id} => {maxWaitTime}");
            _consoleLogger.LogMetrics(("WaitTime médio do sistema: " + CalculateAVGWaitTime(sum, tasks.Count)));
        }

        private static double CalculateAVGWaitTime(double sum, int tasksNumber) => sum / tasksNumber;

        private static void CalculateWaitTimeSum(ref double sum, int waitedTime) => sum += waitedTime;

        public void CalculateStarvedAndHalfExecTasks(SchedulerModel scheduler, List<TaskSOModel> allTasksThroughSystem)
        {
            foreach (var task in allTasksThroughSystem)
            {
                if (task.ExecutedTime == 0)
                {
                    scheduler.StarvedTasks.Add(task);
                }
                else if (task.ExecutedTime < task.ComputationTime)
                {
                    scheduler.HalfExecTasks.Add(task);
                }
            }
        }

        //private void CompareMinMaxWaitTime(ref int waitedTime, ref int maxWaitTime, ref int minWaitTime, TaskSOModel task, TaskSOModel? taskMaxWaitTime, TaskSOModel? taskMinWaitTime)
        //{
        //    if (waitedTime > maxWaitTime)
        //    {
        //        maxWaitTime = waitedTime;
        //        taskMaxWaitTime = task;
        //    }

        //    if (waitedTime < minWaitTime)
        //    {
        //        minWaitTime = waitedTime;
        //        taskMinWaitTime = task;
        //    }
        //}

        public void CalculateTableOfTasks(int taskNumber, int totalSimulationTime, List<TaskSOModel> allTasksThroughSystem)
        {
            int[] tasks = new int[taskNumber], time = new int[totalSimulationTime];

            Console.OutputEncoding = Encoding.UTF8;

            for (int i = tasks.Length; i > 0; i--)
            {
                string idToSearch = "T" + i;
                Console.Write(allTasksThroughSystem.Find(t => t.Id.Equals(idToSearch)).Id +  " |");
                for (int j = 0; j < time.Length; j++)
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

            for (int i = 0; i < time.Length; i++)
            {
                Console.Write($" {i} ");
            }
            Console.WriteLine();
        }

        public void ShowCPUUtilization(int simulationTime, double[] series)
        {
            _consoleLogger.LogMetrics("Uso de CPU:");

            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(AsciiChart.Sharp.AsciiChart.Plot(series, new Options
            {
                Height = simulationTime,
                Fill = '·',
                AxisLabelFormat = "0",
            }));
        }
    }
}
