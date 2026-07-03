namespace Common;

public class CommonUtility
{
    public static string GenerateGuid()
    {
        return Guid.CreateVersion7().ToString("N");
    }
    
}