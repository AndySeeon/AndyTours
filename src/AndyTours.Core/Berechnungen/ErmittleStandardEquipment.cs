using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt das wirksame Standard-Equipment einer Aktivität.
/// </summary>
public static class ErmittleStandardEquipment
{
    /// <summary>
    /// Liefert das Equipment, das bei neuen Einheiten dieser Aktivität vorbelegt wird.
    /// </summary>
    /// <remarks>
    /// Wirksam ist nur ein Standard-Equipment, das nicht ausgemustert ist. Ein ausgemustertes, das aus
    /// dem Altbestand noch als Standard gekennzeichnet ist (z. B. „Namur“), zählt nicht. Sollten mehrere
    /// wirksam sein, gilt das erste im Datenbestand; beim Speichern bleibt ohnehin nur eines übrig.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Id der Aktivität; <c>null</c> oder 0 = keine Aktivität gewählt.</param>
    /// <returns>Das Standard-Equipment oder <c>null</c>, wenn die Aktivität keines hat.</returns>
    public static Equipment? Ausfuehren(Datenbestand bestand, long? aktivitaetId) =>
        aktivitaetId is long id && id != 0
            ? bestand.Equipments.FirstOrDefault(e => e.AktivitaetId == id && e.IstStandard && !e.Ausgemustert)
            : null;

    /// <summary>
    /// Prüft, ob ein Equipment als Standard gilt (gekennzeichnet und nicht ausgemustert).
    /// </summary>
    /// <param name="equipment">Das Equipment.</param>
    /// <returns><c>true</c>, wenn es als Standard angezeigt wird.</returns>
    public static bool IstWirksam(Equipment equipment) => equipment.IstStandard && !equipment.Ausgemustert;
}
