using System;
using System.Xml.Serialization;


namespace RacetimeDisplayGenerator.Configuration
{
  [XmlRoot("Configuration")]
  public class ConfigurationElement
  {
    public DateTime StartTime { get; set; }


    public string InputDirectory { get; set; }


    public string OutputDirectory { get; set; }
  }
}
