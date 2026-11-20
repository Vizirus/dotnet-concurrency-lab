namespace Tpl;

public static class TaskStatusHandler
{
    public static Task CreateTaskWithCreatedStatus()
    {
        Task task = new Task(() => { Console.WriteLine("Initialized"); });
        if (task.Status == TaskStatus.Created)
        {
            return task;
        }

        throw new TaskSchedulerException("The task should have been created only!");
    }

    public static Task CreateTaskWithWaitingForActivationStatus()
    {
        TaskCompletionSource taskCompletionSource = new TaskCompletionSource();
        Task task = taskCompletionSource.Task;
        if (task.Status == TaskStatus.WaitingForActivation)
        {
            return taskCompletionSource.Task;
        }

        throw new TaskSchedulerException("The task should have been waiting for activation only!");
    }

    public static Task CreateTaskWithWaitingToRunStatus()
    {
        Task task = Task.Factory.StartNew(() => { Console.WriteLine("CreateTaskWithWaitingToRunStatus"); });
        if (task.Status == TaskStatus.WaitingToRun)
        {
            return task;
        }

        throw new TaskSchedulerException("The task should have been 'Waiting to run' only!");
    }

    public static Task CreateTaskWithRunningStatus()
    {
        Task task = new Task(() =>
        {
            for (int i = 0; i < 1000000; i++)
            {
                Console.WriteLine(Math.Sqrt(i));
            }
        });
        task.Start();

        _ = SpinWait.SpinUntil(() => task.Status == TaskStatus.Running, 1000);
        if (task.Status == TaskStatus.Running)
        {
            return task;
        }

        throw new TaskSchedulerException("The task should have been 'running' only!");
    }

    public static Task CreateTaskWithRanToCompletionStatus()
    {
        Task task = new Task(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(Math.Sqrt(i));
            }
        });
        task.Start();
        task.Wait();
        if (task.Status == TaskStatus.RanToCompletion)
        {
            return task;
        }

        throw new TaskSchedulerException("The task should have been 'ran to complete' only!");
    }

    public static Task CreateTaskWithWaitingForChildrenToCompleteStatus()
    {
        Task taskParent = new Task(() =>
        {
            Task child = new Task(() => Thread.Sleep(2000), TaskCreationOptions.AttachedToParent);
            child.Start();
        });

        taskParent.Start();

        _ = SpinWait.SpinUntil(() => taskParent.Status == TaskStatus.WaitingForChildrenToComplete, 1000);

        if (taskParent.Status == TaskStatus.WaitingForChildrenToComplete)
        {
            return taskParent;
        }

        throw new TaskSchedulerException("The task should have been 'waiting for child' only!");
    }

    public static Task CreateTaskWithIsCompletedStatus()
    {
        Task task = new Task(() => Console.WriteLine("Task ended"));
        task.Start();
        task.Wait();
        if (task.IsCompleted)
        {
            return task;
        }

        throw new TaskSchedulerException("Task havent been completed!");
    }

    public static Task CreateTaskWithIsCancelledStatus()
    {
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        var token = cancellationToken.Token;
        Task task = new Task(
            () =>
            {
                for (int i = 0; i < 1000000; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Console.WriteLine(Math.Sqrt(i));
                }
            }, token);
        task.Start();
        cancellationToken.Cancel();
        _ = SpinWait.SpinUntil(() => task.IsFaulted, 1000);
        if (task.IsCanceled)
        {
            cancellationToken.Dispose();
            return task;
        }

        throw new TaskSchedulerException("Haven`t een canceled!");
    }

    public static Task CreateTaskWithIsFaultedStatus()
    {
        Task task = new Task(() => throw new NotImplementedException());
        task.Start();
        try
        {
            task.Wait();
        }
        catch (AggregateException)
        {
            if (task.IsFaulted)
            {
                return task;
            }
        }

        throw new TaskSchedulerException("Is not faulted!");
    }
}
