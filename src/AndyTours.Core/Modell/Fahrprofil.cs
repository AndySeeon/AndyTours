namespace AndyTours.Core.Modell;

/// <summary>
/// Fahrprofil der Tourenplanung: bestimmt, über welche Wege der Routingdienst die Strecke legt.
/// </summary>
/// <remarks>
/// Die Anzeigetexte lauten „Rennrad (nur Straße)“, „Trekking/Tourenrad“ und „Mountainbike (bevorzugt offroad)“.
/// Welches Profil des Routingdienstes dazu gehört, legt <see cref="Berechnungen.BildeRoutinganfrage"/> fest
/// (siehe <c>docs/adr/0004-routingdienst.md</c>).
/// </remarks>
public enum Fahrprofil
{
    /// <summary>„Rennrad (nur Straße)“: nur asphaltierte Straßen und Radwege, soweit möglich.</summary>
    Rennrad,

    /// <summary>„Trekking/Tourenrad“: Straßen, Radwege und gut befahrbare Schotterwege.</summary>
    Trekking,

    /// <summary>„Mountainbike (bevorzugt offroad)“: bevorzugt Feld-, Wald- und Schotterwege.</summary>
    Mountainbike,
}
