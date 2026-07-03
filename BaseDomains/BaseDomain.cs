using BaseCommands;
using BaseEvents;
using BaseReadModels;
using Common;

namespace BaseDomains;

public abstract class BaseDomain
{
    public BaseDomain()
    {
        Code = CommonUtility.GenerateGuid();
        CreatedUid = string.Empty;
        CreatedDate = Extension.GetCurrentDate();
        CreatedDateUtc = Extension.GetCurrentDateUtc();
        UpdatedUid = CreatedUid;
        UpdatedDate = CreatedDate;
        UpdatedDateUtc = CreatedDateUtc;
        Version = 0;
        LoginUid = CreatedUid;
    }

    public BaseDomain(BaseReadModel model)
    {
        NumericalOrder = model.NumericalOrder;
        Code = model.Code;
        CreatedDate = model.CreatedDate;
        CreatedDateUtc = model.CreatedDateUtc;
        CreatedUid = model.CreatedUid;
        UpdatedDate = model.UpdatedDate;
        UpdatedDateUtc = model.UpdatedDateUtc;
        UpdatedUid = model.UpdatedUid;
        Version = model.Version;
        LoginUid = model.LoginUid;
    }

    public BaseDomain(BaseCommand message)
    {
        CreatedDate = message.ProcessDate;
        CreatedDateUtc = message.ProcessDateUtc;
        CreatedUid = message.ProcessUid.AsEmptyString();
        UpdatedDate = message.ProcessDate;
        UpdatedDateUtc = message.ProcessDateUtc;
        UpdatedUid = message.ProcessUid.AsEmptyString();
        LoginUid = message.LoginUid.AsEmptyString();
        Version = 0;
    }

    public BaseDomain(BaseDomain message)
    {
        CreatedDate = message.CreatedDate;
        CreatedDateUtc = message.CreatedDateUtc;
        CreatedUid = message.CreatedUid.AsEmptyString();
        UpdatedDate = message.UpdatedDate;
        UpdatedDateUtc = message.UpdatedDateUtc;
        UpdatedUid = message.UpdatedUid.AsEmptyString();
        LoginUid = message.LoginUid.AsEmptyString();
        Version = 0;
    }

    public BaseDomain(Event @event)
    {
        CreatedDate = @event.ProcessDate;
        CreatedDateUtc = @event.ProcessDateUtc;
        CreatedUid = @event.ProcessUid.AsEmptyString();
        UpdatedDate = @event.ProcessDate;
        UpdatedDateUtc = @event.ProcessDateUtc;
        UpdatedUid = @event.ProcessUid.AsEmptyString();
        LoginUid = @event.LoginUid.AsEmptyString();
        Version = 0;
    }

    public void Changed(BaseCommand message)
    {
        UpdatedDate = message.ProcessDate;
        UpdatedDateUtc = message.ProcessDateUtc;
        UpdatedUid = message.ProcessUid.AsEmptyString();
        LoginUid = message.LoginUid.AsEmptyString();
        Version++;
    }

    public void Changed(BaseDomain message)
    {
        UpdatedDate = message.UpdatedDate;
        UpdatedDateUtc = message.UpdatedDateUtc;
        UpdatedUid = message.UpdatedUid.AsEmptyString();
        LoginUid = message.LoginUid.AsEmptyString();
        Version++;
    }

    public void Changed(Event @event)
    {
        UpdatedDate = @event.ProcessDate;
        UpdatedDateUtc = @event.ProcessDateUtc;
        UpdatedUid = @event.ProcessUid.AsEmptyString();
        LoginUid = @event.LoginUid.AsEmptyString();
        Version++;
    }

    public void Changed()
    {
        UpdatedDate = Extension.GetCurrentDate();
        UpdatedDateUtc = Extension.GetCurrentDateUtc();
        Version++;
    }

    public long NumericalOrder { get; protected set; }

    public virtual string Id
    {
        get => Code.AsEmptyString();
        set => Code = value;
    }

    #region Properties

    public string? Code { get; protected set; }
    public DateTime CreatedDate { get; protected set; }
    public DateTime CreatedDateUtc { get; protected set; }
    public string? CreatedUid { get; protected set; }
    public DateTime UpdatedDate { get; protected set; }
    public DateTime UpdatedDateUtc { get; protected set; }
    public string? UpdatedUid { get; protected set; }
    public string? LoginUid { get; protected set; }
    public int Version { get; protected set; }

    #endregion
}