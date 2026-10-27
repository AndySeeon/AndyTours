namespace AndyTours.Core.Modell;

/// <summary>
/// Ein benannter Wertebereich mit Farbe, z. B. der Pulsbereich „Grundlagenausdauer 1" von 109 bis 127.
/// </summary>
/// <remarks>
/// Schwellwerte dienen zum Einfärben von Höhenprofilen und für Auswertungen „Zeit in Zone".
/// Im Original steht je Bereich eine Zeile <c>Name;von,bis;R,G,B</c>.
/// Die Bereiche einer Art dürfen sich überschneiden (so ist es auch in den Altdaten).
/// </remarks>
public class Schwellwert
{
    /// <summary>Eindeutige Nummer in der Datenbank.</summary>
    public long Id { get; set; }

    /// <summary>Messgröße, für die der Bereich gilt.</summary>
    public SchwellwertArt Art { get; set; }

    /// <summary>Position innerhalb der Art (0 = erster Bereich), bestimmt die Anzeigereihenfolge.</summary>
    public int Reihenfolge { get; set; }

    /// <summary>Name des Bereichs.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Untergrenze des Bereichs (einschließlich).</summary>
    public double Von { get; set; }

    /// <summary>Obergrenze des Bereichs.</summary>
    public double Bis { get; set; }

    /// <summary>Anzeigefarbe als Hex-Text „#RRGGBB" (unabhängig von der Oberflächentechnik).</summary>
    public string Farbe { get; set; } = "#000000";
}
