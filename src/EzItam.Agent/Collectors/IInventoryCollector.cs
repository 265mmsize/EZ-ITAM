using EzItam.Contracts;

namespace EzItam.Agent.Collectors;

public interface IInventoryCollector
{
    Task<HardwareInfo> CollectHardwareAsync(CancellationToken ct);
    Task<IReadOnlyList<SoftwareItem>> CollectSoftwareAsync(CancellationToken ct);
}
