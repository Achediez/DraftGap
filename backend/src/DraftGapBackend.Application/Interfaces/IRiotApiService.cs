using System.Threading.Tasks;

namespace DraftGapBackend.Application.Interfaces;

public interface IRiotApiService
{
    Task<bool> VerifyRiotAccountAsync(string gameName, string tagLine, string region);
    Task<string?> GetPuuidByRiotIdAsync(string gameName, string tagLine, string region);
}
