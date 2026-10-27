namespace AndyTours.Core.Modell;

/// <summary>
/// Zustand eines Wartungsalarms; bestimmt Farbe und Reihenfolge in der Alarmliste.
/// </summary>
public enum Alarmzustand
{
    /// <summary>Fällig: Seit dem Bezugstag ist das Rad mindestens das Intervall gefahren (rot).</summary>
    Faellig,

    /// <summary>Bald fällig: Höchstens noch 10 % des Intervalls übrig (orange).</summary>
    BaldFaellig,

    /// <summary>Eingeschaltet, aber noch nicht bald fällig.</summary>
    Normal,

    /// <summary>Ausgeschaltet oder das Rad ist ausgemustert: wird nie fällig (grau).</summary>
    Ausgeschaltet,
}
