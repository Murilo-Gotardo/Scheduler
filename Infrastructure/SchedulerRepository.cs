using Scheduler.Enum;
using Scheduler.Model.CpuAggregate;
using Scheduler.Model.SchedulerAggregate;
using Scheduler.Model.TaskAggregate;

namespace Scheduler.Infrastructure;

public class SchedulerRepository(ICpu cpu) : IScheduler
{
    public void FirstComeFirstServe(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime)
    {
        cpu.VerifyTaskCompletion(time);
        cpu.AddTaskToCpu(ref readyQueue, time, totalSimulationTime);
        cpu.ProgressTask(time, totalSimulationTime);
        cpu.MakeTasksWait(readyQueue, time, totalSimulationTime);
    }

    public void RoundRobin(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime)
    {
        CalculatePriority(readyQueue, Priority.Random);
        
        cpu.VerifyTaskCompletion(time);
        if (CpuModel.TaskSo != null && CpuModel.TaskSo.ExecutedTime % CpuModel.TaskSo.Quantum == 0)
        {
            readyQueue.Enqueue(CpuModel.TaskSo);
            CpuModel.TaskSo = null;
        } 
        
        cpu.AddTaskToCpu(ref readyQueue, time, totalSimulationTime);
        cpu.ProgressTask(time, totalSimulationTime);
        cpu.MakeTasksWait(readyQueue, time, totalSimulationTime);
    }

    public void RateMonotonic(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime)
    {
        CalculatePriority(readyQueue, Priority.Rm);
        ReorderReadyQueue(ref readyQueue);
        
        cpu.VerifyTaskCompletion(time);
        if (CpuModel.TaskSo != null && readyQueue.Count != 0 && CpuModel.TaskSo.Priority > readyQueue.Peek().Priority)
        {
            readyQueue.Enqueue(CpuModel.TaskSo);
            CpuModel.TaskSo = null;
        }
        
        cpu.AddTaskToCpu(ref readyQueue, time, totalSimulationTime);
        if (CpuModel.TaskSo == null) return;
        cpu.ProgressTask(time, totalSimulationTime);
        cpu.VerifyLostDeadLine(CpuModel.TaskSo, time);
        cpu.MakeTasksWaitDeadline(readyQueue, time, totalSimulationTime);
    }

    public void EarliestDeadlineFirst(ref Queue<TaskModel> readyQueue, int time, int totalSimulationTime)
    {
        CalculatePriority(readyQueue, Priority.Edf);
        ReorderReadyQueue(ref readyQueue);
        
        cpu.VerifyTaskCompletion(time);
        if (CpuModel.TaskSo != null && readyQueue.Count != 0 && CpuModel.TaskSo.Priority > readyQueue.Peek().Priority)
        {
            readyQueue.Enqueue(CpuModel.TaskSo);
            CpuModel.TaskSo = null;
        }
        
        cpu.AddTaskToCpu(ref readyQueue, time, totalSimulationTime);
        if (CpuModel.TaskSo == null) return;
        cpu.ProgressTask(time, totalSimulationTime);
        cpu.VerifyLostDeadLine(CpuModel.TaskSo, time);
        cpu.MakeTasksWaitDeadline(readyQueue, time, totalSimulationTime);
    }
    
    private static void CalculatePriority(IReadOnlyCollection<TaskModel> readyQueue, Priority priority)
    {
        switch (priority)
        {
            case Priority.Random:
                CalculatePriorityWithRandom(readyQueue);
                break;
            case Priority.Rm:
                CalculateRmPriority(readyQueue);
                break;
            case Priority.Edf:
                CalculateEdfPriority(readyQueue);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
        }
    }

    private static void CalculatePriorityWithRandom(IReadOnlyCollection<TaskModel> readyQueue)
    {
        if (readyQueue.Count == 0) return;
        foreach (var task in readyQueue.Where(task => task.Priority == null))
        {
            task.Priority = new Random().Next(1, 6);
        }
    }

    private static void CalculateRmPriority(IReadOnlyCollection<TaskModel> readyQueue)
    {
        if (readyQueue.Count == 0) return;
        foreach (var task in readyQueue.Where(task => task.Priority == null))
        {
            task.Priority = task.Deadline;
        }
    }
    
    private static void CalculateEdfPriority(IReadOnlyCollection<TaskModel> readyQueue)
    {
        if (readyQueue.Count == 0) return;
        foreach (var task in readyQueue)
        {
            task.Priority = task.ExecutePoints.Count != 0 ? task.AbsoluteDeadline : task.Deadline;
        }
    }
    
    private static void ReorderReadyQueue(ref Queue<TaskModel> readyQueue)
    {
        readyQueue = new Queue<TaskModel>([.. readyQueue.OrderBy(t => t.Priority)]);
    }
}