using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Eine gelesene Zeile aus <c>maintanence.dat</c>, bevor der Equipment-Name in eine Nummer umgesetzt ist.
/// </summary>
public class AltWartung
{
    /// <summary>Zeilennummer in der Datei (1-basiert) – für Meldungen im Bericht.</summary>
    public int Zeilennummer { get; set; }

    /// <summary>Der Wartungseintrag; <see cref="Wartung.EquipmentId"/> ist noch nicht gesetzt.</summary>
    public Wartung Wartung { get; set; } = new();

    /// <summary>
    /// Name des Equipments, wie er in der Datei steht; „-" oder leer bedeutet „für kein bestimmtes Rad".
    /// </summary>
    public string EquipmentName { get; set; } = string.Empty;
}
