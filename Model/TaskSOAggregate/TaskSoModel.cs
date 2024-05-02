using Newtonsoft.Json;
using System.Collections;

namespace Scheduler.Model.TaskSOAggregate
{
    public class TaskSoModel(int offset, int computationTime, int periodTime, string id)
    {
        [JsonIgnore]
        public string? Id { get; } = id;

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; } = offset;

        [JsonIgnore]
        public int CompletionTime { get; set; }

        [JsonProperty("computation_time")]
        public int ComputationTime { get; set; } = computationTime;

        [JsonIgnore]
        public int WaitedTime { get; set; }

        [JsonIgnore]
        public List<int> WaitPoints { get; set; } = [];

        [JsonIgnore]
        public int ExecutedTime { get; set; }

        [JsonIgnore]
        public List<int> ExecutePoints { get; set; } = [];

        [JsonProperty("period_time")]
        public int PeriodTime { get; set; } = periodTime;
        
        [JsonIgnore]
        public int Cicle { get; set; }

        [JsonProperty("quantum")]
        public int? Quantum { get; set; }

        [JsonProperty("deadline")]
        public int? Deadline { get; set; }

        [JsonIgnore]
        public List<int> LostDeadlinePoints { get; set; } = [];
    }
}