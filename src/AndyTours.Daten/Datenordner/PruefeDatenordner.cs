namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Prüft einen Datenordner: erreichbar? beschreibbar? schon AndyTours-Daten darin?
/// </summary>
public static class PruefeDatenordner
{
    /// <summary>Höchstdauer der gesamten Prüfung, damit ein hängendes NAS die App nicht blockiert.</summary>
    public static readonly TimeSpan Zeitlimit = TimeSpan.FromSeconds(20);

    /// <summary>Netzlaufwerk: Wartezeit zwischen Schreiben und Zurücklesen der Probedatei.</summary>
    public static readonly TimeSpan NetzwerkPufferzeit = TimeSpan.FromSeconds(1);

    /// <summary>Netzlaufwerk: Anzahl der Schreibversuche insgesamt.</summary>
    public const int NetzwerkVersuche = 3;

    /// <summary>Netzlaufwerk: Pause zwischen zwei Schreibversuchen.</summary>
    public static readonly TimeSpan NetzwerkPause = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Legt einen fehlenden Ordner an und ermittelt seinen <see cref="DatenordnerZustand"/>.
    /// </summary>
    /// <remarks>
    /// Ablauf:
    /// <list type="number">
    /// <item>Ordner anlegen (ist er schon vorhanden, passiert nichts). Scheitert das, ist er
    /// <see cref="DatenordnerZustand.NichtErreichbar"/> – außer der übergeordnete Ordner existiert,
    /// dann fehlt nur das Schreibrecht (<see cref="DatenordnerZustand.NichtBeschreibbar"/>).</item>
    /// <item>Schreibprobe; auf Netzlaufwerken mit Pufferzeit und Wiederholungen (Werte siehe oben),
    /// lokal ein Versuch ohne Wartezeit.</item>
    /// <item>Liegt die Datenbankdatei im Ordner: <see cref="DatenordnerZustand.MitDaten"/>, sonst
    /// <see cref="DatenordnerZustand.Leer"/>.</item>
    /// </list>
    /// Die Prüfung läuft im Hintergrund. Dauert sie länger als das Zeitlimit (z. B. weil Windows
    /// beim Zugriff auf ein ausgeschaltetes NAS hängt), gilt der Ordner als nicht erreichbar.
    /// </remarks>
    /// <param name="ordner">Vollständiger Pfad des Datenordners.</param>
    /// <param name="zeitlimit">Höchstdauer der Prüfung; <c>null</c> = <see cref="Zeitlimit"/>.</param>
    /// <returns>Der ermittelte Zustand.</returns>
    public static async Task<DatenordnerZustand> Ausfuehren(string ordner, TimeSpan? zeitlimit = null)
    {
        using var abbruch = new CancellationTokenSource();
        Task<DatenordnerZustand> pruefung = Task.Run(() => Pruefen(ordner, abbruch.Token));

        try
        {
            return await pruefung.WaitAsync(zeitlimit ?? Zeitlimit).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            // Die Hintergrundprüfung abbrechen (soweit sie gerade wartet) und nicht länger auf sie warten
            await abbruch.CancelAsync().ConfigureAwait(false);
            return DatenordnerZustand.NichtErreichbar;
        }
    }

    /// <summary>Die eigentliche Prüfung (läuft im Hintergrund).</summary>
    private static async Task<DatenordnerZustand> Pruefen(string ordner, CancellationToken abbruch)
    {
        try
        {
            Directory.CreateDirectory(ordner);
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException
                                           or ArgumentException or NotSupportedException)
        {
            // Übergeordneter Ordner vorhanden → nur das Anlegen ist verboten; sonst ist der Ort nicht erreichbar
            string? uebergeordnet = Path.GetDirectoryName(Path.GetFullPath(ordner));
            return fehler is UnauthorizedAccessException && uebergeordnet is not null && Directory.Exists(uebergeordnet)
                ? DatenordnerZustand.NichtBeschreibbar
                : DatenordnerZustand.NichtErreichbar;
        }

        bool netzwerk = IstNetzwerkpfad.Ausfuehren(ordner);
        bool beschreibbar;
        try
        {
            beschreibbar = await FuehreSchreibprobeDurch.Ausfuehren(ordner,
                netzwerk ? NetzwerkPufferzeit : TimeSpan.Zero,
                netzwerk ? NetzwerkVersuche : 1,
                netzwerk ? NetzwerkPause : TimeSpan.Zero,
                abbruch).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return DatenordnerZustand.NichtErreichbar;
        }

        if (!beschreibbar)
        {
            return DatenordnerZustand.NichtBeschreibbar;
        }

        return File.Exists(Path.Combine(ordner, DatenordnerInhalt.Datenbankdatei))
            ? DatenordnerZustand.MitDaten
            : DatenordnerZustand.Leer;
    }
}
