﻿using Discord;
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
      var initialRider = timeline.GetFrame(0);
      Vec2 initialPosition = GetCenter(initialRider);
      Vec2 initialVelocity = new Vec2(0.1, 0.1);
      Vec2 currVelocity = new Vec2(0, 0);
      Vec2 acceleration= new Vec2(0, 0);

      Debug.Assert(track.GetLastAddedLine() == addedLine);

      int i = 1;
      int idleCount = 0;
      while (idleCount < 5 && !IsGoingTooFast(currVelocity) && i < track.LineLookup.Count * 140)
      {
        TimestampReport r = new TimestampReport();

        var rider = timeline.GetFrame(i);
        Vec2 currPosition = GetCenter(rider);
        currVelocity = CalculateVelocity(initialPosition, currPosition);
        acceleration = CalculateVelocity(initialVelocity, currVelocity);
        initialPosition = currPosition;
        initialVelocity = currVelocity;
        r.position = currPosition;
        r.velocity = currVelocity;
        r.velocityMag = currVelocity.Mag();
        r.acceleration = acceleration;
        r.accelerationMag = acceleration.Mag();
        r.timeStamp = i;
        r.freeFall = (Math.Abs(acceleration.y - 0.175) < 0.0001 &&
          System.Math.Abs(acceleration.x) < 0.0001);

        result.collidedWithNewLine |= timeline.HasCollidedWithLine(i, addedLine.ID);
        result.crashed |= (rider.Crashed || rider.SledBroken);

        // if (timeline.IsFrameUniqueCollision(i))
        // {
        //   Console.WriteLine("unique collision");
        // }

        // idle tracking
        if (IsIdle(currVelocity))
          idleCount++;
        else
          idleCount = 0;


        result.timestamps.Add(r);
        i++;
      }
      if (idleCount >= 5)
      {
        result.stopped = true;
      }
      return result;
    }
    public static Vec2 CalculateVelocity(Vec2 initial, Vec2 curr)
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
