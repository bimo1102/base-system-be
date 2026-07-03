using Common;
using Enums;
using ProtoBuf;

namespace BaseCommands;

public abstract record BaseCommand
{
    public BaseCommand()
    {
        ObjectId = string.Empty;
        ProcessUid = string.Empty;
        ProcessDate = DateTime.Now;
    }

    public BaseCommand(string processUid) : this()
    {
        ProcessUid = processUid;
    }

    public BaseCommand(string objectId, string processUid) : this(objectId, processUid, DateTime.Now)
    {
    }

    public BaseCommand(string objectId, string processUid, DateTime processDate)
    {
        ObjectId = objectId;
        ProcessUid = processUid;
        ProcessDate = processDate;
    }

    public abstract string ObjectId { get; set; }
    public abstract string ProcessUid { get; set; }
    public abstract DateTime ProcessDate { get; set; }
    public DateTime ProcessDateUtc => ProcessDate.ToUniversalTime();
    public abstract string LoginUid { get; set; }
}


[ProtoContract]
public record BaseCommandResponse
{
    [ProtoMember(1)] public bool Status { get; set; }
    [ProtoMember(2)] public ErrorCodeEnum ErrorCode { get; set; }
    [ProtoMember(3)] public List<string> Messages { get; set; } = [];
    [ProtoMember(4)] public int Version { get; set; }
    [ProtoMember(5)] public string ServerTime { get; set; } = DateTime.Now.AsUnixTimeStamp().ToString();
    [ProtoMember(6)] public int? TotalRow { get; set; }

    public void SetSuccess()
    {
        Status = true;
        ErrorCode = ErrorCodeEnum.NoErrorCode;
    }

    public void SetSuccess(string message)
    {
        Status = true;
        Messages.Add(message);
        ErrorCode = ErrorCodeEnum.NoErrorCode;
    }

    public void SetFail(ErrorCodeEnum code)
    {
        Status = false;
        ErrorCode = code;
        var message = code.GetDisplayName();
        Messages.Add(message);
    }

    public void SetFail(string? message, ErrorCodeEnum code = ErrorCodeEnum.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        Messages.Add(message.AsEmptyString());
    }

    public void SetFail(Exception ex, ErrorCodeEnum code = ErrorCodeEnum.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        var message = $"Message: {ex.Message}";
        Messages.Add(message);
    }

    public void SetFail(IEnumerable<string>? messages, ErrorCodeEnum code = ErrorCodeEnum.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        if (messages == null)
        {
            return;
        }

        foreach (var message in messages)
        {
            Messages.Add(message);
        }
    }

    public string? Message => Messages.ToArray().AsArrayJoin();
}

[ProtoContract]
public record BaseCommandResponse<T>
{
    [ProtoMember(1)] public bool Status { get; set; }
    [ProtoMember(2)] public ErrorCodeEnum ErrorCode { get; set; }
    [ProtoMember(3)] public List<string>? Messages { get; set; } = [];
    [ProtoMember(4)] public int Version { get; set; }
    [ProtoMember(5)] public string ServerTime { get; set; } = DateTime.Now.AsUnixTimeStamp().ToString();
    [ProtoMember(6)] public T? Data { get; set; }
    [ProtoMember(7)] public int TotalRow { get; set; }
    public string? Message => Messages?.ToArray().AsArrayJoin();

    public void SetSuccess()
    {
        Status = true;
        ErrorCode = ErrorCodeEnum.NoErrorCode;
    }

    public void SetSuccess(string message)
    {
        Status = true;
        Messages?.Add(message);
        ErrorCode = ErrorCodeEnum.NoErrorCode;
    }

    public void SetFail(ErrorCodeEnum code)
    {
        Status = false;
        ErrorCode = code;
        var message = code.GetDisplayName();
        Messages?.Add(message);
    }

    public void SetFail(string? message, ErrorCodeEnum code = ErrorCodeEnum.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        if (message?.Length > 0)
        {
            Messages?.Add(message);
        }
    }

    public void SetFail(Exception ex, ErrorCodeEnum code = ErrorCodeEnum.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        var message = $"Message: {ex.Message}";
        Messages?.Add(message);
    }

    public void SetFail(IEnumerable<string>? messages, ErrorCodeEnum code = ErrorCodeEnum.NoErrorCode)
    {
        Status = false;
        ErrorCode = code;
        if (messages == null) return;
        foreach (var message in messages)
        {
            Messages?.Add(message);
        }
    }
}

[ProtoContract]
public class RefInt
{
    [ProtoMember(1)] public int Value { get; set; }

    public void Increment()
    {
        Value++;
    }
}