namespace AndyTours.Core.Modell;

/// <summary>
/// Die Spalten der Tabelle „Trainingseinheiten“ in Anzeigereihenfolge.
/// </summary>
/// <remarks>
/// Der Name jedes Werts dient auch als Schlüssel, unter dem die Wahl „ausgeblendet“ gespeichert
/// wird. Werte daher nicht umbenennen – sonst wären gespeicherte Spalten wieder sichtbar.
/// </remarks>
public enum Einheitenspalte
{
    /// <summary>Datum.</summary>
    Datum,

    /// <summary>Startzeit.</summary>
    Startzeit,

    /// <summary>Name der Tour.</summary>
    Name,

    /// <summary>Aktivität.</summary>
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

    /// <summary>Durchschnittsgeschwindigkeit.</summary>
    DurchschnittKmh,

    /// <summary>Höchstgeschwindigkeit.</summary>
    MaxKmh,

    /// <summary>Durchschnittlicher Puls.</summary>
    PulsDurchschnitt,

    /// <summary>Höchster Puls.</summary>
    PulsMax,

    /// <summary>Trittfrequenz.</summary>
    Trittfrequenz,

    /// <summary>Temperatur.</summary>
    Temperatur,

    /// <summary>Maximalhöhe.</summary>
    MaxHoehe,

    /// <summary>Trainingsbereich.</summary>
    Trainingsbereich,

    /// <summary>Kommentar (neue Spalten kommen ans Ende, damit gespeicherte Anordnungen gültig bleiben).</summary>
    Kommentar,
}
