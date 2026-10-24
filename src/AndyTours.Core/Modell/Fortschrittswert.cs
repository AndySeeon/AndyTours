namespace AndyTours.Core.Modell;

/// <summary>
/// Der Wert, den das Diagramm auf der Karteikarte „Aktuell“ zeigt.
/// </summary>
/// <remarks>
/// Die Reihenfolge ist die Umschaltreihenfolge beim Klick ins Diagramm: Kilometer → Anzahl → Höhenmeter →
/// Fahrzeit → wieder Kilometer.
/// </remarks>
public enum Fortschrittswert
{
    /// <summary>Summe der Entfernung in Kilometern.</summary>
    Kilometer,

    /// <summary>Anzahl der Einheiten.</summary>
    Anzahl,

    /// <summary>Summe der Höhenmeter.</summary>
    Hoehenmeter,

    /// <summary>Summe der Fahrzeit (in Stunden); dafür gibt es kein Saisonziel.</summary>
    Fahrzeit,
}
