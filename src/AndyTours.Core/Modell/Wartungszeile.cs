namespace AndyTours.Core.Modell;

/// <summary>
/// Eine Zeile der Tabelle auf der Karteikarte „Wartung“.
/// </summary>
/// <param name="Id">Nummer der Wartung.</param>
/// <param name="Datum">Tag der Wartung.</param>
/// <param name="Rad">Name des Rads; leer, wenn die Wartung keinem Rad zugeordnet ist.</param>
/// <param name="KmStand">km-Stand des Rads an diesem Tag; <c>null</c> ohne Rad.</param>
/// <param name="KmSeitLetzter">km seit der vorherigen gleichartigen Wartung (siehe <see cref="Berechnungen.BerechneHaltbarkeit"/>); <c>null</c> ohne Rad.</param>
/// <param name="TageSeitLetzter">Tage seit der vorherigen gleichartigen Wartung; <c>null</c> ohne Rad oder ohne Anfangstag.</param>
/// <param name="Taetigkeit">Die Tätigkeit, z. B. „Kettenwechsel“.</param>
/// <param name="Kommentar">Freier Kommentar.</param>
public record Wartungszeile(long Id, DateOnly Datum, string Rad, double? KmStand, double? KmSeitLetzter, int? TageSeitLetzter,
    string Taetigkeit, string Kommentar);
