using System.Text.Json;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BildeRoutinganfrage"/>.
/// </summary>
public class BildeRoutinganfrageTests
{
    private static readonly Geoposition Von = new(48.1374, 11.5755);
    private static readonly Geoposition Nach = new(48.15, 11.62);

    /// <summary>Erfundener Testschlüssel.</summary>
    private const string Schluessel = "test-schluessel";

    private static Routingeinstellungen Openrouteservice(string? schluessel = Schluessel) =>
        new(Routingdienst.Openrouteservice, Routingeinstellungen.StandardBRouterAdresse, schluessel);

    [Theory]
    [InlineData(Fahrprofil.Rennrad, "fastbike")]
    [InlineData(Fahrprofil.Trekking, "trekking")]
    [InlineData(Fahrprofil.Mountainbike, "mtb")]
    public void BRouter_bekommt_das_passende_Profil(Fahrprofil profil, string profilname)
    {
        Routinganfrage anfrage = BildeRoutinganfrage.Ausfuehren(Routingeinstellungen.Standard, profil, Von, Nach)!;

        Assert.Equal(Routingdienst.BRouter, anfrage.Dienst);
        Assert.Null(anfrage.Inhalt);
        Assert.Empty(anfrage.Kopfzeilen);
        Assert.Equal(
            $"https://brouter.de/brouter?lonlats=11.5755,48.1374%7C11.62,48.15&profile={profilname}&alternativeidx=0&format=geojson&timode=3&profile:allow_ferries=0",
            anfrage.Adresse.AbsoluteUri);
    }

    [Fact]
    public void Eigene_BRouter_Adresse_wird_verwendet()
    {
        var einstellungen = new Routingeinstellungen(Routingdienst.BRouter, " http://localhost:17777/brouter ", null);

        Routinganfrage anfrage = BildeRoutinganfrage.Ausfuehren(einstellungen, Fahrprofil.Trekking, Von, Nach)!;

        Assert.StartsWith("http://localhost:17777/brouter?lonlats=", anfrage.Adresse.AbsoluteUri);
    }

    [Theory]
    [InlineData("")]
    [InlineData("brouter.de/brouter")]
    [InlineData("ftp://brouter.de/brouter")]
    public void Ungueltige_BRouter_Adresse_ergibt_keine_Anfrage(string adresse)
    {
        var einstellungen = new Routingeinstellungen(Routingdienst.BRouter, adresse, null);

        Assert.Null(BildeRoutinganfrage.Ausfuehren(einstellungen, Fahrprofil.Trekking, Von, Nach));
    }

    [Theory]
    [InlineData(Fahrprofil.Rennrad, "cycling-road")]
    [InlineData(Fahrprofil.Trekking, "cycling-regular")]
    [InlineData(Fahrprofil.Mountainbike, "cycling-mountain")]
    public void Openrouteservice_bekommt_das_passende_Profil(Fahrprofil profil, string profilname)
    {
        Routinganfrage anfrage = BildeRoutinganfrage.Ausfuehren(Openrouteservice(), profil, Von, Nach)!;

        Assert.Equal(Routingdienst.Openrouteservice, anfrage.Dienst);
        Assert.Equal($"https://api.openrouteservice.org/v2/directions/{profilname}/geojson", anfrage.Adresse.AbsoluteUri);
    }

    [Fact]
    public void Openrouteservice_mit_Schluessel_Hoehen_und_Anweisungen()
    {
        Routinganfrage anfrage = BildeRoutinganfrage.Ausfuehren(Openrouteservice(), Fahrprofil.Trekking, Von, Nach)!;

        Assert.Equal(Schluessel, anfrage.Kopfzeilen["Authorization"]);
        using JsonDocument inhalt = JsonDocument.Parse(anfrage.Inhalt!);
        JsonElement wurzel = inhalt.RootElement;
        Assert.True(wurzel.GetProperty("elevation").GetBoolean());
        Assert.True(wurzel.GetProperty("instructions").GetBoolean());
        JsonElement[] koordinaten = [.. wurzel.GetProperty("coordinates").EnumerateArray()];
        Assert.Equal([11.5755, 48.1374], koordinaten[0].EnumerateArray().Select(z => z.GetDouble()));
        Assert.Equal([11.62, 48.15], koordinaten[1].EnumerateArray().Select(z => z.GetDouble()));
        Assert.Equal(["ferries"], wurzel.GetProperty("options").GetProperty("avoid_features").EnumerateArray().Select(f => f.GetString()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Openrouteservice_ohne_Schluessel_ergibt_keine_Anfrage(string? schluessel)
    {
        Assert.Null(BildeRoutinganfrage.Ausfuehren(Openrouteservice(schluessel), Fahrprofil.Trekking, Von, Nach));
    }

    [Fact]
    public void BRouter_ueber_mehrere_Punkte()
    {
        Routinganfrage anfrage = BildeRoutinganfrage.Ausfuehren(Routingeinstellungen.Standard, Fahrprofil.Mountainbike,
            [Von, new Geoposition(48.14, 11.6), Nach])!;

        Assert.StartsWith("https://brouter.de/brouter?lonlats=11.5755,48.1374%7C11.6,48.14%7C11.62,48.15&profile=mtb&",
            anfrage.Adresse.AbsoluteUri);
    }

    [Fact]
    public void Openrouteservice_ueber_mehrere_Punkte()
    {
        Routinganfrage anfrage = BildeRoutinganfrage.Ausfuehren(Openrouteservice(), Fahrprofil.Trekking,
            [Von, new Geoposition(48.14, 11.6), Nach])!;

        using JsonDocument inhalt = JsonDocument.Parse(anfrage.Inhalt!);
        JsonElement[] koordinaten = [.. inhalt.RootElement.GetProperty("coordinates").EnumerateArray()];
        Assert.Equal(3, koordinaten.Length);
        Assert.Equal([11.6, 48.14], koordinaten[1].EnumerateArray().Select(z => z.GetDouble()));
        Assert.True(inhalt.RootElement.GetProperty("instructions").GetBoolean());
    }

    [Fact]
    public void Schluessel_erscheint_nicht_im_Text_der_Einstellungen_und_der_Anfrage()
    {
        Routinganfrage anfrage = BildeRoutinganfrage.Ausfuehren(Openrouteservice(), Fahrprofil.Trekking, Von, Nach)!;

        Assert.DoesNotContain(Schluessel, Openrouteservice().ToString());
        Assert.DoesNotContain(Schluessel, anfrage.ToString());
    }
}
