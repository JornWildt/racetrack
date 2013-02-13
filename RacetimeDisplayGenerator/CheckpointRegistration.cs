using SharpKml.Base;
namespace RacetimeDisplayGenerator
{
  public enum CheckpointType { Race, Intermediate }

  public class CheckpointRegistration
  {
    public string Checkpoint { get; set; }
    public int MaxPoint { get; set; }

    #region Derived values

    public CheckpointType Type { get; set; }
    public Vector Location { get; set; }

    #endregion
  }
}
