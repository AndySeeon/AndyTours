namespace AndyTours.Daten;

/// <summary>
/// Schlüssel der Einträge in der Tabelle <c>Einstellung</c>.
/// </summary>
/// <remarks>
/// Die Schlüssel tragen den Bereich als Vorsilbe („Sicherung.…“), damit spätere Einstellungen
/// (z. B. persönliche Daten) eindeutig bleiben.
/// </remarks>
public static class Einstellungsschluessel
{
    /// <summary>Vom Benutzer gewählter Sicherungsordner; fehlt der Eintrag, gilt der Standardordner.</summary>
    public const string Sicherungsordner = "Sicherung.Ordner";

    /// <summary>Sicherungsintervall in Programmstarts.</summary>
    public const string Sicherungsintervall = "Sicherung.Intervall";

    /// <summary>Höchstzahl der aufbewahrten Sicherungen.</summary>
    public const string Sicherungshoechstzahl = "Sicherung.Hoechstzahl";

    /// <summary>Programmstarts seit der letzten erfolgreichen Sicherung.</summary>
    public const string StartsSeitSicherung = "Sicherung.StartsSeitSicherung";
}
