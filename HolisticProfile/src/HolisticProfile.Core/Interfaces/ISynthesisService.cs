using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

public interface ISynthesisService
{
    Task<SynthesisResult> RunAsync(DateTime birthDate, CancellationToken cancellationToken = default);
}
