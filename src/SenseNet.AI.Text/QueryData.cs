namespace SenseNet.AI.Text;

public struct QueryData
{
    public static QueryData Empty => new() { Query = string.Empty };
    
    public string Query { get; set; }
    public string? ThreadId { get; set; }
}
