namespace RacetimeDisplayGenerator
{
  public class TeamScoreRegistration
  {
    public string Team { get; set; }
    public string Checkpoint { get; set; }
    public int? Fixed { get; set; }
    public int? Work { get; set; }

    #region Derived values

    public int Start { get; set; }
    public int End { get; set; }

    #endregion
  }
}
