namespace BaseReadModels;

public abstract class BaseReadModel
{
    public abstract long NumericalOrder { get; set; }
    public abstract string Id { get; set; }
    public abstract string? Code { get; set; }
    public abstract string CreatedUid { get; set; }
    public abstract DateTime CreatedDate { get; set; }
    public abstract DateTime CreatedDateUtc { get; set; }
    public abstract string UpdatedUid { get; set; }
    public abstract DateTime UpdatedDate { get; set; }
    public abstract DateTime UpdatedDateUtc { get; set; }
    public abstract int Version { get; set; }
    public abstract string LoginUid { get; set; }
    public abstract int TotalRow { get; set; }
    public abstract string ShardId { get; set; }
}