namespace AndyTours.Core.Modell;

/// <summary>
/// Felder des Alarmfensters, die geprüft und bei einem Fehler markiert werden.
/// </summary>
public enum Alarmfeld
{
    /// <summary>Das Rad (keines gewählt).</summary>
    Rad,

    /// <summary>Die Tätigkeit (keine gewählt oder für dieses Rad schon ein Alarm vorhanden).</summary>
    Taetigkeit,

    /// <summary>Das Intervall (leer, keine ganze Zahl oder nicht größer als 0).</summary>
    Intervall,
}
