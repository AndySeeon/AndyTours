namespace AndyTours.Core.Modell;

/// <summary>
/// Verweis auf eine Datei, die zu einer Trainingseinheit gehört (z. B. die FIT-Aufzeichnung).
/// </summary>
/// <remarks>
/// Die Dateien selbst bleiben unverändert in den Jahresordnern des Datenordners liegen;
/// gespeichert wird nur der Pfad relativ zum Datenordner, z. B. <c>2016/2016-03-05-08-40-41.fit</c>.
/// Dadurch kann der Datenordner verschoben werden, ohne dass Verweise ungültig werden.
/// </remarks>
public class Anhang
{
    /// <summary>Eindeutige Nummer in der Datenbank.</summary>
    public long Id { get; set; }

    /// <summary>Art des Anhangs.</summary>
    public AnhangTyp Typ { get; set; }

    /// <summary>Pfad relativ zum Datenordner, mit „/" als Trennzeichen.</summary>
    public string RelativerPfad { get; set; } = string.Empty;
}
