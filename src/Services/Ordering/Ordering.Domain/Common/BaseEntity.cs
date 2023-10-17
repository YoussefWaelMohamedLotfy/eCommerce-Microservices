namespace Ordering.Domain.Common;

public abstract class BaseEntity<T>
{
    public T ID { get; protected set; } = default!;
}
