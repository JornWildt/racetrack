
GoldStar = {
  path: 'M 125,5 155,90 245,90 175,145 200,230 125,180 50,230 75,145 5,90 95,90 z',
  fillColor: "yellow",
  fillOpacity: 0.8,
  scale: 0.07,
  strokeColor: "gold",
  strokeWeight: 0.8
};

BlueStar = {
  path: 'M 125,5 155,90 245,90 175,145 200,230 125,180 50,230 75,145 5,90 95,90 z',
  fillColor: "deepskyblue",
  fillOpacity: 0.8,
  scale: 0.07,
  strokeColor: "darkblue",
  strokeWeight: 0.8,
  anchor: { x: 200, y: 0 }
};

TeamCount = 2;
TeamNames = ["52", null];
TeamMarkers = [null, null];
TeamIcons = [GoldStar, BlueStar];

HeatMap = null;

IsActive = false;

TimeFrame = 0;


function Initialize() {
  var mapOptions = {
    mapTypeId: google.maps.MapTypeId.HYBRID
  };
  map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);
  var ctaLayer = new google.maps.KmlLayer("http://www.elfisk.dk/Checkpoint_locations.7.kml");
  ctaLayer.setMap(map);

  HeatMap = new google.maps.visualization.HeatmapLayer({ maxIntensity: 40 });
  HeatMap.setOptions({ radius: 40 });
  HeatMap.setMap(map);
  for (var i = 0; i < TeamCount; ++i) {
    TeamMarkers[i] = new google.maps.Marker(
          {
            map: map,
            icon: TeamIcons[i]
          });

    var selector = document.getElementById("teamSelector" + i);
    selector.options[selector.options.length] = new Option("- Vælg sjak -", "");
    for (var key in TeamTracks) {
      if (TeamTracks.hasOwnProperty(key)) {
        selector.options[selector.options.length] = new Option(TeamTracks[key].name, TeamTracks[key].name);
      }
    }
    selector.value = TeamNames[i];
  }

  window.setInterval(UpdateTimeFrame, 500);
}


/*********************************************************************************
  Animation handling
*********************************************************************************/

function UpdateTimeFrame() {
  if (!IsActive)
    return;
  UpdateDisplay();
  TimeFrame = (TimeFrame + 1) % CheckpointHeatMaps.length;
}


function UpdateDisplay() {
  HeatMap.setData(CheckpointHeatMaps[TimeFrame].map);
  document.getElementById("time").innerHTML = CheckpointHeatMaps[TimeFrame].time;

  for (var i = 0; i < TeamCount; ++i) {
    if (TeamNames[i] != null) {
      var teamName = TeamNames[i];
      var teamTimes = TeamTracks[teamName].times;
      var teamScores = TeamTracks[teamName].scores;
      var found = false;
      var teamLocation;
      var teamScore = null;
      for (var j = 0; j < teamTimes.length; ++j) {
        var location = teamTimes[j].location;
        if (teamTimes[j].end < TimeFrame && teamScores.hasOwnProperty(location))
          teamScore = teamScores[location].end;
        if (teamTimes[j].start <= TimeFrame && TimeFrame <= teamTimes[j].end) {
          teamLocation = CheckpointLocations[location];
          teamScore = (teamScores.hasOwnProperty(location) ? teamScores[location].start : teamScore);
          TeamMarkers[i].setPosition(teamLocation);
          break;
        }
      }
      var newMap = (teamLocation != null ? map : null);
      if (TeamMarkers[i].getMap() != newMap)
        TeamMarkers[i].setMap(newMap);
      if (teamScore != null)
        document.getElementById("teamScore" + i).innerHTML = teamScore;
    }
  }
}

/*********************************************************************************
  User input handlers
*********************************************************************************/

function OnTogglePauseClicked() {
  IsActive = !IsActive;
  document.getElementById("pauseButton").innerHTML = (IsActive ? "Pause" : "Start");
}

function OnTeamSelectorChanged(selector) {
  TeamNames[selector.id.substring(12)] = selector.value;
}

function OnForward() {
  TimeFrame += 10;
  if (TimeFrame >= CheckpointHeatMaps.length)
    TimeFrame = 0;
  UpdateDisplay();
}

function OnBackward() {
  TimeFrame -= 10;
  // FIXME: use more specific variable for "length"
  if (TimeFrame < 0)
    TimeFrame = CheckpointHeatMaps.length + TimeFrame; 
  UpdateDisplay();
}
