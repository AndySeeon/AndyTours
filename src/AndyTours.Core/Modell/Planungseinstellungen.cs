namespace AndyTours.Core.Modell;

/// <summary>
/// Was sich das Fenster „Tour planen“ von einem Öffnen zum nächsten merkt.
/// </summary>
/// <param name="Profil">Zuletzt gewähltes Fahrprofil; anfangs „Trekking/Tourenrad“.</param>
/// <param name="SuedWest">Südwestecke des zuletzt gezeigten Kartenausschnitts oder <c>null</c>, wenn noch keiner gemerkt ist.</param>
/// <param name="NordOst">Nordostecke des zuletzt gezeigten Kartenausschnitts oder <c>null</c>.</param>
/// <param name="Ordner">Zuletzt zum Speichern oder Öffnen verwendeter Ordner oder <c>null</c>.</param>
public record Planungseinstellungen(Fahrprofil Profil, Geoposition? SuedWest, Geoposition? NordOst, string? Ordner)
{
    /// <summary>Die Einstellungen beim allerersten Öffnen.</summary>
    public static Planungseinstellungen Standard { get; } = new(Fahrprofil.Trekking, null, null, null);
}
