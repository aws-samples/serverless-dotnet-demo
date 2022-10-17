using System;
using Amazon.XRay.Recorder.Core;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Shared
{
    public static class Logging
    {
        private static Logger _logger;
        static Logging()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .MinimumLevel.Override("Amazon.XRay", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter())
                .CreateLogger();
        }

        public static void LogInformation(string message)
        {
            using (LogContext.PushProperty("TraceId", AWSXRayRecorder.Instance.GetEntity().TraceId))
                _logger.Information(message);
        }

        public static void LogWarning(string message, Exception ex = null)
        {
            using (LogContext.PushProperty("TraceId", AWSXRayRecorder.Instance.GetEntity().TraceId))
                _logger.Warning(ex, message);
        }

        public static void LogError(Exception ex, string message)
        {
            using (LogContext.PushProperty("TraceId", AWSXRayRecorder.Instance.GetEntity().TraceId))
                _logger.Error(ex, message);
        }
    }
}