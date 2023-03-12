using linerider;
using linerider.Game;
using linerider.IO;
using linerider.Utils;
using System.Numerics;
using System;
using System.Text.Encodings.Web;

namespace Simulator
{
  public struct Vec2
  {
    public double x;
    public double y;
    public Vec2(double x, double y)
    {
      this.x = x;
      this.y = y;
    }
    public double Mag()
    {
      return Math.Sqrt(x * x + y * y);
    }
    public static bool operator ==(Vec2 a, Vec2 b)
    {
      return (a.x == b.x && a.y == b.y);
    }
    public static bool operator !=(Vec2 a, Vec2 b)
    {
      return (a.x != b.x || a.y != b.y);
    }
  }
  public class Program
  {
    const int NUM_TRIES = 200;
    const int NUM_LINES = 20;

    static void Main(string[] args)
    {
      var test = new Test.TestClass();
      test.TestTrackClone();
      test.TestSimulator();
      test.TestLineCreation();

      // generate the starter track
      var track = new Track();
      track.Name = "yeeoo";
      var startLine = new StandardLine(0, 20, 100, 100);
      track.AddLine(startLine);

      for (int i = 0; i < NUM_LINES; i++)
      {
        var possibleTracks = new List<Track>();
        var scores = new List<int>();
        for (int j = 0; j < NUM_TRIES; j++)
        {
          var currTrack = new Track(track);  // clone track
          var newLine = LineGenerator.AddLineToTrack(currTrack);
          // System.Console.WriteLine($"added line {newLine}");
          currTrack.AddLine(newLine);

          var report = TrackSimulator.Simulate(currTrack, newLine);
          // report.PrintReport($"report_{i}_{j}.csv");

          System.Console.Write($"finished sim {j}");
          var score = Evaluator.calculateCost(report);
          scores.Add(score);
          System.Console.WriteLine("");

          possibleTracks.Add(currTrack);
          // TRKWriter.SaveTrack(possibleTracks[j], $"track_{i}_{j}");
        }
        var winner = FindLowestCost(scores.ToArray());
        System.Console.WriteLine($"Winner: run {winner}");
        track = possibleTracks[winner];
      }
      TRKWriter.SaveTrack(track, "coolest");
    }
    private static int FindLowestCost(int[] scores)
    {
      int result = 0;
      for (int i = 0; i < scores.Length; i++)
      {
        if (scores[i] < scores[result])
          result = i;
      }
      return result;
    }
  }
}
