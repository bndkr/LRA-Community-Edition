using MathNet.Numerics.LinearRegression;
using System;
using System.Collections.Generic;
using System.Globalization;
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
      NumEvaluations++;
      int maxInt = int.MaxValue;
      double multiplier = 1.0;

      if (report.crashed)
      {
        NumCrashes++;
        return maxInt;
      }

      if (!report.collidedWithNewLine)
      {
        NumNoNewLineCollision++;
        return maxInt;
      }

      if (report.stopped)
      {
        NumStalls++;
        return maxInt - 1;
      }

      // calculate number of acc. spikes per 100 samples
      var accelSpikeScore = WeightTransferFunction(1, 1, CountAccelerationSpikes(report.timestamps));
      multiplier *= accelSpikeScore;

      // we want a certain average speed
      var velocityMagList = GetVelocityMagList(report.timestamps);
      var averageSpeedScore = WeightTransferFunction(4, 9, velocityMagList.Average());
      multiplier *= averageSpeedScore;

      // we want a certain spread of speeds
      var velocityStandardDeviation = GetStandardDeviation(velocityMagList);
      var speedSDScore = WeightTransferFunction(2, 2, velocityStandardDeviation);
      multiplier *= speedSDScore;

      // we want the rider going all directions
      var directionScore = WeightTransferFunction(5, 2, GetDirectionScore(report.timestamps));
      multiplier *= directionScore;

      // we want the rider going airborne often
      var airborneScore = WeightTransferFunction(3, 7, GetTouchTransitionScore(report.timestamps));
      multiplier *= airborneScore;

      // we want great variation in height (y postion)
      var heightScore = WeightTransferFunction(400, 750, GetHeightSD(report.timestamps));
      multiplier *= heightScore;

      airborneScores.Add(airborneScore);
      directionScores.Add(directionScore);
      speedSDScores.Add(speedSDScore);
      averageSpeedScores.Add(averageSpeedScore);
      accelSpikeScores.Add(accelSpikeScore);
      heightSDScores.Add(heightScore);

      NumSuccess++;

      var deducted = multiplier * maxInt;
      return (int) (maxInt - deducted);
    }

    private static double GetHeightSD(List<TimestampReport> timestamps)
    {
      var result = new List<double>();
      foreach (var timestamp in timestamps)
      {
        result.Add(timestamp.position.y);
      }
      return GetStandardDeviation(result);
    }

    private static List<double> airborneScores = new List<double>();
    private static List<double> directionScores = new List<double>();
    private static List<double> speedSDScores = new List<double>();
    private static List<double> averageSpeedScores = new List<double>();
    private static List<double> accelSpikeScores = new List<double>();
    private static List<double> heightSDScores = new List<double>();


    private static int NumEvaluations { get; set; }
    private static int NumSuccess { get; set; }
    private static int NumCrashes { get; set; }
    private static int NumNoNewLineCollision { get; set; }
    private static int NumStalls { get; set; }

    private static double GetAirborneScoresAverage() {return airborneScores.Average();}
    private static double GetDirectionScoresAverage() {return directionScores.Average();}
    private static double GetSpeedSDScoresAverage() {return speedSDScores.Average();}
    private static double GetAverageSpeedScoresAverage() {return averageSpeedScores.Average();}
    private static double GetAccelSpikeScoresAverage() {return accelSpikeScores.Average();}
    private static double GetHeightSDScoresAverage() {return heightSDScores.Average();}
    private static double GetAirborneScoresSD() {return GetStandardDeviation(airborneScores);}
    private static double GetDirectionScoresSD() {return GetStandardDeviation(directionScores);}
    private static double GetSpeedSDScoresSD() {return GetStandardDeviation(speedSDScores);}
    private static double GetAverageSpeedScoresSD() {return GetStandardDeviation(averageSpeedScores);}
    private static double GetAccelSpikeScoresSD() {return GetStandardDeviation(accelSpikeScores);}
    private static double GetHeightSDScoresSD() {return GetStandardDeviation(heightSDScores);}

    public static void ResetAllStats()
    {
      airborneScores.Clear();
      directionScores.Clear();
      speedSDScores.Clear();
      averageSpeedScores.Clear();
      accelSpikeScores.Clear();

      NumEvaluations = 0;
      NumSuccess = 0;
      NumCrashes = 0;
      NumNoNewLineCollision = 0;
      NumStalls = 0;
    }

    public static void PrintStatistics()
    {
      Console.WriteLine();
      Console.WriteLine("-----------------------[Evaluation]-------------------------------");
      Console.WriteLine($"AirborneScoresAverage:      {GetAirborneScoresAverage()}");
      Console.WriteLine($"DirectionScoresAverage:     {GetDirectionScoresAverage()}");
      Console.WriteLine($"SpeedSDScoresAverage:       {GetSpeedSDScoresAverage()}");
      Console.WriteLine($"AverageSpeedScoresAverage:  {GetAverageSpeedScoresAverage()}");
      Console.WriteLine($"AccelSpikeScoresAverage:    {GetAccelSpikeScoresAverage()}");
      Console.WriteLine($"GetHeightSDScoresAverage:   {GetHeightSDScoresAverage()}");
      Console.WriteLine($"AirborneScoresSD:           {GetAirborneScoresSD()}");
      Console.WriteLine($"DirectionScoresSD:          {GetDirectionScoresSD()}");
      Console.WriteLine($"SpeedSDScoresSD:            {GetSpeedSDScoresSD()}");
      Console.WriteLine($"AverageSpeedScoresSD        {GetAverageSpeedScoresSD()}");
      Console.WriteLine($"AccelSpikeScoresSD:         {GetAccelSpikeScoresSD()}");
      Console.WriteLine($"GetHeightSDScoresSD:        {GetHeightSDScoresSD()}");

      Console.WriteLine();
      Console.WriteLine($"Number of simulations:             {NumEvaluations}");
      Console.WriteLine($"Number of successful runs:         {NumSuccess}");
      Console.WriteLine($"Number of crashes:                 {NumCrashes}");
      Console.WriteLine($"Number of stalls:                  {NumStalls}");
      Console.WriteLine($"Number of new lines not touched:   {NumNoNewLineCollision}");
      Console.WriteLine();
      Console.WriteLine($"Sucess rate: {100 * (double)NumSuccess / (double)NumEvaluations}%");
      Console.WriteLine("-------------------------------------------------------------------");
      Console.WriteLine();
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

    private static List<double> GetDirecitons(List<TimestampReport> reports)
    {
      var result = new List<double>();
      foreach (var report in reports)
      {
        result.Add(Math.Atan(report.velocity.y / report.velocity.x));
      }
      return result;
    }

    private static double GetDirectionScore(List<TimestampReport> reports)
    {
      var directions = GetDirecitons(reports);
      return GetStandardDeviation(directions);
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
