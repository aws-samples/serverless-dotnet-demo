## Lambda Demo with .NET

With the release of .NET 8 [AWS Lambda](https://aws.amazon.com/lambda/) now supports .NET 8 and .NET 6 as managed runtimes. With the availability of ARM64 using Graviton2 there have been vast improvements to using .NET with Lambda.

But how does that translate to actual application performance? And how does .NET compare to other available runtimes. This repository contains a simple serverless application across a range of .NET implementations and the corresponding benchmarking results.

## Application

![](./imgs/diagram.jpg)

The application consists of an [Amazon API Gateway](https://aws.amazon.com/api-gateway/) backed by four Lambda functions and an [Amazon DynamoDB](https://aws.amazon.com/dynamodb/) table for storage.

It includes the below implementations as well as benchmarking results for both x86 and ARM64:

- .NET 6 Lambda
- .NET 6 Top Level statements
- .NET 6 Minimal API
- .NET 6 Minimal API with AWS Lambda Web Adapter
- .NET 6 NativeAOT compilation
- .NET 8
- .NET 8 Native AOT
- .NET 8 Minimal API

## Requirements

- [AWS CLI](https://aws.amazon.com/cli/)
- [AWS SAM](https://aws.amazon.com/serverless/sam/)
- .NET 8, .NET 6
- [Artillery](https://www.artillery.io/) for load-testing the application
- Docker

## Software

There are four implementations included in the repository, covering a variety of Lambda runtimes and features. All the implementations use 1024MB of memory with Graviton2 (ARM64) as default. Tests are also executed against x86_64 architectures for comparison.

There is a separate project for each of the four Lambda functions, as well as a shared library that contains the data access implementations. It uses the hexagonal architecture pattern to decouple the entry points, from the main domain and storage logic.

## .NET 6

This implementation is the simplest route to upgrade a .NET Core 3.1 function to use .NET 6 as it only requires upgrading the function runtime, project target framework and any dependencies as per the final section of [this link](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/).

### .NET 6 Top Level Statements

This implementation uses the new features detailed in [this link](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/) including
- Top Level Statements
- Source generation
- Executable assemblies

### Minimal API
There is a single project named ApiBootstrap that contains all the start-up code and API endpoint mapping. The SAM template still deploys a separate function per API endpoint to negate concurrency issues.

It uses the new minimal API hosting model as detailed [here](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/). 

### .NET 6 Minimal API with AWS Lambda Web Adapter

Same as minimal API but instead of using Amazon.Lambda.AspNetCoreServer.Hosting/Amazon.Lambda.AspNetCoreServer it is based on [Aws Lambda Web Adapter](https://github.com/awslabs/aws-lambda-web-adapter)

### .NET 6 native AOT

The code is compiled natively for either Linux-x86_64 or Linux-ARM64 and then deployed manually to Lambda as a zip file. The SAM deploy can still be used to stand up the API Gateway endpoints and DynamoDb table, but won't be able to deploy native AOT .NET Lambda functions yet. Packages need to be published from Linux, since cross-OS native compilation is not supported yet. 

Details for compiling .NET 6 native AOT can be found [here](https://github.com/dotnet/runtimelab/blob/feature/NativeAOT/docs/using-nativeaot/compiling.md)

## .NET 8

### .NET 8 Managed

The code is compiled for the .NET 8 AWS Lambda managed runtime. The code is compiled as ReadyToRun for cold start speed. This sample should be able to be tested with `sam build` and then `sam deploy --guided`. 

### .NET 8 native AOT

The code is compiled natively for Linux-x86_64 or ARM64 then deployed to Lambda as a zip file.

Details for compiling .NET native AOT can be found [here](https://github.com/dotnet/runtimelab/blob/feature/NativeAOT/docs/using-nativeaot/compiling.md)

### .NET 8 minimal API with native AOT

There is a single project named ApiBootstrap that contains all the start-up code and API endpoint mapping. The code is compiled natively for Linux-x86_64 then deployed manually to Lambda as a zip file. Microsoft have announced limited support for ASP.NET and native AOT in .NET 8, using the `WebApplication.CreateSlimBuilder(args);` method.

Details for compiling .NET 8 native AOT can be found [here](https://github.com/dotnet/runtimelab/blob/feature/NativeAOT/docs/using-nativeaot/compiling.md)

## Deployment

To deploy the architecture into your AWS account, navigate into the respective folder under the src folder and run 'sam deploy --guided'. This will launch a deployment wizard, complete the required values to initiate the deployment. For example, for .NET 6:

``` bash
cd src/NET6
sam build
sam deploy --guided
```

## Testing

Benchmarks are executed using [Artillery](https://www.artillery.io/). Artillery is a modern load testing & smoke testing library for SRE and DevOps.

To run the tests, use the below scripts. Replace the $API_URL with the API URL output from the deployment:

``` bash
cd loadtest
artillery run load-test.yml --target "$API_URL"
```

## Summary
Below is the cold start and warm start latencies observed. Please refer to the load test folder to see the specifics of the test that were executed.

All latencies listed below are in milliseconds.

 is used to make **100 requests / second for 10 minutes to our API endpoints**.

[AWS Lambda Power Tuning](https://github.com/alexcasalboni/aws-lambda-power-tuning) is used to optimize the cost/performance. 1024MB of function memory provided the optimal balance between cost and performance.

For the .NET 8 Native AOT compiled example the optimal memory allocation was 3008mb.

![](./imgs/power-tuning.PNG)

### Results

The below CloudWatch Log Insights query was used to generate the results:

```
filter @type="REPORT"
| fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart
| stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart
```

### .NET 6

<table class="table-bordered">
        <tr>
            <th colspan="1" style="horizontal-align : middle;text-align:center;"></th>
            <th colspan="4" style="horizontal-align : middle;text-align:center;">Cold Start (ms)</th>
            <th colspan="4" style="horizontal-align : middle;text-align:center;">Warm Start (ms)</th>           
        </tr>
        <tr>
            <th></th>
            <th scope="col">p50</th>
            <th scope="col">p90</th>
            <th scope="col">p99</th>
            <th scope="col">max</th>
            <th scope="col">p50</th>
            <th scope="col">p90</th>
            <th scope="col">p99</th>
            <th scope="col">max</th>
        </tr>
        <tr>
            <th>ARM64</th>
            <td>873.59</td>
            <td>909.23</td>
            <td>944.42</td>
            <td>945.25</td>
            <td><b style="color: green">5.50</b></td>
            <td><b style="color: green">9.24</b></td>
            <td><b style="color: green">19.53</b></td>
            <td>421.72</td>
        </tr>
        <tr>
            <th>X86</th>
            <td>778.74</td>
            <td>966.39</td>
            <td>1470.50</td>
            <td>1659.51</td>
            <td><b style="color: green">6.41</b></td>
            <td><b style="color: green">11.90</b></td>
            <td><b style="color: green">31.33</b></td>
            <td>255.98</td>
        </tr>
        <tr>
            <th>x86 with Powertools</th>
            <td>855.45</td>
            <td>915.61</td>
            <td>1031.25</td>
            <td>1381.09</td>
            <td><b style="color: green">5.82</b></td>
            <td><b style="color: green">9.83</b></td>
            <td><b style="color: green">27.59</b></td>
            <td>748.08</td>
        </tr>
        <tr>
            <th>Container Image on X86</th>
            <td>980.98</td>
            <td>1256.94</td>
            <td>1532.01</td>
            <td>1755.68</td>
            <td><b style="color: green">5.82</b></td>
            <td><b style="color: green">9.84</b></td>
            <td><b style="color: green">24.42</b></td>
            <td>260.25</td>
        </tr>
        <tr>
            <th>ARM64 with top level statements</th>
            <td>916.53</td>
            <td>955.82</td>
            <td>985.90</td>
            <td>1021.40</td>
            <td><b style="color: green">5.73</b></td>
            <td><b style="color: green">9.38</b></td>
            <td><b style="color: green">20.65</b></td>
            <td>417.23</td>
        </tr>
        <tr>
            <th>Minimal API on x86</th>
            <td>1742.83</td>
            <td>1966.88</td>
            <td>2411.74</td>
            <td>2503.31</td>
            <td><b style="color: green">5.91</b></td>
            <td><b style="color: green">9.99</b></td>
            <td><b style="color: green">21.74</b></td>
            <td>108.6</td>
        </tr>        
        <tr>
            <th>Minimal API on ARM64</th>
            <td>2105.21</td>
            <td>2164.96</td>
            <td>2215.31</td>
            <td>2228.18</td>
            <td><b style="color: green">6.20</b></td>
            <td><b style="color: green">9.67</b></td>
            <td><b style="color: green">20.08</b></td>
            <td>528.13</td>
        </tr>
        <tr>
            <th>Minimal API with aws lambda web adapter on x86</th>
            <td>1013.88</td>
            <td>1102.67</td>
            <td>1330.62</td>
            <td>1392.85</td>
            <td><b style="color: green">6.20</b></td>
            <td><b style="color: green">10.31</b></td>
            <td><b style="color: green">21.74</b></td>
            <td>154.62</td>
        </tr>
        <tr>
            <th>Minimal API with aws lambda web adapter on ARM64</th>
            <td>1335.57</td>
            <td>1395.04</td>
            <td>1455.09</td>
            <td>1455.09</td>
            <td><b style="color: green">7.04</b></td>
            <td><b style="color: green">15.58</b></td>
            <td><b style="color: green">36.71</b></td>
            <td>111.28</td>
        </tr>
        <tr>
            <th>Native AOT on ARM64</th>
            <td>1277.19</td>
            <td>1326.64</td>
            <td>1358.84</td>
            <td>1367.49</td>
            <td><b style="color: green">6.10</b></td>
            <td><b style="color: green">9.37</b></td>
            <td><b style="color: green">17.97</b></td>
            <td>838.78</td>
        </tr>
        <tr>
            <th>Native AOT on X86</th>
            <td>466.81</td>
            <td>542.86</td>
            <td>700.45</td>
            <td>730.51</td>
            <td><b style="color: green">6.21</b></td>
            <td><b style="color: green">11.34</b></td>
            <td><b style="color: green">24.69</b></td>
            <td>371.16</td>
        </tr>
</table>

### .NET 8

The .NET 8 benchmarks include the number of cold and warm starts, alongside the performance numbers. Typically, the cold starts account for 1% or less of the total number of invocations.

<table class="table-bordered">
        <tr>
            <th colspan="1" style="horizontal-align : middle;text-align:center;"></th>
            <th colspan="5" style="horizontal-align : middle;text-align:center;">Cold Start (ms)</th>
            <th colspan="5" style="horizontal-align : middle;text-align:center;">Warm Start (ms)</th>           
        </tr>
        <tr>
            <th></th>
            <th scope="col">Invoke Count</th>
            <th scope="col">p50</th>
            <th scope="col">p90</th>
            <th scope="col">p99</th>
            <th scope="col">max</th>
            <th scope="col">Invoke Count</th>
            <th scope="col">p50</th>
            <th scope="col">p90</th>
            <th scope="col">p99</th>
            <th scope="col">max</th>
        </tr>
        <tr>
            <th>x86_64</th>
            <td>1490</td>
            <td>860</td>
            <td>962</td>
            <td>1403</td>
            <td>1676</td>
            <td>45,436</td>
            <td><b style="color: green">6.1</b></td>
            <td><b style="color: green">10.7</b></td>
            <td><b style="color: green">27.7</b></td>
            <td>63.4</td>
        </tr>
        <tr>
            <th>ARM64</th>
            <td>1699</td>
            <td>1063</td>
            <td>1112</td>
            <td>1155</td>
            <td>1209</td>
            <td>45,093</td>
            <td><b style="color: green">6.6</b></td>
            <td><b style="color: green">14.6</b></td>
            <td><b style="color: green">30.8</b></td>
            <td>75.9</td>
        </tr>
        <tr>
            <th>SnapStart x86_64</th>
            <td>432</td>
            <td>689</td>
            <td>850</td>
            <td>1081</td>
            <td>1437</td>
            <td>15,108</td>
            <td><b style="color: green">6.8</b></td>
            <td><b style="color: green">12.3</b></td>
            <td><b style="color: green">27.1</b></td>
            <td>94.9</td>
        </tr>
        <tr>
            <th>SnapStart ARM64</th>
            <td>433</td>
            <td>686</td>
            <td>862</td>
            <td>997</td>
            <td>1094</td>
            <td>15,078</td>
            <td><b style="color: green">7.3</b></td>
            <td><b style="color: green">14.4</b></td>
            <td><b style="color: green">30.8</b></td>
            <td>122.39</td>
        </tr>
        <tr>
            <th>x86_64 Native AOT</th>
            <td>758</td>
            <td>322</td>
            <td>344</td>
            <td>441</td>
            <td>665</td>
            <td>45,914</td>
            <td><b style="color: green">5.0</b></td>
            <td><b style="color: green">7.7</b></td>
            <td><b style="color: green">14.7</b></td>
            <td>77.0</td>
        </tr>
        <tr>
            <th>ARM64 Native AOT</th>
            <td>689</td>
            <td>334</td>
            <td>347</td>
            <td>372</td>
            <td>442</td>
            <td>646,081</td>
            <td><b style="color: green">5.3</b></td>
            <td><b style="color: green">7.9</b></td>
            <td><b style="color: green">13.4</b></td>
            <td>54.6</td>
        </tr>
        <tr>
            <th>ARM64 Native AOT Minimal API</th>
            <td>91</td>
            <td>498</td>
            <td>522</td>
            <td>895</td>
            <td>895</td>
            <td>156,359</td>
            <td><b style="color: green">5.6</b></td>
            <td><b style="color: green">8.8</b></td>
            <td><b style="color: green">16.1</b></td>
            <td>214.3</td>
        </tr>
</table>

**[Microsoft do not officially support all ASP.NET Core features for native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/), some features of ASP.NET may not be supported.*

Native AOT container samples use an Alpine base image. A cold start latency of ~1s was seen the first time an image was pushed and invoked. 

On future invokes, even after forcing new Lambda execution environments, cold start latency is as seen above. Potential reasons why covered in an [AWS blog post on optimizing Lambda functions packaged as containers.](https://aws.amazon.com/blogs/compute/optimizing-lambda-functions-packaged-as-container-images/)

## 👀 With other languages

You can find implementations of this project in other languages here:

* [☕ Java](https://github.com/aws-samples/serverless-java-frameworks-samples)
* [☕ Java (GraalVM)](https://github.com/aws-samples/serverless-graalvm-demo)
* [🦀 Rust](https://github.com/aws-samples/serverless-rust-demo)
* [🏗️ TypeScript](https://github.com/aws-samples/serverless-typescript-demo)
* [🐿️ Go](https://github.com/aws-samples/serverless-go-demo)
* [⭐ Groovy](https://github.com/aws-samples/serverless-groovy-demo)
* [🤖 Kotlin](https://github.com/aws-samples/serverless-kotlin-demo)

## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This library is licensed under the MIT-0 License. See the LICENSE file.
