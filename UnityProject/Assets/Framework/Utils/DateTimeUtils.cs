using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DateTimeUtils
{
    private static bool _serverTimeHasInit = false;
    private static long _serverLocalTimeDiffMs;

    public const string DBDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    public static string GetNowForDB()
    {
        return DateTime.UtcNow.ToString(DBDateTimeFormat);
    }

    public static long GetNowTicks()
    {
        return DateTime.UtcNow.Ticks;
    }

    public static long GetNowSeconds()
    {
        return DateTime.UtcNow.Ticks / 10000000;
    }

    public static DateTime FromDbString(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return DateTime.UtcNow;
        }
        return DateTime.Parse(str);
    }

    public static string ToDbString(DateTime dt)
    {
        return dt.ToString(DBDateTimeFormat);
    }

    /// <summary>
    /// 日期转换成unix时间戳 毫秒
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long DateTimeToUnixTimestampMillis(DateTime dateTime)
    {
        var start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
        return Convert.ToInt64((dateTime - start).TotalMilliseconds);
    }

    /// <summary>
    /// unix时间戳转换成日期
    /// </summary>
    /// <param name="unixTimeStamp">时间戳毫秒</param>
    /// <returns></returns>
    public static DateTime UnixTimestampMillisToLocalDateTime(long unixTimeStamp)
    {
        var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        start = start.AddMilliseconds(unixTimeStamp);
        return TimeZoneInfo.ConvertTime(start, TimeZoneInfo.Local);
    }

    public static long GetNowUnixTimestampMillis()
    {
        return DateTimeToUnixTimestampMillis(DateTime.UtcNow);
    }

    public static void SyncServerTime(long timestamp)
    {
        _serverTimeHasInit = true;
        long localTs = GetNowUnixTimestampMillis();
        _serverLocalTimeDiffMs = timestamp - localTs;
    }

    public static DateTime GetServerTimeNow()
    {
        if (!_serverTimeHasInit)
        {
            return DateTime.Now;
        }
        else
        {
            return UnixTimestampMillisToLocalDateTime(GetNowUnixTimestampMillis() + _serverLocalTimeDiffMs);
        }
    }

    public static long GetServerTimeNowTimestampMillis()
    {
        return DateTimeToUnixTimestampMillis(GetServerTimeNow().ToUniversalTime());
    }

    public static string GetServerSmartDateStringByTimestampMillis(long timestamp)
    {
        DateTime dt = UnixTimestampMillisToLocalDateTime(timestamp);
        TimeSpan span = GetServerTimeNow() - dt;
        if (span.TotalDays > 60)
        {
            return dt.ToString("yyyy-M-d");
        }
        else if (span.TotalDays > 30)
        {
            return "1个月前";
        }
        else if (span.TotalDays > 14)
        {
            return "2周前";
        }
        else if (span.TotalDays > 7)
        {
            return "1周前";
        }
        else if (span.TotalDays > 1)
        {
			return string.Format("{0}天前", (int)Math.Floor(span.TotalDays));
        }
        else if (span.TotalHours > 1)
        {
			return string.Format("{0}小时前", (int)Math.Floor(span.TotalHours));
        }
        else if (span.TotalMinutes > 1)
        {
			return string.Format("{0}分钟前", (int)Math.Floor(span.TotalMinutes));
        }
        else
        {
            return "刚刚";
        }
    }

    public static bool IsSameDay(DateTime time1, DateTime time2)
    {
        return time1.Date == time2.Date;
    }

    public static DateTime ParseDateStringInt(int dateStringInt)
    {
        int leftInt = dateStringInt;
        int year = leftInt / 10000;
        leftInt = leftInt - year * 10000;
        int month = leftInt / 100;
        leftInt = leftInt - month * 100;
        int day = leftInt;
        try
        {
            return new DateTime(year, month, day);
        }
        catch
        {
            return DateTime.Now;
        }
    }
    
    public static string GetHmsTime(float time)
    {
        float h = Mathf.FloorToInt(time / 3600f);
        float m = Mathf.FloorToInt(time / 60f - h * 60f);
        float s = Mathf.FloorToInt(time - m * 60f - h * 3600f);
        if (h <= 0) {
            return m.ToString("00") + ":" + s.ToString("00");
        } else {
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }
    }
}
