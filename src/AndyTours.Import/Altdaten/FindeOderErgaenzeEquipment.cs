using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Liefert das Equipment zu einem Namen aus den Altdaten und legt es an, falls es noch fehlt.
/// </summary>
public static class FindeOderErgaenzeEquipment
{
    /// <summary>
    /// Sucht das Equipment über <see cref="FindeEquipmentZuName"/>; fehlt es, wird es ergänzt.
    /// </summary>
    /// <remarks>
    /// In den Altdaten kommen Equipments vor, die nicht (mehr) in <c>bikexperience.ini</c> stehen,
    /// z. B. „GENERIC" und „MOUNTAIN" (vermutlich vom Garmin-Gerät vorgegebene Namen).
    /// Damit die Einheiten ihre Zuordnung behalten, werden solche Equipments angelegt und als
    /// ausgemustert markiert, weil sie offensichtlich nicht mehr gepflegt werden.
    /// </remarks>
    /// <param name="bestand">Datenbestand, in dem gesucht bzw. ergänzt wird.</param>
    /// <param name="name">Equipment-Name aus der Altdatei.</param>
    /// <param name="aktivitaetId">Aktivität für ein neu angelegtes Equipment (falls bekannt).</param>
    /// <param name="bericht">Bericht für Hinweise auf Ergänzungen und Namenszuordnungen.</param>
    /// <returns>Das gefundene oder neu angelegte Equipment.</returns>
    public static Equipment Ausfuehren(Datenbestand bestand, string name, long? aktivitaetId, Migrationsbericht bericht)
    {
        Equipment? gefunden = FindeEquipmentZuName.Ausfuehren(bestand.Equipments, name);
        if (gefunden is not null)
        {
            // Zuordnung über den Kurznamen im Bericht festhalten, damit sie nachvollziehbar ist.
            if (!gefunden.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                bericht.HinweisEinmalig($"Equipment „{name}“ wurde „{gefunden.Name}“ zugeordnet.");
            }

            return gefunden;
        }

        var neu = new Equipment
        {
            // Nächste freie Nummer: eins größer als die bisher höchste
            Id = bestand.Equipments.Count == 0 ? 1 : bestand.Equipments.Max(e => e.Id) + 1,
            Name = name.Trim(),
            AktivitaetId = aktivitaetId,
            Ausgemustert = true,
        };
        bestand.Equipments.Add(neu);
        bericht.HinweisEinmalig($"Equipment „{neu.Name}“ stand nicht in bikexperience.ini und wurde als ausgemustert ergänzt.");
        return neu;
    }
}
