using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

public interface IReferentielKnowledgeBaseRepository
{
    Task<string> LoadProfileContentAsync(ReferentielNaissanceProfile profile);
}
