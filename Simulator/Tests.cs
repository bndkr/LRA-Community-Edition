using linerider;
using linerider.Game;
using System.Diagnostics;
using Simulator;
using System.ComponentModel.Design;
using linerider.IO;

namespace Test
{
  public class TestClass
  {
    public void TestTrackClone()
    {
      var track = new Track();
      track.Name = "test";

      var originalLine = new StandardLine(0.0, 0.0, 200.0, 200.0);
      track.AddLine(originalLine);

      var clonedTrack = new Track(track);
      Debug.Assert(clonedTrack.LineLookup.Count == track.LineLookup.Count);

      Debug.Assert(clonedTrack.LineLookup[0].GetX1() == track.LineLookup[0].GetX1());
      Debug.Assert(clonedTrack.LineLookup[0].GetX2() == track.LineLookup[0].GetX2());
      Debug.Assert(clonedTrack.LineLookup[0].GetY1() == track.LineLookup[0].GetY1());
      Debug.Assert(clonedTrack.LineLookup[0].GetY2() == track.LineLookup[0].GetY2());

      // add another line to the original track
      var newLine = new StandardLine(200.0, 200.0, 0, 0);
      track.AddLine(newLine);

      Debug.Assert(clonedTrack.LineLookup.Count == 1);
      Debug.Assert(track.LineLookup.Count == 2);
    }

    public void TestSimulator()
    {
      Track[] tracks = new Track[100];
      Report[] reports = new Report[100];

      for (int i = 0; i < tracks.Length; i++)
      {
        tracks[i] = new Track();
        var line = CreateRandomLine();
        tracks[i].AddLine(line);
        reports[i] = TrackSimulator.Simulate(tracks[i], line);
        reports[i].PrintReport($"report_{i}.csv");
      }
    }

    public void TestLineCreation()
    {
      var track = new Track();

      var line = new StandardLine(200, 200, 0, 0, true);
      track.AddLine(line);

      for (int i = 0; i < 20; i++)
      {
        var newline = LineGenerator.AddLineToTrack(track);
        track.AddLine(newline);
        TRKWriter.SaveTrack(track, $"iteration_{i}");
      }
    }

    private static StandardLine CreateRandomLine()
    {
      Random r = new Random();
      return new StandardLine(
        r.NextDouble() * 100 - 50,
        r.NextDouble() * 100 - 50,
        r.NextDouble() * 100 - 50,
        r.NextDouble() * 100 - 50);
    }

    public void TestTrackInitialState()
    {
      var track = new Track(); // track should have default initial state
      track.AddLine(new StandardLine(0, 20, 200, 200));

      var timeline = new Timeline(track);
      var state = timeline.GetFrame(40); // calculate the rider's position

      Track track2 = new Track(track);
      track2.SetInitialState(state);


      var timeline2 = new Timeline(track2);
      var initialState = timeline2.GetFrame(0);

    }
  }
}
