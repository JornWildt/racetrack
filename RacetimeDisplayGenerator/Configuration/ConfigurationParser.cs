using System.IO;
using System.Text;
using System.Xml.Serialization;


namespace RacetimeDisplayGenerator.Configuration
{
  public class ConfigurationParser
  {
    public ConfigurationElement ParseConfiguration(string filename)
    {
      XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationElement));
      using (StreamReader r = new StreamReader(filename, Encoding.Default))
      {
        ConfigurationElement cfg = (ConfigurationElement)serializer.Deserialize(r);
        return cfg;
      }
    }
  }
}
