namespace AndyTours.Core.Modell;

/// <summary>
/// Die ermittelte Strecke zwischen zwei benachbarten Wegpunkten einer <see cref="Tourenplanung"/>.
/// </summary>
/// <remarks>
/// Von den <see cref="Verlaufspunkt"/>en sind nur Position und, falls bekannt, Höhe belegt. Der erste Punkt liegt am
/// Anfangs-, der letzte am Endwegpunkt (bzw. an der nächsten befahrbaren Stelle, die der Routingdienst gefunden hat).
/// </remarks>
/// <param name="Punkte">Die Punkte der Strecke in Fahrtrichtung, mindestens zwei.</param>
/// <param name="Abbiegepunkte">Die Abbiegepunkte in Fahrtrichtung; bei einer Luftlinie keine.</param>
/// <param name="IstLuftlinie">
/// <c>true</c>, wenn der Routingdienst die Strecke nicht ermitteln konnte und eine gerade Linie die Wegpunkte verbindet.
/// </param>
public record Teilstueck(IReadOnlyList<Verlaufspunkt> Punkte, IReadOnlyList<Abbiegepunkt> Abbiegepunkte, bool IstLuftlinie);
