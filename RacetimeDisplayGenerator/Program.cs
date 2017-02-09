using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using RacetimeDisplayGenerator.Registrations;
using System;
using System.Globalization;
using RacetimeDisplayGenerator.Configuration;

namespace RacetimeDisplayGenerator
{
  class Program
  {
    static readonly DateTime RaceStartTime = new DateTime(2016, 2, 6, 18, 00, 00);

    const string CheckpointsFilename = "Checkpoints.csv";

    const string CheckpointTimesFilename = "Checkpoint_times.csv";

    const string TeamsFilename = "Teams.csv";

    const string TeamScoresFilename = "Team_scores.csv";

    const string CheckpointLocationsFilename = "Checkpoint_locations.kml";

    const string JSONDataFilename = "CheckpointData.js";


    static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("USAGE:");
        Console.WriteLine("  RacetimeDisplayGenerator <configurationFilename>");
        Console.WriteLine("");
        Console.WriteLine("  <configurationFilename> is the name of an XML file containing the configuration for your race.");
        Environment.Exit(1);
      }

      try
      {
        Start(args);
      }
      catch (Exception ex)
      {
        HandleException(ex);
      }
    }


    private static void HandleException(Exception ex)
    {
      Console.WriteLine(ex.Message);
      Console.WriteLine(ex.StackTrace);
      if (ex.InnerException != null)
        HandleException(ex.InnerException);
    }


    private static void Start(string[] args)
    {
      string configurationFilename = args[0];

      Console.WriteLine("Reading configuration");
      ConfigurationParser cfgPs = new ConfigurationParser();
      ConfigurationElement cfg = cfgPs.ParseConfiguration(configurationFilename);

      if (string.IsNullOrEmpty(cfg.InputDirectory))
        throw new InvalidOperationException(string.Format("Missing or empty <InputDirectory> element in {0}.", configurationFilename));
      if (string.IsNullOrEmpty(cfg.OutputDirectory))
        throw new InvalidOperationException(string.Format("Missing or empty <OutputDirectory> element in {0}.", configurationFilename));

      Console.WriteLine("Reading teams");
      CsvReader teamsReader = new CsvReader(new StreamReader(Path.Combine(cfg.InputDirectory, TeamsFilename), Encoding.UTF8));
      //teamsReader.Configuration.Delimiter = ";";
      teamsReader.Configuration.WillThrowOnMissingField = false;
      List<TeamRegistration> teams = teamsReader.GetRecords<TeamRegistration>().ToList();

      Console.WriteLine("Reading times");
      CsvReader timestampReader = new CsvReader(new StreamReader(Path.Combine(cfg.InputDirectory, CheckpointTimesFilename), Encoding.UTF8));
      timestampReader.Configuration.Delimiter = ";";
      timestampReader.Configuration.WillThrowOnMissingField = false;
      timestampReader.Configuration.CultureInfo = CultureInfo.GetCultureInfo("en-US");
      List<CheckpointTimestamp> timestamps = timestampReader.GetRecords<CheckpointTimestamp>()
                                                            .Where(c => c.Time != null && c.Time.Value > RaceStartTime)
                                                            .ToList();

      List<CheckpointTimeRegistration> checkpointTimes = ConvertTimestamps(timestamps);

      Console.WriteLine("Reading scores");
      string teamScoresFilename = Path.Combine(cfg.InputDirectory, TeamScoresFilename);
      List<TeamScoreRegistration> teamScores = null;
      if (File.Exists(teamScoresFilename))
      {
        CsvReader teamScoresReader = new CsvReader(new StreamReader(teamScoresFilename, Encoding.GetEncoding(1252)));
        teamScores = new List<TeamScoreRegistration>();
      }

      Console.WriteLine("Reading checkpoints");
      CsvReader checkpointsReader = new CsvReader(new StreamReader(Path.Combine(cfg.InputDirectory, CheckpointsFilename), Encoding.GetEncoding(1252)));
      //checkpointsReader.Configuration.Delimiter = ";";
      checkpointsReader.Configuration.WillThrowOnMissingField = false;
      List<CheckpointRegistration> checkpoints = checkpointsReader.GetRecords<CheckpointRegistration>().ToList();

      CheckpointLocationsLoader locationsLoader = new CheckpointLocationsLoader();
      List<CheckpointLocation> checkpointLocations = locationsLoader.LoadCheckpointsFromKML(Path.Combine(cfg.InputDirectory, CheckpointLocationsFilename));

      RaceDataCalculator converter = new RaceDataCalculator();
      RaceData indecies = converter.CalcuateData(teams, checkpointTimes, teamScores, checkpoints, checkpointLocations);

      JSONDataGenerator generator = new JSONDataGenerator();
      generator.Write(Path.Combine(cfg.OutputDirectory, JSONDataFilename), checkpointLocations, indecies);
    }

    
    private static List<CheckpointTimeRegistration> ConvertTimestamps(List<CheckpointTimestamp> timestamps)
    {
      Dictionary<string, CheckpointTimeRegistration> regIndex = new Dictionary<string, CheckpointTimeRegistration>();
      foreach (CheckpointTimestamp t in timestamps)
      {
        string name = null;
        bool isStart = false;
        bool isStop = false;

        if (t.Checkpoint == "P7" && t.Remark == "Ankomst")
        {
          name = "P7a";
          isStart = true;
        }
        else if (t.Checkpoint == "P7" && t.Remark == "Afgang til 8")
        {
          name = "P7a";
          isStop = true;
        }
        else if (t.Checkpoint == "P7" && t.Remark == "Ankomst fra 8")
        {
          name = "P7b";
          isStart = true;
        }
        else if (t.Checkpoint == "P7" && t.Remark == "Afgang")
        {
          name = "P7b";
          isStop = true;
        }
        else if (t.Remark == "Dødpost")
        {
          name = t.Checkpoint;
          isStart = true;
          isStop = true;
        }
        else if (t.Remark == "Ankomst")
        {
          name = t.Checkpoint;
          isStart = true;
        }
        else if (t.Remark == "Afgang")
        {
          name = t.Checkpoint;
          isStop = true;
        }
        else if (t.Remark == "Sluttid")
        {
          name = t.Checkpoint;
          isStop = true;
        }
        else if (t.Checkpoint == "SLUTTID") // Why this in the file?
        {
        }
        else
          throw new InvalidOperationException("Unknown checkpoint: " + t.Checkpoint + "/" + t.Remark + " (team " + t.Team + ")");

        if (name != null)
        {
          string key = t.Team + ":" + name;

          if (!regIndex.ContainsKey(key))
            regIndex[key] = new CheckpointTimeRegistration { Checkpoint = name, Team = t.Team };

          if (regIndex[key].Checkpoint == "P7a" || regIndex[key].Checkpoint == "P7b")
            regIndex[key].Checkpoint = "P7";

          if (isStart)
            regIndex[key].Start = t.Time;
          if (isStop)
            regIndex[key].End = t.Time;
        }
      }

      List<CheckpointTimeRegistration> registrations = new List<CheckpointTimeRegistration>();
      foreach (string key in regIndex.Keys.OrderBy(k => k))
        registrations.Add(regIndex[key]);

      return registrations;
    }
  }
}
