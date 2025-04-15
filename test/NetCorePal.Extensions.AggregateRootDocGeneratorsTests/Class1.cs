using NetCorePal.Extensions.Domain;

namespace NetCorePal.Extensions.AggregateRootDocGeneratorsTests;

public partial record TestEntityId(Int64 Id) : IStronglyTypedId<Int64>;

public class TestEntity: Entity<TestEntityId> ,IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
}