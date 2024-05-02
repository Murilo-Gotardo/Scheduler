using Newtonsoft.Json;
using Scheduler.Model.TaskSOAggregate;

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
        public List<TaskSoModel>? Tasks { get; set; }

        [JsonIgnore]
        public List<TaskSoModel> StarvedTasks { get; set; } = [];

        [JsonIgnore]
        public List<TaskSoModel> HalfExecTasks { get; set; } = [];
    }
}
