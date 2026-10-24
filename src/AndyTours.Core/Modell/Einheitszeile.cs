namespace AndyTours.Core.Modell;

/// <summary>
/// Eine Zeile der Tabelle „Trainingseinheiten“: die Werte einer Einheit, wie sie angezeigt werden.
/// </summary>
/// <remarks>
/// Erzeugt von <see cref="Berechnungen.ErstelleEinheitszeilen"/>. Die Werte sind typisiert (Zahl, Datum,
/// Dauer), damit die Tabelle richtig sortiert; die deutsche Darstellung übernimmt die Oberfläche.
/// Fehlende Werte sind <c>null</c> – die Zelle bleibt dann leer statt „0“. Die Merker <c>IstBest…</c>
/// kennzeichnen die Bestwerte der angezeigten Auswahl (rot dargestellt).
/// </remarks>
public class Einheitszeile
{
    /// <summary>Nummer der Einheit in der Datenbank (für spätere Funktionen wie Bearbeiten).</summary>
    public long Id { get; init; }

    /// <summary>Datum der Einheit.</summary>
    public DateOnly Datum { get; init; }

    /// <summary>Startzeit, falls bekannt.</summary>
    public TimeOnly? Startzeit { get; init; }

    /// <summary>Name der Tour.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Name der Aktivität.</summary>
    public string Aktivitaet { get; init; } = string.Empty;

    /// <summary>Namen der Kategorien, alphabetisch und durch Komma getrennt.</summary>
    public string Kategorien { get; init; } = string.Empty;

    /// <summary>Name des Equipments; leer, wenn keins zugeordnet ist.</summary>
    public string Equipment { get; init; } = string.Empty;

    /// <summary>Distanz in km.</summary>
    public double? DistanzKm { get; init; }

    /// <summary>Höhenmeter (0 ist ein echter Wert, z. B. bei einer Flachtour).</summary>
    public int Hoehenmeter { get; init; }

    /// <summary>Fahrzeit.</summary>
    public TimeSpan? Fahrzeit { get; init; }

    /// <summary>
    /// Fahrzeit als Text „h:mm:ss“ (leer, wenn sie fehlt). Angezeigt wird dieser Text, sortiert wird nach
    /// <see cref="Fahrzeit"/>.
    /// </summary>
    public string FahrzeitText => Fahrzeit is TimeSpan dauer ? Berechnungen.FormatiereDauer.Ausfuehren(dauer) : string.Empty;

    /// <summary>Durchschnittsgeschwindigkeit in km/h (aus Distanz und Fahrzeit).</summary>
    public double? DurchschnittKmh { get; init; }

    /// <summary>Höchstgeschwindigkeit in km/h.</summary>
    public double? MaxKmh { get; init; }

    /// <summary>Durchschnittlicher Puls.</summary>
    public int? HfDurchschnitt { get; init; }

    /// <summary>Höchster Puls.</summary>
    public int? HfMax { get; init; }

    /// <summary>Durchschnittliche Trittfrequenz.</summary>
    public int? Trittfrequenz { get; init; }

    /// <summary>Durchschnittstemperatur in °C.</summary>
    public int? Temperatur { get; init; }

    /// <summary>Höchster Punkt der Tour in Metern.</summary>
    public int? MaxHoehe { get; init; }

    /// <summary>Kürzel des Trainingsbereichs.</summary>
    public string Trainingsbereich { get; init; } = string.Empty;

    /// <summary><c>true</c>, wenn die Distanz der Bestwert der Auswahl ist.</summary>
    public bool IstBestDistanz { get; init; }

    /// <summary><c>true</c>, wenn die Höhenmeter der Bestwert der Auswahl sind.</summary>
    public bool IstBestHoehenmeter { get; init; }

    /// <summary><c>true</c>, wenn die Fahrzeit der Bestwert der Auswahl ist.</summary>
    public bool IstBestFahrzeit { get; init; }

    /// <summary><c>true</c>, wenn die Durchschnittsgeschwindigkeit der Bestwert der Auswahl ist.</summary>
    public bool IstBestDurchschnittKmh { get; init; }

    /// <summary><c>true</c>, wenn die Höchstgeschwindigkeit der Bestwert der Auswahl ist.</summary>
    public bool IstBestMaxKmh { get; init; }
}
