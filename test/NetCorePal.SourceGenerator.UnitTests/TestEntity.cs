using NetCorePal.Extensions.Domain;

namespace NetCorePal.SourceGenerator.UnitTests;

public partial record TestEntityId : IInt64StronglyTypedId;

public class TestEntity : Entity<TestEntityId>, IAggregateRoot
{
    protected TestEntity()
    {
    }

    public TestEntity(string name, int count)
    {
        this.Name = name;
        this.Count = count;
    }

    public string Name { get; private set; } = default!;
    public int Count { get; private set; }
    
    public string TestString { get; private set; } = default!;
    public int TestInt { get; private set; }
    public DateTime CreatedTime { get; private set; }
    public DateTime UpdatedTime { get; private set; }
    public TestEntityId TestEntityId { get; private set; } = default!;

    public void ChangeName(string name)
    {
        this.Name = name;
        AddDomainEvent(new TestEntityNameChangedDomainEvent(this));
    }
}

public record TestEntityNameChangedDomainEvent(TestEntity TestEntity) : IDomainEvent;


public class TestEntityNameChangeHandler : IDomainEventHandler<TestEntityNameChangedDomainEvent>
{
    public Task Handle(TestEntityNameChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 处理事件的逻辑
        return Task.CompletedTask;
    }
}