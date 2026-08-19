namespace HolisticProfile.Infrastructure.KnowledgeBase;

public class AstrologyKnowledgeBaseOptions
{
    /// <summary>
    /// Répertoire racine de la base de connaissances astrologie.
    /// Attend les sous-dossiers : planets_in_signs/, planets_in_houses/, aspects/.
    /// </summary>
    public string BasePath { get; set; } = string.Empty;
}
