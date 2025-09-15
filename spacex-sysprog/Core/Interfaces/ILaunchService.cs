using System.Threading.Tasks;

namespace spacex_sysprog.Core.Interfaces;

public interface ILaunchService
{
    Task<LaunchQueryResult> QueryLaunchesAsync(LaunchQueryParameters p, CancellationToken ct = default);
}