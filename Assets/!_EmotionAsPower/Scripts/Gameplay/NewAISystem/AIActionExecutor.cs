public abstract class AIActionExecutor
{
    protected AIActionData actionData;

    public AIActionExecutor(AIActionData data)
    {
        actionData = data;
    }

    public abstract void StartAction();
    public abstract ActionState UpdateAction();
    public abstract void StopAction();
    public abstract void OnActionComplete();
    public abstract void OnActionInterrupted();
    public abstract void Perform();
}