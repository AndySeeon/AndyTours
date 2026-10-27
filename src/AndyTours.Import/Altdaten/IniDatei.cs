namespace AndyTours.Import.Altdaten;

/// <summary>
/// Inhalt einer INI-Datei, gegliedert nach Abschnitten.
/// </summary>
/// <remarks>
/// Je Abschnitt werden die Zeilen unverändert als Text abgelegt, weil <c>bikexperience.ini</c> zwei
/// Arten von Zeilen enthält: übliche Schlüssel-Wert-Paare (<c>weight=93</c>) und reine Datenzeilen
/// ohne Gleichheitszeichen (<c>Flach;0,3;170,0,0</c> in den Schwellwert-Abschnitten).
/// Schlüssel-Wert-Paare liefert <see cref="HoleIniSchluesselwerte"/>.
/// </remarks>
public class IniDatei
{
    /// <summary>
    /// Zeilen je Abschnitt; der Abschnittsname steht ohne eckige Klammern, Groß-/Kleinschreibung
    /// wird beim Nachschlagen ignoriert.
    /// </summary>
    public Dictionary<string, List<string>> Abschnitte { get; } = new(StringComparer.OrdinalIgnoreCase);
}
