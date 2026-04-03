using FluentAssertions;
using HolisticProfile.Infrastructure.KnowledgeBase;

namespace HolisticProfile.Infrastructure.Tests.KnowledgeBase;

public class FileKnowledgeBaseRepositoryTests : IDisposable
{
    private readonly string _tempDir;

    public FileKnowledgeBaseRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public async Task LoadAsync_ExistingFile_ReturnsContent()
    {
        var content = "# Chemin 34/7\nContenu de test.";
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "path_34_7.md"), content);
        var repo = new FileKnowledgeBaseRepository(_tempDir);

        var result = await repo.LoadAsync("34_7");

        result.Should().Be(content);
    }

    [Fact]
    public async Task LoadAsync_ThreePartPathKey_ReturnsContent()
    {
        var content = "# Chemin 29/11/2\nDouble défi du 1.";
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "path_29_11_2.md"), content);
        var repo = new FileKnowledgeBaseRepository(_tempDir);

        var result = await repo.LoadAsync("29_11_2");

        result.Should().Be(content);
    }

    [Fact]
    public async Task LoadAsync_NonExistentFile_ReturnsNull()
    {
        var repo = new FileKnowledgeBaseRepository(_tempDir);

        var result = await repo.LoadAsync("99_9");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_ReturnsEmptyString()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "path_34_7.md"), string.Empty);
        var repo = new FileKnowledgeBaseRepository(_tempDir);

        var result = await repo.LoadAsync("34_7");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_PathTraversalAttempt_ThrowsArgumentException()
    {
        var repo = new FileKnowledgeBaseRepository(_tempDir);

        var act = () => repo.LoadAsync("../secret");

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
