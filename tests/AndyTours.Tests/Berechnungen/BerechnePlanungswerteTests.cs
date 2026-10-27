using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BerechnePlanungswerte"/>.
/// </summary>
public class BerechnePlanungswerteTests
{
    // Ein Grad Breite sind auf der Kugel mit 6371 km Radius rund 111,19 km
    private static readonly Geoposition A = new(48.0, 11.0);
    private static readonly Geoposition B = new(48.1, 11.0);
    private static readonly Geoposition C = new(48.2, 11.0);

    /// <summary>Teilstück von <paramref name="von"/> nach <paramref name="nach"/> mit den Höhen gleichmäßig verteilt.</summary>
    private static Teilstueck Stueck(Geoposition von, Geoposition nach, params double[] hoehen) =>
        new(
            [
                .. hoehen.Select((hoehe, i) => new Verlaufspunkt
                {
                    Hoehe = hoehe,
                    Position = new Geoposition(
                        von.Breite + (nach.Breite - von.Breite) * i / (hoehen.Length - 1),
                        von.Laenge + (nach.Laenge - von.Laenge) * i / (hoehen.Length - 1)),
                }),
            ],
            [], false);

    [Fact]
    public void Leere_Planung_hat_keine_Werte()
    {
        Assert.Equal(new Planungswerte(0, 0, 0), BerechnePlanungswerte.Ausfuehren(Tourenplanung.Leer(Fahrprofil.Trekking)));
    }

    [Fact]
    public void Laenge_ueber_alle_Teilstuecke()
    {
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A, B, C], [Stueck(A, B, 500, 500), Stueck(B, C, 500, 500)]);

        Assert.Equal(22.24, BerechnePlanungswerte.Ausfuehren(planung).LaengeKm, 2);
    }

    [Fact]
    public void Offenes_Teilstueck_zaehlt_als_gerade_Linie()
    {
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A, B, C], [Stueck(A, B, 500, 500), null]);

        Planungswerte werte = BerechnePlanungswerte.Ausfuehren(planung);

        Assert.Equal(22.24, werte.LaengeKm, 2);
    }

    [Fact]
    public void Hoehenmeter_bergauf_und_bergab_ueber_die_Teilstuecke_hinweg()
    {
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A, B, C],
            [Stueck(A, B, 500, 520, 560), Stueck(B, C, 560, 530, 540)]);

        Planungswerte werte = BerechnePlanungswerte.Ausfuehren(planung);

        Assert.Equal(70, werte.HoehenmeterBergauf);
        Assert.Equal(30, werte.HoehenmeterBergab);
    }

    [Fact]
    public void Kleine_Schwankungen_zaehlen_nicht()
    {
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A, B], [Stueck(A, B, 500, 501, 500, 502, 500, 501)]);

        Planungswerte werte = BerechnePlanungswerte.Ausfuehren(planung);

        Assert.Equal(0, werte.HoehenmeterBergauf);
        Assert.Equal(0, werte.HoehenmeterBergab);
    }

    [Fact]
    public void Luftlinie_zaehlt_mit_Laenge_aber_ohne_Hoehenmeter()
    {
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A, B], [ErstelleLuftlinie.Ausfuehren(A, B)]);

        Planungswerte werte = BerechnePlanungswerte.Ausfuehren(planung);

        Assert.Equal(11.12, werte.LaengeKm, 2);
        Assert.Equal(0, werte.HoehenmeterBergauf);
        Assert.Equal(0, werte.HoehenmeterBergab);
    }
}
