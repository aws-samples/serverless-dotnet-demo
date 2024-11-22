using Amazon.Lambda.Core;

namespace Shared;

public class FakeLambdaContext : ILambdaContext
{
    private static readonly FakeLambdaLogger FakeLogger = new();
    
    public string AwsRequestId => throw new NotImplementedException();

    public IClientContext ClientContext => throw new NotImplementedException();

    public string FunctionName => throw new NotImplementedException();

    public string FunctionVersion => throw new NotImplementedException();

    public ICognitoIdentity Identity => throw new NotImplementedException();

    public string InvokedFunctionArn => throw new NotImplementedException();

    public ILambdaLogger Logger => FakeLogger;

    public string LogGroupName => throw new NotImplementedException();

    public string LogStreamName => throw new NotImplementedException();

    public int MemoryLimitInMB => throw new NotImplementedException();

    public TimeSpan RemainingTime => throw new NotImplementedException();
}

public class FakeLambdaLogger : ILambdaLogger
{
    public void Log(string message)
    {
        Console.Write(message);
    }

    public void LogLine(string message)
    {
        Console.WriteLine(message);
    }
}