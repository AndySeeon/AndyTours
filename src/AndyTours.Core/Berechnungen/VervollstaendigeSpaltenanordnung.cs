using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Macht aus einer gespeicherten Spaltenanordnung eine vollständige Anordnung aller Spalten.
/// </summary>
public static class VervollstaendigeSpaltenanordnung
{
    /// <summary>
    /// Liefert jede <see cref="Einheitenspalte"/> genau einmal, in gespeicherter Reihenfolge.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Doppelte Einträge zählen nur beim ersten Vorkommen.</item>
    /// <item>Breiten, die nicht größer als 0 oder keine Zahl sind, gelten als „automatisch“ (<c>null</c>).</item>
    /// <item>Fehlende Spalten – z. B. in einer späteren Version neu hinzugekommene – werden in ihrer
    /// Standardreihenfolge hinten angefügt, mit automatischer Breite.</item>
    /// </list>
    /// Ohne gespeicherte Anordnung ergibt sich so die Standardreihenfolge mit automatischen Breiten.
    /// </remarks>
    /// <param name="gespeichert">Die gespeicherte Anordnung (kann unvollständig sein).</param>
    /// <returns>Die vollständige Anordnung in Anzeigereihenfolge.</returns>
    public static List<Spalteneinstellung> Ausfuehren(IEnumerable<Spalteneinstellung> gespeichert)
    {
        var ergebnis = new List<Spalteneinstellung>();
        var vorhanden = new HashSet<Einheitenspalte>();

        foreach (Spalteneinstellung eintrag in gespeichert)
        {
            if (Enum.IsDefined(eintrag.Spalte) && vorhanden.Add(eintrag.Spalte))
            {
                bool brauchbar = eintrag.Breite is double breite && breite > 0 && double.IsFinite(breite);
                ergebnis.Add(brauchbar ? eintrag : eintrag with { Breite = null });
            }
        }

        foreach (Einheitenspalte spalte in Enum.GetValues<Einheitenspalte>())
        {
            if (vorhanden.Add(spalte))
            {
                ergebnis.Add(new Spalteneinstellung(spalte, null));
            }
        }

        return ergebnis;
    }
}
