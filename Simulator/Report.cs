using linerider.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
  public class TimestampReport
  {
    public int timeStamp;
    public Vec2 position;
    public Vec2 velocity;
    public Vec2 acceleration;
    public double velocityMag;
    public double accelerationMag;
    public bool freeFall;
    public bool idle;
  }
  public class Report
  {
    public List<TimestampReport> timestamps = new List<TimestampReport>();
    public bool crashed = false;
    public bool collidedWithNewLine = false;
    public bool stopped;
    public Rider? finalPosition = null;

    public bool PrintReport(string path)
    {
      if (timestamps.Count == 0) return false;

      var str = new StringBuilder();
      str.AppendLine("timestamp,xpos,ypos,xvel,yvel,speed,xacc,yacc,accmag,freefall");

      foreach (var report in timestamps)
      {
        str.AppendLine(
          String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
          report.timeStamp,
          report.position.x,
          -report.position.y,
          report.velocity.x,
          report.velocity.y,
          report.velocityMag,
          report.acceleration.x,
          report.acceleration.y,
          report.accelerationMag,
          (report.freeFall ? 1 : 0)));
      }
      File.WriteAllText(path, str.ToString());
      return true;
    }
  }
}
