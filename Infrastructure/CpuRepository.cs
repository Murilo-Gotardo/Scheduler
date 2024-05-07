using AsciiChart.Sharp;
using Scheduler.Model.CpuAggregate;
using Scheduler.Util;
using System.Text;
using Scheduler.Model.TaskAggregate;

namespace Scheduler.Infrastructure
{
    public class CpuRepository(IConsoleLogger consoleLogger) : ICpu
    {

        public void AddTaskToCpu(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime)
        {
            if (readyQueue.Count == 0 || CpuModel.TaskSo != null || time >= totalSimulationTime) return;
            CpuModel.TaskSo = readyQueue.Dequeue();
                
            if (readyQueue.Count != 0 && readyQueue.Peek().Priority < CpuModel.TaskSo.Priority)
            {
                readyQueue.Peek().ListOfInversions.Add(new PriorityInversionData(readyQueue.Peek(), CpuModel.TaskSo, time));
            }
        }
        
        public void VerifyTaskCompletion(int time)
        {
            if (CpuModel.TaskSo == null || CpuModel.TaskSo.ExecutedTime % CpuModel.TaskSo.ComputationTime != 0) return;
            CpuModel.TaskSo.CompletionTime = time;
            CpuModel.TaskSo.Deadline += CpuModel.TaskSo.PeriodTime;
            CpuModel.TaskSo = null;
        }

        public void VerifyLostDeadLine(TaskModel task, int time)
        {
            // var enter = task.Cycle * task.PeriodTime + task.Offset;
            // int computed = 0, waited = 0;
            //
            // if (task.ExecutePoints.Count == 0)
            // {
            //     waited += task.WaitPoints.Count(e => e > task.EntryPoints.Last());
            //     
            //     if (waited < task.Deadline)
            //     {
            //         task.LostDeadlinePoints.Add(time);
            //     }
            // }
            // else if (task.WaitPoints.Count == 0)
            // {
            //     computed += task.ExecutePoints.Count(e => e > task.EntryPoints.Last());
            //
            //     if (computed < task.Deadline)
            //     {
            //         task.LostDeadlinePoints.Add(time);
            //     }
            // }
            // else
            // {
            //     computed += task.ExecutePoints.Count(a => a <= task.EntryPoints.Last() && task.ExecutePoints.Contains(a));
            //
            //     waited += task.WaitPoints.Count(b => b <= task.EntryPoints.Last() && task.ExecutePoints.Contains(b));
            //     
            //     // var executeOnCycle = Math.Abs(task.ExecutePoints.Last() - task.ComputationTime * task.Cycle);
            //     // var waitedOnCycle = Math.Abs(task.WaitPoints.Last() - task.WaitedTime / task.Cycle);
            //     if (computed + waited - task.Deadline > task.Deadline)
            //     {
            //         task.LostDeadlinePoints.Add(time);
            //     }
            // }


            if (time > task.Deadline)
            {
                task.LostDeadlinePoints.Add(time);
            }
        }

        public void ProgressTask(int time, int totalSimulationTime)
        {
            if (CpuModel.TaskSo == null) return;
            if (time == totalSimulationTime) return;
            CpuModel.Utilization += 1;
            CpuModel.TaskSo.ExecutedTime += 1;
            CpuModel.TaskSo.ExecutePoints.Add(time);
        }

        public void MakeTasksWait(Queue<TaskModel> readyQueue, int time)
        {
            if (readyQueue.Count == 0) return;
            foreach (var task in readyQueue)
            {
                task.WaitPoints.Add(time);
                task.WaitedTime++;
            }
        }

        public void ShowCpuUtilization(int simulationTime, IEnumerable<double> series)
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
