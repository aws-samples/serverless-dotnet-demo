using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

public record QueryResultWrapper
{
    public string? LoadTestType { get; set; }
    
    public QueryResult? ColdStart { get; set; }
    
    public QueryResult? WarmStart { get; set; }

    public string? ErrorCount { get; set; }
}

public record QueryResult
{
    public string? Count { get; set; }
    
    public string? P50 { get; set; }
    
    public string? P90 { get; set; }
    
    public string? P99 { get; set; }
    
    public string? Max { get; set; }
}

public class CloudWatchMetricResultWrapper
{
    public string? Label { get; set; }
    public List<CloudWatchMetricDatapoint>? Datapoints { get; set; }
}
public class CloudWatchMetricDatapoint
{
    public DateTime? Timestamp { get; set; }
    public double? Sum { get; set; }
    public string? Unit { get; set; }
}

    

public class LoadTestResults
{
    public List<List<LoadTestSubResult>>? results { get; set; }
}

public class LoadTestSubResult
{
    public string? field { get; set; }
    public string? value { get; set; }
}

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var folderToAnalize = "";
        var reportFileOutputPath = "";
        var snsTopicArn="";

        var reportResultList = new List<QueryResultWrapper>();
        
        DisplayStartMsg();
        
        if(args.Count()<1)
        {
            DisplayHelp();
            return -1;
        }
        folderToAnalize=args[0];

        if(args.Count()>=2)
        {
            snsTopicArn=args[1];
        }

         if(args.Count()>=3)
        {
            reportFileOutputPath=args[2];
        }
        else
        {
            reportFileOutputPath=$"./Report/{DateTime.UtcNow:yyyy-MM-dd-hh-mm}-loadtest-report.html";
        }
    
        try
        {
            System.Console.WriteLine($"STARTING using folder: {folderToAnalize}");
            System.Console.WriteLine($" - using folder: {folderToAnalize}");
            System.Console.WriteLine($" - destination path: {reportFileOutputPath}");
        
            if(!Directory.Exists(folderToAnalize))
            {
                System.Console.WriteLine($"ERROR: folder {folderToAnalize} does not exist");
                return -2;
            }
            foreach (var folderPath in Directory.EnumerateDirectories(folderToAnalize))
            {
                System.Console.WriteLine($"Processing folder: {folderPath}");
                var reportFolderPath = Path.Combine(folderPath, "Report");
                System.Console.WriteLine($" - Searching sub folder {reportFolderPath}");
                if (Directory.Exists(reportFolderPath))
                {
                    System.Console.WriteLine($" - Processing sub folder: {reportFolderPath}");
                    var reportFolderInfo = new DirectoryInfo(reportFolderPath);
                    foreach (var reportFileInfo in reportFolderInfo.GetFiles("load-test-report-*.json"))
                    {
                        QueryResultWrapper ltResult;
                        try
                        {
                            System.Console.WriteLine($" - Processing sub folder file: {reportFileInfo.Name}");
                            var ltName = reportFileInfo.Name.Replace("load-test-report-", "").Replace(reportFileInfo.Extension,"");
                            var title = $"{reportFolderInfo.Parent?.Name} - {ltName}";
                            System.Console.WriteLine($" - Report: {title}");
                            ltResult = await DeserializeLoadTestResultAsync(reportFileInfo.FullName, title);
                            ltResult.ErrorCount=await GetLoadtestErrorCountAsync(reportFileInfo.FullName.Replace("load-test-report-","load-test-errors-"));
                        }
                        catch(Exception ex)
                        {
                            ltResult=new QueryResultWrapper()
                            {
                                LoadTestType= $"error processing file {reportFileInfo.FullName}: {ex.Message}"
                            };
                            System.Console.WriteLine($"    !!!!  ERROR PROCESSING FILE: {reportFileInfo.FullName}: {ex.Message}  !!!!");
                            System.Console.WriteLine(ex.ToString());
                        }
                        reportResultList.Add(ltResult);
                    }
                }
            }
            System.Console.WriteLine("Generating HTML Report!");
            GenerateHtmlGlobalReport(reportResultList,reportFileOutputPath,snsTopicArn);
            System.Console.WriteLine("FINISHED!");
        }
        catch(Exception ex)
        {
            System.Console.WriteLine($"ERROR!:{ex.Message}");
            System.Console.WriteLine($"ERROR!:{ex.ToString()}");
        }
        return 0;
    }

    private static void DisplayStartMsg()
    {
        System.Console.WriteLine("LOAD-TEST CLI");
        System.Console.WriteLine("--------------------------------------");
    }

    private static void DisplayHelp()
    {
        System.Console.WriteLine("ERROR - Missing required parameters:");
        System.Console.WriteLine("- param1: mandatory folder to process");
        System.Console.WriteLine("- param2: optional sns arn");
        System.Console.WriteLine("- param3: optional output file path");
    }
    private static void GenerateHtmlGlobalReport(List<QueryResultWrapper> reportResultList,string reportFileOutputPath,string snsTopicArn)
    {
        /*
        we are producing an html similar to this:
        <table class="table-bordered">
            <tr>
                <th colspan="1" style="horizontal-align : middle;text-align:center;"></th>
                <th colspan="5" style="horizontal-align : middle;text-align:center;">Cold Start (ms)</th>
                <th colspan="5" style="horizontal-align : middle;text-align:center;">Warm Start (ms)</th>           
            </tr>
            <tr>
                <th></th>
                <th scope=""col"">Invoke Count</th>
                <th scope=""col"">p50</th>
                <th scope=""col"">p90</th>
                <th scope=""col"">p99</th>
                <th scope=""col"">max</th>
                <th scope=""col"">Invoke Count</th>
                <th scope=""col"">p50</th>
                <th scope=""col"">p90</th>
                <th scope=""col"">p99</th>
                <th scope=""col"">max</th>
            </tr>

            <tr>
                <th>Minimal API on ARM64</th>
                <td>242</td>
                <td>1972.79</td>
                <td>2049.16</td>
                <td>2107.32</td>
                <td>2124.55</td>
                <td>136,816</td>
                <td><b style="color: green">6.01</b></td>
                <td><b style="color: green">9.37</b></td>
                <td><b style="color: green">24.69</b></td>
                <td>331.6</td>
            </tr>

        </table>
        */
        var sb = new StringBuilder();

        sb.Append(@"
            <table class=""table-bordered"">
                <tr>
                    <th colspan=""1"" style=""horizontal-align : middle;text-align:center;""></th>
                    <th colspan=""5"" style=""horizontal-align : middle;text-align:center;"">Cold Start (ms)</th>
                    <th colspan=""5"" style=""horizontal-align : middle;text-align:center;"">Warm Start (ms)</th>           
                </tr>
                <tr>
                    <th></th>
                    <th scope=""col"">Invoke Count</th>
                    <th scope=""col"">p50</th>
                    <th scope=""col"">p90</th>
                    <th scope=""col"">p99</th>
                    <th scope=""col"">max</th>
                    <th scope=""col"">Invoke Count</th>
                    <th scope=""col"">p50</th>
                    <th scope=""col"">p90</th>
                    <th scope=""col"">p99</th>
                    <th scope=""col"">max</th>
                </tr>
        ");
        foreach (var ltResult in reportResultList)
        {
            sb.Append($@"
                    <tr>
                        <th>{ltResult.LoadTestType} errors:{ltResult.ErrorCount}</th>
                        <td>{FormatCount(ltResult.ColdStart?.Count??"-")}</td>
                        <td>{FormatMilli(ltResult.ColdStart?.P50??"-")}</td>
                        <td>{FormatMilli(ltResult.ColdStart?.P90??"-")}</td>
                        <td>{FormatMilli(ltResult.ColdStart?.P99??"-")}</td>
                        <td>{FormatMilli(ltResult.ColdStart?.Max??"-")}</td>
                        <td>{FormatCount(ltResult.WarmStart?.Count??"-")}</td>
                        <td><b style=""color: green"">{FormatMilli(ltResult.WarmStart?.P50??"-")}</b></td>
                        <td><b style=""color: green"">{FormatMilli(ltResult.WarmStart?.P90??"-")}</b></td>
                        <td><b style=""color: green"">{FormatMilli(ltResult.WarmStart?.P99??"-")}</b></td>
                        <td>{FormatMilli(ltResult.WarmStart?.Max??"-")}</td>
                    </tr>
            ");
        }
        sb.Append($@"
            </table>
        ");

        var outputDir=Path.GetDirectoryName(reportFileOutputPath);
        if(!String.IsNullOrEmpty(outputDir))
        {
            System.Console.WriteLine($"CREATING OUTPUT FOLDER:{outputDir}");
            Directory.CreateDirectory(outputDir);
        }
        File.WriteAllText(reportFileOutputPath, sb.ToString());

        if(!string.IsNullOrEmpty(snsTopicArn))
        {
            SendSNSMsg(snsTopicArn,"servereless dotnet demo - LOAD TEST FINAL REPORT",sb.ToString());
        }
        else
        {
            Console.WriteLine("SKIPPING SENSID SNS MSG");
        }
        System.Console.WriteLine($"OUTPUT REPORT CREATED: {reportFileOutputPath}");
    }

    private static async Task<string> GetLoadtestErrorCountAsync(string filePath)
    {
        int ret=-1;
        System.Console.WriteLine($" - PROCESSING ERROR ERROR COUNT FILE:{filePath}");
        try
        {
            if(!File.Exists(filePath))
                return "missing";
            
            var json = await File.ReadAllTextAsync(filePath);
            var tmp=JsonSerializer.Deserialize<CloudWatchMetricResultWrapper>(json);
            if(tmp!=null)
            {
                if(tmp.Datapoints!=null)
                {
                    ret=0;
                    foreach(var i in tmp.Datapoints)
                    {
                        ret+=(int)(i.Sum??0);
                    }
                }
            }
            else
                return "invalid";
        }
        catch(Exception ex)
        {
            System.Console.WriteLine($"FAILED TO PROCESS ERROR COUNT FILE!:{ex.Message}");
            System.Console.WriteLine($"{ex.ToString()}");
            return $"failed: {ex.Message}";
        }
        return ret.ToString();
    }

    private static async Task<QueryResultWrapper> DeserializeLoadTestResultAsync(string filePath,string title)
    {
        var json = await File.ReadAllTextAsync(filePath);
        
        var ret = new QueryResultWrapper
        {
            LoadTestType = title,
            WarmStart=null,
            ColdStart=null
        };

        var lt=JsonSerializer.Deserialize<LoadTestResults>(json);

        if(lt?.results==null)
            return ret;

        foreach (var ltItem in lt.results)
        {
            var isWarm = ltItem.Where(r => r.field == "coldstart" && r.value == "0").Count() > 0;
            System.Console.WriteLine($"    . isWarm: {isWarm}");

            var tmp = new QueryResult
            {
                Count = ltItem?.Where(r => r.field == "count").FirstOrDefault()?.value,
                P50 = ltItem?.Where(r => r.field == "p50").FirstOrDefault()?.value,
                P90 = ltItem?.Where(r => r.field == "p90").FirstOrDefault()?.value,
                P99 = ltItem?.Where(r => r.field == "p99").FirstOrDefault()?.value,
                Max = ltItem?.Where(r => r.field == "max").FirstOrDefault()?.value
            };

            if (isWarm)
                ret.WarmStart = tmp;
            else
                ret.ColdStart = tmp;
        }

        if(ret.WarmStart==null)
        {
            System.Console.WriteLine("    !!!! ATTENTION MISING WARM STATS !!!!");
        }

        if(ret.ColdStart==null)
        {
            System.Console.WriteLine("    !!!! ATTENTION MISING COLD STATS !!!!");
        }

        return ret;
    }

    private static string FormatMilli(string value)
    {
        Double valueNr=0;
        if(!Double.TryParse(value,out valueNr))
            return value;
        return ($"{valueNr:#,##0.00}");
    }

    private static string FormatCount(string value)
    {
        Double valueNr=0;
        if(!Double.TryParse(value,out valueNr))
            return value;
        return ($"{valueNr:#,##0}");
    }

    private static void SendSNSMsg(string snsTopicArn,string subject,string msg)
    {
        Console.WriteLine($"SENDING SNS MSG: {snsTopicArn}");
        try
        {
            using(var snsClient = new AmazonSimpleNotificationServiceClient())
            {
                var request = new PublishRequest
                {
                    TopicArn = snsTopicArn,
                    Subject=subject
                    Message = msg,
                };

                var t =  snsClient.PublishAsync(request);
                var result=t.Result;

                Console.WriteLine($"SNS Message Published ID:{result.MessageId}");

            }
        }
        catch(Exception ex)
        {
            System.Console.WriteLine($"FAILED TO SEND SNS MSG!:{ex.Message}");
            System.Console.WriteLine($"{ex.ToString()}");
        }
    }
}