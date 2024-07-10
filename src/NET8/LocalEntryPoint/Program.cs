using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using GetProducts;

namespace LocalEntryPoint;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var functionHandler = new GetProducts.Function();

        var request = new APIGatewayHttpApiV2ProxyRequest()
        {
            RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext()
            {
                Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription()
                {
                    Method = HttpMethod.Get.Method
                }
            },
            PathParameters = new Dictionary<string, string>()
            {
                {"id", "1"}
            }
        };
        
        var result = functionHandler.FunctionHandler(request, new MyContext()).GetAwaiter().GetResult();

        Console.WriteLine($"Test Function StatusCode: {result.StatusCode}");
    }
}

internal class MyContext : ILambdaContext
{
    public MyContext()
    {
        Logger = new MyLogger();
    }

    public string AwsRequestId { get; } = null!;
    public IClientContext ClientContext { get; } = null!;
    public string FunctionName { get; } = null!;
    public string FunctionVersion { get; } = null!;
    public ICognitoIdentity Identity { get; } = null!;
    public string InvokedFunctionArn { get; } = null!;
    public ILambdaLogger Logger { get; }
    public string LogGroupName { get; } = null!;
    public string LogStreamName { get; } = null!;
    public int MemoryLimitInMB { get; } = 100000;
    public TimeSpan RemainingTime { get; } = TimeSpan.MaxValue;
}

internal class MyLogger : ILambdaLogger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }

    public void LogLine(string message)
    {
        Console.WriteLine(message);
    }
}