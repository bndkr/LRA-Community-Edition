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
  abstract class EvalCriteria
  {
    public string Name;
    public EvalCriteria(string name, double ideal, double width)
    {
      this.Name = name;
      this.Ideal = ideal;
      this.Width = width;
    }

    protected List<double> values = new List<double>();
    protected List<double> scores = new List<double>();

    public double Ideal;
    public double Width;

    public double GetAverageValue()
    {
      if (values.Count == 0) return 0;
      return values.Average();
    }
    public double GetAverageScore()
    {
      if (scores.Count == 0) return 0;
      return scores.Average();
    }

    public double GetValueSD()
    {
      if (values.Count == 0) return 0;
      return GetStandardDeviation(values);
    }
    public double GetScoreSD()
    {
      if (scores.Count == 0) return 0;
      return GetStandardDeviation(scores);
    }

    public abstract double EvaluateScore(List<TimestampReport> reports);

    protected static double WeightTransferFunction(double width,
                                                 double ideal,
                                                 double input)
    {
      return Math.Exp(-Math.Pow((input - ideal) / width, 2.0));
    }
    protected static double GetStandardDeviation(List<double> doubleList)
    {
      if (doubleList.Count == 0) return 0;
      double average = doubleList.Average();
      double sumOfSquaresOfDifferences = doubleList.Select(val => (val - average) * (val - average)).Sum();
      return Math.Sqrt(sumOfSquaresOfDifferences / (doubleList.Count - 1));
    }
  }
  class AccelSpikeCritera : EvalCriteria
  {
    public AccelSpikeCritera(double ideal, double width)
      : base("Acceleration Spikes", ideal, width) { }
    public override double EvaluateScore(List<TimestampReport> reports)
    {
      double numSpikes = 0;
      foreach (var time in reports)
      {
        var magnitude = time.acceleration.Mag();
        if (magnitude > 1.8) numSpikes++;
      }
      var num = (numSpikes * 100) / (double)reports.Count;
      this.values.Add(num);
      var score = WeightTransferFunction(Width, Ideal, num);
      this.scores.Add(score);
      return score;
    }
  }
  class AverageSpeed : EvalCriteria
  {
    public AverageSpeed(double ideal, double width) :
      base("Average Speed", ideal, width)
    { }
    public override double EvaluateScore(List<TimestampReport> reports)
    {
      var velocities = new List<double>();
      foreach (var report in reports)
      {
        velocities.Add(report.velocityMag);
      }
      var average = 0.0;
      if (velocities.Count > 0)
      {
        average = velocities.Average();
      }
      this.values.Add(average);
      var score = WeightTransferFunction(Width, Ideal, average);
      this.scores.Add(score);
      return score;
    }
  }
  class SpeedSD : EvalCriteria
  {
    public SpeedSD(double ideal, double width) :
      base("Standard Deviation of Speeds", ideal, width)
    { }
    public override double EvaluateScore(List<TimestampReport> reports)
    {
      var velocities = new List<double>();
      foreach (var report in reports)
      {
        velocities.Add(report.velocityMag);
      }
      var sd = 0.0;
      if (velocities.Count > 0)
      {
        sd = GetStandardDeviation(velocities);
      }
      this.values.Add(sd);
      var score = WeightTransferFunction(Width, Ideal, sd);
      this.scores.Add(score);
      return score;
    }
  }
  class DirectionSD : EvalCriteria
  {
    public DirectionSD(double ideal, double width) :
      base("Standard Deviation of Directions", ideal, width)
    { }
    public override double EvaluateScore(List<TimestampReport> reports)
    {
      var directions = new List<double>();
      foreach (var report in reports)
      {
        directions.Add(Math.Atan(report.velocity.y / report.velocity.x));
      }
      var sd = 0.0;
      if (directions.Count > 0)
      {
        sd = GetStandardDeviation(directions);
      }
      this.values.Add(sd);
      var score = WeightTransferFunction(Width, Ideal, sd);
      this.scores.Add(score);
      return score;
    }
  }
  class TouchTransition : EvalCriteria
  {
    public TouchTransition(double ideal, double width) :
      base("Touch Transition", ideal, width)
    { }
    public override double EvaluateScore(List<TimestampReport> reports)
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
      var num = (100 * touchTransitions) / reports.Count;
      this.values.Add(num);
      var score = WeightTransferFunction(Width, Ideal, num);
      this.scores.Add(score);
      return score;
    }
  }
  class HeightSD : EvalCriteria
  {
    public HeightSD(double ideal, double width)
      : base("Height Standard Deviation", ideal, width) { }
    public override double EvaluateScore(List<TimestampReport> reports)
    {
      var positions = new List<double>();
      foreach (var timestamp in reports)
      {
        positions.Add(timestamp.position.y);
      }
      var num = GetStandardDeviation(positions);
      this.values.Add(num);
      var score = WeightTransferFunction(Width, Ideal, num);
      this.scores.Add(score);
      return score;
    }
  }
  class MinSpeed : EvalCriteria
  {
    public MinSpeed(double ideal, double width)
      : base("Minimum Speed", ideal, width) { }
    public override double EvaluateScore(List<TimestampReport> reports)
    {
      foreach (var time in reports)
      {
        if (time.velocityMag < 0.5) // TODO: try to detect stalling and reversing.
          // with backpropagation, this should fix the stalling issue
          return 0.01;
      }
      return 1;
    }
  }

  public static class Evaluator
  {
    static List<EvalCriteria> criteria = new List<EvalCriteria>();

    private static object m_lock = new object();

    private static void checkCriteriaInitialized()
    {
      lock (m_lock)
      {
        if (criteria.Count == 0)
        {
          // create a sum acceleration criteria? (excluding freefall)
          criteria.Add(new AccelSpikeCritera(1.5, 1));
          criteria.Add(new AverageSpeed(9, 3));
          criteria.Add(new SpeedSD(1, 1));
          // criteria.Add(new DirectionSD(0.5, 0.2));
          criteria.Add(new TouchTransition(7, 3));
          // criteria.Add(new HeightSD(400, 75));
          criteria.Add(new MinSpeed(0, 0)); // params unused
        }
      }
    }

    public static int calculateCost(Report report)
    {
      checkCriteriaInitialized();

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

      foreach (var item in criteria)
      {
        if (item != null)
        {
          multiplier *= item.EvaluateScore(report.timestamps);
        }
      }

      NumSuccess++;

      var deducted = multiplier * maxInt;
      return (int)(maxInt - deducted);
    }

    private static int NumEvaluations { get; set; }
    private static int NumSuccess { get; set; }
    private static int NumCrashes { get; set; }
    private static int NumNoNewLineCollision { get; set; }
    private static int NumStalls { get; set; }

    public static void PrintStatistics()
    {
      Console.WriteLine();
      Console.WriteLine("-----------------------[Evaluation]-------------------------------");
      Console.WriteLine();
      Console.WriteLine($"Number of simulations:             {NumEvaluations}");
      Console.WriteLine($"Number of successful runs:         {NumSuccess}");
      Console.WriteLine($"Number of crashes:                 {NumCrashes}");
      Console.WriteLine($"Number of stalls:                  {NumStalls}");
      Console.WriteLine($"Number of new lines not touched:   {NumNoNewLineCollision}");
      Console.WriteLine();
      Console.WriteLine($"Sucess rate: {100 * (double)NumSuccess / (double)NumEvaluations}%");
      Console.WriteLine();

      Console.WriteLine("Criteria Name                     Average  Ideal    Width     Score");
      foreach (var item in criteria)
      {
        if (item != null)
        {
          Console.WriteLine(
            String.Format("{0,-33} {1,-10:N0} {2,-8:N0} {3,-8:N0} {4,-8:N0}",
            item.Name,
            item.GetAverageValue().ToString("0.00"),
            item.Ideal.ToString(".0"),
            item.Width.ToString(".0"),
            item.GetAverageScore().ToString("0.00")
            ));
        }
      }
      Console.WriteLine();
      Console.WriteLine("-------------------------------------------------------------------");
      Console.WriteLine();
    }


    
  }
}
