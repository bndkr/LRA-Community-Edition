using linerider;
using linerider.Game;
using linerider.IO;
using linerider.Utils;
using System.Numerics;
using System;
using System.Text.Encodings.Web;
using System.Diagnostics;

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

    public override string ToString()
    {
      return $"x: {x}, y: {y}";
    }
  }
  public class Program
  {
    const int NUM_TRIES = 200;
    const int NUM_LINES = 20;

    static void Main(string[] args)
    {
      {
        var test = new Test.TestClass();
        test.TestTrackClone();
        test.TestSimulator();
        test.TestLineCreation();
      }

      // generate the starter track
      var track = new Track();
      track.Name = "yeeoo";
      var startLine = new StandardLine(0, 20, 100, 100);
      track.AddLine(startLine);

      var lineage = new List<Track>();
      Report? winnerLastReport = null;

      for (int i = 0; i < NUM_LINES; i++)
      {
        var possibleTracks = new Track[NUM_TRIES];
        var scores = new int[NUM_TRIES];
        var reports = new Report[NUM_TRIES];
        var taskResults = new Task<TryTrackResult>[NUM_TRIES];

        // launch simulations
        for (int j = 0; j < NUM_TRIES; j++)
        {
          var p = new TryTrackParams(track, j, winnerLastReport);
          taskResults[j] = TryTrackAsync(p);
        }

        for (int j = 0; j < NUM_TRIES; j++)
        {
          taskResults[j].Wait();
          // unpack results
          var result = taskResults[j].Result;
          possibleTracks[j] = result.resultTrack;
          scores[j] = result.score;
          reports[j] = result.report;
        }

        var winner = FindLowestCost(scores);
        System.Console.WriteLine($"Winner: run {winner}");
        track = possibleTracks[winner];
        lineage.Add(new Track(track));
        winnerLastReport = reports[winner];
      }
      TRKWriter.SaveTrack(track, "coolest");
      for (int i = 0; i < lineage.Count; i++)
      {
        TRKWriter.SaveTrack(lineage[i], $"lin_{i}");
      }
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

    private static async Task<TryTrackResult> TryTrackAsync(TryTrackParams p)
    {
      return await Task.Run(() => TryTrack(p));
    }

    private static TryTrackResult TryTrack(TryTrackParams p)
    {
      var result = new TryTrackResult();
      var currTrack = new Track(p.lastWinner);  // clone track
      var newLine = LineGenerator.AddLineToTrack(currTrack, p.winnerLastReport);
      currTrack.AddLine(newLine);
      
      result.report = TrackSimulator.Simulate(currTrack, newLine);

      var score = Evaluator.calculateCost(result.report);
      result.score = score;
      result.resultTrack = currTrack;
      System.Console.WriteLine($"finished run {p.index}: {result.score}");
      return result;
    }
  }


  class TryTrackParams
  {
    public TryTrackParams(Track lastWinner, int index, Report? winnerLastReport)
    {
      this.lastWinner = lastWinner;
      this.index = index;
      this.winnerLastReport = winnerLastReport;
    }
    public Track lastWinner;
    public int index;
    public Report? winnerLastReport;
  }

  class TryTrackResult
  {
    public int score;
    public Track resultTrack;
    public Report report;
  }

}
