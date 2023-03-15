using Discord;
using linerider;
using linerider.Game;
using linerider.IO;
using linerider.Utils;
using Simulator;
using System.Numerics;
using System.Diagnostics;


namespace Simulator
{
  public static class TrackSimulator
  {
    public static Report Simulate(Track track, GameLine addedLine)
    {
      var result = new Report();

      Timeline timeline = new Timeline(track);
      Vec2 initialVelocity = new Vec2(0.1, 0.1);
      Vec2 currVelocity = new Vec2(0, 0);
      Vec2 acceleration= new Vec2(0, 0);

      Debug.Assert(track.GetLastAddedLine() == addedLine);
      Rider lastLocation = timeline.GetFrame(0);

      int i = 1;
      int idleCount = 0;
      int airborneCount = 0;
      while (idleCount < 5 &&
             airborneCount < 75 &&
             i < track.LineLookup.Count * 140 &&
             !result.crashed)
      {
        TimestampReport r = new TimestampReport();

        var rider = timeline.GetFrame(i);
        Vec2 currPosition = GetCenter(rider);

        currVelocity.x = rider.CalculateMomentumX();
        currVelocity.y = rider.CalculateMomentumY();

        acceleration = CalculateAcceleration(initialVelocity, currVelocity);
        initialVelocity = currVelocity;
        r.position = currPosition;
        r.velocity = currVelocity;
        r.velocityMag = currVelocity.Mag();
        r.acceleration = acceleration;
        r.accelerationMag = acceleration.Mag();
        r.timeStamp = i;
        r.freeFall = IsInFreeFall(acceleration);

        if (IsIdle(currVelocity))
        {
          r.idle = true;
          result.stopped = true;
        }

        result.collidedWithNewLine |= timeline.HasCollidedWithLine(i, addedLine.ID);
        result.crashed |= (rider.Crashed || rider.SledBroken);
        result.finalPosition = null;

        // freefall tracking
        if (IsInFreeFall(acceleration))
          airborneCount++;
        else
          airborneCount = 0;

        if (i > 500 && !r.freeFall && !result.crashed)
        {
          result.finalPosition = lastLocation; //  prevent off-by-one error
          break;
        }

        lastLocation = rider;
        result.timestamps.Add(r);
        i++;
      }

      if (idleCount >= 5)
      {
        result.stopped = true;
      }
      return result;
    }

    private static bool IsInFreeFall(Vec2 acceleration)
    {
      return Math.Abs(acceleration.y - 0.175) < 0.0001 &&
             Math.Abs(acceleration.x) < 0.0001;
    }

    public static Vec2 CalculateAcceleration(Vec2 initial, Vec2 curr)
    {
      return new Vec2(curr.x - initial.x, curr.y - initial.y);
    }

    public static Vec2 GetCenter(Rider rider)
    {
      return new Vec2(
        rider.CalculateCenterX(),
        rider.CalculateCenterY());
    }

    private static bool IsIdle(Vec2 velocity)
    {
      return velocity.Mag() < 0.05;
    }

    private static bool IsGoingTooFast(Vec2 velocity)
    {
      return velocity.Mag() > 15;
    }
  }
}
