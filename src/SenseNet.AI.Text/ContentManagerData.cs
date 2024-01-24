namespace SenseNet.AI.Text;

public struct ContentManagerData
{
    public static ContentManagerData Empty => new() { Text = string.Empty, ThreadId = string.Empty };

    public string Text { get; set; }
    public string ThreadId { get; set; }
}