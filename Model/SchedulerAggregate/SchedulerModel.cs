using Newtonsoft.Json;
using Scheduler.Model.TaskAggregate;

namespace Scheduler.Model.SchedulerAggregate
{
    public class SchedulerModel
    {
        [JsonProperty("simulation_time")]
        public int SimulationTime { get; set; }

        [JsonProperty("scheduler_name")]
        public string? SchedulerName { get; set; }

        [JsonProperty("tasks_number")]
        public int TasksNumber { get; set; }

        [JsonProperty("tasks")]
        public List<TaskModel>? Tasks { get; set; }

        [JsonIgnore]
        public List<TaskModel> StarvedTasks { get; } = [];

        [JsonIgnore]
        public List<TaskModel> HalfExecTasks { get; } = [];
    }
}
