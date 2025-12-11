using RailwayMonitorClient.Models.Dtos;

namespace RailwayMonitorClient.Models.Utils;

public class EnumResponseUtil
{
    public static List<EnumResponse> ToList<T>() where T : struct, Enum
    {
        return Enum
            .GetValues<T>()
            .Select(e => new EnumResponse { Name = e.ToString(), Value = (int)(object)e })
            .ToList();
    } 
}