using System;
using System.Collections.Generic;
using RacetimeDisplayGenerator.Registrations;
using SharpKml.Base;


namespace RacetimeDisplayGenerator
{
  public class RaceDataCalculator
  {
    public RaceData CalcuateData(
      List<TeamRegistration> teams,
      List<CheckpointTimeRegistration> checkpointTimes, 
      List<TeamScoreRegistration> teamScores,
      List<CheckpointRegistration> checkpoints,
      List<CheckpointLocation> checkpointLocations)
    {
      RaceData data = new RaceData();
      data.Teams = teams;

      InitializeRegistrations(data, checkpointTimes, teamScores);

      CalculateCheckpointLocations(checkpoints, checkpointLocations, data);

      List<CheckpointRegistration> extendedCheckpoints = new List<CheckpointRegistration>();
      for (int i=0; i<checkpoints.Count; ++i)
      {
        CheckpointRegistration c1 = checkpoints[i];
        extendedCheckpoints.Add(c1);
        for (int j = i+1; j < checkpoints.Count; ++j)
        {
          if (i != j)
          {
            CheckpointRegistration c2 = checkpoints[j];
            if (c1.Location != null && c2.Location != null)
            {
              CheckpointRegistration c = new CheckpointRegistration
              {
                Checkpoint = c1.Checkpoint + "#" + c2.Checkpoint,
                Location = new Vector((c1.Location.Latitude + c2.Location.Latitude) / 2, (c1.Location.Longitude + c2.Location.Longitude) / 2),
                Type = CheckpointType.Intermediate
              };
              checkpointLocations.Add(new CheckpointLocation { Name = c.Checkpoint, Location = c.Location });
              extendedCheckpoints.Add(c);

              c = new CheckpointRegistration
              {
                Checkpoint = c2.Checkpoint + "#" + c1.Checkpoint,
                Location = new Vector((c1.Location.Latitude + c2.Location.Latitude) / 2, (c1.Location.Longitude + c2.Location.Longitude) / 2),
                Type = CheckpointType.Intermediate
              };
              checkpointLocations.Add(new CheckpointLocation { Name = c.Checkpoint, Location = c.Location });
              extendedCheckpoints.Add(c);
            }
          }
        }
      }
      checkpoints.Clear();
      checkpoints.AddRange(extendedCheckpoints);

      foreach (CheckpointRegistration reg in checkpoints)
      {
        data.CheckpointRegistration[reg.Checkpoint] = reg;
      }

      foreach (CheckpointTimeRegistration reg in checkpointTimes)
      {
        UpdateCheckpointAndTimeIndex(data.CheckpointAndTimeFrame, reg);
        UpdateTimeFrameAndCheckpointIndex(data.TimeFrameAndCheckpoint, reg);
        UpdateTeamAndTimeIndex(data.TeamAndTimeFrame, reg);
        UpdateTeamAndTimeIntervalIndex(data.TeamAndTimeInterval, reg);
      }

      foreach (TeamScoreRegistration reg in teamScores)
      {
        UpdateTeamAndCheckpointTeamScoreIndex(data.TeamTeamScore, reg);
      }

      CalculateSummarizedScores(data);

      return data;
    }


    private void InitializeRegistrations(RaceData data, IList<CheckpointTimeRegistration> checkpointTimes, IList<TeamScoreRegistration> teamScores)
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

      foreach (TeamScoreRegistration reg in teamScores)
      {
        if (reg.Fixed == null)
          reg.Fixed = 0;
        if (reg.Work == null)
          reg.Work = 0;
      }
    }


    private static void CalculateCheckpointLocations(IList<CheckpointRegistration> checkpoints, IList<CheckpointLocation> checkpointLocations, RaceData data)
    {
      foreach (CheckpointRegistration reg in checkpoints)
      {
        data.CheckpointRegistration[reg.Checkpoint] = reg;
      }

      foreach (CheckpointLocation l in checkpointLocations)
      {
        if (data.CheckpointRegistration.ContainsKey(l.Name))
          data.CheckpointRegistration[l.Name].Location = l.Location;
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


    private void UpdateTeamAndCheckpointTeamScoreIndex(TeamTeamScoreIndex index, TeamScoreRegistration reg)
    {
      if (!index.ContainsKey(reg.Team))
        index[reg.Team] = new List<TeamScoreRegistration>();

      index[reg.Team].Add(reg);
    }


    private static void CalculateSummarizedScores(RaceData data)
    {
      foreach (string team in data.TeamTeamScore.Keys)
      {
        int score = 0;
        foreach (TeamScoreRegistration reg in data.TeamTeamScore[team])
        {
          score += reg.Fixed.Value;
          reg.Start = score;
          score += reg.Work.Value;
          reg.End = score;
        }
      }
    }


    public static DateTime Trim(DateTime date, TimeSpan t)
    {
      return new DateTime(date.Ticks - date.Ticks % t.Ticks);
    }
  }
}
