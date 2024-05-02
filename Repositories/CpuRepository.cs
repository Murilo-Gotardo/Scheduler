using AsciiChart.Sharp;
using Scheduler.Model.CPUAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskSOAggregate;
using Scheduler.Util;
using System.Text;

namespace Scheduler.Repositories
{
    public class CpuRepository(IConsoleLogger consoleLogger) : ICpu
    {
        public void ShowTurnAroundTime(List<TaskSoModel> tasks)
        {
            double sum = 0;
            foreach (var task in tasks)
            {
                CalculateTurnAroundTimeSum(ref sum, task.CompletionTime, task.Offset);
                if (!(CalculateTurnAroundTimeForTask(task.CompletionTime, task.Offset) <= 0))
                { 
                    consoleLogger.LogStatistics("Turnaround time de " + task.Id + ": " + CalculateTurnAroundTimeForTask(task.CompletionTime, task.Offset));
                }
            }

            consoleLogger.LogStatistics("Turnaround time médio do sistema: " + CalculateAvgTurnAroundTime(sum, tasks.Count));
        }

        private static double CalculateAvgTurnAroundTime(double sum, int tasksNumber) 
        {
            return sum / tasksNumber;
        }

        private static void CalculateTurnAroundTimeSum(ref double sum, int completionTime, int offset) => sum += (completionTime - offset);

        private static int CalculateTurnAroundTimeForTask(int completionTime, int offset) => completionTime - offset;

        public void ShowWaitTime(List<TaskSoModel> tasks)
        {
            double sum = 0;
            TaskSoModel? taskMaxWaitTime = null;
            TaskSoModel? taskMinWaitTime = null;
            var maxWaitTime = int.MinValue;
            var minWaitTime = int.MaxValue;

            foreach (var task in tasks)
            {
                CalculateWaitTimeSum(ref sum, task.WaitedTime);
                if (task.ExecutedTime != 0)
                {
                    consoleLogger.LogMetrics("WaitTime de " + task.Id + ": " + task.WaitedTime);

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

            consoleLogger.LogMetrics($"Menor WaitTime: {taskMinWaitTime?.Id} => {minWaitTime}");
            consoleLogger.LogMetrics($"Maior WaitTime: {taskMaxWaitTime?.Id} => {maxWaitTime}");
            consoleLogger.LogMetrics(("WaitTime médio do sistema: " + CalculateAvgWaitTime(sum, tasks.Count)));
        }

        private static double CalculateAvgWaitTime(double sum, int tasksNumber) => sum / tasksNumber;

        private static void CalculateWaitTimeSum(ref double sum, int waitedTime) => sum += waitedTime;

        public void CalculateStarvedAndHalfExecTasks(SchedulerModel scheduler, List<TaskSoModel> allTasksThroughSystem)
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

        public void CalculateTableOfTasks(int taskNumber, int totalSimulationTime, List<TaskSoModel> allTasksThroughSystem)
        {
            int[] tasks = new int[taskNumber], time = new int[totalSimulationTime];

            Console.OutputEncoding = Encoding.UTF8;

            for (var i = tasks.Length; i > 0; i--)
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

        public void ShowLostDeadline(List<TaskSoModel> allTasksThroughSystem)
        {

        }

        public void ShowCpuUtilization(int simulationTime, double[] series)
        {
            consoleLogger.LogMetrics("Uso de CPU:");

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
