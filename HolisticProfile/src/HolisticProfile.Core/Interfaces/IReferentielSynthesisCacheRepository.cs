using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

public interface IReferentielSynthesisCacheRepository
{
    Task<string?> LoadAsync(ReferentielNaissanceProfile profile);
    Task SaveAsync(ReferentielNaissanceProfile profile, string synthesisText);
}
