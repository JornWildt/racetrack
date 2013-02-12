using System;


namespace RacetimeDisplayGenerator
{
  public class CheckpointTimeRegistration
  {
    public string Team { get; set; }
    public string Checkpoint { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }

    #region Derived values

    public int? StartTimeFrame { get; set; }
    public int? EndTimeFrame { get; set; }

    #endregion
  }
}
