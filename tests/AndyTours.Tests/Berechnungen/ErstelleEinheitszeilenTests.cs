using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErstelleEinheitszeilen"/>.
/// </summary>
public class ErstelleEinheitszeilenTests
{
    /// <summary>Datenbestand mit zwei Aktivitäten, einem Equipment und drei Kategorien.</summary>
    private static Datenbestand Bestand() => new()
    {
        Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike" }, new Aktivitaet { Id = 2, Name = "Wandern" } },
        Equipments = { new Equipment { Id = 7, Name = "Genius" } },
        Kategorien =
        {
            new Kategorie { Id = 1, Name = "Tour" }, new Kategorie { Id = 2, Name = "Alpen" }, new Kategorie { Id = 3, Name = "Training" },
        },
    };

    /// <summary>Vollständig erfasste Einheit.</summary>
    private static Trainingseinheit Einheit(long id, double km = 50, int hm = 500, double stunden = 2, double maxKmh = 60) => new()
    {
        Id = id, Datum = new DateOnly(2026, 8, 15), Startzeit = new TimeOnly(9, 30), Name = "Bründlingalm",
        AktivitaetId = 1, EquipmentId = 7, KategorieIds = [1, 2],
        DistanzKm = km, Hoehenmeter = hm, Fahrzeit = TimeSpan.FromHours(stunden), MaxGeschwindigkeit = maxKmh,
        HfDurchschnitt = 130, HfMax = 170, Trittfrequenz = 80, Temperatur = 18, MaxHoehe = 1567, Trainingsbereich = "GA1",
        Kommentar = "mit Sabine",
    };

    [Fact]
    public void Ausgemustertes_Equipment_mit_Zusatz()
    {
        Datenbestand bestand = Bestand();
        bestand.Equipments[0].Ausgemustert = true;

        Assert.Equal("Genius (ausgemustert)", ErstelleEinheitszeilen.Ausfuehren([Einheit(1)], bestand)[0].Equipment);
    }

    [Fact]
    public void Eine_Zeile_je_Einheit_mit_aufgeloesten_Namen()
    {
        List<Einheitszeile> zeilen = ErstelleEinheitszeilen.Ausfuehren([Einheit(1), Einheit(2)], Bestand());

        Assert.Equal(2, zeilen.Count);
        Einheitszeile zeile = zeilen[0];
        Assert.Equal(1, zeile.Id);
        Assert.Equal(new DateOnly(2026, 8, 15), zeile.Datum);
        Assert.Equal(new TimeOnly(9, 30), zeile.Startzeit);
        Assert.Equal("Bründlingalm", zeile.Name);
        Assert.Equal("Bike", zeile.Aktivitaet);
        Assert.Equal("Genius", zeile.Equipment);
        Assert.Equal("Alpen, Tour", zeile.Kategorien); // alphabetisch
        Assert.Equal(50, zeile.DistanzKm);
        Assert.Equal(500, zeile.Hoehenmeter);
        Assert.Equal(TimeSpan.FromHours(2), zeile.Fahrzeit);
        Assert.Equal(25, zeile.DurchschnittKmh);
        Assert.Equal(60, zeile.MaxKmh);
        Assert.Equal(130, zeile.HfDurchschnitt);
        Assert.Equal(170, zeile.HfMax);
        Assert.Equal(80, zeile.Trittfrequenz);
        Assert.Equal(18, zeile.Temperatur);
        Assert.Equal(1567, zeile.MaxHoehe);
        Assert.Equal("GA1", zeile.Trainingsbereich);
        Assert.Equal("mit Sabine", zeile.Kommentar);
    }

    [Fact]
    public void Fehlende_Werte_werden_null()
    {
        Trainingseinheit leer = new() { Id = 1, Datum = new DateOnly(2026, 1, 1), AktivitaetId = 1 };

        Einheitszeile zeile = Assert.Single(ErstelleEinheitszeilen.Ausfuehren([leer], Bestand()));

        Assert.Null(zeile.Startzeit);
        Assert.Null(zeile.DistanzKm);
        Assert.Null(zeile.Fahrzeit);
        Assert.Null(zeile.DurchschnittKmh);
        Assert.Null(zeile.MaxKmh);
        Assert.Null(zeile.HfDurchschnitt);
        Assert.Null(zeile.HfMax);
        Assert.Null(zeile.Trittfrequenz);
        Assert.Null(zeile.Temperatur);
        Assert.Null(zeile.MaxHoehe);
        Assert.Equal(string.Empty, zeile.Equipment);
        Assert.Equal(string.Empty, zeile.Kategorien);
        Assert.Equal(string.Empty, zeile.Kommentar); // ohne Kommentar bleibt die Zelle leer
        Assert.False(zeile.IstBestDistanz || zeile.IstBestHoehenmeter || zeile.IstBestFahrzeit || zeile.IstBestDurchschnittKmh || zeile.IstBestMaxKmh);
    }

    [Fact]
    public void Negative_Temperatur_bleibt_erhalten()
    {
        Trainingseinheit winter = Einheit(1);
        winter.Temperatur = -5;

        Assert.Equal(-5, Assert.Single(ErstelleEinheitszeilen.Ausfuehren([winter], Bestand())).Temperatur);
    }

    [Fact]
    public void Nur_die_Hoechstwerte_werden_markiert()
    {
        List<Einheitszeile> zeilen = ErstelleEinheitszeilen.Ausfuehren(
        [
            Einheit(1, km: 100, hm: 500, stunden: 5, maxKmh: 60),  // längste und weiteste
            Einheit(2, km: 60, hm: 1800, stunden: 4, maxKmh: 72),  // höchste und schnellste
            Einheit(3, km: 50, hm: 300, stunden: 2, maxKmh: 55),   // beste Ø (25 km/h)
        ], Bestand());

        Assert.Equal([true, false, false], zeilen.Select(z => z.IstBestDistanz));
        Assert.Equal([true, false, false], zeilen.Select(z => z.IstBestFahrzeit));
        Assert.Equal([false, true, false], zeilen.Select(z => z.IstBestHoehenmeter));
        Assert.Equal([false, true, false], zeilen.Select(z => z.IstBestMaxKmh));
        Assert.Equal([false, false, true], zeilen.Select(z => z.IstBestDurchschnittKmh));
    }

    [Fact]
    public void Bei_Gleichstand_werden_alle_markiert()
    {
        List<Einheitszeile> zeilen = ErstelleEinheitszeilen.Ausfuehren(
            [Einheit(1, km: 80), Einheit(2, km: 80), Einheit(3, km: 40)], Bestand());

        Assert.Equal([true, true, false], zeilen.Select(z => z.IstBestDistanz));
    }

    [Fact]
    public void Leere_Auswahl_ergibt_keine_Zeilen()
    {
        Assert.Empty(ErstelleEinheitszeilen.Ausfuehren([], Bestand()));
    }
}
