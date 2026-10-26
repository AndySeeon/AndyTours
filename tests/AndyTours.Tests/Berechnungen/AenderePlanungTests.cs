using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="AenderePlanung"/>.
/// </summary>
public class AenderePlanungTests
{
    private static readonly Geoposition A = new(48.0, 11.0);
    private static readonly Geoposition B = new(48.1, 11.1);
    private static readonly Geoposition C = new(48.2, 11.2);
    private static readonly Geoposition D = new(48.3, 11.3);

    /// <summary>Ein ermitteltes Teilstück; die Kennung (als Höhe) macht es in den Tests unterscheidbar.</summary>
    private static Teilstueck Stueck(int kennung) =>
        new([new Verlaufspunkt { Hoehe = kennung }, new Verlaufspunkt { Hoehe = kennung }], [], false);

    /// <summary>Kennungen der Teilstücke; <c>null</c> für ein noch zu ermittelndes.</summary>
    private static List<double?> Kennungen(Tourenplanung planung) =>
        [.. planung.Teilstuecke.Select(t => t?.Punkte[0].Hoehe)];

    /// <summary>Planung A–B–C mit den ermittelten Teilstücken 1 und 2.</summary>
    private static Tourenplanung Dreipunkte() =>
        new(Fahrprofil.Trekking, [A, B, C], [Stueck(1), Stueck(2)]);

    [Fact]
    public void Erster_Punkt_ergibt_kein_Teilstueck()
    {
        Tourenplanung planung = AenderePlanung.Anhaengen(Tourenplanung.Leer(Fahrprofil.Rennrad), A);

        Assert.Equal([A], planung.Wegpunkte);
        Assert.Empty(planung.Teilstuecke);
        Assert.Equal(Fahrprofil.Rennrad, planung.Profil);
    }

    [Fact]
    public void Anhaengen_setzt_neues_Ziel_mit_offenem_Teilstueck()
    {
        Tourenplanung planung = AenderePlanung.Anhaengen(Dreipunkte(), D);

        Assert.Equal([A, B, C, D], planung.Wegpunkte);
        Assert.Equal([1, 2, null], Kennungen(planung));
    }

    [Fact]
    public void Einfuegen_teilt_das_Teilstueck_in_zwei_offene()
    {
        Tourenplanung planung = AenderePlanung.Einfuegen(Dreipunkte(), 2, D);

        Assert.Equal([A, B, D, C], planung.Wegpunkte);
        Assert.Equal([1, null, null], Kennungen(planung));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Einfuegen_nur_zwischen_zwei_Wegpunkten(int nummer)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AenderePlanung.Einfuegen(Dreipunkte(), nummer, D));
    }

    [Fact]
    public void Verschieben_oeffnet_nur_die_angrenzenden_Teilstuecke()
    {
        Tourenplanung planung = AenderePlanung.Verschieben(AenderePlanung.Anhaengen(Dreipunkte(), D), 1, D);

        Assert.Equal([A, D, C, D], planung.Wegpunkte);
        Assert.Equal([null, null, null], Kennungen(planung));
    }

    [Fact]
    public void Verschieben_des_Starts_oeffnet_nur_das_erste_Teilstueck()
    {
        Tourenplanung planung = AenderePlanung.Verschieben(Dreipunkte(), 0, D);

        Assert.Equal([D, B, C], planung.Wegpunkte);
        Assert.Equal([null, 2], Kennungen(planung));
    }

    [Fact]
    public void Verschieben_des_Ziels_oeffnet_nur_das_letzte_Teilstueck()
    {
        Tourenplanung planung = AenderePlanung.Verschieben(Dreipunkte(), 2, D);

        Assert.Equal([A, B, D], planung.Wegpunkte);
        Assert.Equal([1, null], Kennungen(planung));
    }

    [Fact]
    public void Entfernen_in_der_Mitte_verbindet_die_Nachbarn_mit_einem_offenen_Teilstueck()
    {
        Tourenplanung planung = AenderePlanung.Entfernen(Dreipunkte(), 1);

        Assert.Equal([A, C], planung.Wegpunkte);
        Assert.Equal([null], Kennungen(planung));
    }

    [Fact]
    public void Entfernen_des_Starts_laesst_das_uebrige_Teilstueck()
    {
        Tourenplanung planung = AenderePlanung.Entfernen(Dreipunkte(), 0);

        Assert.Equal([B, C], planung.Wegpunkte);
        Assert.Equal([2], Kennungen(planung));
    }

    [Fact]
    public void Letzten_Punkt_entfernen_nimmt_Ziel_und_sein_Teilstueck()
    {
        Tourenplanung planung = AenderePlanung.EntferneLetzten(Dreipunkte());

        Assert.Equal([A, B], planung.Wegpunkte);
        Assert.Equal([1], Kennungen(planung));
    }

    [Fact]
    public void Letzten_Punkt_entfernen_bis_die_Planung_leer_ist()
    {
        Tourenplanung planung = Dreipunkte();
        for (int i = 0; i < 4; i++)
        {
            planung = AenderePlanung.EntferneLetzten(planung);
        }

        Assert.Empty(planung.Wegpunkte);
        Assert.Empty(planung.Teilstuecke);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void Unbekannter_Wegpunkt_wirft_eine_Ausnahme(int nummer)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AenderePlanung.Entfernen(Dreipunkte(), nummer));
        Assert.Throws<ArgumentOutOfRangeException>(() => AenderePlanung.Verschieben(Dreipunkte(), nummer, D));
    }

    [Fact]
    public void Profilwechsel_oeffnet_alle_Teilstuecke_und_laesst_die_Wegpunkte()
    {
        Tourenplanung planung = AenderePlanung.WechsleProfil(Dreipunkte(), Fahrprofil.Mountainbike);

        Assert.Equal(Fahrprofil.Mountainbike, planung.Profil);
        Assert.Equal([A, B, C], planung.Wegpunkte);
        Assert.Equal([null, null], Kennungen(planung));
    }

    [Fact]
    public void Ermitteltes_Teilstueck_wird_eingetragen()
    {
        Tourenplanung offen = AenderePlanung.Anhaengen(Dreipunkte(), D);

        Tourenplanung planung = AenderePlanung.SetzeTeilstueck(offen, 2, Stueck(3));

        Assert.Equal([1, 2, 3], Kennungen(planung));
    }

    [Fact]
    public void Aenderungen_lassen_die_alte_Planung_unveraendert()
    {
        Tourenplanung alt = Dreipunkte();

        AenderePlanung.Verschieben(alt, 1, D);
        AenderePlanung.Entfernen(alt, 1);
        AenderePlanung.Einfuegen(alt, 1, D);

        Assert.Equal([A, B, C], alt.Wegpunkte);
        Assert.Equal([1, 2], Kennungen(alt));
    }
}
