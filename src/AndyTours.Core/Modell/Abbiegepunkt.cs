namespace AndyTours.Core.Modell;

/// <summary>
/// Ein automatisch ermittelter Punkt der geplanten Strecke, an dem abgebogen werden muss.
/// </summary>
/// <remarks>
/// Abbiegepunkte liefert der Routingdienst; der Benutzer setzt sie nicht selbst (das sind die Wegpunkte der
/// <see cref="Tourenplanung"/>). Straßennamen enthalten sie bewusst nicht (Spec 030).
/// </remarks>
/// <param name="Position">Ort der Abzweigung.</param>
/// <param name="Richtung">Richtung, in die abgebogen wird.</param>
/// <param name="Ausfahrt">Nummer der Ausfahrt bei <see cref="Abbiegerichtung.Kreisverkehr"/>, sonst <c>null</c>.</param>
public record Abbiegepunkt(Geoposition Position, Abbiegerichtung Richtung, int? Ausfahrt = null);
