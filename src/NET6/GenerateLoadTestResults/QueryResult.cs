namespace GenerateLoadTestResults;

public record QueryResultWrapper
{
    public string LoadTestType { get; set; }
    
    public QueryResult ColdStart { get; set; }
    
    public QueryResult WarmStart { get; set; }
    
    public string AsMarkdownTableRow() => $"<tr><th>{LoadTestType}</th><td>{ColdStart.P50}</td><td>{ColdStart.P90}</td><td>{ColdStart.P99}</td><td>{ColdStart.Max}</td><td><b style=\"color: green\">{WarmStart.P50}</b></td><td><b style=\"color: green\">{WarmStart.P90}</b></td><td><b style=\"color: green\">{WarmStart.P99}</b></td><td>{WarmStart.Max}</td></tr>";
}

public record QueryResult
{
    public string Count { get; set; }
    
    public string P50 { get; set; }
    
    public string P90 { get; set; }
    
    public string P99 { get; set; }
    
    public string Max { get; set; }
}