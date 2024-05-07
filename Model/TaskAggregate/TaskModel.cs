using System.Collections;
using Newtonsoft.Json;

namespace Scheduler.Model.TaskAggregate
{
    public class TaskModel(int offset, int computationTime, int periodTime, string id)
    {
        [JsonIgnore]
        public string? Id { get; } = id;
        
        [JsonProperty("offset")]
        public int Offset { get; set; } = offset;
        
        [JsonProperty("computation_time")]
        public int ComputationTime { get; set; } = computationTime;
        
        [JsonProperty("period_time")]
        public int PeriodTime { get; set; } = periodTime;
        
        [JsonProperty("quantum")]
        public int? Quantum { get; set; }
        
        [JsonProperty("deadline")]
        public int? Deadline { get; set; }

        [JsonIgnore]
        public double? Priority { get; set; }
        
        [JsonIgnore]
        public int CompletionTime { get; set; }

        [JsonIgnore]
        public int WaitedTime { get; set; }

        [JsonIgnore]
        public int ExecutedTime { get; set; }
        
        [JsonIgnore]
        public int Cycle { get; set; }
        
        [JsonIgnore]
        public List<int> WaitPoints { get; } = [];
        
        [JsonIgnore]
        public List<int> ExecutePoints { get; } = [];

        [JsonIgnore] public List<int> EntryPoints { get; } = [];

        [JsonIgnore]
        public List<int> LostDeadlinePoints { get; } = [];

        [JsonIgnore] 
        public List<PriorityInversionData> ListOfInversions { get; } = [];
    }
    
    public struct PriorityInversionData(TaskModel higherPriorityTask, TaskModel lowerPriorityTask, int pointOfInversion)
    {
        public TaskModel HigherPriorityTask { get; set; } = higherPriorityTask;

        public TaskModel LowerPriorityTask { get; set; } = lowerPriorityTask;

        public int PointOfInversion { get; set; } = pointOfInversion;
    }
}