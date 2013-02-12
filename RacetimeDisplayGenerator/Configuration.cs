using System;


namespace RacetimeDisplayGenerator
{
  public class Configuration
  {
    public static readonly TimeSpan InputSampleTime = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan OutputSampleTime = TimeSpan.FromMinutes(5);


    public static int TrimTimeFrameToOutputSample(int t)
    {
      return t - t % OutputSampleTime.Minutes;
    }


    public static int ConvertInputTimeFrameToOutput(int t)
    {
      return t / OutputSampleTime.Minutes;
    }
  }
}
