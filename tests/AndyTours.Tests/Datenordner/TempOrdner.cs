namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Legt für einen Test einen leeren, eindeutigen Ordner im Temp-Verzeichnis an und löscht ihn am Ende wieder.
/// </summary>
/// <remarks>
/// Verwendung: <c>using var temp = new TempOrdner();</c> – beim Verlassen des Tests wird
/// <see cref="Dispose"/> aufgerufen und der Ordner samt Inhalt entfernt.
/// </remarks>
public sealed class TempOrdner : IDisposable
{
    /// <summary>Erzeugt den Ordner.</summary>
    public TempOrdner()
    {
        Pfad = Path.Combine(Path.GetTempPath(), "AndyToursTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Pfad);
    }

    /// <summary>Vollständiger Pfad des Ordners.</summary>
    public string Pfad { get; }

    /// <summary>Löscht den Ordner samt Inhalt; Fehler beim Aufräumen werden ignoriert.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(Pfad, recursive: true);
        }
        catch (IOException)
        {
            // Aufräumen ist nicht Gegenstand des Tests
        }
        catch (UnauthorizedAccessException)
        {
            // dto. (z. B. nach einem Test mit verweigertem Schreibrecht)
        }
    }
}
