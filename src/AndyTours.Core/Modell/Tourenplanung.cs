namespace AndyTours.Core.Modell;

/// <summary>
/// Eine geplante Tour: die Wegpunkte des Benutzers, das Fahrprofil und die Teilstücke dazwischen.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Zwischen je zwei benachbarten Wegpunkten liegt genau ein Teilstück; bei n Wegpunkten sind es n − 1
/// (bei keinem oder einem Wegpunkt keines).</item>
/// <item>Ein Teilstück <c>null</c> ist noch zu ermitteln, z. B. nach dem Setzen oder Verschieben eines Wegpunkts.</item>
/// <item>Die Planung wird nie verändert; jede Änderung (<see cref="Berechnungen.AenderePlanung"/>) liefert eine neue.</item>
/// </list>
/// </remarks>
/// <param name="Profil">Das Fahrprofil, mit dem die Teilstücke ermittelt werden.</param>
/// <param name="Wegpunkte">Die Wegpunkte in ihrer Reihenfolge, vom Start bis zum Ziel.</param>
/// <param name="Teilstuecke">Die Teilstücke zwischen den Wegpunkten; <c>null</c> = noch zu ermitteln.</param>
public record Tourenplanung(Fahrprofil Profil, IReadOnlyList<Geoposition> Wegpunkte, IReadOnlyList<Teilstueck?> Teilstuecke)
{
    /// <summary>
    /// Eine leere Planung ohne Wegpunkte.
    /// </summary>
    /// <param name="profil">Das Fahrprofil.</param>
    /// <returns>Die leere Planung.</returns>
    public static Tourenplanung Leer(Fahrprofil profil) => new(profil, [], []);
}
