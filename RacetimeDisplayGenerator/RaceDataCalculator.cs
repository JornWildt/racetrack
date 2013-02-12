using System;
using System.Linq;
using System.Collections.Generic;


namespace RacetimeDisplayGenerator
{
  public class RaceDataCalculator
  {
    public RaceData CalcuateData(IList<CheckpointTimeRegistration> checkpointTimes)
    {
      RaceData data = new RaceData();
      InitializeRegistrations(data, checkpointTimes);

      foreach (CheckpointTimeRegistration reg in checkpointTimes)
      {
        UpdateCheckpointAndTimeIndex(data.CheckpointAndTimeFrame, reg);
        UpdateTimeFrameAndCheckpointIndex(data.TimeFrameAndCheckpoint, reg);
        UpdateTeamAndTimeIndex(data.TeamAndTimeFrame, reg);
        UpdateTeamAndTimeIntervalIndex(data.TeamAndTimeInterval, reg);
      }

      return data;
    }

    
    private void InitializeRegistrations(RaceData data, IList<CheckpointTimeRegistration> checkpointTimes)
    {
      DateTime? start = null;
      DateTime? end = null;

      // Validate data and calculate min/max 
      foreach (CheckpointTimeRegistration reg in checkpointTimes)
      {
        if (reg.Start == null && reg.End != null)
          reg.Start = reg.End - Configuration.OutputSampleTime;
        else if (reg.End == null && reg.Start != null)
          reg.End = reg.Start + Configuration.OutputSampleTime;

        if (start == null || reg.Start != null && reg.Start < start)
          start = reg.Start;
        if (end == null || reg.End != null && reg.End > end)
          end = reg.End;

        reg.Team = reg.Team.Trim();
      }

      data.StartTime = Trim(start.Value, Configuration.OutputSampleTime);
      data.EndTime = end.Value;
      data.EndTimeFrame = data.ConvertDateTimeToTimeFrame(data.EndTime);

      // Update checkpoint time frames
      foreach (CheckpointTimeRegistration reg in checkpointTimes)
      {
        if (reg.Start != null)
          reg.StartTimeFrame = data.ConvertDateTimeToTimeFrame(reg.Start.Value);
        if (reg.End != null)
          reg.EndTimeFrame = data.ConvertDateTimeToTimeFrame(reg.End.Value);
      }
    }

    
    private void UpdateCheckpointAndTimeIndex(CheckpointAndTimeFrameIndex index, CheckpointTimeRegistration reg)
    {
      if (reg.StartTimeFrame != null && reg.EndTimeFrame != null)
      {
        if (!index.ContainsKey(reg.Checkpoint))
          index[reg.Checkpoint] = new Dictionary<int, List<CheckpointTimeRegistration>>();

        Dictionary<int, List<CheckpointTimeRegistration>> checkpointMinutes = index[reg.Checkpoint];

        for (int d1 = reg.StartTimeFrame.Value; d1 <= reg.EndTimeFrame.Value; ++d1)
        {
          int d = Configuration.TrimTimeFrameToOutputSample(d1);
          if (!checkpointMinutes.ContainsKey(d))
            checkpointMinutes[d] = new List<CheckpointTimeRegistration>();

          if (!checkpointMinutes[d].Contains(reg))
            checkpointMinutes[d].Add(reg);
        }
      }
    }


    private void UpdateTimeFrameAndCheckpointIndex(TimeFrameAndCheckpointIndex index, CheckpointTimeRegistration reg)
    {
      if (reg.StartTimeFrame != null && reg.EndTimeFrame != null)
      {
        for (int d1=reg.StartTimeFrame.Value; d1<reg.EndTimeFrame.Value; ++d1)
        {
          int d = Configuration.TrimTimeFrameToOutputSample(d1);
          if (!index.ContainsKey(d))
            index[d] = new Dictionary<string, List<CheckpointTimeRegistration>>();

          if (!index[d].ContainsKey(reg.Checkpoint))
            index[d][reg.Checkpoint] = new List<CheckpointTimeRegistration>();

          if (!index[d][reg.Checkpoint].Contains(reg))
            index[d][reg.Checkpoint].Add(reg);
        }
      }
    }


    private void UpdateTeamAndTimeIndex(TeamAndTimeFrameIndex index, CheckpointTimeRegistration reg)
    {
      if (reg.StartTimeFrame != null && reg.EndTimeFrame != null)
      {
        if (!index.ContainsKey(reg.Team))
          index[reg.Team] = new Dictionary<int, CheckpointTimeRegistration>();

        Dictionary<int, CheckpointTimeRegistration> teamMinutes = index[reg.Team];

        for (int d1 = reg.StartTimeFrame.Value; d1 <= reg.EndTimeFrame.Value; ++d1)
        {
          int d = Configuration.TrimTimeFrameToOutputSample(d1);
          teamMinutes[d] = reg;
        }
      }
    }


    private void UpdateTeamAndTimeIntervalIndex(TeamAndTimeIntervalIndex index, CheckpointTimeRegistration reg)
    {
      if (reg.StartTimeFrame != null && reg.EndTimeFrame != null)
      {
        if (!index.ContainsKey(reg.Team))
          index[reg.Team] = new List<CheckpointTimeRegistration>();

        index[reg.Team].Add(reg);
      }
    }


    public static DateTime Trim(DateTime date, TimeSpan t)
    {
      return new DateTime(date.Ticks - date.Ticks % t.Ticks);
    }
  }
}
