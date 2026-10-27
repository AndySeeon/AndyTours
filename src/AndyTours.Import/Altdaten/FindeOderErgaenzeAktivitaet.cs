using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Liefert die Aktivität zu einem Namen aus den Altdaten und legt sie an, falls sie noch fehlt.
/// </summary>
public static class FindeOderErgaenzeAktivitaet
{
    /// <summary>
    /// Sucht die Aktivität über den Namen (ohne Beachtung der Groß-/Kleinschreibung); fehlt sie, wird sie ergänzt.
    /// </summary>
    /// <param name="bestand">Datenbestand, in dem gesucht bzw. ergänzt wird.</param>
    /// <param name="name">Aktivitätsname aus der Altdatei, z. B. „Bike".</param>
    /// <param name="bericht">Bericht für Hinweise auf Ergänzungen.</param>
    /// <returns>Die gefundene oder neu angelegte Aktivität.</returns>
    public static Aktivitaet Ausfuehren(Datenbestand bestand, string name, Migrationsbericht bericht)
    {
        string gesucht = name.Trim();
        Aktivitaet? gefunden = bestand.Aktivitaeten.FirstOrDefault(a => a.Name.Equals(gesucht, StringComparison.OrdinalIgnoreCase));
        if (gefunden is not null)
        {
            return gefunden;
        }

        var neu = new Aktivitaet
        {
            Id = bestand.Aktivitaeten.Count == 0 ? 1 : bestand.Aktivitaeten.Max(a => a.Id) + 1,
            Name = gesucht,
        };
        bestand.Aktivitaeten.Add(neu);
        bericht.HinweisEinmalig($"Aktivität „{neu.Name}“ stand nicht in bikexperience.ini und wurde ergänzt.");
        return neu;
    }
}
