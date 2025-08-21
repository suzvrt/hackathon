namespace hackathon.Infrastructure.Threading;

public static class TaskExtensions
{
    // Observa exceções de Tasks "fire-and-forget" e só loga.
    public static void SafeFireAndForget(this Task task, ILogger logger, string operation)
    {
        _ = task.ContinueWith(
            t => logger.LogError(t.Exception, "Falha no fire-and-forget: {Operation}", operation),
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
    }
}
