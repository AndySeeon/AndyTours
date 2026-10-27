namespace AndyTours.Core.Modell;

/// <summary>
/// Eine bei einem Equipment gemerkte Kennung, an der AndyTours beim Import das Rad erkennt.
/// </summary>
/// <param name="Art">Art der Kennung (Rad-Kennung, Sensor oder Altwert).</param>
/// <param name="Wert">Der Wert, z. B. „7af22db6-d829-4395-9e42-a200867c4abf“ oder „3452708077“.</param>
public record Radkennung(Radkennungsart Art, string Wert);
