using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BerechneHaltbarkeit"/>.
/// </summary>
public class BerechneHaltbarkeitTests
{
    /// <summary>
    /// Rad 7 (100 Anfangskilometer, angeschafft am 01.01.2026) mit zwei Kettenwechseln; dazu ein zweites Rad,
    /// eine andere Tätigkeit und ein gleichnamiger Alarm, die alle nicht zählen dürfen.
    /// </summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments =
        {
            new Equipment { Id = 7, Name = "Genius", AnfangsdistanzKm = 100, Anschaffungsdatum = new DateOnly(2026, 1, 1) },
            new Equipment { Id = 8, Name = "Stevens" },
        },
        Trainingseinheiten =
        {
            new Trainingseinheit { Datum = new DateOnly(2026, 2, 1), EquipmentId = 7, DistanzKm = 500 },
            new Trainingseinheit { Datum = new DateOnly(2026, 5, 1), EquipmentId = 7, DistanzKm = 40 },    // Tag des 1. Wechsels
            new Trainingseinheit { Datum = new DateOnly(2026, 5, 2), EquipmentId = 7, DistanzKm = 300 },
            new Trainingseinheit { Datum = new DateOnly(2026, 5, 31), EquipmentId = 7, DistanzKm = 60.5 }, // Tag des 2. Wechsels
            new Trainingseinheit { Datum = new DateOnly(2026, 6, 1), EquipmentId = 7, DistanzKm = 80 },    // danach
            new Trainingseinheit { Datum = new DateOnly(2026, 5, 10), EquipmentId = 8, DistanzKm = 999 },  // anderes Rad
        },
        Wartungen =
        {
            new Wartung { Id = 1, Datum = new DateOnly(2026, 5, 1), EquipmentId = 7, Taetigkeit = "Kettenwechsel" },
            new Wartung { Id = 2, Datum = new DateOnly(2026, 5, 20), EquipmentId = 7, Taetigkeit = "Inspektion" },     // andere Tätigkeit
            new Wartung { Id = 3, Datum = new DateOnly(2026, 5, 25), EquipmentId = 8, Taetigkeit = "Kettenwechsel" },  // anderes Rad
            new Wartung
            {
                Id = 4, Datum = new DateOnly(2026, 5, 28), EquipmentId = 7, Taetigkeit = "Kettenwechsel",          // Alarm
                Typ = WartungsTyp.Alarm, AlarmKm = 1000,
            },
            new Wartung { Id = 5, Datum = new DateOnly(2026, 5, 31), EquipmentId = 7, Taetigkeit = " KETTENWECHSEL" }, // andere Schreibweise
        },
    };

    /// <summary>Holt eine Wartung des Bestands über ihre Nummer.</summary>
    private static Wartung Hole(Datenbestand bestand, long id) => bestand.Wartungen.Single(w => w.Id == id);

    [Fact]
    public void Zaehlt_km_und_Tage_seit_der_vorherigen_gleichartigen_Wartung()
    {
        Datenbestand bestand = Bestand();

        Haltbarkeit haltbarkeit = BerechneHaltbarkeit.Ausfuehren(bestand, Hole(bestand, 5));

        // nach dem 01.05. bis einschließlich 31.05.: 300 + 60,5; vom 01.05. bis zum 31.05. sind es 30 Tage.
        // Andere Tätigkeit (Nr. 2), anderes Rad (Nr. 3) und der Alarm (Nr. 4) zählen nicht als vorherige.
        Assert.Equal(new Haltbarkeit(360.5, 30), haltbarkeit);
    }

    [Fact]
    public void Erste_ihrer_Art_zaehlt_ab_der_Anschaffung_mit_Anfangskilometern()
    {
        Datenbestand bestand = Bestand();

        // 100 Anfangskilometer + 500 + 40; vom 01.01. bis zum 01.05. sind es 120 Tage
        Assert.Equal(new Haltbarkeit(640, 120), BerechneHaltbarkeit.Ausfuehren(bestand, Hole(bestand, 1)));
    }

    [Fact]
    public void Ohne_Anschaffungsdatum_ab_der_ersten_Einheit_mit_dem_Rad()
    {
        Datenbestand bestand = Bestand();
        bestand.Equipments[0].Anschaffungsdatum = null;

        // erste Einheit am 01.02.: bis zum 01.05. sind es 89 Tage
        Assert.Equal(new Haltbarkeit(640, 89), BerechneHaltbarkeit.Ausfuehren(bestand, Hole(bestand, 1)));
    }

    [Fact]
    public void Anschaffung_nach_der_Wartung_zaehlt_nicht()
    {
        Datenbestand bestand = Bestand();
        bestand.Equipments[0].Anschaffungsdatum = new DateOnly(2026, 6, 1);

        Assert.Equal(89, BerechneHaltbarkeit.Ausfuehren(bestand, Hole(bestand, 1)).Tage);
    }

    [Fact]
    public void Ohne_Anschaffung_und_ohne_Einheit_bis_zur_Wartung_bleiben_die_Tage_leer()
    {
        Datenbestand bestand = Bestand();
        bestand.Equipments[1].AnfangsdistanzKm = 50;

        // Rad 8 hat nur eine Einheit am 10.05.; eine Wartung am 01.05. hat davor keinen Anfangstag
        var wartung = new Wartung { Id = 9, Datum = new DateOnly(2026, 5, 1), EquipmentId = 8, Taetigkeit = "Reifen" };
        bestand.Wartungen.Add(wartung);

        Assert.Equal(new Haltbarkeit(50, null), BerechneHaltbarkeit.Ausfuehren(bestand, wartung));
    }

    [Fact]
    public void Gleicher_Tag_ergibt_bei_der_spaeter_erfassten_null_km_und_null_Tage()
    {
        Datenbestand bestand = Bestand();
        var doppelt = new Wartung { Id = 10, Datum = new DateOnly(2026, 5, 31), EquipmentId = 7, Taetigkeit = "Kettenwechsel" };
        bestand.Wartungen.Add(doppelt);

        Assert.Equal(new Haltbarkeit(0, 0), BerechneHaltbarkeit.Ausfuehren(bestand, doppelt));
        Assert.Equal(new Haltbarkeit(360.5, 30), BerechneHaltbarkeit.Ausfuehren(bestand, Hole(bestand, 5)));
    }

    [Fact]
    public void Ohne_Rad_bleiben_beide_Werte_leer()
    {
        Datenbestand bestand = Bestand();
        var wartung = new Wartung { Id = 11, Datum = new DateOnly(2026, 6, 1), Taetigkeit = "Kettenwechsel" };
        bestand.Wartungen.Add(wartung);

        Assert.Equal(new Haltbarkeit(null, null), BerechneHaltbarkeit.Ausfuehren(bestand, wartung));
    }
}
