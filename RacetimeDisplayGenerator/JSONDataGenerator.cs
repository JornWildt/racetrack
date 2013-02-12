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
    public void Write(string filename, List<CheckpointLocation> checkpointLocations, RaceData indecies)
    {
      using (StreamWriter w = new StreamWriter(filename, false, Encoding.UTF8))
      {
        WriteCheckpointLocations(checkpointLocations, w);

        Dictionary<string, decimal> maxTeamsOnCheckpoint = new Dictionary<string, decimal>();
        foreach (string checkpoint in indecies.CheckpointAndTimeFrame.Keys)
        {
          int max = indecies.CheckpointAndTimeFrame[checkpoint].Values.Max(r => r.Count);
          maxTeamsOnCheckpoint[checkpoint] = max;
        }

        HashSet<string> checkpointNames = new HashSet<string>();
        foreach (CheckpointLocation checkpoint in checkpointLocations)
          checkpointNames.Add(checkpoint.Name);
        WriteHeatMapData(indecies, checkpointNames, maxTeamsOnCheckpoint, w);

        WriteTeamData(indecies, checkpointNames, maxTeamsOnCheckpoint, w);
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


    private void WriteTeamData(RaceData indecies, HashSet<string> checkpointNames, Dictionary<string, decimal> maxTeamsOnCheckpoint, StreamWriter w)
    {
      List<string> teams = indecies.TeamAndTimeInterval.Keys.OrderBy(x => x.PadLeft(4)).ToList();
      string lastTeam = teams.Last();
      w.WriteLine("TeamTracks = {");
      foreach (string team in teams)
      {
        w.WriteLine("  \"{0}\": {{", team);
        w.WriteLine("    name: \"{0}\",", team);
        w.WriteLine("    times: [");
        foreach (CheckpointTimeRegistration reg in indecies.TeamAndTimeInterval[team])
        {
          w.WriteLine("      {{ start: {0}, end: {1}, location: \"{2}\" }},",
            Configuration.ConvertInputTimeFrameToOutput(reg.StartTimeFrame.Value), 
            Configuration.ConvertInputTimeFrameToOutput(reg.EndTimeFrame.Value), 
            reg.Checkpoint);
        }
        w.WriteLine("    ]");
        w.Write("  }");
        if (team != lastTeam)
          w.WriteLine(",");
        else
          w.WriteLine("");
      }
      w.WriteLine("}");
    }
  }
}
