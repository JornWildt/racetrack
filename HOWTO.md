# SETTING UP

## Files
Create a new data directory <Data> for the data files.
Create a new web directory <Web> for the website.
Copy the core website directory to <Web>.


### Website
Edit .js and .html files in <Web> to replace references to the current year.


### Configuration file
Add an XML configuration file "<Racename>.config" to the <Data> directory. Content should be:

<Configuration>
  <InputDirectory>... the <Data> directoy ...</InputDirectory>
  <OutputDirectory>.. the <Web> directory ...</OutputDirectory>
  <StartTime>YYYY-MM-DD'Z'HH:MM:SS</StartTime>
</Configuration>


### KML Geolocations file
Edit <Web>/Checkpoint_locations.kml to define the location of all checkpoints. The KML file is referenced in 
the JSON file <Web>/CheckpointAnimation.js - make sure the reference is updated.


### Teams file
Create the teams definition file <Data>/Teams.csv. 
The teams file must at least contain the following columns:

  Name: Name of team (string).
  Type: Type of team (string).
  TotalScore: Total race score (int).
  TotalPosition: Total position in the race (int).


### Checkpoints file
Create the checkpoints file as <Data>/Checkpoints.csv.
The checkpoints file must at least contain the following columns:

  Checkpoint: name of checkpoint.
  MaxPoint: max score at this checkpoint (UNUSED!).


### Timestamps file
Create the timestamp file <Data>/Checkpoint_times.csv.
The timestamp file must at least contain the following columns:

  Team: Name of team (string) - must match team name from teams file.
  Checkpoint: Name of checkpoint (string) - must match checkpoint names in KML file.
  Time: Timestamp registration for checkpoint (datetime).
  Remark: Aditional comment about the checkpoint (string). Used for various hardcoded modifications.
          Known values are:
            Ankomst: arriving at checkpoint.
            Afgang: leaving checkpoint.
            Afgang til 8: leaving for checkpoint 8 (and only 8!).
            Ankomst fra 8: arriving from checkpoint 8 (and only 8!).
            Dødpost: unmanned checkpoint with identical arrival and departure times.
            Sluttid: last checkpoint time.


# CREATE WEBSITE
Start racetracker.exe with <Data>/<Racename>.config as commandline parameter. 
It will read the data files and generate the core statistics JSON file in the specified output directory.


# INSTALL WEBSITE
Copy the <web> directory to the webserver.


# VIEW
Open the website at CheckpointAnimation.html.

