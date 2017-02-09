GoldStar = {
  path: 'M 125,5 155,90 245,90 175,145 200,230 125,180 50,230 75,145 5,90 95,90 z',
  fillColor: "yellow",
  fillOpacity: 0.8,
  scale: 0.07,
  strokeColor: "black",
  strokeWeight: 0.8
};

BlueStar = {
  path: 'M 125,5 155,90 245,90 175,145 200,230 125,180 50,230 75,145 5,90 95,90 z',
  fillColor: "deepskyblue",
  fillOpacity: 0.8,
  scale: 0.07,
  strokeColor: "black",
  strokeWeight: 0.8,
  anchor: { x: 200, y: 0 }
};

GoldStarSmall = {
  path: 'M 125,5 155,90 245,90 175,145 200,230 125,180 50,230 75,145 5,90 95,90 z',
  fillColor: "yellow",
  fillOpacity: 0.8,
  scale: 0.06,
  strokeColor: "black",
  strokeWeight: 0.8,
  anchor: { x: 0, y: 100 }
};

BlueStarSmall = {
  path: 'M 125,5 155,90 245,90 175,145 200,230 125,180 50,230 75,145 5,90 95,90 z',
  fillColor: "deepskyblue",
  fillOpacity: 0.8,
  scale: 0.06,
  strokeColor: "darkblue",
  strokeWeight: 0.8,
  anchor: { x: 200, y: 100 }
};


TeamCount = 2;
TeamNamesDefault = ["1", "2"]; // Xxx
TeamNames = TeamNamesDefault;
TeamMarkers = [null, null];
TeamIcons = [GoldStar, BlueStar];
TeamIconsSmall = [GoldStarSmall, BlueStarSmall];

HeatMap = null;

IsActive = false;

TimeFrame = 0;


function Initialize() {
  var mapOptions = {
    mapTypeId: google.maps.MapTypeId.HYBRID
  };
  map = new google.maps.Map(document.getElementById("map_canvas"), mapOptions);
  var ctaLayer = new google.maps.KmlLayer("http://www.elfisk.dk/allitrack-2017-fredefup/Checkpoint_locations.kml");
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
        selector.options[selector.options.length] = new Option(TeamTracks[key].extendedName, TeamTracks[key].name);
      }
    }
    selector.value = TeamNames[i];
  }

  window.setInterval(UpdateTimeFrame, 200);

  TINY.box.show({ url: 'Introduction.html', width: 500, height: 250 });

  UpdateDisplay();
}


function ClearScores() {
  for (var i = 0; i < TeamCount; ++i) {
    //document.getElementById("teamScore" + i).innerHTML = 0;
    TeamScore[i] = 0;
  }
}


/*********************************************************************************
  Animation handling
*********************************************************************************/

function UpdateTimeFrame() {
  if (!IsActive)
    return;
  UpdateDisplay();
  TimeFrame = (TimeFrame + 1) % CheckpointHeatMaps.length;
  if (TimeFrame == 0) {
    IsActive = false;
    RefreshButtons();
  }
  else if (TimeFrame == 1) {
    ClearScores();
  }
}

TeamScore = [0,0];

function UpdateDisplay() {
  HeatMap.setData(CheckpointHeatMaps[TimeFrame].map);
  document.getElementById("time").innerHTML = CheckpointHeatMaps[TimeFrame].time;

  for (var i = 0; i < TeamCount; ++i) {
    if (TeamNames[i] != null && TeamNames[i] != "") {
      var teamName = TeamNames[i];
      var teamTimes = TeamTracks[teamName].times;
      var teamScores = TeamTracks[teamName].scores;
      var found = false;
      var teamLocation;
      var teamScore = null;
      for (var j = 0; j < teamTimes.length; ++j) {
        var location = teamTimes[j].location;
        if (teamTimes[j].end < TimeFrame && teamScores.hasOwnProperty(location) && teamScores[location].end > TeamScore[i])
          teamScore = teamScores[location].end;

        if (teamTimes[j].start <= TimeFrame && TimeFrame <= teamTimes[j].end) {
          if (teamScores.hasOwnProperty(location) && teamScores[location].start > TeamScore[i])
            teamScore = teamScores[location].start;
          if (location.indexOf("#") > 0) {
            locations = location.split("#");
            teamLocation0 = CheckpointLocations[locations[0]];
            teamLocation1 = CheckpointLocations[locations[1]];

            if (teamLocation0 == null)
              window.alert("Unknown location: " + locations[0]);
            if (teamLocation1 == null)
              window.alert("Unknown location: " + locations[1]);

            var interval = teamTimes[j].end - teamTimes[j].start;
            var dist = TimeFrame - teamTimes[j].start;

            var lat = teamLocation0.lat() + (teamLocation1.lat() - teamLocation0.lat()) * dist / interval;
            var lng = teamLocation0.lng() + (teamLocation1.lng() - teamLocation0.lng()) * dist / interval;

            teamLocation = new google.maps.LatLng(lat, lng);
            TeamMarkers[i].setIcon(TeamIconsSmall[i]);
          }
          else {
            TeamMarkers[i].setIcon(TeamIcons[i]);
            teamLocation = CheckpointLocations[location];
          }
          TeamMarkers[i].setPosition(teamLocation);
          break;
        }
      }
      var newMap = (teamLocation != null ? map : null);
      if (TeamMarkers[i].getMap() != newMap)
        TeamMarkers[i].setMap(newMap);
      if (teamScore != null) {
        TeamScore[i] = teamScore;
        //document.getElementById("teamScore" + i).innerHTML = teamScore;
      }
    }
  }
}


/*********************************************************************************
  Display update helpers
*********************************************************************************/

function RefreshButtons() {
  document.getElementById("pauseButton").innerHTML = (IsActive ? "Pause" : "Start");
}

function RefreshScores() {
}

function RefreshTeamNames(teamIndex) {
}

/*********************************************************************************
  User input handlers
*********************************************************************************/

function OnTogglePauseClicked() {
  IsActive = !IsActive;
  RefreshButtons();
}

function OnTeamSelectorChanged(selector) {
  var teamIndex = selector.id.substring(12);
  TeamScore[teamIndex] = 0;
  TeamMarkers[teamIndex].setMap(null);
  //document.getElementById("teamScore" + teamIndex).innerHTML = 0;
  TeamNames[teamIndex] = selector.value;
  alert(select.value);
}

function OnForward() {
  TimeFrame += (IsActive ? 10 : 12);
  if (TimeFrame >= CheckpointHeatMaps.length) {
    TimeFrame = 0;
    for (var i = 0; i < TeamCount; ++i)
      TeamScore[i] = 0;
  }
  UpdateDisplay();
}

function OnBackward() {
  TimeFrame -= (IsActive ? 10 : 12);
  // FIXME: use more specific variable for "length"
  if (TimeFrame < 0)
    TimeFrame = CheckpointHeatMaps.length + TimeFrame;
  ClearScores();
  UpdateDisplay();
}

function OnReset() {
  TimeFrame = 0;
  IsActive = false;
  TeamNames = TeamNamesDefault;
  for (var i=0; i<TeamCount; ++i) {
    TeamScore[i] = 0;
  }
  RefreshButtons();
  UpdateDisplay();
}

function OnShowTeamHelp() {
  TINY.box.show({ url: 'TeamHelp.html', width: 500, height: 250 });
}

function OnShowAbout() {
  TINY.box.show({ url: 'About.html', width: 500, height: 250 });
}