using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="EinheitEingaben"/>.
/// </summary>
public class EinheitEingabenTests
{
    /// <summary>Einheit, in der jedes Feld einen eigenen Wert hat.</summary>
    private static Trainingseinheit Vollstaendig() => new()
    {
        Id = 42, FahrerId = 3, Datum = new DateOnly(2026, 8, 15), Startzeit = new TimeOnly(9, 30), Name = "Bründlingalm",
        AktivitaetId = 1, KategorieIds = [2, 1], EquipmentId = 7,
        DistanzKm = 52.34567, Hoehenmeter = 1234, Fahrzeit = new TimeSpan(3, 45, 12), MaxGeschwindigkeit = 61.5,
        HfDurchschnitt = 130, HfMax = 171, HfMin = 62, Trittfrequenz = 81, Temperatur = -4, LeistungWatt = 180.5,
        KoerpergewichtKg = 78.4, Erholungspuls = 95, StartHoehe = 480, MaxHoehe = 1567, Trainingsbereich = "GA1",
        Kommentar = "Schöne Runde\nmit Einkehr",
        Anhaenge = [new Anhang { Id = 5, Typ = AnhangTyp.Hoehenprofil, RelativerPfad = "2026/tour.fit" }],
    };

    [Fact]
    public void Alle_Felder_werden_zu_Eingaben()
    {
        Einheiteneingaben eingaben = EinheitEingaben.AlsEingaben(Vollstaendig());

        Assert.Equal("15.08.2026", eingaben.Datum);
        Assert.Equal("9:30", eingaben.Startzeit);
        Assert.Equal("Bründlingalm", eingaben.Name);
        Assert.Equal(1, eingaben.AktivitaetId);
        Assert.Equal([2, 1], eingaben.KategorieIds);
        Assert.Equal(7, eingaben.EquipmentId);
        Assert.Equal("52,34567", eingaben.Distanz);
        Assert.Equal("1234", eingaben.Hoehenmeter);
        Assert.Equal("3:45:12", eingaben.Fahrzeit);
        Assert.Equal("61,5", eingaben.MaxKmh);
        Assert.Equal("130", eingaben.PulsDurchschnitt);
        Assert.Equal("171", eingaben.PulsMax);
        Assert.Equal("62", eingaben.PulsMin);
        Assert.Equal("81", eingaben.Trittfrequenz);
        Assert.Equal("-4", eingaben.Temperatur);
        Assert.Equal("180,5", eingaben.Leistung);
        Assert.Equal("78,4", eingaben.Koerpergewicht);
        Assert.Equal("95", eingaben.Erholungspuls);
        Assert.Equal("480", eingaben.StartHoehe);
        Assert.Equal("1567", eingaben.MaxHoehe);
        Assert.Equal("GA1", eingaben.Trainingsbereich);
        Assert.Equal("Schöne Runde\nmit Einkehr", eingaben.Kommentar);
    }

    [Fact]
    public void Hin_und_zurueck_ergibt_dieselbe_Einheit()
    {
        Trainingseinheit original = Vollstaendig();

        Eingabeergebnis ergebnis = EinheitEingaben.AlsEinheit(EinheitEingaben.AlsEingaben(original), original);

        Assert.True(ergebnis.IstGueltig);
        Assert.Empty(ergebnis.Fehler);
        Trainingseinheit e = ergebnis.Einheit!;
        Assert.Equal(42, e.Id);
        Assert.Equal(3, e.FahrerId);
        Assert.Equal(original.Datum, e.Datum);
        Assert.Equal(original.Startzeit, e.Startzeit);
        Assert.Equal(original.Name, e.Name);
        Assert.Equal(original.AktivitaetId, e.AktivitaetId);
        Assert.Equal(original.KategorieIds, e.KategorieIds);
        Assert.Equal(original.EquipmentId, e.EquipmentId);
        Assert.Equal(original.DistanzKm, e.DistanzKm);
        Assert.Equal(original.Hoehenmeter, e.Hoehenmeter);
        Assert.Equal(original.Fahrzeit, e.Fahrzeit);
        Assert.Equal(original.MaxGeschwindigkeit, e.MaxGeschwindigkeit);
        Assert.Equal(original.HfDurchschnitt, e.HfDurchschnitt);
        Assert.Equal(original.HfMax, e.HfMax);
        Assert.Equal(original.HfMin, e.HfMin);
        Assert.Equal(original.Trittfrequenz, e.Trittfrequenz);
        Assert.Equal(original.Temperatur, e.Temperatur);
        Assert.Equal(original.LeistungWatt, e.LeistungWatt);
        Assert.Equal(original.KoerpergewichtKg, e.KoerpergewichtKg);
        Assert.Equal(original.Erholungspuls, e.Erholungspuls);
        Assert.Equal(original.StartHoehe, e.StartHoehe);
        Assert.Equal(original.MaxHoehe, e.MaxHoehe);
        Assert.Equal(original.Trainingsbereich, e.Trainingsbereich);
        Assert.Equal(original.Kommentar, e.Kommentar);
    }

