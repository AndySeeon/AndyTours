namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Ermittelt, mit welchem Datenordner die App startet.
/// </summary>
public static class ErmittleStartDatenordner
{
    /// <summary>
    /// Wählt den Datenordner für den Programmstart nach festen Regeln.
    /// </summary>
    /// <remarks>
    /// Reihenfolge:
    /// <list type="number">
    /// <item>Während der Entwicklung immer <c>AppDaten</c> im Projektordner – eine gemerkte
    /// Einstellung der fertigen App spielt dann keine Rolle, damit die echten Daten unberührt bleiben.</item>
    /// <item>Sonst der zuletzt gewählte (gemerkte) Ordner.</item>
    /// <item>Sonst, beim ersten Start, der Standardordner (<c>Dokumente\AndyTours</c>).</item>
    /// </list>
    /// Ob der Ordner existiert oder erreichbar ist, prüft erst <see cref="PruefeDatenordner"/>.
    /// </remarks>
    /// <param name="istEntwicklung"><c>true</c>, wenn die App als Entwicklungsversion (Debug-Build) läuft.</param>
    /// <param name="programmordner">Ordner, in dem das Programm liegt; Ausgangspunkt für die Suche nach dem Projektordner.</param>
    /// <param name="gemerkterOrdner">Der zuletzt gewählte Ordner oder <c>null</c>.</param>
    /// <param name="standardordner">Der Standardordner für den ersten Start.</param>
    /// <returns>Vollständiger Pfad des zu verwendenden Datenordners.</returns>
    /// <exception cref="InvalidOperationException">Entwicklungsversion, aber kein Projektordner oberhalb des Programmordners.</exception>
    public static string Ausfuehren(bool istEntwicklung, string programmordner, string? gemerkterOrdner, string standardordner)
    {
        if (istEntwicklung)
        {
            string projektordner = FindeProjektordner.Ausfuehren(programmordner)
                ?? throw new InvalidOperationException(
                    $"Die Entwicklungsversion findet ihren Projektordner nicht (keine {DatenordnerInhalt.Projektmappe} oberhalb von {programmordner}).");
            return Path.Combine(projektordner, DatenordnerInhalt.Entwicklungsordner);
        }

        return gemerkterOrdner ?? standardordner;
    }
}
