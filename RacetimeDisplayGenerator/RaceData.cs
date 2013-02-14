using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RacetimeDisplayGenerator
{
  public class CheckpointRegistrationIndex : Dictionary<string, CheckpointRegistration>
  {
  }


  public class CheckpointAndTimeFrameIndex : Dictionary<string, Dictionary<int, List<CheckpointTimeRegistration>>>
  {
  }


  public class TimeFrameAndCheckpointIndex : Dictionary<int, Dictionary<string, List<CheckpointTimeRegistration>>>
  {
  }


  public class TeamTeamScoreIndex : Dictionary<string, List<TeamScoreRegistration>>
  {
  }


  public class TeamAndTimeFrameIndex : Dictionary<string, Dictionary<int, CheckpointTimeRegistration>>
  {
  }


  public class TeamAndTimeIntervalIndex : Dictionary<string, List<CheckpointTimeRegistration>>
  {
  }


  public class RaceData
  {
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int EndTimeFrame { get; set; }

    public List<TeamRegistration> Teams { get; set; }
    public CheckpointRegistrationIndex CheckpointRegistration { get; set; }
    public CheckpointAndTimeFrameIndex CheckpointAndTimeFrame { get; set; }
    public TimeFrameAndCheckpointIndex TimeFrameAndCheckpoint { get; set; }
    public TeamTeamScoreIndex TeamTeamScore { get; set; }
    public TeamAndTimeFrameIndex TeamAndTimeFrame { get; set; }
    public TeamAndTimeIntervalIndex TeamAndTimeInterval { get; set; }

    public RaceData()
    {
      Teams = new List<TeamRegistration>();
      CheckpointRegistration = new CheckpointRegistrationIndex();
      CheckpointAndTimeFrame = new CheckpointAndTimeFrameIndex();
      TimeFrameAndCheckpoint = new TimeFrameAndCheckpointIndex();
      TeamTeamScore = new TeamTeamScoreIndex();
      TeamAndTimeFrame = new TeamAndTimeFrameIndex();
      TeamAndTimeInterval = new TeamAndTimeIntervalIndex();
    }
    
    
    public int ConvertDateTimeToTimeFrame(DateTime d)
    {
      return (int)(d - StartTime).TotalMinutes / Configuration.InputSampleTime.Minutes;
    }


    public DateTime ConvertTimeFrameToDateTime(int d)
    {
      return StartTime + TimeSpan.FromMinutes(d * Configuration.InputSampleTime.Minutes);
    }
  }
}
