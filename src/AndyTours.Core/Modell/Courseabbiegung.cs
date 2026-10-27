namespace AndyTours.Core.Modell;

/// <summary>
/// Eine Abbiegung, die der Edge beim Nachfahren eines <see cref="Course"/> ankündigt.
/// </summary>
/// <param name="Punkt">Nummer des <see cref="Coursepunkt"/>s (ab 0), an dem abgebogen wird.</param>
/// <param name="Richtung">Richtung, in die abgebogen wird.</param>
/// <param name="Ausfahrt">Nummer der Ausfahrt bei <see cref="Abbiegerichtung.Kreisverkehr"/>, sonst <c>null</c>.</param>
public record Courseabbiegung(int Punkt, Abbiegerichtung Richtung, int? Ausfahrt = null);
