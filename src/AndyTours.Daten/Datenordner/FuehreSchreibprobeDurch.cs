namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Prüft durch einen echten Schreibversuch, ob die App in einem Ordner schreiben darf.
/// </summary>
public static class FuehreSchreibprobeDurch
{
    /// <summary>
    /// Schreibt eine kleine Probedatei, liest sie nach der Pufferzeit zurück, vergleicht den
    /// Inhalt und löscht sie wieder. Schlägt das fehl, wird es nach einer Pause wiederholt.
    /// </summary>
    /// <remarks>
    /// Warum ein echter Schreibversuch? Die Zugriffsrechte eines Ordners lassen sich – besonders
    /// auf einem NAS – nicht zuverlässig im Voraus abfragen. Warum Zurücklesen nach einer
    /// Pufferzeit? Manche Netzlaufwerke nehmen eine Datei zunächst an und verwerfen sie erst
    /// danach (verzögerte Rechteprüfung, Zwischenspeicher). Der Vergleich stellt sicher, dass
    /// die Datei wirklich angekommen ist.
    /// <para>
    /// Die Methode arbeitet asynchron (<c>Task</c>), damit die Wartezeiten die Oberfläche
    /// nicht blockieren.
    /// </para>
    /// </remarks>
    /// <param name="ordner">Der zu prüfende, bereits vorhandene Ordner.</param>
    /// <param name="pufferzeit">Wartezeit zwischen Schreiben und Zurücklesen (lokal: 0).</param>
    /// <param name="versuche">Anzahl der Versuche insgesamt (mindestens 1).</param>
    /// <param name="pause">Pause zwischen zwei Versuchen.</param>
    /// <param name="abbruch">Ermöglicht den Abbruch, z. B. bei Überschreitung eines Zeitlimits.</param>
    /// <returns><c>true</c>, wenn ein Versuch gelungen ist; <c>false</c>, wenn alle fehlgeschlagen sind.</returns>
    public static async Task<bool> Ausfuehren(string ordner, TimeSpan pufferzeit, int versuche, TimeSpan pause,
        CancellationToken abbruch = default)
    {
        for (int versuch = 1; versuch <= Math.Max(1, versuche); versuch++)
        {
            if (versuch > 1)
            {
                await Task.Delay(pause, abbruch).ConfigureAwait(false);
            }

            if (await EinVersuch(ordner, pufferzeit, abbruch).ConfigureAwait(false))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Ein einzelner Schreib-, Lese- und Löschversuch.</summary>
    private static async Task<bool> EinVersuch(string ordner, TimeSpan pufferzeit, CancellationToken abbruch)
    {
        // Eindeutiger Name, damit sich gleichzeitige Proben (oder Reste alter Proben) nicht stören
        string probedatei = Path.Combine(ordner, $".andytours_schreibprobe_{Guid.NewGuid():N}.tmp");
        string inhalt = Guid.NewGuid().ToString();

        try
        {
            await File.WriteAllTextAsync(probedatei, inhalt, abbruch).ConfigureAwait(false);
            await Task.Delay(pufferzeit, abbruch).ConfigureAwait(false);

            string gelesen = await File.ReadAllTextAsync(probedatei, abbruch).ConfigureAwait(false);
            return gelesen == inhalt;
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            return false;
        }
        finally
        {
            // Probedatei in jedem Fall wieder entfernen; gelingt das nicht, bleibt sie als kleine .tmp-Datei liegen.
            try
            {
                File.Delete(probedatei);
            }
            catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
            {
                // bewusst ignoriert
            }
        }
    }
}
