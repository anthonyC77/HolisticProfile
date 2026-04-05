using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

public interface IReferentielPromptBuilder
{
    string Build(ReferentielNaissanceProfile profile, string knowledgeContent);
}
