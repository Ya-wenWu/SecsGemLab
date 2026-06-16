namespace SecsGemLab.Web.Models;

internal enum MessageDirection
{
    Sent,
    Received
}

internal enum MessageStatus
{
    Success,
    Error,
    Pending
}

internal class MessageLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Stream { get; set; }
    public int Function { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public MessageDirection Direction { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.Success;
    public string? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int DeviceId { get; set; }
}
