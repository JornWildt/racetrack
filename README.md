Racetrack
=========

Small demonstration of how to display geo located time registrations from an adventure race using Goggle maps with KML overlay and heat maps.

Demo: http://www.elfisk.dk/allitrack/CheckpointAnimation.html

Implementation:

- Read CVS file with checkpoint/team/time registrations

- Read KML file with checkpoint locations

- Create Javascript file containing time framed data of all teams geo location at any given time (modula the sampling frequency)

- Include Javascript data in HTML page and show KML overlay together with heat maps for the number of teams at any given checkpoint/time.