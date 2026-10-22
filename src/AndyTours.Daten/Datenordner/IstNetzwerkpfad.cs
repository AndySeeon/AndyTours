namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Erkennt, ob ein Pfad auf einem Netzlaufwerk liegt (z. B. auf dem NAS).
/// </summary>
public static class IstNetzwerkpfad
{
    /// <summary>
    /// Prüft, ob der Pfad ein UNC-Pfad ist oder auf einem verbundenen Netzlaufwerk liegt.
    /// </summary>
    /// <remarks>
    /// Netzlaufwerke brauchen bei der Schreibprobe eine Pufferzeit (siehe
    /// <see cref="PruefeDatenordner"/>), weil manche NAS-Systeme Rechte und Dateien erst mit
    /// Verzögerung bestätigen.
    /// <list type="bullet">
    /// <item>UNC-Pfad: beginnt mit <c>\\Server\Freigabe</c> (auch <c>\\?\UNC\…</c>).</item>
    /// <item>Verbundenes Laufwerk (z. B. <c>Z:</c> → NAS): Windows meldet den Laufwerkstyp „Netzwerk“.</item>
    /// </list>
    /// </remarks>
    /// <param name="pfad">Zu prüfender Pfad (absolut oder relativ).</param>
    /// <returns><c>true</c> für Netzwerkpfade, sonst <c>false</c> (auch bei unbekannten oder fehlenden Laufwerken).</returns>
    public static bool Ausfuehren(string pfad)
    {
        string vollerPfad = Path.GetFullPath(pfad);

        // \\?\C:\… ist ein lokaler Pfad in Langschreibweise, \\?\UNC\Server\… dagegen ein Netzwerkpfad
        if (vollerPfad.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (vollerPfad.StartsWith(@"\\?\", StringComparison.Ordinal))
        {
            vollerPfad = vollerPfad[4..];
        }
        else if (vollerPfad.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return true;
        }

        string? laufwerk = Path.GetPathRoot(vollerPfad);
        if (string.IsNullOrEmpty(laufwerk))
        {
            return false;
        }

        try
        {
            return new DriveInfo(laufwerk).DriveType == DriveType.Network;
        }
        catch (ArgumentException)
        {
            // Kein gültiger Laufwerksbuchstabe – dann sicher kein Netzlaufwerk
            return false;
        }
    }
}
