using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using linerider;
using linerider.Game;

namespace Simulator
{
  internal class TrackExtender
  {
    public Track CombineTracks(Track original, Track offset)
    {
      return null;
    }

    public void UpdateTrackInitPosition(ref Track toSplit, int timestep)
    {
      // determine the position and velocity of the rider at the timestep
      Timeline t = new Timeline(toSplit);

      var rider = t.GetFrame(timestep);

      // these may not be accurate enough for our needs 
      var position = new Vec2(rider.CalculateCenterX(), rider.CalculateCenterY());
      var velocity = new Vec2(rider.CalculateMomentumX(), rider.CalculateMomentumY());
    }

    public Tuple<Vec2, Vec2> GetInitialState(Track track)
    {
      // return the initial position and velocity of the track

      // var position = new Vec2(track.
      return null;
    }
  }
}
