namespace BaseDock.Domain.Enums;

public enum DeploymentStatus
{
    NotDeployed = 0,
    Deploying = 1,
    Running = 2,
    Stopped = 3,
    Error = 4,
    PartiallyRunning = 5
}
