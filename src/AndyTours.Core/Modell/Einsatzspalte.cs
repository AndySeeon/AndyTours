namespace AndyTours.Core.Modell;

/// <summary>
/// Die sortierbaren Spalten der Auskunft „Equipment-Einsatz“.
/// </summary>
public enum Einsatzspalte
{
    /// <summary>Name des Equipments (alphabetisch).</summary>
    Equipment,

    /// <summary>„vorher (km)“: Anfangskilometer.</summary>
    VorherKm,

    /// <summary>„Gesamt (km)“.</summary>
    GesamtKm,

    /// <summary>„Gesamt (hm)“.</summary>
    GesamtHm,
}
