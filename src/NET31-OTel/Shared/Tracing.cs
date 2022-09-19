using System;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;

namespace Shared
{
    public static class Tracing
    {
        public static TracerProvider TraceProvider;
        
        public static void Init()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            TraceProvider = Sdk.CreateTracerProviderBuilder()
                .AddAWSInstrumentation()
                .AddOtlpExporter()
                .AddAWSLambdaConfigurations()
                .Build();
        }
    }
}