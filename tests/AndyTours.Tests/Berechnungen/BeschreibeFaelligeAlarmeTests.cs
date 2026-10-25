using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BeschreibeFaelligeAlarme"/>.
/// </summary>
public class BeschreibeFaelligeAlarmeTests
{
    /// <summary>Zwei Räder (eines ausgemustert) mit je 1.200 km seit Jahresbeginn.</summary>
    private static Datenbestand Bestand(params Wartung[] alarme)
    {
        var bestand = new Datenbestand
        {
            Equipments =
            {
                new Equipment { Id = 7, Name = "Genius" },
                new Equipment { Id = 8, Name = "Stevens" },
                new Equipment { Id = 9, Name = "Alt", Ausgemustert = true },
            },
            Trainingseinheiten =
            {
                new Trainingseinheit { Datum = new DateOnly(2026, 2, 1), EquipmentId = 7, DistanzKm = 1200 },
                new Trainingseinheit { Datum = new DateOnly(2026, 2, 1), EquipmentId = 8, DistanzKm = 1200 },
                new Trainingseinheit { Datum = new DateOnly(2026, 2, 1), EquipmentId = 9, DistanzKm = 1200 },
            },
        };
        bestand.Wartungen.AddRange(alarme);
        return bestand;
    }

    /// <summary>Ein Alarm, angelegt am 01.01.2026.</summary>
    private static Wartung Alarm(long id, long rad, string taetigkeit, double intervall, bool aktiv = true) => new()
    {
        Id = id, Datum = new DateOnly(2026, 1, 1), EquipmentId = rad, Taetigkeit = taetigkeit,
        Typ = WartungsTyp.Alarm, AlarmKm = intervall, Aktiv = aktiv,
    };

    [Fact]
    public void Nennt_jeden_faelligen_Alarm_mit_Rad_Taetigkeit_und_Ueberschreitung()
    {
        string? text = BeschreibeFaelligeAlarme.Ausfuehren(Bestand(
            Alarm(1, 7, "Kettenwechsel", 1080),
            Alarm(2, 8, "Bremsbeläge", 1200),
            Alarm(3, 8, "Inspektion", 1250)));    // bald fällig: nicht genannt

        Assert.Equal("Folgende Wartungen sind fällig:\r\n\r\n• Genius – Kettenwechsel: 120 km überschritten\r\n• Stevens – Bremsbeläge: Intervall genau erreicht",
            text?.ReplaceLineEndings("\r\n"));
    }

    [Fact]
    public void Ohne_faellige_Alarme_kein_Hinweis_auch_nicht_ausgeschaltet_oder_ausgemustert()
    {
        Assert.Null(BeschreibeFaelligeAlarme.Ausfuehren(Bestand()));
        Assert.Null(BeschreibeFaelligeAlarme.Ausfuehren(Bestand(Alarm(1, 7, "Kettenwechsel", 100, aktiv: false), Alarm(2, 9, "Kettenwechsel", 100))));
    }

    [Fact]
    public void Ein_faelliger_Alarm_in_der_Einzahl()
    {
        Assert.StartsWith("Folgende Wartung ist fällig:", BeschreibeFaelligeAlarme.Ausfuehren(Bestand(Alarm(1, 7, "Kettenwechsel", 1000))));
    }
}
