namespace AndyTours.Core.Modell;

/// <summary>
/// Die persönlichen Daten des Sportlers, wie sie im Fenster „Persönliche Daten“ stehen.
/// </summary>
/// <remarks>
/// Alle Werte sind Text, weil der Benutzer auch Ungültiges tippen kann; geprüft und umgewandelt wird mit
/// <see cref="Berechnungen.FahrerEingaben.AlsFahrer"/>. Ein leeres Feld bedeutet „unbekannt“.
/// <para>
/// Nicht enthalten sind die Werte, die AndyTours hier nicht pflegt (Trainingspuls, Koordinaten des
/// Heimatorts); sie bleiben beim Speichern unverändert erhalten.
/// </para>
/// </remarks>
public record Fahrereingaben
{
    /// <summary>Vorname.</summary>
    public string Vorname { get; init; } = string.Empty;

    /// <summary>Nachname.</summary>
    public string Nachname { get; init; } = string.Empty;

    /// <summary>Geburtstag, z. B. „28.05.1963“; leer = unbekannt.</summary>
    public string Geburtstag { get; init; } = string.Empty;

    /// <summary>Körpergröße in Zentimetern, z. B. „182“; leer = unbekannt.</summary>
    public string Groesse { get; init; } = string.Empty;

    /// <summary>Körpergewicht in Kilogramm, z. B. „78,5“; leer = unbekannt.</summary>
    public string Gewicht { get; init; } = string.Empty;

    /// <summary>Ruhepuls in Schlägen pro Minute; leer = unbekannt.</summary>
    public string Ruhepuls { get; init; } = string.Empty;

    /// <summary>Maximalpuls in Schlägen pro Minute; leer = unbekannt.</summary>
    public string Maxpuls { get; init; } = string.Empty;

    /// <summary>Name des Heimatorts.</summary>
    public string Heimatort { get; init; } = string.Empty;
}
