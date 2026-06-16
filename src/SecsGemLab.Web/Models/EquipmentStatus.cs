namespace SecsGemLab.Web.Models;

internal class EquipmentStatus
{
    public bool Connected { get; set; }
    public string Mode { get; set; } = "Offline";
    public string ModelName { get; set; } = "SECS-GEM-200";
    public string SoftwareRev { get; set; } = "1.0.0";
    public int MessageCount { get; set; }
    public int ErrorCount { get; set; }
    public DateTime LastActivity { get; set; }
    public string ControlState { get; set; } = "EQUIPMENT_OFFLINE";
    public string ProcessingState { get; set; } = "IDLE";
}
