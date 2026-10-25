namespace AndyTours.Core.Modell;

/// <summary>
/// Der Stand eines Wartungsalarms an einem Tag.
/// </summary>
/// <param name="Bezugstag">
/// Ab wann gezählt wird: Tag der letzten Wartung mit derselben Tätigkeit an diesem Rad, ohne eine solche der Tag,
/// an dem der Alarm angelegt wurde.
/// </param>
/// <param name="GefahrenKm">Distanzen aller Einheiten mit diesem Rad nach dem Bezugstag.</param>
/// <param name="VerbleibendKm">Intervall minus gefahrene km; negativ, wenn das Intervall überschritten ist.</param>
/// <param name="Zustand">Fällig, bald fällig, normal oder ausgeschaltet.</param>
public record Alarmstand(DateOnly Bezugstag, double GefahrenKm, double VerbleibendKm, Alarmzustand Zustand);
