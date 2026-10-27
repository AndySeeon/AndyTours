namespace AndyTours.Core.Modell;

/// <summary>
/// Wie lange ein Teil gehalten hat: Kilometer und Tage seit der vorherigen gleichartigen Wartung
/// (Spalten „km seit letzter“ und „Tage seit letzter“ der Wartungstabelle).
/// </summary>
/// <param name="Km">
/// Gefahrene km seit der vorherigen Wartung mit derselben Tätigkeit an diesem Rad, bei der ersten ihrer Art der
/// km-Stand des Rads; <c>null</c> ohne Rad.
/// </param>
/// <param name="Tage">
/// Kalendertage seit der vorherigen gleichartigen Wartung, bei der ersten ihrer Art seit dem Anschaffungsdatum bzw.
/// der ersten Einheit mit dem Rad; <c>null</c> ohne Rad oder wenn es keinen Anfangstag gibt.
/// </param>
public record Haltbarkeit(double? Km, int? Tage);
