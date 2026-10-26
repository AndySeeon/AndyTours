// WPF-Projekte binden System.Net.Http nicht automatisch ein
using System.Net.Http;
using System.Reflection;
using System.Text;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Schickt eine Anfrage an den Routingdienst der Tourenplanung und wandelt die Antwort in ein Teilstück um
/// (siehe <c>docs/adr/0004-routingdienst.md</c>).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Jede Anfrage trägt die Kennung „AndyTours/Version“.</item>
/// <item>Antwortet der Dienst nicht innerhalb von <see cref="Zeitlimit"/>, mit einem Fehler oder mit etwas Unlesbarem,
/// kommt <c>null</c> zurück; der Aufrufer zeichnet dann eine Luftlinie und zeigt einen Hinweis.</item>
/// <item>Ein bewusster Abbruch (die Planung hat sich inzwischen geändert) wirft
/// <see cref="OperationCanceledException"/>.</item>
/// <item>Die Kopfzeilen (mit dem Zugangsschlüssel) werden nirgends ausgegeben.</item>
/// </list>
/// Alle Planungsfenster teilen sich eine Verbindung, wie es für HttpClient empfohlen ist.
/// </remarks>
public static class Routingabruf
{
    /// <summary>Längste Wartezeit auf eine Antwort.</summary>
    private static readonly TimeSpan Zeitlimit = TimeSpan.FromSeconds(20);

    /// <summary>Gemeinsame Verbindung; lebt so lange wie die App.</summary>
    private static readonly HttpClient Verbindung = ErzeugeVerbindung();

    /// <summary>
    /// Fragt ein Teilstück beim Routingdienst ab.
    /// </summary>
    /// <param name="anfrage">Die fertige Anfrage (<see cref="BildeRoutinganfrage"/>).</param>
    /// <param name="abbruch">Bricht die Anfrage ab, wenn sie nicht mehr gebraucht wird.</param>
    /// <returns>Das ermittelte Teilstück oder <c>null</c>, wenn der Dienst es nicht geliefert hat.</returns>
    /// <exception cref="OperationCanceledException">Die Anfrage wurde über <paramref name="abbruch"/> abgebrochen.</exception>
    public static async Task<Teilstueck?> Ermittle(Routinganfrage anfrage, CancellationToken abbruch)
    {
        using var nachricht = new HttpRequestMessage(anfrage.Inhalt is null ? HttpMethod.Get : HttpMethod.Post, anfrage.Adresse);
        if (anfrage.Inhalt is not null)
        {
            nachricht.Content = new StringContent(anfrage.Inhalt, Encoding.UTF8, "application/json");
        }

        foreach ((string name, string wert) in anfrage.Kopfzeilen)
        {
            nachricht.Headers.TryAddWithoutValidation(name, wert);
        }

        try
        {
            using HttpResponseMessage antwort = await Verbindung.SendAsync(nachricht, abbruch).ConfigureAwait(false);
            if (!antwort.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await antwort.Content.ReadAsStringAsync(abbruch).ConfigureAwait(false);
            return anfrage.Dienst == Routingdienst.Openrouteservice
                ? LeseOpenrouteserviceAntwort.Ausfuehren(json)
                : LeseBRouterAntwort.Ausfuehren(json);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!abbruch.IsCancellationRequested)
        {
            // Zeitüberschreitung; ein bewusster Abbruch wird an den Aufrufer weitergegeben
            return null;
        }
    }

    /// <summary>Erzeugt die Verbindung mit Zeitlimit und der Kennung der App.</summary>
    private static HttpClient ErzeugeVerbindung()
    {
        var verbindung = new HttpClient { Timeout = Zeitlimit };
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0";
        verbindung.DefaultRequestHeaders.UserAgent.ParseAdd($"AndyTours/{version}");
        return verbindung;
    }
}
