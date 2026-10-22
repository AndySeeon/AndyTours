using AndyTours.Daten.Datenordner;

namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Tests für <see cref="KopiereDatenbestand"/>.
/// </summary>
public class KopiereDatenbestandTests
{
    /// <summary>
    /// Sammelt Fortschrittsmeldungen sofort und im selben Thread
    /// (<see cref="Progress{T}"/> würde sie verzögert über den Synchronisationskontext zustellen).
    /// </summary>
    private sealed class Mitschrift : IProgress<KopierFortschritt>
    {
        public List<KopierFortschritt> Meldungen { get; } = [];

        public void Report(KopierFortschritt value) => Meldungen.Add(value);
    }

    /// <summary>Legt einen kleinen Datenbestand an: Datenbank, Bericht und zwei Jahresordner mit FIT-Dateien.</summary>
    private static void ErzeugeBestand(string ordner)
    {
        File.WriteAllText(Path.Combine(ordner, DatenordnerInhalt.Datenbankdatei), "DB-Inhalt");
        File.WriteAllText(Path.Combine(ordner, "migrationsbericht.txt"), "Bericht");
        Directory.CreateDirectory(Path.Combine(ordner, "2025"));
        Directory.CreateDirectory(Path.Combine(ordner, "2026"));
        File.WriteAllBytes(Path.Combine(ordner, "2025", "a.fit"), new byte[1000]);
        File.WriteAllBytes(Path.Combine(ordner, "2026", "b.fit"), new byte[2000]);
    }

    [Fact]
    public void Alle_Dateien_werden_mit_Unterordnern_kopiert_und_die_Quelle_bleibt_unveraendert()
    {
        using var quelle = new TempOrdner();
        using var ziel = new TempOrdner();
        ErzeugeBestand(quelle.Pfad);
        string[] vorher = Directory.GetFiles(quelle.Pfad, "*", SearchOption.AllDirectories);

        KopiereDatenbestand.Ausfuehren(quelle.Pfad, ziel.Pfad, null);

        foreach (string datei in vorher)
        {
            string kopie = Path.Combine(ziel.Pfad, Path.GetRelativePath(quelle.Pfad, datei));
            Assert.Equal(File.ReadAllBytes(datei), File.ReadAllBytes(kopie));
        }

        Assert.Equal(vorher.Length, Directory.GetFiles(ziel.Pfad, "*", SearchOption.AllDirectories).Length);
        Assert.Equal(vorher.Order(), Directory.GetFiles(quelle.Pfad, "*", SearchOption.AllDirectories).Order());
    }

    [Fact]
    public void Fortschritt_wird_je_Datei_gemeldet_und_endet_mit_den_Gesamtwerten()
    {
        using var quelle = new TempOrdner();
        using var ziel = new TempOrdner();
        ErzeugeBestand(quelle.Pfad);
        var mitschrift = new Mitschrift();

        KopiereDatenbestand.Ausfuehren(quelle.Pfad, ziel.Pfad, mitschrift);

        // 4 Dateien: DB (9 Byte) + Bericht (7 Byte) + 1000 + 2000 Byte
        Assert.Equal([1, 2, 3, 4], mitschrift.Meldungen.Select(m => m.KopierteDateien));
        KopierFortschritt letzte = mitschrift.Meldungen[^1];
        Assert.Equal(new KopierFortschritt(4, 4, 3016, 3016), letzte);
    }

    [Fact]
    public void Datenbank_wird_als_letzte_Datei_kopiert()
    {
        using var quelle = new TempOrdner();
        using var ziel = new TempOrdner();
        ErzeugeBestand(quelle.Pfad);
        string zielDb = Path.Combine(ziel.Pfad, DatenordnerInhalt.Datenbankdatei);
        var dbVorDerLetztenMeldung = new List<bool>();

        // Bei jeder Meldung festhalten, ob die Datenbank im Ziel schon existiert
        var beobachter = new Beobachter(() => dbVorDerLetztenMeldung.Add(File.Exists(zielDb)));
        KopiereDatenbestand.Ausfuehren(quelle.Pfad, ziel.Pfad, beobachter);

        Assert.Equal([false, false, false, true], dbVorDerLetztenMeldung);
    }

    [Fact]
    public void Fehler_entfernt_bereits_kopierte_Dateien_und_wird_gemeldet()
    {
        using var quelle = new TempOrdner();
        using var ziel = new TempOrdner();
        ErzeugeBestand(quelle.Pfad);

        // Datenbank exklusiv sperren: Sie wird zuletzt kopiert, d. h. vorher sind schon 3 Dateien im Ziel
        using (File.Open(Path.Combine(quelle.Pfad, DatenordnerInhalt.Datenbankdatei), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Assert.ThrowsAny<IOException>(() => KopiereDatenbestand.Ausfuehren(quelle.Pfad, ziel.Pfad, null));
        }

        Assert.Empty(Directory.GetFileSystemEntries(ziel.Pfad));
    }

    /// <summary>Ruft bei jeder Fortschrittsmeldung eine Aktion auf.</summary>
    private sealed class Beobachter(Action aktion) : IProgress<KopierFortschritt>
    {
        public void Report(KopierFortschritt value) => aktion();
    }
}
