using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErstelleNeueEinheit"/> und <see cref="KopiereEinheit"/>.
/// </summary>
public class NeueUndKopierteEinheitTests
{
    private static readonly DateOnly Heute = new(2026, 10, 24);

    /// <summary>Datenbestand mit Fahrer, zwei Aktivitäten (eine Hauptaktivität) und Standardkategorien.</summary>
    private static Datenbestand Bestand() => new()
    {
        Fahrer = { new Fahrer { Id = 3 } },
        Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike", IstHaupt = true }, new Aktivitaet { Id = 2, Name = "Wandern" } },
        Kategorien =
        {
            new Kategorie { Id = 1, Name = "Tour", IstStandard = true }, new Kategorie { Id = 2, Name = "Alpen" },
            new Kategorie { Id = 3, Name = "Heimrunde", IstStandard = true },
        },
        Equipments = { new Equipment { Id = 7, Name = "Genius" } },
    };

    [Fact]
    public void Neue_Einheit_mit_heutigem_Datum_Aktivitaet_der_Werkzeugleiste_und_Standardkategorien()
    {
        Trainingseinheit neu = ErstelleNeueEinheit.Ausfuehren(Bestand(), aktivitaetId: 2, Heute);

        Assert.Equal(0, neu.Id);
        Assert.Equal(Heute, neu.Datum);
        Assert.Equal(2, neu.AktivitaetId);
        Assert.Equal([1, 3], neu.KategorieIds);
        Assert.Equal(3, neu.FahrerId);
        Assert.Null(neu.EquipmentId);
        Assert.Equal("", neu.Name);
        Assert.Equal(0, neu.DistanzKm);
        Assert.Null(neu.Startzeit);
        Assert.Empty(neu.Anhaenge);
    }

    [Fact]
    public void Bei_alle_Aktivitaeten_gilt_die_Hauptaktivitaet()
    {
        Assert.Equal(1, ErstelleNeueEinheit.Ausfuehren(Bestand(), aktivitaetId: null, Heute).AktivitaetId);
    }

    [Fact]
    public void Ohne_Hauptaktivitaet_und_ohne_Fahrer_bleibt_beides_offen()
    {
        var bestand = new Datenbestand { Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike" } } };

        Trainingseinheit neu = ErstelleNeueEinheit.Ausfuehren(bestand, aktivitaetId: null, Heute);

        Assert.Equal(0, neu.AktivitaetId);
        Assert.Equal(0, neu.FahrerId);
        Assert.Null(EinheitEingaben.AlsEingaben(neu).AktivitaetId); // im Fenster noch keine Aktivität gewählt
    }

    [Fact]
    public void Standard_Equipment_der_Aktivitaet_ist_vorbelegt_und_seine_Standardkategorie_angehakt()
    {
        // Bike: Rennrad ist wirksamer Standard (Altrad ist ausgemustert); Rennrad hat Standardkategorie „urlaub“
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        bestand.Equipments.Single(e => e.Name == "Rennrad").StandardKategorie = "urlaub";

        Trainingseinheit neu = ErstelleNeueEinheit.Ausfuehren(bestand, aktivitaetId: null, Heute);

        Assert.Equal(5, neu.EquipmentId);
        Assert.Equal([2L, 1L], neu.KategorieIds); // Standardkategorie „Trainieren“, dazu „Urlaub“
    }

    [Fact]
    public void Bereits_angehakte_Standardkategorie_des_Equipments_kommt_nicht_doppelt()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        bestand.Equipments.Single(e => e.Name == "Rennrad").StandardKategorie = "Trainieren";

        Assert.Equal([2L], ErstelleNeueEinheit.Ausfuehren(bestand, aktivitaetId: 1, Heute).KategorieIds);
    }

    [Fact]
    public void Ohne_wirksames_Standard_Equipment_bleibt_das_Equipment_leer()
    {
        // Laufen hat kein Standard-Equipment; Altrad (Bike) wäre Standard, ist aber ausgemustert
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        Assert.Null(ErstelleNeueEinheit.Ausfuehren(bestand, aktivitaetId: 2, Heute).EquipmentId);

        bestand.Equipments.RemoveAll(e => e.Name == "Rennrad");
        Assert.Null(ErstelleNeueEinheit.Ausfuehren(bestand, aktivitaetId: 1, Heute).EquipmentId);
    }

    [Fact]
    public void Kopie_uebernimmt_alles_ausser_Nummer_Datum_und_Anhaengen()
    {
        var original = new Trainingseinheit
        {
            Id = 42, FahrerId = 3, Datum = new DateOnly(2026, 8, 15), Startzeit = new TimeOnly(9, 30), Name = "Alm",
            AktivitaetId = 1, KategorieIds = [1, 2], EquipmentId = 7, DistanzKm = 50, Hoehenmeter = 900,
            Fahrzeit = TimeSpan.FromHours(3), MaxGeschwindigkeit = 60, HfDurchschnitt = 130, HfMax = 170, HfMin = 60,
            Trittfrequenz = 80, Temperatur = -2, LeistungWatt = 180, KoerpergewichtKg = 78, Erholungspuls = 95,
            StartHoehe = 480, MaxHoehe = 1500, Trainingsbereich = "GA1", Kommentar = "schön",
            Anhaenge = [new Anhang { Id = 5, Typ = AnhangTyp.Hoehenprofil, RelativerPfad = "a.fit" }],
        };

        Trainingseinheit kopie = KopiereEinheit.Ausfuehren(original, Heute);

        Assert.Equal(0, kopie.Id);
        Assert.Equal(Heute, kopie.Datum);
        Assert.Empty(kopie.Anhaenge);
        // Alle übrigen Werte gleich: Vergleich über die Eingaben, die jedes Feld enthalten
        Assert.Equal(
            EinheitEingaben.AlsEingaben(original) with { Datum = "" },
            EinheitEingaben.AlsEingaben(kopie) with { Datum = "" });
        Assert.Equal(3, kopie.FahrerId);
    }

    [Fact]
    public void Original_bleibt_unveraendert()
    {
        var original = new Trainingseinheit { Id = 42, Datum = new DateOnly(2026, 8, 15), KategorieIds = [1] };

        Trainingseinheit kopie = KopiereEinheit.Ausfuehren(original, Heute);
        kopie.KategorieIds.Add(2);

        Assert.Equal(42, original.Id);
        Assert.Equal(new DateOnly(2026, 8, 15), original.Datum);
        Assert.Equal([1], original.KategorieIds);
    }
}
