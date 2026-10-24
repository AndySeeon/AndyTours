namespace AndyTours.Core.Modell;

/// <summary>
/// Die Felder des Bearbeitungsfensters einer Trainingseinheit.
/// </summary>
/// <remarks>
/// Dient zum Kennzeichnen fehlerhafter Eingaben (<see cref="Eingabeergebnis.Fehler"/>), damit die
/// Oberfläche genau diese Felder bzw. Tabellenzellen rot markiert.
/// </remarks>
public enum Eingabefeld
{
    /// <summary>Datum (Pflicht).</summary>
    Datum,

    /// <summary>Startzeit.</summary>
    Startzeit,

    /// <summary>Name der Tour.</summary>
    Name,

    /// <summary>Aktivität (Pflicht).</summary>
    Aktivitaet,

    /// <summary>Kategorien.</summary>
    Kategorien,

    /// <summary>Equipment.</summary>
    Equipment,

    /// <summary>Distanz in km.</summary>
    Distanz,

    /// <summary>Höhenmeter.</summary>
    Hoehenmeter,

    /// <summary>Fahrzeit.</summary>
    Fahrzeit,

    /// <summary>Höchstgeschwindigkeit in km/h.</summary>
    MaxKmh,

    /// <summary>Durchschnittlicher Puls.</summary>
    PulsDurchschnitt,

    /// <summary>Höchster Puls.</summary>
    PulsMax,

    /// <summary>Niedrigster Puls.</summary>
    PulsMin,

    /// <summary>Durchschnittliche Trittfrequenz.</summary>
    Trittfrequenz,

    /// <summary>Durchschnittstemperatur in °C (darf negativ sein).</summary>
    Temperatur,

    /// <summary>Durchschnittliche Leistung in Watt.</summary>
    Leistung,

    /// <summary>Körpergewicht in kg.</summary>
    Koerpergewicht,

    /// <summary>Erholungspuls.</summary>
    Erholungspuls,

    /// <summary>Höhe des Startpunkts in Metern.</summary>
    StartHoehe,

    /// <summary>Höchster Punkt der Tour in Metern.</summary>
    MaxHoehe,

    /// <summary>Trainingsbereich.</summary>
    Trainingsbereich,

    /// <summary>Kommentar.</summary>
    Kommentar,
}
