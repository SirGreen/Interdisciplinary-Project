public class AdminNotifier : IObserver
{
    public void Notify(string message)
    {
        Console.WriteLine($"[Notification to Admin]: {message}");
    }
}
