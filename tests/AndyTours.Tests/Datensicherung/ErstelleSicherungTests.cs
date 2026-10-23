using AndyTours.Core.Modell;
using AndyTours.Daten.Datensicherung;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Tests für <see cref="ErstelleSicherung"/>.
/// </summary>
public class ErstelleSicherungTests
{
    [Fact]
    public void Sicherung_enthaelt_alle_Touren_Raeder_und_Wartungen()
    {
        using var temp = new TempOrdner();
        string datei = TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(5));
        Datenbestand original = TestDatenbestand.Lade(datei);

        string sicherung = ErstelleSicherung.Ausfuehren(datei, Path.Combine(temp.Pfad, "Sicherungen"), new DateTime(2026, 10, 23, 7, 39, 12));
        Datenbestand gesichert = TestDatenbestand.Lade(sicherung);

        Assert.Equal(original.Trainingseinheiten.Select(e => (e.Id, e.Datum, e.Name, e.DistanzKm, e.Hoehenmeter)),
            gesichert.Trainingseinheiten.Select(e => (e.Id, e.Datum, e.Name, e.DistanzKm, e.Hoehenmeter)));
        Assert.Equal(original.Equipments.Select(e => e.Name), gesichert.Equipments.Select(e => e.Name));
        Assert.Equal(original.Wartungen.Select(w => (w.Datum, w.Taetigkeit)), gesichert.Wartungen.Select(w => (w.Datum, w.Taetigkeit)));
    }

    [Fact]
    public void Name_enthaelt_Datum_und_Uhrzeit_und_Ordner_wird_angelegt()
    {
        using var temp = new TempOrdner();
        string datei = TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(1));
        string ordner = Path.Combine(temp.Pfad, "NAS", "Sicherungen");

        string sicherung = ErstelleSicherung.Ausfuehren(datei, ordner, new DateTime(2026, 10, 23, 7, 39, 12));

        Assert.Equal(Path.Combine(ordner, "andytours_2026-10-23_07-39-12.db"), sicherung);
        Assert.True(File.Exists(sicherung));
        // Keine übrig gebliebene Zwischendatei
        Assert.Single(Directory.GetFiles(ordner));
    }

    [Fact]
    public void Original_bleibt_unveraendert()
    {
        using var temp = new TempOrdner();
        string datei = TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(3));
        byte[] vorher = File.ReadAllBytes(datei);

        ErstelleSicherung.Ausfuehren(datei, Path.Combine(temp.Pfad, "Sicherungen"), DateTime.Now);

        Assert.Equal(vorher, File.ReadAllBytes(datei));
    }

    [Fact]
    public void Fehlende_Datenbank_hinterlaesst_keine_Zwischendatei()
    {
        using var temp = new TempOrdner();
        string ordner = Path.Combine(temp.Pfad, "Sicherungen");

        Assert.ThrowsAny<Exception>(() => ErstelleSicherung.Ausfuehren(Path.Combine(temp.Pfad, "fehlt.db"), ordner, DateTime.Now));
        Assert.Empty(Directory.GetFiles(ordner));
    }
}
