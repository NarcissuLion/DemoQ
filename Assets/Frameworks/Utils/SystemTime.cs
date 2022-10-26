using System;

namespace Framework.Utils
{
    static public class SystemTime
    {
        static private double _timeAtLaunch = time;

        //1970.1.1开始到现在的时间 秒
        static public double time
        {
            get
            {
                const double ticks2seconds = 1 / (double) TimeSpan.TicksPerSecond;
                long ticks = DateTime.Now.Ticks;
                double seconds = ( (double) ticks ) * ticks2seconds;
                return seconds;
            }
        }

        //系统启动以后的时间 秒
        static public double timeSinceLaunch
        {
            get
            {
                return time - _timeAtLaunch;
            }
        }
    }
}