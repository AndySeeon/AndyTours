using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Liefert die Kategorie zu einem Namen aus den Altdaten und legt sie an, falls sie noch fehlt.
/// </summary>
public static class FindeOderErgaenzeKategorie
{
    /// <summary>
    /// Sucht die Kategorie über den Namen (ohne Beachtung der Groß-/Kleinschreibung); fehlt sie, wird sie ergänzt.
    /// </summary>
    /// <remarks>
    /// In <c>workouts.csv</c> kommt z. B. die Kategorie „Training" vor, die in <c>bikexperience.ini</c>
    /// nicht (mehr) steht – dort gibt es nur „Trainieren". Die Kategorien werden nicht
    /// automatisch zusammengelegt, weil das eine inhaltliche Entscheidung ist; der Bericht weist darauf hin.
    /// </remarks>
    /// <param name="bestand">Datenbestand, in dem gesucht bzw. ergänzt wird.</param>
    /// <param name="name">Kategoriename aus der Altdatei.</param>
    /// <param name="bericht">Bericht für Hinweise auf Ergänzungen.</param>
    /// <returns>Die gefundene oder neu angelegte Kategorie.</returns>
    public static Kategorie Ausfuehren(Datenbestand bestand, string name, Migrationsbericht bericht)
    {
        string gesucht = name.Trim();
        Kategorie? gefunden = bestand.Kategorien.FirstOrDefault(k => k.Name.Equals(gesucht, StringComparison.OrdinalIgnoreCase));
        if (gefunden is not null)
        {
            return gefunden;
        }

        var neu = new Kategorie
        {
            Id = bestand.Kategorien.Count == 0 ? 1 : bestand.Kategorien.Max(k => k.Id) + 1,
            Name = gesucht,
        };
        bestand.Kategorien.Add(neu);
        bericht.HinweisEinmalig($"Kategorie „{neu.Name}“ stand nicht in bikexperience.ini und wurde ergänzt.");
        return neu;
    }
}
