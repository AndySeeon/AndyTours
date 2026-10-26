using System.Globalization;
using System.Text.Json;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bildet die Anfrage an den gewählten Routingdienst für ein Teilstück der Tourenplanung
/// (siehe <c>docs/adr/0004-routingdienst.md</c>).
/// </summary>
public static class BildeRoutinganfrage
{
    /// <summary>Adresse der Richtungsabfrage von openrouteservice ohne Profil und Format.</summary>
    private const string OpenrouteserviceAdresse = "https://api.openrouteservice.org/v2/directions/";

    /// <summary>Wege, die openrouteservice meiden soll: Fähren.</summary>
    private static readonly string[] GemiedeneWege = ["ferries"];

    /// <summary>
    /// Liefert die Anfrage für die Strecke von einem Wegpunkt zum nächsten.
    /// </summary>
    /// <remarks>
    /// Zuordnung der Fahrprofile:
    /// <list type="table">
    /// <item><term>Rennrad</term><description>BRouter <c>fastbike</c>, openrouteservice <c>cycling-road</c></description></item>
    /// <item><term>Trekking</term><description>BRouter <c>trekking</c>, openrouteservice <c>cycling-regular</c></description></item>
    /// <item><term>Mountainbike</term><description>BRouter <c>mtb</c>, openrouteservice <c>cycling-mountain</c></description></item>
    /// </list>
    /// BRouter wird per GET gefragt (GeoJSON, Abbiegehinweise mit <c>timode=3</c>); die Höhen liefert er immer mit.
    /// openrouteservice wird per POST mit Höhen und Fahranweisungen gefragt; der Schlüssel steht in der Kopfzeile
    /// <c>Authorization</c>.
    /// <para>
    /// Fähren werden bei beiden Diensten gemieden (BRouter <c>profile:allow_ferries=0</c>, openrouteservice
    /// <c>avoid_features: ["ferries"]</c>). Sonst führt die Strecke z. B. mit der Fähre quer über den Chiemsee, und auf
    /// der Karte erscheint eine gerade Linie über das Wasser.
    /// </para>
    /// </remarks>
    /// <param name="einstellungen">Die Routingeinstellungen.</param>
    /// <param name="profil">Das Fahrprofil der Planung.</param>
    /// <param name="von">Anfangswegpunkt.</param>
    /// <param name="nach">Endwegpunkt.</param>
    /// <returns>
    /// Die Anfrage oder <c>null</c>, wenn sie sich nicht bilden lässt: BRouter-Adresse ist keine http(s)-Adresse, oder
    /// für openrouteservice fehlt der Zugangsschlüssel.
    /// </returns>
    public static Routinganfrage? Ausfuehren(Routingeinstellungen einstellungen, Fahrprofil profil, Geoposition von, Geoposition nach) =>
        einstellungen.Dienst switch
        {
            Routingdienst.Openrouteservice => Openrouteservice(einstellungen.OpenrouteserviceSchluessel, profil, von, nach),
            _ => BRouter(einstellungen.BRouterAdresse, profil, von, nach),
        };

    /// <summary>GET-Anfrage an einen BRouter-Server.</summary>
    private static Routinganfrage? BRouter(string adresse, Fahrprofil profil, Geoposition von, Geoposition nach)
    {
        if (!Uri.TryCreate(adresse.Trim(), UriKind.Absolute, out Uri? basis) || (basis.Scheme != Uri.UriSchemeHttps && basis.Scheme != Uri.UriSchemeHttp))
        {
            return null;
        }

        string profilname = profil switch
        {
            Fahrprofil.Rennrad => "fastbike",
            Fahrprofil.Mountainbike => "mtb",
            _ => "trekking",
        };
        string abfrage = $"lonlats={Zahl(von.Laenge)},{Zahl(von.Breite)}%7C{Zahl(nach.Laenge)},{Zahl(nach.Breite)}"
            + $"&profile={profilname}&alternativeidx=0&format=geojson&timode=3&profile:allow_ferries=0";
        var ziel = new UriBuilder(basis) { Query = abfrage };
        return new Routinganfrage(Routingdienst.BRouter, ziel.Uri, null, new Dictionary<string, string>());
    }

    /// <summary>POST-Anfrage an openrouteservice.</summary>
    private static Routinganfrage? Openrouteservice(string? schluessel, Fahrprofil profil, Geoposition von, Geoposition nach)
    {
        if (string.IsNullOrWhiteSpace(schluessel))
        {
            return null;
        }

        string profilname = profil switch
        {
            Fahrprofil.Rennrad => "cycling-road",
            Fahrprofil.Mountainbike => "cycling-mountain",
            _ => "cycling-regular",
        };
        string inhalt = JsonSerializer.Serialize(new
        {
            coordinates = new[] { new[] { von.Laenge, von.Breite }, new[] { nach.Laenge, nach.Breite } },
            elevation = true,
            instructions = true,
            options = new { avoid_features = GemiedeneWege },
        });
        return new Routinganfrage(
            Routingdienst.Openrouteservice,
            new Uri($"{OpenrouteserviceAdresse}{profilname}/geojson"),
            inhalt,
            new Dictionary<string, string> { ["Authorization"] = schluessel.Trim() });
    }

    /// <summary>Zahl mit Punkt als Dezimaltrennzeichen und sechs Nachkommastellen (rund 10 cm).</summary>
    private static string Zahl(double wert) => wert.ToString("0.######", CultureInfo.InvariantCulture);
}
