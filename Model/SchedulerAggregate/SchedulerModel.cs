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
        public List<TaskSOModel>? Tasks { get; set; }

        [JsonIgnore]
        public List<TaskSOModel> StarvedTasks { get; set; } = [];

        [JsonIgnore]
        public List<TaskSOModel> HalfExecTasks { get; set; } = [];
    }
}
