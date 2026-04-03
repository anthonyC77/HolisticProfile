namespace HolisticProfile.Core.Interfaces;

public interface IKnowledgeBaseRepository
{
    Task<string?> LoadAsync(string pathKey);
}
