namespace AndyTours.Core.Modell;

/// <summary>
/// Art eines Eintrags in der Wartungsliste.
/// </summary>
/// <remarks>
/// Im Original steht der Typ als Zahl in der letzten Spalte von <c>maintanence.dat</c>:
/// 0 = Wartung, 1 = Alarm.
/// </remarks>
public enum WartungsTyp
{
    /// <summary>Durchgeführte Wartung (z. B. „Kettenwechsel").</summary>
    Wartung = 0,

    /// <summary>Erinnerung, die nach einer bestimmten Anzahl Kilometer ausgelöst wird.</summary>
    Alarm = 1
}
