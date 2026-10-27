namespace AndyTours.Core.Modell;

/// <summary>
/// Art einer bei einem Equipment gemerkten <see cref="Radkennung"/>.
/// </summary>
public enum Radkennungsart
{
    /// <summary>Kennung des Rads, die der Edge nach der Tour abfragt und in der Aufzeichnung speichert.</summary>
    Rad,

    /// <summary>Seriennummer eines Geschwindigkeits- oder Trittfrequenzsensors am Rad.</summary>
    Sensor,

    /// <summary>Wert aus BikeXperience (z. B. „Bike1“); wird beim Import nicht verwendet.</summary>
    Altwert,
}
