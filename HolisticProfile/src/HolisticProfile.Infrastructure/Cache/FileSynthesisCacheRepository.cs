using HolisticProfile.Core.Interfaces;
using HolisticProfile.Core.Models;

namespace HolisticProfile.Infrastructure.Cache;

public class FileSynthesisCacheRepository : ISynthesisCacheRepository
{
    private readonly string _basePath;

    public FileSynthesisCacheRepository(string basePath)
    {
        _basePath = Path.GetFullPath(basePath);
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string?> LoadAsync(BirthProfile profile)
    {
        var filePath = BuildFilePath(profile);
        if (!File.Exists(filePath))
            return null;

        return await File.ReadAllTextAsync(filePath);
    }

    public async Task SaveAsync(BirthProfile profile, string synthesisText)
    {
        var filePath = BuildFilePath(profile);
        await File.WriteAllTextAsync(filePath, synthesisText);
    }

    private string BuildFilePath(BirthProfile profile)
    {
        var datePart = profile.BirthDate.ToString("dd_MM_yyyy");
        var fileName = $"{datePart}_path_{profile.MillmanLifePath.PathKey}.md";
        return Path.Combine(_basePath, fileName);
    }
}
