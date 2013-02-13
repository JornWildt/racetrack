using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using System.Text;

// Skal skalere til max. pr. post
// ... eller hvad? Der er også en finte i at kunne se hvor mange der maks. kommer på posten.
// Flytte javascript code til egen fil
// Skal kunne stoppe loopet
// Skal kunne vælge tidspunkt
// Mangler fornuftig afslutning (ellers dør det lige efter post 11)
// Skal kunne speede op og ned
// Skal kunne udpege sjak
// Skal kunne vise hvor mange der er på vej mellem to poster
// Tegne linier ind på det færdige kort

namespace RacetimeDisplayGenerator
{
  class Program
  {
    const string BaseDir = "C:\\Jørn\\Alligatorløbsdata\\Alligatorløbet 2013";
    
    const string CheckpointTimesFilename = "Checkpoint_times.csv";
    
    const string CheckpointLocationsFilename = "Checkpoint_locations.kml";

    const string TeamScoresFilename = "Team_scores.csv";

    const string CheckpointsFilename = "Checkpoints.csv";

    const string OutputDir = "C:\\Projects\\Racetracker\\Web";

    const string JSONDataFilename = "CheckpointData.js";

    
    static void Main(string[] args)
    {
      CsvConfiguration cfg = new CsvConfiguration { Delimiter = ";", IsStrictMode = false };
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
      RaceData indecies = converter.CalcuateData(checkpointTimes, teamScores, checkpoints, checkpointLocations);

      JSONDataGenerator generator = new JSONDataGenerator();
      generator.Write(Path.Combine(OutputDir, JSONDataFilename), checkpointLocations, indecies);
    }
  }
}
