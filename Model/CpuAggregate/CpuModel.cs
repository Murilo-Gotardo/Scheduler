using Scheduler.Model.TaskAggregate;

namespace Scheduler.Model.CpuAggregate
{
    internal static class CpuModel
    {
        public static TaskModel? TaskSo { get; set; }

        public static double Utilization { get; set; }
    }
}
