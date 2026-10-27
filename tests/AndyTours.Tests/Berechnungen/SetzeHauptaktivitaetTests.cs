using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="SetzeHauptaktivitaet"/>.
/// </summary>
public class SetzeHauptaktivitaetTests
{
    [Fact]
    public void Genau_die_gewaehlte_Aktivitaet_ist_Hauptaktivitaet()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        SetzeHauptaktivitaet.Ausfuehren(bestand.Aktivitaeten, 2);

        Assert.Equal([2L], bestand.Aktivitaeten.Where(a => a.IstHaupt).Select(a => a.Id));
    }

    [Fact]
    public void Neue_Einheit_erhaelt_die_neue_Hauptaktivitaet()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        SetzeHauptaktivitaet.Ausfuehren(bestand.Aktivitaeten, 3);

        Assert.Equal(3, ErstelleNeueEinheit.Ausfuehren(bestand, null, new DateOnly(2026, 10, 24)).AktivitaetId);
    }
}
