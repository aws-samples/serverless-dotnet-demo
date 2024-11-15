using Amazon.CDK;

namespace AWS.DotnetServerlessDemo.Cdk;

sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();

        new GithubOIDCConnectionStack(
            app,
            "GithubOIDCConnectionStack",
            new StackProps
            {
                TerminationProtection = true
            }
        );

        app.Synth();
    }
}
