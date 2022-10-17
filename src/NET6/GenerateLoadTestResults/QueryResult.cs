namespace GenerateLoadTestResults;

public record QueryResultWrapper
{
    public string LoadTestType { get; set; }
    
    public QueryResult ColdStart { get; set; }
    
    public QueryResult WarmStart { get; set; }
    
    public string AsMarkdownTableRow() => $"<table class=\"table-bordered\"><tr><th colspan=\"1\" style=\"horizontal-align : middle;text-align:center;\"></th><th colspan=\"4\" style=\"horizontal-align : middle;text-align:center;\">Cold Start (ms)</th><th colspan=\"4\" style=\"horizontal-align : middle;text-align:center;\">Warm Start (ms)</th></tr> <tr><th></th><th scope=\"col\">p50</th><th scope=\"col\">p90</th><th scope=\"col\">p99</th><th scope=\"col\">max</th><th scope=\"col\">p50</th><th scope=\"col\">p90</th><th scope=\"col\">p99</th><th scope=\"col\">max</th> </tr><tr><th>{LoadTestType}</th><td>{ColdStart.P50}</td><td>{ColdStart.P90}</td><td>{ColdStart.P99}</td><td>{ColdStart.Max}</td><td><b style=\"color: green\">{WarmStart.P50}</b></td><td><b style=\"color: green\">{WarmStart.P90}</b></td><td><b style=\"color: green\">{WarmStart.P99}</b></td><td>{WarmStart.Max}</td></tr></table>";
}

public record QueryResult
{
    public string Count { get; set; }
    
    public string P50 { get; set; }
    
    public string P90 { get; set; }
    
    public string P99 { get; set; }
    
    public string Max { get; set; }
}