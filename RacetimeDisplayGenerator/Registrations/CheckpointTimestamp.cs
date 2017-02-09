using System;


namespace RacetimeDisplayGenerator.Registrations
{
  public class CheckpointTimestamp
  {
    public string Team { get; set; }
    public string Checkpoint { get; set; }
    public DateTime? Time { get; set; }
    public string Remark { get; set; }
  }
}
