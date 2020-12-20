using System;
using InfluxDB.Client.Core;

namespace HomeController.Processors.Output.Models
{
    [Measurement("usage")]
    public record UsageModel : BaseModel
    {
        public enum State
        {
            InActive = 0,
            Active = 1
        }

        public State CurrentState { get; init; }

        [Column("state")]
        public int CurrentStateInt => (int) CurrentState;

        public bool? JustGotActivated { get; init; }

        [Column("activated")]
        public int? Activated => JustGotActivated ?? false ? 1 : null;

        [Column("deactivated")]
        public int? Deactivated => JustGotActivated ?? true ? null : 1;

        public TimeSpan Duration { get; init; }

        [Column("duration_s")]
        public double? DurationSeconds => Duration.TotalSeconds;
    }
}
