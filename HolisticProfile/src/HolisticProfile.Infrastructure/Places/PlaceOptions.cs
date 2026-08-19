namespace HolisticProfile.Infrastructure.Places;

public class PlaceOptions
{
    /// <summary>
    /// Chemin du fichier JSON contenant la table des lieux de naissance
    /// (nom, région, pays, latitude, longitude, fuseau IANA).
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}
