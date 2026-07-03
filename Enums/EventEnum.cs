namespace Enums;

public enum EventTypeEnum
{
    NotifyMessage = 1,
    System = 2,
    Email = 3,
    EmailMarketing = 4,
    Notification = 5,
    NotificationMessage = 6,
    Sms = 7,
    Cache = 8,
}
public enum EventStatusEnum
{
    Fail = -1,
    New = 0,
    Success = 1,
    Retry = 2
}
public enum SerializeTypeEnum
{
    Json = 1,
    Protobuf = 2,
    String = 3,
    Byte = 4
}