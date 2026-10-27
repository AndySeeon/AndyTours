namespace AndyTours.Core.Modell;

/// <summary>
/// Die Arten der Einzelrekorde, in der Reihenfolge der Anzeige auf der Karteikarte „Aktuell“.
/// </summary>
public enum Einzelrekordart
{
    /// <summary>Längste Tour (größte Distanz).</summary>
    Distanz,

    /// <summary>Meiste Höhenmeter einer Einheit.</summary>
    Hoehenmeter,

    /// <summary>Längste Fahrzeit einer Einheit.</summary>
    Fahrzeit,

    /// <summary>Höchste Durchschnittsgeschwindigkeit.</summary>
    Durchschnittsgeschwindigkeit,

    /// <summary>Höchste Höchstgeschwindigkeit.</summary>
    Hoechstgeschwindigkeit,

    /// <summary>Höchster Punkt (größte maximale Höhe).</summary>
    HoechsterPunkt,
}
