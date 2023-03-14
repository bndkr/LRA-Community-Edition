using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
  public static class Evaluator
  {
    public static int calculateCost(Report report)
    {
      int maxInt = int.MaxValue;
      double multiplier = 1.0;

      if (report.crashed)
      {
        return maxInt;
      }

      if (!report.collidedWithNewLine)
      {
        return maxInt;
      }


      if (report.stopped)
      {
        return maxInt - 1;
      }

      // calculate number of acc. spikes per 100 samples
      multiplier *= WeightTransferFunction(
        1, 1, CountAccelerationSpikes(report.timestamps));

      var velocityMagList = GetVelocityMagList(report.timestamps);
      multiplier *= WeightTransferFunction(4, 6, velocityMagList.Average());
      
      var velocityStandardDeviation = GetStandardDeviation(velocityMagList);

      multiplier *= WeightTransferFunction(3, 4, velocityStandardDeviation);

      multiplier *= WeightTransferFunction(
        3, 7, GetTouchTransitionScore(report.timestamps));

      var deducted = multiplier * maxInt;
      return (int) (maxInt - deducted);
    }
    
    private static double WeightTransferFunction(double width,
                                                 double ideal,
                                                 double input)
    {
      return Math.Exp(-Math.Pow((input - ideal) / width, 2.0));
    }

    private static double CountAccelerationSpikes(List<TimestampReport> reports)
    {
      double numSpikes = 0;
      foreach (var time in reports)
      {
        var magnitude = time.acceleration.Mag();
        if (magnitude > 1.8) numSpikes++;
      }
      return (numSpikes * 100) / (double)reports.Count;
    }

    private static List<double> GetVelocityMagList(List<TimestampReport> reports)
    {
      var velocities = new List<double>();
      foreach (var report in reports)
      {
        velocities.Add(report.velocityMag);
      }
      return velocities;
    }

    private static double GetStandardDeviation(List<double> doubleList)
    {
      double average = doubleList.Average();
      double sumOfSquaresOfDifferences = doubleList.Select(val => (val - average) * (val - average)).Sum();
      return Math.Sqrt(sumOfSquaresOfDifferences / (doubleList.Count - 1));
    }

    private static double GetTouchTransitionScore(List<TimestampReport> reports)
    {
      double touchTransitions = 0;
      bool last = reports[3].freeFall;
      bool curr = reports[4].freeFall;
      for (int i = 4; i < reports.Count; i++)
      {
        curr = reports[i].freeFall;
        if (last != curr)
          touchTransitions++;
        last = curr;
      }
      return (100 * touchTransitions) / reports.Count;
    }
  }
}
