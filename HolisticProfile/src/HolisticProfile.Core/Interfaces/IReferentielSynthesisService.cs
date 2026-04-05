using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

public interface IReferentielSynthesisService
{
    Task<ReferentielSynthesisResult> RunAsync(DateTime birthDate, CancellationToken cancellationToken = default);
}
