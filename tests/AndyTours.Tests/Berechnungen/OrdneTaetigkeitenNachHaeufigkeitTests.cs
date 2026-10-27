using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="OrdneTaetigkeitenNachHaeufigkeit"/>.
/// </summary>
public class OrdneTaetigkeitenNachHaeufigkeitTests
{
    /// <summary>
    /// Dreimal „Kettenwechsel“ (zwei Schreibweisen, zwei Räder), einmal „Bremsbelag HR“, dazu zwei Alarme
    /// „Reifen VR“, die nicht mitzählen.
    /// </summary>
    private static List<Wartung> Wartungen() =>
    [
        new() { Id = 1, EquipmentId = 1, Taetigkeit = "Kettenwechsel" },
        new() { Id = 2, EquipmentId = 2, Taetigkeit = "kettenwechsel " },
        new() { Id = 3, EquipmentId = 1, Taetigkeit = "Kettenwechsel" },
        new() { Id = 4, EquipmentId = 2, Taetigkeit = "Bremsbelag HR" },
        new() { Id = 5, EquipmentId = 1, Taetigkeit = "Reifen VR", Typ = WartungsTyp.Alarm, AlarmKm = 3000 },
        new() { Id = 6, EquipmentId = 2, Taetigkeit = "Reifen VR", Typ = WartungsTyp.Alarm, AlarmKm = 3000 },
    ];

    [Fact]
    public void Haeufigste_Taetigkeit_ueber_alle_Raeder_zuerst()
    {
        List<string> ergebnis = OrdneTaetigkeitenNachHaeufigkeit.Ausfuehren(
            ["Bremsbelag HR", "Dämpfer eingestellt", "Kettenwechsel"], Wartungen());

        Assert.Equal(["Kettenwechsel", "Bremsbelag HR", "Dämpfer eingestellt"], ergebnis);
    }

    [Fact]
    public void Gleichstand_behaelt_Reihenfolge_der_Liste()
    {
        // „Reifen VR“ und „Dämpfer eingestellt“ kommen in keiner Wartung vor (Alarme zählen nicht)
        List<string> ergebnis = OrdneTaetigkeitenNachHaeufigkeit.Ausfuehren(
            ["Reifen VR", "Kettenwechsel", "Dämpfer eingestellt"], Wartungen());

        Assert.Equal(["Kettenwechsel", "Reifen VR", "Dämpfer eingestellt"], ergebnis);
    }

    [Fact]
    public void Ohne_Wartungen_bleibt_die_Liste_unveraendert()
    {
        List<string> ergebnis = OrdneTaetigkeitenNachHaeufigkeit.Ausfuehren(["Reifen VR", "Kettenwechsel"], []);

        Assert.Equal(["Reifen VR", "Kettenwechsel"], ergebnis);
    }
}
