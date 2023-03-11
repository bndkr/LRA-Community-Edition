using linerider;
using linerider.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
  public static class LineGenerator
  {
    public static GameLine AddLineToTrack(Track track)
    {
      Random r = new Random();
      
      var lastLine = (StandardLine) track.GetLastAddedLine();
      var lastLineLength = lastLine.GetLength();
      var lastLineAngle = GetLineAngle(GetLineVec2(lastLine));
      var lastLineInverted = lastLine.inv;

      var lastLineP1 = new Vec2(lastLine.GetX1(), lastLine.GetY1());
      var lastLineP2 = new Vec2(lastLine.GetX2(), lastLine.GetY2());

      // chance for new, seperate line segment
      if (r.NextDouble() < 0.1)
      {
        var newP1 = new Vec2((lastLineP1.x + lastLineP2.x) / 2,
                                (lastLineP1.y + lastLineP2.y) / 2);
        newP1.x += r.NextDouble() * 200 - 100;
        newP1.y += r.NextDouble() * 200 - 100;

        var newAngle = r.NextDouble() * Math.PI - (Math.PI / 2);
        var newLength = r.NextDouble() * 50;
        var newLine = new StandardLine(newP1.x,
                                       newP1.y,
                                       newP1.x + newLength * Math.Cos(newAngle),
                                       newP1.y + newLength * Math.Sin(newAngle));
        return newLine;
      }
      else // generate a line that connects to the last one
      {
        // find the second-to-last line
        var secondLine = (StandardLine)track.GetSecondToLastAddedLine();
        if (secondLine == null)
        {
          var line = AddLine(lastLineP2,
                             lastLineAngle,
                             lastLineLength,
                             lastLineInverted);
          return line;
        }
        var secondLineP1 = new Vec2(secondLine.GetX1(), secondLine.GetY1());
        var secondLineP2 = new Vec2(secondLine.GetX2(), secondLine.GetY2());

        if (secondLineP2 == lastLineP1)
        {
          // add the new line to lastLineP2
          var line = AddLine(lastLineP2,
                             lastLineAngle,
                             lastLineLength,
                             lastLineInverted);
          return line;
          
        }
        else if (secondLineP1 == lastLineP2)
        {
          // add the new line to lastLineP1
          var line = AddLine(lastLineP1,
                  lastLineAngle,
                  lastLineLength,
                  lastLineInverted);
          return line;
        }
        else
        {
          // the second line is floating, add to either side
          if (r.NextDouble() < 0.5)
          {
            // add the new line to lastLineP1 
            var line = AddLine(lastLineP1,
                    lastLineAngle,
                    lastLineLength,
                    lastLineInverted);
            return line;
          }
          else
          {
            // add the new line to lastLineP2
            var line = AddLine(lastLineP2,
                    lastLineAngle,
                    lastLineLength,
                    lastLineInverted);
            return line;
          }
        }
      }
    }

    private static GameLine AddLine(Vec2 point,
                                double angle,
                                double length,
                                bool inverted)
    {
      Random r = new Random();
      double dAngle = r.NextDouble() - 0.5;
      double dLength = r.NextDouble() * 30 - 15;

      if (!inverted)
      {
        var newLine = new StandardLine(
          point.x,
          point.y,
          point.x + (length + dLength) * Math.Cos(angle + dAngle),
          point.y + (length + dLength) * Math.Sin(angle + dAngle),
          inverted);
        return newLine;
      }
      else
      {
        var newLine = new StandardLine(
          point.x + (length + dLength) * Math.Cos(angle + dAngle),
          point.y + (length + dLength) * Math.Sin(angle + dAngle),
          point.x,
          point.y,
          inverted);
        return newLine;
      }
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
