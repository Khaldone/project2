// Assets/_Project/CoreDomain/Hardware/IDeviceInfoService.cs
public enum DeviceFormFactor
{
    Phone,
    Tablet,
    Desktop // (Future-proofing for a PC port)
}

public interface IDeviceInfoService
{
    DeviceFormFactor CurrentFormFactor { get; }
    bool IsNotchDevice { get; } // Useful for adjusting top UI padding
}