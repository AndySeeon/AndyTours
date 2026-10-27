using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErstelleAlarmzeilen"/>.
/// </summary>
public class ErstelleAlarmzeilenTests
{
    /// <summary>Zwei Räder mit je 1.000 km seit Jahresbeginn, vier Alarme in allen Zuständen und eine Wartung.</summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments =
        {
            new Equipment { Id = 7, Name = "Genius" },
            new Equipment { Id = 8, Name = "Stevens" },
        },
        Trainingseinheiten =
        {
            new Trainingseinheit { Datum = new DateOnly(2026, 2, 1), EquipmentId = 7, DistanzKm = 1000 },
            new Trainingseinheit { Datum = new DateOnly(2026, 2, 1), EquipmentId = 8, DistanzKm = 1000 },
        },
        Wartungen =
        {
            Alarm(1, 7, "Inspektion", 5000),                  // normal
            Alarm(2, 7, "Kettenwechsel", 800),                // fällig (−200)
            Alarm(3, 8, "Bremsbeläge", 1050),                 // bald fällig
            Alarm(4, 8, "Reifen HR", 900, aktiv: false),      // ausgeschaltet
            Alarm(5, 8, "Kettenwechsel", 900),                // fällig (−100)
            new Wartung { Id = 6, Datum = new DateOnly(2026, 1, 1), EquipmentId = 7, Taetigkeit = "Inspektion" },
        },
    };

    /// <summary>Ein Alarm, angelegt am 01.01.2026.</summary>
    private static Wartung Alarm(long id, long rad, string taetigkeit, double intervall, bool aktiv = true) => new()
    {
        Id = id, Datum = new DateOnly(2026, 1, 1), EquipmentId = rad, Taetigkeit = taetigkeit,
        Typ = WartungsTyp.Alarm, AlarmKm = intervall, Aktiv = aktiv,
    };

    [Fact]
    public void Faellige_zuerst_dann_bald_faellige_ausgeschaltete_am_Ende_ohne_Wartungen()
    {
        List<Alarmzeile> zeilen = ErstelleAlarmzeilen.Ausfuehren(Bestand(), null);

        Assert.Equal([2, 5, 3, 1, 4], zeilen.Select(z => z.Id));
        Assert.Equal(new Alarmzeile(2, "Genius", "Kettenwechsel", 800, 1000, -200, Alarmzustand.Faellig), zeilen[0]);
    }

    [Fact]
    public void Bei_allen_Equipments_nur_die_Alarme_der_Aktivitaet_auch_ausgemusterter()
    {
        Datenbestand bestand = Bestand();
        bestand.Equipments[0].AktivitaetId = 1;
        bestand.Equipments[1].AktivitaetId = 2;
        bestand.Equipments[1].Ausgemustert = true;

        Assert.Equal([2, 1], ErstelleAlarmzeilen.Ausfuehren(bestand, null, 1).Select(z => z.Id));
        // ausgemustert: alle ausgeschaltet, nach verbleibenden km
        Assert.Equal([4, 5, 3], ErstelleAlarmzeilen.Ausfuehren(bestand, null, 2).Select(z => z.Id));
        Assert.All(ErstelleAlarmzeilen.Ausfuehren(bestand, null, 2), z => Assert.Equal("Stevens (ausgemustert)", z.Rad));
        // ohne Aktivität alle (Titel der Karteikarte, Hinweis beim Programmstart)
        Assert.Equal(5, ErstelleAlarmzeilen.Ausfuehren(bestand, null).Count);
    }

    [Fact]
    public void Nur_die_Alarme_eines_Rads()
    {
        Assert.Equal([2, 1], ErstelleAlarmzeilen.Ausfuehren(Bestand(), 7).Select(z => z.Id));
    }

    [Fact]
    public void Ohne_Alarme_keine_Zeilen()
    {
        Datenbestand bestand = Bestand();
        bestand.Wartungen.RemoveAll(w => w.Typ == WartungsTyp.Alarm);

        Assert.Empty(ErstelleAlarmzeilen.Ausfuehren(bestand, null));
    }
}
