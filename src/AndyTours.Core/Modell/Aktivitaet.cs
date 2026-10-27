namespace AndyTours.Core.Modell;

/// <summary>
/// Eine Sportart, unter der Trainingseinheiten erfasst werden (z. B. „Bike" oder „Laufen").
/// </summary>
/// <remarks>
/// Entspricht im Original dem INI-Abschnitt <c>[Activity]</c>; dort steht je Aktivität eine Zeile
/// <c>Name=Haupt;Einheit;Ziel km;Ziel hm;Ziel Anzahl</c>.
/// Die Saisonziele hier sind das <b>Grundziel</b> aus BikeXperience: Es gilt für alle Jahre vor dem ersten
/// eigenen <see cref="Saisonziel"/> der Aktivität (siehe <see cref="Berechnungen.ErmittleSaisonziel"/>).
/// </remarks>
public class Aktivitaet
{
    /// <summary>Eindeutige Nummer in der Datenbank.</summary>
    public long Id { get; set; }

    /// <summary>Anzeigename der Aktivität, z. B. „Bike".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// <c>true</c>, wenn dies die Hauptaktivität ist; sie wird beim Programmstart vorausgewählt.
    /// </summary>
    public bool IstHaupt { get; set; }

    /// <summary>
    /// Einheit, in der die Geschwindigkeit angezeigt wird: „km/h" (Rad) oder „min/km" (Laufen).
    /// </summary>
    public string Geschwindigkeitseinheit { get; set; } = "km/h";

    /// <summary>Saisonziel Distanz in Kilometern (0 = kein Ziel).</summary>
    public double ZielKm { get; set; }

    /// <summary>Saisonziel Höhenmeter (0 = kein Ziel).</summary>
    public double ZielHm { get; set; }

    /// <summary>Saisonziel Anzahl Trainingseinheiten (0 = kein Ziel).</summary>
    public int ZielAnzahl { get; set; }
}
