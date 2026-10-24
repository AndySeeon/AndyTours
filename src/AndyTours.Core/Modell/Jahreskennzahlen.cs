namespace AndyTours.Core.Modell;

/// <summary>
/// Die Kennzahlen des laufenden Jahres für einen Wert: Stand gegenüber Ziel und Soll-Stand sowie die nötige
/// Wochenleistung bis zum 31. Dezember.
/// </summary>
/// <remarks>Alle Angaben außer <see cref="Stand"/> sind <c>null</c>, wenn es kein Ziel gibt.</remarks>
/// <param name="Jahr">Das laufende Jahr.</param>
/// <param name="Stand">Bisherige Summe im laufenden Jahr.</param>
/// <param name="Ziel">Saisonziel.</param>
/// <param name="Anteil">Stand geteilt durch Ziel (1 = 100 %).</param>
/// <param name="Soll">Soll-Stand zum heutigen Tag: Ziel anteilig zu den verstrichenen Tagen des Jahres.</param>
/// <param name="Differenz">Stand minus Soll-Stand: positiv = Vorsprung, negativ = Rückstand.</param>
/// <param name="Rest">Was bis zum Ziel noch fehlt; 0, wenn es erreicht ist.</param>
/// <param name="ProWoche">Nötige Wochenleistung bis zum 31. Dezember; <c>null</c>, wenn das Ziel erreicht ist.</param>
public record Jahreskennzahlen(
    int Jahr,
    double Stand,
    double? Ziel,
    double? Anteil,
    double? Soll,
    double? Differenz,
    double? Rest,
    double? ProWoche);
