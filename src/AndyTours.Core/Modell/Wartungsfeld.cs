namespace AndyTours.Core.Modell;

/// <summary>
/// Felder des Wartungsfensters, die geprüft und bei einem Fehler markiert werden.
/// </summary>
public enum Wartungsfeld
{
    /// <summary>Das Datum (leer, ungültig oder in der Zukunft).</summary>
    Datum,

    /// <summary>Das Rad (keines gewählt).</summary>
    Rad,

    /// <summary>Die Tätigkeit (keine gewählt).</summary>
    Taetigkeit,
}