    [Fact]
    public void Anhaenge_bleiben_beim_Bearbeiten_erhalten()
    {
        Trainingseinheit original = Vollstaendig();
        Einheiteneingaben geaendert = EinheitEingaben.AlsEingaben(original) with { Name = "Neuer Name" };

        Trainingseinheit e = EinheitEingaben.AlsEinheit(geaendert, original).Einheit!;

        Anhang anhang = Assert.Single(e.Anhaenge);
        Assert.Equal(5, anhang.Id);
        Assert.Equal("2026/tour.fit", anhang.RelativerPfad);
        Assert.Equal("Neuer Name", e.Name);
    }

    [Fact]
    public void Nullwerte_werden_zu_leeren_Feldern_und_zurueck_zu_null()
    {
        var leer = new Trainingseinheit { Datum = new DateOnly(2026, 1, 1), AktivitaetId = 1 };

        Einheiteneingaben eingaben = EinheitEingaben.AlsEingaben(leer);

        Assert.Equal("", eingaben.Startzeit);
        Assert.Equal("", eingaben.Distanz);
        Assert.Equal("", eingaben.Hoehenmeter);
        Assert.Equal("", eingaben.Fahrzeit);
        Assert.Equal("", eingaben.Temperatur);
        Assert.Null(eingaben.EquipmentId);

        Trainingseinheit e = EinheitEingaben.AlsEinheit(eingaben, leer).Einheit!;
        Assert.Equal(0, e.DistanzKm);
        Assert.Equal(0, e.Hoehenmeter);
        Assert.Equal(TimeSpan.Zero, e.Fahrzeit);
        Assert.Null(e.Startzeit);
    }

    [Fact]
    public void Nur_Datum_und_Aktivitaet_sind_Pflicht()
    {
        var eingaben = new Einheiteneingaben { Datum = "24.10.2026", AktivitaetId = 1 };

        Assert.True(EinheitEingaben.AlsEinheit(eingaben, new Trainingseinheit()).IstGueltig);
    }

    [Fact]
    public void Fehlendes_Datum_und_fehlende_Aktivitaet_sind_Fehler()
    {
        Eingabeergebnis ergebnis = EinheitEingaben.AlsEinheit(new Einheiteneingaben(), new Trainingseinheit());

        Assert.False(ergebnis.IstGueltig);
        Assert.Null(ergebnis.Einheit);
        Assert.Equal([Eingabefeld.Datum, Eingabefeld.Aktivitaet], ergebnis.Fehler.Order());
    }

    [Fact]
    public void Ungueltige_Werte_werden_alle_gemeldet()
    {
        var eingaben = new Einheiteneingaben
        {
            Datum = "31.02.2026", AktivitaetId = 1, Startzeit = "25:00", Distanz = "abc", Hoehenmeter = "12,5",
            Fahrzeit = "3:5", PulsMax = "-1", Leistung = "-10",
        };

        Eingabeergebnis ergebnis = EinheitEingaben.AlsEinheit(eingaben, new Trainingseinheit());

        Assert.Equal(
            [Eingabefeld.Datum, Eingabefeld.Startzeit, Eingabefeld.Distanz, Eingabefeld.Hoehenmeter, Eingabefeld.Fahrzeit,
             Eingabefeld.PulsMax, Eingabefeld.Leistung],
            ergebnis.Fehler.Order());
    }

    [Fact]
    public void Negative_Temperatur_ist_erlaubt_andere_negative_Werte_nicht()
    {
        var basis = new Einheiteneingaben { Datum = "24.10.2026", AktivitaetId = 1 };

        Eingabeergebnis kalt = EinheitEingaben.AlsEinheit(basis with { Temperatur = "-12" }, new Trainingseinheit());
        Eingabeergebnis tiefer = EinheitEingaben.AlsEinheit(basis with { StartHoehe = "-5" }, new Trainingseinheit());

        Assert.Equal(-12, kalt.Einheit!.Temperatur);
        Assert.Equal([Eingabefeld.StartHoehe], tiefer.Fehler);
    }

    [Fact]
    public void Eingaben_sind_gleich_auch_bei_anderer_Reihenfolge_der_Kategorien()
    {
        var a = new Einheiteneingaben { Name = "Tour", KategorieIds = [1, 2] };
        var b = new Einheiteneingaben { Name = "Tour", KategorieIds = [2, 1] };

        Assert.Equal(a, b);
        Assert.NotEqual(a, b with { Name = "Andere" });
        Assert.NotEqual(a, b with { KategorieIds = [1] });
    }
}
