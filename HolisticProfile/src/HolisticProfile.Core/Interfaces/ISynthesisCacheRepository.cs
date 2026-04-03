using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

public interface ISynthesisCacheRepository
{
    Task<string?> LoadAsync(BirthProfile profile);
    Task SaveAsync(BirthProfile profile, string synthesisText);
}
