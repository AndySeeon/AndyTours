using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bildet den angezeigten Namen eines Equipments und findet zu einem angezeigten Namen das Equipment zurück.
/// </summary>
/// <remarks>
/// Ausgemustertes Equipment trägt in allen Listen, Tabellen und Auswahlen den Zusatz „(ausgemustert)“. Der Zusatz
/// gehört nur zur Anzeige; der gespeicherte Name bleibt unverändert. Jede Anzeige eines Equipment-Namens bildet
/// ihn deshalb über <see cref="Bilden"/>, und wo ein angezeigter Name wieder in ein Equipment übersetzt wird
/// (Zellbearbeitung, Filter), vergleicht <see cref="Passt"/>.
/// </remarks>
public static class EquipmentAnzeigename
{
    /// <summary>Zusatz hinter dem Namen eines ausgemusterten Equipments.</summary>
    public const string ZusatzAusgemustert = " (ausgemustert)";

    /// <summary>
    /// Liefert den angezeigten Namen: bei einem ausgemusterten Equipment mit dem Zusatz „(ausgemustert)“.
    /// </summary>
    /// <param name="equipment">Das Equipment.</param>
    /// <returns>Der Name, ggf. mit Zusatz.</returns>
    public static string Bilden(Equipment equipment) =>
        equipment.Ausgemustert ? equipment.Name + ZusatzAusgemustert : equipment.Name;

    /// <summary>
    /// Prüft, ob ein Name zu einem Equipment gehört.
    /// </summary>
    /// <remarks>
    /// Es zählen der gespeicherte Name und der Name mit dem Zusatz „(ausgemustert)“ – unabhängig davon, ob das
    /// Equipment gerade ausgemustert ist. So passt auch ein Wert, der gewählt wurde, bevor das Equipment
    /// ausgemustert oder wieder in Einsatz genommen wurde, z. B. ein gesetzter Filter.
    /// </remarks>
    /// <param name="equipment">Das Equipment.</param>
    /// <param name="name">Der angezeigte oder gespeicherte Name.</param>
    /// <returns><c>true</c>, wenn der Name zum Equipment gehört.</returns>
    public static bool Passt(Equipment equipment, string? name) =>
        name is not null && (name == equipment.Name || name == equipment.Name + ZusatzAusgemustert);
}
