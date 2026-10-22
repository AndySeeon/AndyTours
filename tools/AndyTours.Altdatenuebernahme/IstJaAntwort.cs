namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Wertet die Antwort auf die Rückfrage „Vorhandene Daten ersetzen? (j/N)“ aus.
/// </summary>
public static class IstJaAntwort
{
    /// <summary>
    /// Prüft, ob der Benutzer ausdrücklich mit Ja geantwortet hat.
    /// </summary>
    /// <remarks>
    /// Nur „j“ oder „ja“ (Groß-/Kleinschreibung egal, Leerzeichen am Rand egal) zählen als Ja.
    /// Alles andere – auch eine leere Eingabe oder das Ende der Eingabe (<c>null</c>, z. B. wenn das
    /// Programm aus einem Skript ohne Tastatur läuft) – gilt als Nein. So werden vorhandene Daten
    /// nie versehentlich ersetzt.
    /// <para>
    /// Ein Byte-Order-Mark (U+FEFF) am Anfang wird ebenfalls entfernt: Die PowerShell stellt es beim
    /// Weiterleiten einer Antwort (<c>'j' | AndyTours.Altdatenuebernahme</c>) der Eingabe voran, und
    /// <see cref="string.Trim()"/> zählt es nicht zu den Leerzeichen.
    /// </para>
    /// </remarks>
    /// <param name="eingabe">Die eingegebene Zeile oder <c>null</c>.</param>
    /// <returns><c>true</c> nur bei ausdrücklichem Ja.</returns>
    public static bool Ausfuehren(string? eingabe)
    {
        string antwort = (eingabe ?? string.Empty).Trim().Trim('﻿').Trim();
        return antwort.Equals("j", StringComparison.OrdinalIgnoreCase)
            || antwort.Equals("ja", StringComparison.OrdinalIgnoreCase);
    }
}
