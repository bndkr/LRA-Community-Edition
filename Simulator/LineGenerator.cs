using linerider;
using linerider.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using System.Runtime.CompilerServices;

namespace Simulator
{
  public static class LineGenerator
  {
    public static GameLine AddLineToTrack(Track track,
                                          Report? winnerLastReport = null,
                                          int? numTotalFails = null)
    {
      Random r = new Random();


      var winnerLastReportVel = 1.0;
      if (winnerLastReport != null)
      {
        winnerLastReportVel = winnerLastReport.timestamps[winnerLastReport.timestamps.Count - 1].velocity.x;
      }

      var lastLine = (StandardLine) track.GetLastAddedLine();
      var scale = lastLine.GetLength() / 2.5; // this constant seems to control line length

      var lastPoint1 = new Vec2(lastLine.GetX1(), lastLine.GetY1());
      var lastPoint2 = new Vec2(lastLine.GetX2(), lastLine.GetY2());

      Vec2 lastPointLeft = (lastPoint1.x < lastPoint2.x ? lastPoint1 : lastPoint2);
      Vec2 lastPointRight = (lastPoint1.x >= lastPoint2.x ? lastPoint1 : lastPoint2);
      var lastLineInverted = lastLine.inv;

      // chance for new, seperate line segment
      if (r.NextDouble() < 0.3)
      {
        if (winnerLastReportVel > 0)
        {
          // try to extend to the right
          var xOffset = lastPointRight.x - lastPointLeft.x;
          var yOffset = lastPointRight.y - lastPointLeft.y;

          var dxOffset = xOffset + GetScaledRandom(r, scale);
          var dyOffset = yOffset + GetScaledRandom(r, scale);

          var p1 = new Vec2(
            lastPointRight.x + xOffset + GetScaledRandom(r, scale),
            lastPointRight.y + yOffset + GetScaledRandom(r, scale));
          var p2 = new Vec2(p1.x + dxOffset, p1.y + dyOffset);
          return new StandardLine(p1.x, p1.y, p2.x, p2.y, lastLineInverted);
        }
        else
        {
          // try to extend to the left
          var xOffset = lastPointLeft.x - lastPointRight.x;
          var yOffset = lastPointLeft.y - lastPointRight.y;

          var dxOffset = xOffset + GetScaledRandom(r, scale);
          var dyOffset = yOffset + GetScaledRandom(r, scale);

          var p1 = new Vec2(
            lastPointRight.x - xOffset + GetScaledRandom(r, scale),
            lastPointRight.y - yOffset + GetScaledRandom(r, scale));
          var p2 = new Vec2(p1.x + dxOffset, p1.y + dyOffset);
          return new StandardLine(p1.x, p1.y, p2.x, p2.y, lastLineInverted);
        }
      }
      else // generate a line that connects to the last one
      {
        // find the second-to-last line
        var secondLine = (StandardLine)track.GetSecondToLastAddedLine();
        if (secondLine == null)
        {
          // add the new line to lastPointRight

          var xOffset = lastPointRight.x - lastPointLeft.x + GetScaledRandom(r, scale);
          var yOffset = lastPointRight.y - lastPointLeft.y + GetScaledRandom(r, scale);

          return new StandardLine(lastPointRight.x,
                                  lastPointRight.y,
                                  lastPointRight.x + xOffset,
                                  lastPointRight.y + yOffset,
                                  lastLineInverted);
        }
        var secondLineP1 = new Vec2(secondLine.GetX1(), secondLine.GetY1());
        var secondLineP2 = new Vec2(secondLine.GetX2(), secondLine.GetY2());

        var secondLineLeft = (secondLineP1.x < secondLineP2.x ? secondLineP1 : secondLineP2);
        var secondLineRight = (secondLineP1.x >= secondLineP2.x ? secondLineP1 : secondLineP2);

        if (secondLineRight == lastPointLeft)
        {
          // add the new line to lastPointRight

          var xOffset = lastPointRight.x - lastPointLeft.x + GetScaledRandom(r, scale);
          var yOffset = lastPointRight.y - lastPointLeft.y + GetScaledRandom(r, scale);

          return new StandardLine(lastPointRight.x,
                                  lastPointRight.y,
                                  lastPointRight.x + xOffset,
                                  lastPointRight.y + yOffset, 
                                  lastLineInverted);
        }
        else if (secondLineLeft == lastPointRight)
        {
          // add the new line to lastPointLeft
          var xOffset = lastPointLeft.x - lastPointRight.x + GetScaledRandom(r, scale);
          var yOffset = lastPointLeft.y - lastPointRight.y + GetScaledRandom(r, scale);

          return new StandardLine(lastPointRight.x,
                                  lastPointRight.y,
                                  lastPointRight.x + xOffset,
                                  lastPointRight.y + yOffset,
                                  lastLineInverted);
        }
        else
        {
          // the second line is floating, add to either side
          if (winnerLastReportVel > 0)
          {
            // add the new line to lastPointRight
            var xOffset = lastPointRight.x - lastPointLeft.x + GetScaledRandom(r, scale);
            var yOffset = lastPointRight.y - lastPointLeft.y + GetScaledRandom(r, scale);

            return new StandardLine(lastPointRight.x,
                                    lastPointRight.y,
                                    lastPointRight.x + xOffset,
                                    lastPointRight.y + yOffset,
                                    lastLineInverted);
          }
          else
          {
            // add the new line to lastPointLeft
            var xOffset = lastPointLeft.x - lastPointRight.x + GetScaledRandom(r, scale);
            var yOffset = lastPointLeft.y - lastPointRight.y + GetScaledRandom(r, scale);

            return new StandardLine(lastPointRight.x,
                                    lastPointRight.y,
                                    lastPointRight.x - xOffset,
                                    lastPointRight.y - yOffset,
                                    lastLineInverted);
          }
        }
      }
    }

    private static double GetScaledRandom(Random r, double length)
    {
      return r.NextDouble() * length - (length / 2);
    }

    private static Vec2 GetLineVec2(GameLine line)
    {
      return new Vec2(
        line.GetX2() - line.GetX1(), line.GetY2() - line.GetY1());
    }
    private static double GetLineAngle(Vec2 vec)
    {
      return Math.Atan(vec.y / vec.x);
    }
  }
}
