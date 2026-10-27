using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BerechneAlarmstand"/>.
/// </summary>
public class BerechneAlarmstandTests
{
    /// <summary>Ein Alarm „Kettenwechsel“ am Rad 7, angelegt am 01.01.2026.</summary>
    private static Wartung Alarm(double intervall = 1000, bool aktiv = true) => new()
    {
        Id = 50, Datum = new DateOnly(2026, 1, 1), EquipmentId = 7, Taetigkeit = "Kettenwechsel",
        Typ = WartungsTyp.Alarm, AlarmKm = intervall, Aktiv = aktiv,
    };

    /// <summary>Zwei Räder; Einheiten vor und nach den Kettenwechseln, eine am Tag des letzten Wechsels.</summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments =
        {
            new Equipment { Id = 7, Name = "Genius" },
            new Equipment { Id = 8, Name = "Stevens" },
        },
        Trainingseinheiten =
        {
            new Trainingseinheit { Datum = new DateOnly(2026, 2, 1), EquipmentId = 7, DistanzKm = 500 },
            new Trainingseinheit { Datum = new DateOnly(2026, 5, 1), EquipmentId = 7, DistanzKm = 40 },   // Tag des Wechsels
            new Trainingseinheit { Datum = new DateOnly(2026, 5, 2), EquipmentId = 7, DistanzKm = 300 },
            new Trainingseinheit { Datum = new DateOnly(2026, 6, 1), EquipmentId = 7, DistanzKm = 200.5 },
            new Trainingseinheit { Datum = new DateOnly(2026, 6, 1), EquipmentId = 8, DistanzKm = 999 },  // anderes Rad
        },
        Wartungen =
        {
            new Wartung { Id = 1, Datum = new DateOnly(2026, 3, 1), EquipmentId = 7, Taetigkeit = "Kettenwechsel" },
            new Wartung { Id = 2, Datum = new DateOnly(2026, 5, 1), EquipmentId = 7, Taetigkeit = "kettenwechsel " },
            new Wartung { Id = 3, Datum = new DateOnly(2026, 5, 20), EquipmentId = 7, Taetigkeit = "Inspektion" },     // andere Tätigkeit
            new Wartung { Id = 4, Datum = new DateOnly(2026, 5, 25), EquipmentId = 8, Taetigkeit = "Kettenwechsel" },  // anderes Rad
        },
    };

    [Fact]
    public void Zaehlt_ab_der_letzten_gleichen_Wartung_ohne_deren_Tag()
    {
        Alarmstand stand = BerechneAlarmstand.Ausfuehren(Bestand(), Alarm());

        Assert.Equal(new DateOnly(2026, 5, 1), stand.Bezugstag);
        Assert.Equal(500.5, stand.GefahrenKm);
        Assert.Equal(499.5, stand.VerbleibendKm);
        Assert.Equal(Alarmzustand.Normal, stand.Zustand);
    }

    [Fact]
    public void Ohne_passende_Wartung_ab_dem_Tag_des_Anlegens()
    {
        Datenbestand bestand = Bestand();
        bestand.Wartungen.RemoveAll(w => w.Id is 1 or 2);

        Alarmstand stand = BerechneAlarmstand.Ausfuehren(bestand, Alarm());

        Assert.Equal(new DateOnly(2026, 1, 1), stand.Bezugstag);
        Assert.Equal(1040.5, stand.GefahrenKm);
        Assert.Equal(Alarmzustand.Faellig, stand.Zustand);
        Assert.Equal(-40.5, stand.VerbleibendKm);
    }

    [Fact]
    public void Ein_anderer_Alarm_ist_kein_Bezugspunkt()
    {
        Datenbestand bestand = Bestand();
        bestand.Wartungen.Add(new Wartung { Id = 9, Datum = new DateOnly(2026, 6, 1), EquipmentId = 7, Taetigkeit = "Kettenwechsel", Typ = WartungsTyp.Alarm });

        Assert.Equal(new DateOnly(2026, 5, 1), BerechneAlarmstand.Ausfuehren(bestand, Alarm()).Bezugstag);
    }

    [Theory]
    [InlineData(500.5, Alarmzustand.Faellig)]      // genau erreicht
    [InlineData(400, Alarmzustand.Faellig)]        // überschritten
    [InlineData(520, Alarmzustand.BaldFaellig)]    // 19,5 übrig ≤ 10 %
    [InlineData(600, Alarmzustand.Normal)]         // 99,5 übrig > 10 %
    public void Faellig_und_bald_faellig(double intervall, Alarmzustand erwartet)
    {
        Assert.Equal(erwartet, BerechneAlarmstand.Ausfuehren(Bestand(), Alarm(intervall)).Zustand);
    }

    [Theory]
    [InlineData(1000, Alarmzustand.BaldFaellig)]   // genau 10 % übrig (100 von 1.000)
    [InlineData(1001, Alarmzustand.Normal)]        // 101 von 1.001 übrig: etwas mehr als 10 %
    public void Grenze_von_zehn_Prozent(double intervall, Alarmzustand erwartet)
    {
        var bestand = new Datenbestand
        {
            Equipments = { new Equipment { Id = 7, Name = "Genius" } },
            Trainingseinheiten = { new Trainingseinheit { Datum = new DateOnly(2026, 2, 1), EquipmentId = 7, DistanzKm = 900 } },
        };

        Assert.Equal(erwartet, BerechneAlarmstand.Ausfuehren(bestand, Alarm(intervall)).Zustand);
    }

    [Fact]
    public void Ausgeschaltet_oder_ausgemustert_wird_nie_faellig()
    {
        Assert.Equal(Alarmzustand.Ausgeschaltet, BerechneAlarmstand.Ausfuehren(Bestand(), Alarm(100, aktiv: false)).Zustand);

        Datenbestand bestand = Bestand();
        bestand.Equipments[0].Ausgemustert = true;
        Assert.Equal(Alarmzustand.Ausgeschaltet, BerechneAlarmstand.Ausfuehren(bestand, Alarm(100)).Zustand);
    }
}
