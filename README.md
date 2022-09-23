## Lambda Demo with .NET

With the release of .NET 6 [AWS Lambda](https://aws.amazon.com/lambda/) now supports .NET Core 3.1 and .NET 6 as managed runtimes. With the availability of ARM64 using Graviton2 there have been vast improvements to using .NET with Lambda.

But how does that translate to actual application performance? And how does .NET compare to other available runtimes. This repository contains a simple serverless application across a range of .NET implementations and the corresponding benchmarking results.

## Application

![](./imgs/diagram.jpg)

The application consists of an [Amazon API Gateway](https://aws.amazon.com/api-gateway/) backed by four Lambda functions and an [Amazon DynamoDB](https://aws.amazon.com/dynamodb/) table for storage.

It includes the below implementations as well as benchmarking results for both x86 and ARM64:

- .NET Core 3.1
- .NET Core 3.1 with Open Telemetry tracing
- .NET 6 Lambda
- .NET 6 Top Level statements
- .NET 6 Minimal API
- .NET 6 NativeAOT compilation
- .NET 7 NativeAOT compilation

## Requirements

- [AWS CLI](https://aws.amazon.com/cli/)
- [AWS SAM](https://aws.amazon.com/serverless/sam/)
- .NET 6 OR .NET Core 3.1
- [Artillery](https://www.artillery.io/) for load-testing the application
- Docker

## Software

There are four implementations included in the repository, covering a variety of Lambda runtimes and features. All the implementations use 1024MB of memory with Graviton2 (ARM64) as default. Tests are also executed against x86_64 architectures for comparison.

There is a separate project for each of the four Lambda functions, as well as a shared library that contains the data access implementations. It uses the hexagonal architecture pattern to decouple the entry points, from the main domain and storage logic.

### .NET 6

This implementation is the simplest route to upgrade a .NET Core 3.1 function to use .NET 6 as it only requires upgrading the function runtime, project target framework and any dependencies as per the final section of [this link](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/).

### .NET 6 Top Level Statements

This implementation uses the new features detailed in [this link](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/) including
- Top Level Statements
- Source generation
- Executable assemblies

### .NET 6 NativeAOT

The code is compiled natively for either Linux-x86_64 or Linux-ARM64 and then deployed manually to Lambda as a zip file. The SAM deploy can still be used to stand up the API Gateway endpoints and DynamoDb table, but won't be able to deploy NativeAOT .NET Lambda functions yet. Packages need to be published from Linux, since cross-OS native compilation is not supported yet. 

Details for compiling .NET 6 NativeAOT can be found [here](https://github.com/dotnet/runtimelab/blob/feature/NativeAOT/docs/using-nativeaot/compiling.md)

### .NET 7 NativeAOT

The code is compiled natively for Linux-x86_64 then deployed manually to Lambda as a zip file. The SAM deploy can still be used to stand up the API Gateway endpoints and DynamoDb table, but won't be able to deploy NativeAOT .NET Lambda functions yet. Packages need to be published from Linux, since cross-OS native compilation is not supported yet. 

Details for compiling .NET 7 NativeAOT can be found [here](https://github.com/dotnet/runtimelab/blob/feature/NativeAOT/docs/using-nativeaot/compiling.md)

### Minimal API
There is a single project named ApiBootstrap that contains all the start-up code and API endpoint mapping. The SAM template still deploys a separate function per API endpoint to negate concurrency issues.

It uses the new minimal API hosting model as detailed [here](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/). 

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

![](./imgs/power-tuning.PNG)

### Results

The below CloudWatch Log Insights query was used to generate the results:

```
filter @type="REPORT"
| fields greatest(@initDuration, 0) + @duration as duration, ispresent(@initDuration) as coldstart
| stats count(*) as count, pct(duration, 50) as p50, pct(duration, 90) as p90, pct(duration, 99) as p99, max(duration) as max by coldstart
```

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
            <th>.NET Core 3.1 (arm64)</th>
            <td>1122.70</td>
            <td>1170.83</td>
            <td>1225.92</td>
            <td>1326.32</td>
            <td><b style="color: green">5.55</b></td>
            <td><b style="color: green">8.74</b></td>
            <td><b style="color: green">19.85</b></td>
            <td>256.55</td>
        </tr>
        <tr>
            <th>.NET Core 3.1 (x86_64)</th>
            <td>1004.80</td>
            <td>1135.81</td>
            <td>1422.78</td>
            <td>1786.78</td>
            <td><b style="color: green">6.11</b></td>
            <td><b style="color: green">10.82</b></td>
            <td><b style="color: green">29.40</b></td>
            <td>247.32</td>
        </tr>
        <tr>
            <th>.NET Core 3.1 with Open Telemetry (x86_64)</th>
            <td>1615.31</td>
            <td>1704.93</td>
            <td>1931.82</td>
            <td>2067.97</td>
            <td><b style="color: green">7.04</b></td>
            <td><b style="color: green">12.08</b></td>
            <td><b style="color: green">35.57</b></td>
            <td>1059.78</td>
        </tr>
        <tr>
            <th>NET 6 (arm64)</th>
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
            <th>NET 6 with Lambda Powertools v0.0.1 (arm64)</th>
            <td>981.97</td>
            <td>1031.25</td>
            <td>1062.65</td>
            <td>1091.56</td>
            <td><b style="color: green">6.72</b></td>
            <td><b style="color: green">13.76</b></td>
            <td><b style="color: green">35.79</b></td>
            <td>70.58</td>
        </tr>
        <tr>
            <th>NET 6 (x86_64)</th>
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
            <th>NET 6 Top Level Statements (arm64)</th>
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
            <th>Minimal API (arm64)</th>
            <td>1149.95</td>
            <td>1194.47</td>
            <td>1239.47</td>
            <td>1315.07</td>
            <td><b style="color: green">6.10</b></td>
            <td><b style="color: green">10.00</b></td>
            <td><b style="color: green">22.91</b></td>
            <td>1315.07</td>
        </tr>
        <tr>
            <th>NET 6 NativeAOT (arm64)</th>
            <td>448.97</td>
            <td>467.75</td>
            <td>493.20</td>
            <td>516.6</td>
            <td><b style="color: green">6.30</b></td>
            <td><b style="color: green">10.49</b></td>
            <td><b style="color: green">21.50</b></td>
            <td>461.35</td>
        </tr>
        <tr>
            <th>NET 6 NativeAOT (x86_64)</th>
            <td>466.81</td>
            <td>542.86</td>
            <td>700.45</td>
            <td>730.51</td>
            <td><b style="color: green">6.21</b></td>
            <td><b style="color: green">11.34</b></td>
            <td><b style="color: green">24.69</b></td>
            <td>371.16</td>
        </tr>
        <tr>
            <th>NET 7 NativeAOT (x86_64)</th>
            <td>381.64</td>
            <td>411.94</td>
            <td>411.94</td>
            <td>411.94</td>
            <td><b style="color: green">5.64</b></td>
            <td><b style="color: green">9.99</b></td>
            <td><b style="color: green">22.81</b></td>
            <td>245.61</td>
        </tr>
</table>

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
