namespace AndyTours.Core.Modell;

/// <summary>
/// Eine Strecke zum Nachfahren auf dem Garmin Edge („Course“): Linie mit Höhen und Zeiten sowie die Abbiegungen.
/// </summary>
/// <remarks>
/// Ein Course entsteht aus einer Planung oder aus der Aufzeichnung einer Einheit
/// (<see cref="Berechnungen.BildeCourse"/>) und wird als FIT-Datei auf das Gerät übertragen (Spec 032).
/// </remarks>
/// <param name="Name">Name, unter dem der Course auf dem Edge erscheint.</param>
/// <param name="Punkte">Die Punkte der Linie in Fahrtrichtung, mindestens zwei.</param>
/// <param name="Abbiegungen">Die Abbiegungen in Fahrtrichtung; leer, wenn keine bekannt sind.</param>
public record Course(string Name, IReadOnlyList<Coursepunkt> Punkte, IReadOnlyList<Courseabbiegung> Abbiegungen);
