using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="UebernimmAbbiegungen"/>.
/// </summary>
public class UebernimmAbbiegungenTests
{
    /// <summary>Position in Metern nördlich und östlich eines festen Ursprungs.</summary>
    private static Geoposition P(double nord, double ost) =>
        new(48.0 + nord / 111_195, 11.0 + ost / (111_195 * Math.Cos(48.0 * Math.PI / 180)));

    /// <summary>Linie mit einem Punkt alle 10 m durch die angegebenen Ecken (Meter nördlich, östlich).</summary>
    private static List<Geoposition> Linie(params (double Nord, double Ost)[] ecken)
    {
        List<Geoposition> punkte = [P(ecken[0].Nord, ecken[0].Ost)];
        for (int e = 1; e < ecken.Length; e++)
        {
            (double n0, double o0) = ecken[e - 1];
            (double n1, double o1) = ecken[e];
            int schritte = (int)Math.Round(Math.Sqrt((n1 - n0) * (n1 - n0) + (o1 - o0) * (o1 - o0)) / 10);
            for (int s = 1; s <= schritte; s++)
            {
                punkte.Add(P(n0 + (n1 - n0) * s / schritte, o0 + (o1 - o0) * s / schritte));
            }
        }

        return punkte;
    }

    private static Teilstueck Berechnet(List<Geoposition> linie, params Abbiegepunkt[] abbiegepunkte) =>
        new([.. linie.Select(p => new Verlaufspunkt { Position = p })], abbiegepunkte, false);

    /// <summary>500 m nach Norden, dann 500 m nach Osten; die Ecke ist Punkt 50.</summary>
    private static readonly List<Geoposition> Winkel = Linie((0, 0), (500, 0), (500, 500));

    [Fact]
    public void Abbiegung_auf_der_Spur_kommt_an_den_naechsten_Spurpunkt()
    {
        Teilstueck berechnet = Berechnet(Winkel, new Abbiegepunkt(P(502, 3), Abbiegerichtung.Rechts));

        IReadOnlyList<Courseabbiegung> abbiegungen = UebernimmAbbiegungen.Ausfuehren(Winkel, [berechnet]);

        Assert.Equal([new Courseabbiegung(50, Abbiegerichtung.Rechts)], abbiegungen);
    }

    [Fact]
    public void Kreisverkehr_behaelt_seine_Ausfahrt()
    {
        Teilstueck berechnet = Berechnet(Winkel, new Abbiegepunkt(P(500, 0), Abbiegerichtung.Kreisverkehr, 3));

        Assert.Equal([new Courseabbiegung(50, Abbiegerichtung.Kreisverkehr, 3)], UebernimmAbbiegungen.Ausfuehren(Winkel, [berechnet]));
    }

    [Fact]
    public void Abseits_von_Wegen_keine_Abbiegung()
    {
        // Die Spur schneidet die Ecke quer übers Feld ab, der Dienst fährt außen herum
        List<Geoposition> spur = Linie((0, 0), (400, 0), (500, 100), (500, 500));
        Teilstueck berechnet = Berechnet(Winkel, new Abbiegepunkt(P(500, 0), Abbiegerichtung.Rechts));

        Assert.Empty(UebernimmAbbiegungen.Ausfuehren(spur, [berechnet]));
    }

    [Fact]
    public void Abweichung_kurz_nach_der_Abbiegung_verwirft_sie()
    {
        // An der Ecke liegt der Abbiegepunkt auf der Spur, aber gleich danach nimmt der Dienst einen anderen Weg
        List<Geoposition> spur = Linie((0, 0), (500, 0), (500, 500));
        List<Geoposition> umweg = Linie((0, 0), (500, 0), (500, 50), (560, 100), (500, 150), (500, 500));
        Teilstueck berechnet = Berechnet(umweg, new Abbiegepunkt(P(500, 0), Abbiegerichtung.Rechts));

        Assert.Empty(UebernimmAbbiegungen.Ausfuehren(spur, [berechnet]));
    }

    [Fact]
    public void Wendehinweise_entfallen()
    {
        Teilstueck berechnet = Berechnet(Winkel, new Abbiegepunkt(P(250, 0), Abbiegerichtung.Wenden));

        Assert.Empty(UebernimmAbbiegungen.Ausfuehren(Winkel, [berechnet]));
    }

    [Fact]
    public void Luftlinien_liefern_keine_Abbiegungen()
    {
        Teilstueck berechnet = Berechnet(Winkel, new Abbiegepunkt(P(500, 0), Abbiegerichtung.Rechts)) with { IstLuftlinie = true };

        Assert.Empty(UebernimmAbbiegungen.Ausfuehren(Winkel, [berechnet]));
    }

    [Fact]
    public void Doppelte_Abbiegung_am_selben_Spurpunkt_entfaellt()
    {
        // Die Ecke liegt am Rand zweier Pakete; beide melden sie
        Teilstueck erstes = Berechnet(Winkel[..52], new Abbiegepunkt(P(500, 0), Abbiegerichtung.Rechts));
        Teilstueck zweites = Berechnet(Winkel[49..], new Abbiegepunkt(P(500, 0), Abbiegerichtung.Rechts));

        Assert.Equal([new Courseabbiegung(50, Abbiegerichtung.Rechts)], UebernimmAbbiegungen.Ausfuehren(Winkel, [erstes, zweites]));
    }

    [Fact]
    public void Spur_ueber_dieselbe_Stelle_erhaelt_die_Abbiegungen_am_richtigen_Durchgang()
    {
        // Ein Rundkurs, der die Kreuzung bei (500, 0) zweimal passiert: erst nach Norden, später von Osten kommend
        List<Geoposition> spur = Linie((0, 0), (500, 0), (1000, 0), (1000, 500), (500, 500), (500, 0), (500, -500));
        Teilstueck berechnet = Berechnet(spur,
            new Abbiegepunkt(P(1000, 0), Abbiegerichtung.Rechts),
            new Abbiegepunkt(P(500, 0), Abbiegerichtung.Links));

        IReadOnlyList<Courseabbiegung> abbiegungen = UebernimmAbbiegungen.Ausfuehren(spur, [berechnet]);

        // Punkt 100 ist die Ecke oben links; die Kreuzung wird beim zweiten Mal bei Punkt 250 erreicht
        Assert.Equal([new Courseabbiegung(100, Abbiegerichtung.Rechts), new Courseabbiegung(250, Abbiegerichtung.Links)], abbiegungen);
    }
}
