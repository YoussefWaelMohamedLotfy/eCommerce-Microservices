namespace Shared.Utilites.EventBus.Events;

public abstract class IntegrationBaseEvent
{
    public Guid Id { get; private set; }
    public DateTimeOffset CreationDate { get; private set; }

    protected IntegrationBaseEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTimeOffset.UtcNow;
    }

    protected IntegrationBaseEvent(Guid id, DateTime createDate)
    {
        Id = id;
        CreationDate = createDate;
    }
}
