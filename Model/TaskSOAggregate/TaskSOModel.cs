using Newtonsoft.Json;
using System.Collections;

namespace Scheduler.Model.TaskSOAggregate
{
    public class TaskSOModel(int offset, int computationTime, int periodTime, string Id)
    {
        [JsonIgnore]
        public string? Id { get; set; } = Id;

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; } = offset;

        [JsonIgnore]
        public int CompletionTime { get; set; } = 0;

        [JsonProperty("computation_time")]
        public int ComputationTime { get; set; } = computationTime;

        [JsonIgnore]
        public int WaitedTime { get; set; } = 0;

        [JsonIgnore]
        public List<int> WaitPoints { get; set; } = [];

        [JsonIgnore]
        public int ExecutedTime { get; set; } = 0;

        [JsonIgnore]
        public List<int> ExecutePoints { get; set; } = [];

        [JsonProperty("period_time")]
        public int PeriodTime { get; set; } = periodTime;

        [JsonProperty("quantum")]
        public int? Quantum { get; set; }

        [JsonProperty("deadline")]
        public int? Deadline { get; set; }

        [JsonIgnore]
        public List<int> LostDeadlinePoints { get; set; } = [];
    }
}