using System;

namespace SwiftJot.Models;

public class Note
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "New Jot";
    public string Content { get; set; } = string.Empty;
}
