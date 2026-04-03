using HolisticProfile.Core.Models;

namespace HolisticProfile.Core.Interfaces;

public interface IPromptBuilder
{
    string Build(BirthProfile profile, string? knowledgeContent);
}
