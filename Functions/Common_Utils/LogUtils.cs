using Microsoft.Extensions.Logging;
using System;

namespace Common_Utils
{
    class LogUtils
    {
        public static string LogError(ILogger log, Exception ex, string err)
        {
            log.LogError(err);
            log.LogError($"{ex.Message}");
            return err.TrimEnd() + " " + ex.Message;
        }
    }
}