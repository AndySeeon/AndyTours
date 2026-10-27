using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="AlarmEingaben"/>.
/// </summary>
public class AlarmEingabenTests
{
    /// <summary>Ein Rad mit einem Alarm „Kettenwechsel“.</summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments = { new Equipment { Id = 7, Name = "Genius" }, new Equipment { Id = 8, Name = "Stevens" } },
        Wartungen =
        {
            new Wartung { Id = 40, Datum = new DateOnly(2026, 3, 1), EquipmentId = 7, Taetigkeit = "Kettenwechsel", Typ = WartungsTyp.Alarm, AlarmKm = 3000 },
        },
    };

    /// <summary>Ein neuer Alarm, angelegt am 25.10.2026.</summary>
    private static Wartung Neu() => new() { Datum = new DateOnly(2026, 10, 25), Typ = WartungsTyp.Alarm };

    /// <summary>Gültige Eingaben für einen neuen Alarm.</summary>
    private static Alarmeingaben Gueltig() => new() { EquipmentId = 7, Taetigkeit = " Inspektion ", Intervall = " 5000 ", Eingeschaltet = true };

    [Fact]
    public void Gueltige_Eingaben_ergeben_den_Alarm()
    {
        Alarmergebnis ergebnis = AlarmEingaben.AlsAlarm(Gueltig(), Neu(), Bestand());

        Wartung alarm = ergebnis.Alarm!;
        Assert.True(ergebnis.IstGueltig);
        Assert.Equal((0L, new DateOnly(2026, 10, 25), 7L, "Inspektion", WartungsTyp.Alarm, 5000.0, true),
            (alarm.Id, alarm.Datum, alarm.EquipmentId!.Value, alarm.Taetigkeit, alarm.Typ, alarm.AlarmKm, alarm.Aktiv));
    }

    [Fact]
    public void Hin_und_zurueck_bleiben_die_Werte_und_der_Tag_des_Anlegens()
    {
        Wartung vorhanden = Bestand().Wartungen[0];
        vorhanden.Aktiv = false;

        Alarmeingaben eingaben = AlarmEingaben.AlsEingaben(vorhanden);
        Wartung zurueck = AlarmEingaben.AlsAlarm(eingaben, vorhanden, Bestand()).Alarm!;

        Assert.Equal((7L, "Kettenwechsel", "3000", false), (eingaben.EquipmentId!.Value, eingaben.Taetigkeit, eingaben.Intervall, eingaben.Eingeschaltet));
        Assert.Equal((40L, new DateOnly(2026, 3, 1), 3000.0, false), (zurueck.Id, zurueck.Datum, zurueck.AlarmKm, zurueck.Aktiv));
    }

    [Fact]
    public void Neuer_Alarm_hat_ein_leeres_Intervall()
    {
        Assert.Equal(string.Empty, AlarmEingaben.AlsEingaben(Neu()).Intervall);
    }

    [Fact]
    public void Ohne_Rad_und_Taetigkeit_sind_beide_Felder_markiert()
    {
        Alarmergebnis ergebnis = AlarmEingaben.AlsAlarm(Gueltig() with { EquipmentId = null, Taetigkeit = "" }, Neu(), Bestand());

        Assert.Equal([Alarmfeld.Rad, Alarmfeld.Taetigkeit], ergebnis.Fehler.Order());
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("2,5")]
    [InlineData("viel")]
    public void Ungueltiges_Intervall_ist_markiert(string intervall)
    {
        Alarmergebnis ergebnis = AlarmEingaben.AlsAlarm(Gueltig() with { Intervall = intervall }, Neu(), Bestand());

        Assert.Equal([Alarmfeld.Intervall], ergebnis.Fehler);
    }

    [Fact]
    public void Zweiter_Alarm_fuer_Rad_und_Taetigkeit_ist_nicht_erlaubt()
    {
        Alarmergebnis ergebnis = AlarmEingaben.AlsAlarm(Gueltig() with { Taetigkeit = "KETTENWECHSEL" }, Neu(), Bestand());

        Assert.Equal([Alarmfeld.Taetigkeit], ergebnis.Fehler);
        Assert.Equal("Für ‚Genius‘ gibt es schon den Alarm ‚Kettenwechsel‘ (alle 3.000 km).", ergebnis.Hinweis);
    }

    [Fact]
    public void Gleiche_Taetigkeit_an_anderem_Rad_und_der_Alarm_selbst_sind_erlaubt()
    {
        Assert.True(AlarmEingaben.AlsAlarm(Gueltig() with { EquipmentId = 8, Taetigkeit = "Kettenwechsel" }, Neu(), Bestand()).IstGueltig);

        Wartung vorhanden = Bestand().Wartungen[0];
        Assert.True(AlarmEingaben.AlsAlarm(AlarmEingaben.AlsEingaben(vorhanden), vorhanden, Bestand()).IstGueltig);
    }
}
