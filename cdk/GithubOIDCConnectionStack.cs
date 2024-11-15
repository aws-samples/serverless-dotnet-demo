using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace AWS.DotnetServerlessDemo.Cdk;

public class GithubOIDCConnectionStack : Stack
{
    private const string DotnetServerlessRepoName = "aws-samples/serverless-dotnet-demo";

    internal GithubOIDCConnectionStack(Construct scope, string id, IStackProps props = null)
        : base(scope, id, props)
    {
        var githubIdentity = CreateGitHubOidcTestRunner();

        AddSamDeploymentRole(githubIdentity);
        AddLoadTestRunnerRole(githubIdentity);
    }

    /// <remarks>
    /// GitHub Documentation: https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-amazon-web-services
    /// Example Blog: https://towardsthecloud.com/aws-cdk-openid-connect-github
    /// </remarks>
    private WebIdentityPrincipal CreateGitHubOidcTestRunner()
    {
        var githubProvider =
            new OpenIdConnectProvider(
                this,
                "githubProvider",
                new OpenIdConnectProviderProps
                {
                    Url = "https://token.actions.githubusercontent.com",
                    ClientIds = new[] {"sts.amazonaws.com"}
                }
            );

        var assumeRoleIdentity = new WebIdentityPrincipal(
            githubProvider.OpenIdConnectProviderArn,
            conditions: new Dictionary<string, object>
            {
            {
                "StringLike",
                new Dictionary<string, string>
                {
                    { "token.actions.githubusercontent.com:sub", $"repo:{DotnetServerlessRepoName}:*" },
                    { "token.actions.githubusercontent.com:aud", "sts.amazonaws.com" }
                }
            }
            }
        );

        return assumeRoleIdentity;
    }

    /// <summary>
    /// Create a role that <paramref name="githubIdentity"/> can use to
    /// invoke `sam deploy` commands from GitHub Actions.
    /// </summary>
    private void AddSamDeploymentRole(WebIdentityPrincipal githubIdentity)
    {
        var githubSamDeploymentRole = new Role(
            this,
            "githubSamDeploymentRole",
            new RoleProps
            {
                AssumedBy = githubIdentity,
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("AdministratorAccess"),
                    ManagedPolicy.FromAwsManagedPolicyName("CloudWatchAgentServerPolicy")
                },
                RoleName = "githubSamDeploymentRole",
                MaxSessionDuration = Duration.Hours(1)
            }
        );

        new CfnOutput(
            this,
            "githubSamDeploymentRoleArn",
            new CfnOutputProps
            {
                Value = githubSamDeploymentRole.RoleArn,
                ExportName = "githubSamDeploymentRoleArn"
            }
        );
    }

    /// <summary>
    /// Create a role that <paramref name="githubIdentity"/> can use to
    /// invoke Load Test commands from GitHub Actions.
    /// </summary>
    private void AddLoadTestRunnerRole(WebIdentityPrincipal githubIdentity)
    {
        var githubLoadTestRunnerRole = new Role(
            this,
            "githubLoadTestRunnerRole",
            new RoleProps
            {
                AssumedBy = githubIdentity,
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("AdministratorAccess"),
                    ManagedPolicy.FromAwsManagedPolicyName("CloudWatchAgentServerPolicy")
                },
                RoleName = "githubLoadTestRunnerRole",
                MaxSessionDuration = Duration.Hours(1)
            }
        );

        new CfnOutput(
            this,
            "githubLoadTestRunnerRoleArn",
            new CfnOutputProps
            {
                Value = githubLoadTestRunnerRole.RoleArn,
                ExportName = "githubLoadTestRunnerRoleArn"
            }
        );
    }
}