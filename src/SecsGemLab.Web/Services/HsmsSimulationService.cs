using System.Collections.ObjectModel;
using SecsGemLab.Web.Models;

namespace SecsGemLab.Web.Services;

internal class HsmsSimulationService
{
    public EquipmentStatus Status { get; } = new();
    public ObservableCollection<MessageLog> Messages { get; } = new();
    public event EventHandler? OnStateChanged;

    public async Task ConnectAsync()
    {
        Status.Connected = true;
        Status.Mode = "Online";
        Status.ControlState = "EQUIPMENT_ONLINE";
        Status.LastActivity = DateTime.UtcNow;
        NotifyStateChanged();

        await Task.Delay(300).ConfigureAwait(false);
        AddMessage(1, 13, "Establish Communications Request (S1F13)", MessageDirection.Sent, "<S1F13>");

        await Task.Delay(500).ConfigureAwait(false);
        AddMessage(1, 14, "Establish Communications Confirm (S1F14)", MessageDirection.Received, """
<S1F14>
  <L>
    <A>"SECS-GEM-200"</A>
    <A>"1.0.0"</A>
  </L>
</S1F14>
""");

        await Task.Delay(200).ConfigureAwait(false);
        AddMessage(1, 1, "Are You There Request (S1F1)", MessageDirection.Sent, "<S1F1>");

        await Task.Delay(400).ConfigureAwait(false);
        AddMessage(1, 2, "Are You There Response (S1F2)", MessageDirection.Received, """
<S1F2>
  <L>
    <A>"SECS-GEM-200"</A>
  </L>
</S1F2>
""");
        Status.ModelName = "SECS-GEM-200";
        NotifyStateChanged();
    }

    public async Task DisconnectAsync()
    {
        Status.Connected = false;
        Status.Mode = "Offline";
        Status.ControlState = "EQUIPMENT_OFFLINE";
        Status.ProcessingState = "IDLE";
        Status.LastActivity = DateTime.UtcNow;
        NotifyStateChanged();

        AddMessage(9, 1, "Disconnect Notification", MessageDirection.Sent, "<S9F1> Disconnected");

        await Task.Delay(100).ConfigureAwait(false);
        NotifyStateChanged();
    }

    public async Task SendCustomMessageAsync(int stream, int func, string data)
    {
        AddMessage(stream, func, $"Custom S{stream}F{func}", MessageDirection.Sent, data);

        await Task.Delay(400).ConfigureAwait(false);
        AddMessage(stream, func, $"S{stream}F{func} Acknowledge", MessageDirection.Received, $"<S{stream}F{func + 1}> OK");

        Status.MessageCount = Messages.Count;
        Status.LastActivity = DateTime.UtcNow;
        NotifyStateChanged();
    }

    public async Task SimulateErrorAsync()
    {
        AddMessage(9, 1, "Communication Error (S9F1)", MessageDirection.Received, "<S9F1> Invalid Message", MessageStatus.Error);
        Status.ErrorCount++;
        Status.LastActivity = DateTime.UtcNow;
        NotifyStateChanged();
        await Task.CompletedTask.ConfigureAwait(false);
    }

    private void AddMessage(int stream, int function, string name, MessageDirection dir, string data, MessageStatus status = MessageStatus.Success)
    {
        Messages.Insert(0, new MessageLog
        {
            Stream = stream,
            Function = function,
            Name = name,
            Direction = dir,
            Status = status,
            Data = data,
            Timestamp = DateTime.UtcNow,
            DeviceId = 1
        });
        Status.MessageCount = Messages.Count;
    }

    public void ClearMessages()
    {
        Messages.Clear();
        Status.MessageCount = 0;
        Status.ErrorCount = 0;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke(this, EventArgs.Empty);
}
