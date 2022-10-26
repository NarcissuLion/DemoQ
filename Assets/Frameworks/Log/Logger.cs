using System;
using System.IO;

namespace Framework.Log
{
    public class Logger
    {
        public enum Level
        {
            Normal,
            Warning,
            Error,
            Exception
        }

        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        public static void Log(object message,UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(message,context);
        }
        
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public static void LogWarning(object message,UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(message,context);
        }

        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public static void LogError(object message,UnityEngine.Object context)
        {
            UnityEngine.Debug.LogError(message,context);
        }

        public static void LogException(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        public static void LogException(Exception exception,UnityEngine.Object context)
        {
            UnityEngine.Debug.LogException(exception,context);
        }

        public static void Assert(bool InCondition)
        {
            Assert(InCondition, null, null);
        }
            
        public static void Assert(bool InCondition, string InFormat)
        {
            Assert(InCondition, InFormat, null);
        }
            
        public static void Assert(bool InCondition, string InFormat, params object[] InParameters)
        {
            if (!InCondition)
            {
                try
                {
                    string str = null;
                    if (!string.IsNullOrEmpty(InFormat))
                    {
                        try
                        {
                            if (InParameters != null)
                            {
                                str = string.Format(InFormat, InParameters);
                            }
                            else
                            {
                                str = InFormat;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        str = string.Format(" no assert detail, stacktrace is :{0}", Environment.StackTrace);
                    }
                    if (str != null)
                    {
                        string str2 = "Assert failed! " + str;
                        LogError(str2);
                    }
                    else
                    {
                        LogError("Assert failed!");
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public static void Log(Level level, UnityEngine.Object context, System.Object msg)
        {
            switch (level)
            {
                case Level.Normal:
                    {
                        if (context == null)
                            Log(msg);
                        else
                            Log(msg, context);
                    }
                    break;
                case Level.Warning:
                    {
                        if (context == null)
                            LogWarning(msg);
                        else
                            LogWarning(msg, context);
                    }
                    break;
                case Level.Error:
                    {
                        if (context == null)
                            LogError(msg);
                        else
                            LogError(msg, context);
                    }
                    break;
                case Level.Exception:
                    {
                        Exception ex = msg as Exception;
                        if (ex != null)
                        {
                            if (context == null)
                                LogException(ex);
                            else
                                LogException(ex, context);
                        }
                    }
                    break;
            }
        }

        public static void LogFormat(Level level, UnityEngine.Object context, string strFormat, params System.Object[] aArgs)
        {
            switch (level)
            {
                case Level.Normal:
                    {
                        if (context == null)
                            Log(string.Format(strFormat, aArgs));
                        else
                            Log(string.Format(strFormat, aArgs), context);
                    }
                    break;
                case Level.Warning:
                    {
                        if (context == null)
                            LogWarning(string.Format(strFormat, aArgs));
                        else
                            LogWarning(string.Format(strFormat, aArgs), context);
                    }
                    break;
                case Level.Error:
                    {
                        if (context == null)
                            LogError(string.Format(strFormat, aArgs));
                        else
                            LogError(string.Format(strFormat, aArgs), context);
                    }
                    break;
            }
        }

        public static void Log(string strFormat, params string[] aArgs)
        {
            Log(string.Format(strFormat, aArgs));
        }
    }
}
