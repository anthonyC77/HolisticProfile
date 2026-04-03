using HolisticProfile.Core.Interfaces;

namespace HolisticProfile.Infrastructure.KnowledgeBase;

public class FileKnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly string _basePath;

    public FileKnowledgeBaseRepository(string basePath)
    {
        _basePath = Path.GetFullPath(basePath);
    }

    public async Task<string?> LoadAsync(string pathKey)
    {
        // Sécurité : interdire toute tentative de path traversal
        if (pathKey.Contains("..") || pathKey.Contains('/') || pathKey.Contains('\\'))
            throw new ArgumentException($"Clé de chemin invalide : '{pathKey}'", nameof(pathKey));

        var filePath = Path.GetFullPath(Path.Combine(_basePath, $"path_{pathKey}.md"));

        // Vérifier que le chemin résolu reste dans le répertoire de base
        if (!filePath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Clé de chemin invalide : '{pathKey}'", nameof(pathKey));

        if (!File.Exists(filePath))
            return null;

        return await File.ReadAllTextAsync(filePath);
    }
}
