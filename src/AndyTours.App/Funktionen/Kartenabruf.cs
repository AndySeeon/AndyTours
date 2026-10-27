// WPF-Projekte binden System.Net.Http nicht automatisch ein
using System.Net.Http;
using System.Reflection;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Verbindung der Streckenkarte ins Internet: liefert die Kennung der App für jede Anfrage nach einem
/// Kartenausschnitt und meldet, wenn ein Ausschnitt nicht geladen werden konnte.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Kennung: Die Nutzungsregeln von OpenStreetMap und OpenTopoMap verlangen eine eindeutige Kennung der App bei
/// jeder Anfrage; sie lautet „AndyTours/Version“. Die Kachelquelle setzt sie über <see cref="SetzeKennung"/>, denn
/// BruTile prüft sie schon beim Erzeugen der Anfrage und schickt eine Anfrage ohne Kennung gar nicht erst ab.</item>
/// <item>Ladefehler: Mapsui verschluckt Fehler beim Laden der Kartenausschnitte (keine Meldung, kein Ereignis).
/// Deshalb erkennt diese Klasse sie selbst: keine Verbindung, Zeitüberschreitung oder eine Fehlerantwort des
/// Servers lösen <see cref="Fehlgeschlagen"/> aus. Ausschnitte aus dem Zwischenspeicher kommen hier gar nicht an.</item>
/// </list>
/// Alle Tourdetail-Fenster teilen sich eine Verbindung (<see cref="Verbindung"/>), wie es für HttpClient empfohlen
/// ist. Das Ereignis kommt aus einem Hintergrund-Thread; die Oberfläche muss es selbst in ihren Thread holen. Wer
/// sich am Ereignis anmeldet, muss sich beim Schließen wieder abmelden.
/// </remarks>
public sealed class Kartenabruf : DelegatingHandler
{
    /// <summary>Kennung der App für die Kartenserver, z. B. „AndyTours/0.1.0“.</summary>
    private static readonly string Kennung =
        $"AndyTours/{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0"}";

    /// <summary>
    /// Erzeugt den Handler mit der normalen Internetverbindung des Rechners.
    /// </summary>
    private Kartenabruf()
        : base(new HttpClientHandler())
    {
    }

    /// <summary>Gemeinsame Verbindung aller Kartenebenen; lebt so lange wie die App.</summary>
    public static HttpClient Verbindung { get; } = new(new Kartenabruf());

    /// <summary>Wird ausgelöst, wenn ein Kartenausschnitt nicht geladen werden konnte.</summary>
    public static event EventHandler? Fehlgeschlagen;

    /// <summary>
    /// Setzt die Kennung der App in eine Anfrage nach einem Kartenausschnitt.
    /// </summary>
    /// <param name="anfrage">Die Anfrage, die die Kachelquelle gerade erzeugt.</param>
    public static void SetzeKennung(HttpRequestMessage anfrage) => anfrage.Headers.UserAgent.ParseAdd(Kennung);

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage antwort = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!antwort.IsSuccessStatusCode)
            {
                Fehlgeschlagen?.Invoke(this, EventArgs.Empty);
            }

            return antwort;
        }
        catch (HttpRequestException)
        {
            Fehlgeschlagen?.Invoke(this, EventArgs.Empty);
            throw;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Zeitüberschreitung; ein bewusster Abbruch (z. B. weil die Karte weiterbewegt wurde) zählt nicht
            Fehlgeschlagen?.Invoke(this, EventArgs.Empty);
            throw;
        }
    }
}
