using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text;
using System.Globalization;


namespace RacetimeDisplayGenerator
{
  public class JSONDataGenerator
  {
    public void Write(string filename, List<CheckpointLocation> checkpointLocations, RaceData data)
    {
      using (StreamWriter w = new StreamWriter(filename, false, Encoding.UTF8))
      {
        WriteCheckpointLocations(checkpointLocations, w);

        Dictionary<string, decimal> maxTeamsOnCheckpoint = new Dictionary<string, decimal>();
        foreach (string checkpoint in data.CheckpointAndTimeFrame.Keys)
        {
          int max = data.CheckpointAndTimeFrame[checkpoint].Values.Max(r => r.Count);
          maxTeamsOnCheckpoint[checkpoint] = max;
        }

        HashSet<string> checkpointNames = new HashSet<string>();
        foreach (CheckpointLocation checkpoint in checkpointLocations)
          checkpointNames.Add(checkpoint.Name);
        WriteHeatMapData(data, checkpointNames, maxTeamsOnCheckpoint, w);

        WriteTeamData(data, checkpointNames, maxTeamsOnCheckpoint, w);
      }
    }


    private static void WriteCheckpointLocations(List<CheckpointLocation> checkpointLocations, StreamWriter w)
    {
      string lastCheckpoint = checkpointLocations.Last().Name;
      w.WriteLine("CheckpointLocations = {");
      foreach (CheckpointLocation cpl in checkpointLocations)
      {
        w.Write("  \"{0}\" : new google.maps.LatLng({1}, {2})", cpl.Name, cpl.Location.Latitude.ToString(CultureInfo.InvariantCulture), cpl.Location.Longitude.ToString(CultureInfo.InvariantCulture));
        if (cpl.Name != lastCheckpoint)
          w.WriteLine(",");
        else
          w.WriteLine("");
      }
      w.WriteLine("};");
    }


    private void WriteHeatMapData(RaceData data, HashSet<string> checkpointNames, Dictionary<string, decimal> maxTeamsOnCheckpoint, StreamWriter w)
    {
      bool firstHeatmap = true;
      w.WriteLine("CheckpointHeatMaps = [");
      for (int d=0; d<data.EndTimeFrame; d += Configuration.OutputSampleTime.Minutes)
      {
        if (!firstHeatmap)
          w.WriteLine(",");
        firstHeatmap= false;

        bool firstCheckpoint = true;
        w.WriteLine("  {");
        w.WriteLine("    time: \"{0}\",", data.ConvertTimeFrameToDateTime(d));
        w.WriteLine("    map: [");
        if (data.TimeFrameAndCheckpoint.ContainsKey(d))
        {
          foreach (string checkpoint in data.TimeFrameAndCheckpoint[d].Keys)
          {
            if (checkpointNames.Contains(checkpoint))
            {
              if (!firstCheckpoint)
                w.WriteLine(",");
              firstCheckpoint = false;

              decimal max = maxTeamsOnCheckpoint[checkpoint];
              decimal count = data.TimeFrameAndCheckpoint[d][checkpoint].Count;
              count = 50.0m * count / max;
              w.Write("      {{ location: CheckpointLocations[\"{0}\"], weight: {1} }}", checkpoint, (int)count);
            }
          }
          w.WriteLine("");
        }
        w.WriteLine("    ]");
        w.Write("    }");
      }
      w.WriteLine("];");
    }


    private void WriteTeamData(RaceData data, HashSet<string> checkpointNames, Dictionary<string, decimal> maxTeamsOnCheckpoint, StreamWriter w)
    {
      List<string> teams = data.TeamAndTimeInterval.Keys.OrderBy(x => x.PadLeft(4)).ToList();
      string lastTeam = teams.Last();
      w.WriteLine("TeamTracks = {");
      foreach (string teamName in teams)
      {
        TeamRegistration team = data.Teams.FirstOrDefault(t => t.Name == teamName);
        string extendedTeamName = teamName;
        if (team != null && !string.IsNullOrEmpty(team.Comment))
          extendedTeamName += " (" + team.Comment + ")";
        w.WriteLine("  \"{0}\": {{", teamName);
        w.WriteLine("    name: \"{0}\",", teamName);
        w.WriteLine("    extendedName: \"{0}\",", extendedTeamName);
        w.WriteLine("    times: [");
        CheckpointTimeRegistration prevReg = null;
        foreach (CheckpointTimeRegistration reg in data.TeamAndTimeInterval[teamName].OrderBy(r => r.StartTimeFrame))
        {
          if (prevReg != null)
          {
            w.WriteLine("      {{ start: {0}, end: {1}, location: \"{2}\" }},",
              Configuration.ConvertInputTimeFrameToOutput(prevReg.EndTimeFrame.Value + 1),
              Configuration.ConvertInputTimeFrameToOutput(reg.StartTimeFrame.Value - 1),
              prevReg.Checkpoint + "#" + reg.Checkpoint);
          }
          w.WriteLine("      {{ start: {0}, end: {1}, location: \"{2}\" }},",
            Configuration.ConvertInputTimeFrameToOutput(reg.StartTimeFrame.Value), 
            Configuration.ConvertInputTimeFrameToOutput(reg.EndTimeFrame.Value), 
            reg.Checkpoint);
          prevReg = reg;
        }
        w.WriteLine("    ],");
        w.WriteLine("    scores: {");
        if (data.TeamTeamScore.ContainsKey(teamName))
        {
          foreach (TeamScoreRegistration reg in data.TeamTeamScore[teamName])
          {
            w.WriteLine("      \"{0}\": {{ start:{1}, end:{2} }},",
              reg.Checkpoint,
              reg.Start,
              reg.End);
          }
        }
        w.WriteLine("    }");
        w.Write("  }");
        if (teamName != lastTeam)
          w.WriteLine(",");
        else
          w.WriteLine("");
      }
      w.WriteLine("}");
    }
  }
}
