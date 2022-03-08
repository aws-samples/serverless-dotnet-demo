## Lambda demo with .NET

![](./imgs/diagram.jpg)

This is a simple serverless application built in using .NET 6. It is a .NET implementation of the [Java Serverless Samples](https://github.com/aws-samples/serverless-java-frameworks-samples) that can be used for benchmarking.

It consists of an [Amazon API Gateway](https://aws.amazon.com/api-gateway/) backed by four [AWS Lambda](https://aws.amazon.com/lambda/)
functions and an [Amazon DynamoDB](https://aws.amazon.com/dynamodb/) table for storage.

It includes a .NET Core 3.1 and .NET 6 Lambda implementation using a project per function as well as using the new .NET 6 minimal API hosting model.

## Requirements

- [AWS CLI](https://aws.amazon.com/cli/)
- [AWS SAM](https://aws.amazon.com/serverless/sam/)
- .NET 6 OR .NET Core 3.1
- [Artillery](https://www.artillery.io/) for load-testing the application
- Docker

## Software

There are four implementations included in the repository, covering a variety of Lambda runtimes and features. 

### .NET Core 3.1
There is a separate project for each of the four Lambda functions, as well as a shared library that contains the data access implementations. It uses the hexagonal architecture pattern to decouple the entry points, from the main domain logic
and the storage logic. It uses .NET Core 3.1 to allow a direct comparison to .NET 6 benchmarks.

### .NET 6
There is a separate project for each of the four Lambda functions, as well as a shared library that contains the data access implementations. It uses the hexagonal architecture pattern to decouple the entry points, from the main domain logic
and the storage logic. The functions are build using the standard pattern of having a class and a Handler method. This is the same as required when using .NET Core 3.1.

This is the simples route to upgrade a .NET Core 3.1 function to use .NET 6 as it only requires upgrading the function runtime, project target framework and any dependencies as per the final section of [this link](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/).

### .NET 6 Top Level Statements
There is a separate project for each of the four Lambda functions, as well as a shared library that contains the data access implementations. It uses the hexagonal architecture pattern to decouple the entry points, from the main domain logic
and the storage logic.

The Lambda implementation uses the new features detailed in [this link](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/) including
- Top Level Statements
- Source generation
- Executable assemblies

### Minimal API
There is a single project named ApiBootstrap that contains all of the startup code and API endpoint mapping. The SAM template still deploys a separate function per API endpoint to negate concurrency issues.

It uses the new minimal API hosting model as detailed [here](https://aws.amazon.com/blogs/compute/introducing-the-net-6-runtime-for-aws-lambda/). 

## Deployment and Testing

To deploy the architecture into your AWS account navigate into the respective folder under the src folder and run 'sam deploy'. For example for .NET 6:

``` bash
cd src/.NET6
sam build
sam deploy
```

## Summary
Below is the cold start and warm start latencies observed. Please refer to the load test folder to see the specifics of the test that were executed.

All latencies listed below are in milliseconds.

[Artillery](https://www.artillery.io/) is used to make **100 requests / second for 10 minutes to our API endpoints**.

[AWS Lambda Power Tuning](https://github.com/alexcasalboni/aws-lambda-power-tuning) is used to optimize the cost/performance. 1024mb of function memory provided the optimal balance between cost and performance.

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
            <th>.NET Core 3.1</th>
            <td>1122.70</td>
            <td>1170.83</td>
            <td>1225.92</td>
            <td>1326.32</td>
            <td><b style="color: green">5.55</b></td>
            <td><b style="color: green">8.74</b></td>
            <td><b style="color: green">19.85</b></td>
            <td>1326.32</td>
        </tr>        
        <tr>
            <th>.NET 6</th>
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
            <th>.NET 6 Top Level Statements</th>
            <td>2654.62</td>
            <td>2801.84</td>
            <td>2901.59</td>
            <td>2974.93</td>
            <td><b style="color: green">6.21</b></td>
            <td><b style="color: green">19.93</b></td>
            <td><b style="color: green">79.70</b></td>
            <td>728.16</td>
        </tr>        
        <tr>
            <th>Minimal API</th>
            <td>1149.95</td>
            <td>1194.47</td>
            <td>1239.47</td>
            <td>1315.07</td>
            <td><b style="color: green">6.10</b></td>
            <td><b style="color: green">10.00</b></td>
            <td><b style="color: green">22.91</b></td>
            <td>1315.07</td>
        </tr>
</table>

## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This library is licensed under the MIT-0 License. See the LICENSE file.
