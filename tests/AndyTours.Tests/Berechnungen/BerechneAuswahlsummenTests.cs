using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BerechneAuswahlsummen"/>.
/// </summary>
public class BerechneAuswahlsummenTests
{
    /// <summary>Erzeugt eine Einheit mit Datum, Aktivität, Distanz und Höhenmetern.</summary>
    private static Trainingseinheit Einheit(int jahr, long aktivitaetId, double km, int hm) =>
        new() { Datum = new DateOnly(jahr, 6, 1), AktivitaetId = aktivitaetId, DistanzKm = km, Hoehenmeter = hm, Fahrzeit = TimeSpan.FromHours(2) };

    [Fact]
    public void Anzahl_Kilometer_Hoehenmeter_und_Fahrzeit_werden_addiert()
    {
        Auswahlsumme summe = BerechneAuswahlsummen.Ausfuehren([Einheit(2026, 1, 77.83, 859), Einheit(2026, 1, 40.5, 300)]);

        Assert.Equal(new Auswahlsumme(2, 118.33, 1159, TimeSpan.FromHours(4)), summe);
    }

    [Fact]
    public void Leere_Auswahl_ergibt_null()
    {
        Assert.Equal(new Auswahlsumme(0, 0, 0, TimeSpan.Zero), BerechneAuswahlsummen.Ausfuehren([]));
    }

    [Fact]
    public void Kilometer_haben_keine_Rundungsreste()
    {
        // 0,1 + 0,2 ergibt bei double-Werten 0,30000000000000004
        Auswahlsumme summe = BerechneAuswahlsummen.Ausfuehren([Einheit(2026, 1, 0.1, 0), Einheit(2026, 1, 0.2, 0)]);

        Assert.Equal(0.3, summe.DistanzKm);
    }

    [Fact]
    public void Ein_Jahr_mit_allen_Aktivitaeten_entspricht_der_Jahressumme_im_Bericht()
    {
        List<Trainingseinheit> einheiten =
        [
            Einheit(2025, 1, 50.11, 500), Einheit(2025, 2, 10.27, 100),
            Einheit(2026, 1, 77.83, 859), Einheit(2026, 2, 12.34, 56), Einheit(2026, 1, 33.33, 333),
        ];

        // BerechneJahressummen erzeugt die Jahressummen im Umwandlungsbericht
        foreach (Jahressumme jahressumme in BerechneJahressummen.Ausfuehren(einheiten))
        {
            Auswahlsumme summe = BerechneAuswahlsummen.Ausfuehren(FiltereAuswahl.Ausfuehren(einheiten, jahressumme.Jahr, null));

            Assert.Equal(jahressumme.Anzahl, summe.Anzahl);
            Assert.Equal(jahressumme.DistanzKm, summe.DistanzKm);
            Assert.Equal(jahressumme.Hoehenmeter, summe.Hoehenmeter);
        }
    }
}
