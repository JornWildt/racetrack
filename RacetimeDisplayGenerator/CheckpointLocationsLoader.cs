using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpKml.Engine;
using SharpKml.Dom;

namespace RacetimeDisplayGenerator
{
  public class CheckpointLocationsLoader
  {
    public List<CheckpointLocation> LoadCheckpointsFromKML(string filename)
    {
      List<CheckpointLocation> locations = new List<CheckpointLocation>();
      KmlFile kml = SharpKml.Engine.KmlFile.Load(filename);
      FindCheckpoints(kml.Root, locations);
      return locations;
    }


    private void FindCheckpoints(Element root, List<CheckpointLocation> locations)
    {
      foreach (Element e in root.Flatten())
      {
        if (e is Placemark)
          FindCheckpoints((Placemark)e, locations);
      }
      //if (e is Document)
      //  FindCheckpoints((Document)e, locations);
      //else if (e is Folder)
      //  FindCheckpoints((Folder)e, locations);
    }


    private void FindCheckpoints(Placemark p, List<CheckpointLocation> locations)
    {
      CheckpointLocation location = new CheckpointLocation
      {
        Name = p.Name,
        Location = p.Flatten().OfType<Point>().First().Coordinate
      };
      locations.Add(location);
    }


    //private void FindCheckpoints(Document doc, List<CheckpointLocation> locations)
    //{
    //  foreach (Element e in doc.Flatten())
    //    FindCheckpoints(e, locations);
    //}


    //private void FindCheckpoints(Folder folder, List<CheckpointLocation> locations)
    //{
    //  foreach (Element e in folder.Flatten())
    //    FindCheckpoints(e, locations);
    //}
  }
}
