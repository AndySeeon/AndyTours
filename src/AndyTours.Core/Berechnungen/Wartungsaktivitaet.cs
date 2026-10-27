using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Regeln zur Aktivität der Karteikarte „Wartung“: welche Aktivität gilt und welche Equipments dazu gehören.
/// </summary>
/// <remarks>
/// Die Karteikarte „Wartung“ und die Fenster für Wartungen und Alarme bieten nur die Equipments der gültigen
/// Aktivität an – bei „Bike“ die Räder, bei „Laufen“ die Schuhe – und zeigen nur deren Einträge.
/// </remarks>
public static class Wartungsaktivitaet
{
    /// <summary>
    /// Liefert die Aktivität, die für die Karteikarte „Wartung“ gilt.
    /// </summary>
    /// <remarks>
    /// Das ist die oben im Hauptfenster gewählte Aktivität. Bei „alle Aktivitäten“ gilt die Hauptaktivität, damit
    /// die Auswahl kurz bleibt (z. B. keine Schuhe bei den Rädern); gibt es keine Hauptaktivität, wird nicht nach
    /// Aktivität eingeschränkt.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand mit den Aktivitäten.</param>
    /// <param name="gewaehlteAktivitaetId">Nummer der oben gewählten Aktivität; <c>null</c> bei „alle Aktivitäten“.</param>
    /// <returns>Nummer der gültigen Aktivität; <c>null</c> = keine Einschränkung.</returns>
    public static long? Bestimmen(Datenbestand bestand, long? gewaehlteAktivitaetId) =>
        gewaehlteAktivitaetId ?? bestand.Aktivitaeten.FirstOrDefault(a => a.IstHaupt)?.Id;

    /// <summary>
    /// Prüft, ob ein Equipment zur gültigen Aktivität gehört.
    /// </summary>
    /// <remarks>
    /// Ohne Einschränkung passt jedes Equipment. Ein Equipment ohne zugeordnete Aktivität passt zu jeder
    /// Aktivität, damit es erreichbar bleibt; ebenso passt „kein Equipment“ (eine Wartung ohne Rad bleibt sichtbar).
    /// </remarks>
    /// <param name="equipment">Das Equipment; <c>null</c>, wenn ein Eintrag keinem Equipment zugeordnet ist.</param>
    /// <param name="aktivitaetId">Die gültige Aktivität (siehe <see cref="Bestimmen"/>); <c>null</c> = keine Einschränkung.</param>
    /// <returns><c>true</c>, wenn das Equipment angezeigt bzw. angeboten wird.</returns>
    public static bool Passt(Equipment? equipment, long? aktivitaetId) =>
        aktivitaetId is null || equipment?.AktivitaetId is null || equipment.AktivitaetId == aktivitaetId;
}
