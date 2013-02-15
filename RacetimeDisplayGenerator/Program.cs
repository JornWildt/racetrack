using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using RacetimeDisplayGenerator.Registrations;


namespace RacetimeDisplayGenerator
{
  class Program
  {
    const string BaseDir = "C:\\Jørn\\Alligatorløbsdata\\Alligatorløbet 2013";
    
    const string CheckpointTimesFilename = "Checkpoint_times.csv";
    
    const string CheckpointLocationsFilename = "Checkpoint_locations.kml";

    const string TeamScoresFilename = "Team_scores.csv";

    const string CheckpointsFilename = "Checkpoints.csv";

    const string TeamsFilename = "Teams.csv";

    const string OutputDir = "C:\\Projects\\Racetracker\\Web";

    const string JSONDataFilename = "CheckpointData.js";

    
    static void Main(string[] args)
    {
      CsvConfiguration cfg = new CsvConfiguration { Delimiter = ";", IsStrictMode = false };
      CsvReader teamsReader = new CsvReader(new StreamReader(Path.Combine(BaseDir, TeamsFilename), Encoding.GetEncoding(1252)), cfg);
      List<TeamRegistration> teams = teamsReader.GetRecords<TeamRegistration>().ToList();

      cfg = new CsvConfiguration { Delimiter = ";", IsStrictMode = false };
      CsvReader checkPointTimesReader = new CsvReader(new StreamReader(Path.Combine(BaseDir, CheckpointTimesFilename), Encoding.GetEncoding(1252)), cfg);
      List<CheckpointTimeRegistration> checkpointTimes = checkPointTimesReader.GetRecords<CheckpointTimeRegistration>().ToList();

      cfg = new CsvConfiguration { Delimiter = ";", IsStrictMode = false };
      CsvReader teamScoresReader = new CsvReader(new StreamReader(Path.Combine(BaseDir, TeamScoresFilename), Encoding.GetEncoding(1252)), cfg);
      List<TeamScoreRegistration> teamScores = teamScoresReader.GetRecords<TeamScoreRegistration>().ToList();

      cfg = new CsvConfiguration { Delimiter = ";", IsStrictMode = false };
      CsvReader checkpointsReader = new CsvReader(new StreamReader(Path.Combine(BaseDir, CheckpointsFilename), Encoding.GetEncoding(1252)), cfg);
      List<CheckpointRegistration> checkpoints = checkpointsReader.GetRecords<CheckpointRegistration>().ToList();

      CheckpointLocationsLoader locationsLoader = new CheckpointLocationsLoader();
      List<CheckpointLocation> checkpointLocations = locationsLoader.LoadCheckpointsFromKML(Path.Combine(BaseDir, CheckpointLocationsFilename));

      RaceDataCalculator converter = new RaceDataCalculator();
      RaceData indecies = converter.CalcuateData(teams, checkpointTimes, teamScores, checkpoints, checkpointLocations);

      JSONDataGenerator generator = new JSONDataGenerator();
      generator.Write(Path.Combine(OutputDir, JSONDataFilename), checkpointLocations, indecies);
    }
  }
}
