namespace logsToMetrics;

public record QueryResultWrapper
{
    public string LoadTestType { get; set; }
    
    public QueryResult ColdStart { get; set; }
    
    public QueryResult WarmStart { get; set; }
}

public record QueryResult
{
    public string Count { get; set; }
    
    public string P50 { get; set; }
    
    public string P90 { get; set; }
    
    public string P99 { get; set; }
    
    public string Max { get; set; }
}