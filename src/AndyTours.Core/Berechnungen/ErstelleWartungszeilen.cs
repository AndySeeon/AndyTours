using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erstellt die Zeilen der Tabelle auf der Karteikarte „Wartung“.
/// </summary>
public static class ErstelleWartungszeilen
{
    /// <summary>
    /// Liefert die Wartungen als Zeilen mit Rad, km-Stand und Haltbarkeit (km und Tage seit der letzten gleichartigen
    /// Wartung, siehe <see cref="BerechneHaltbarkeit"/>), auf Wunsch nur die eines Rads oder einer Aktivität, die neueste oben.
    /// </summary>
    /// <remarks>
    /// Nur Einträge vom Typ <see cref="WartungsTyp.Wartung"/>; Alarme zeigt eine eigene Funktion. Bei gleichem Datum
    /// steht die zuletzt erfasste (höhere Nummer) oben. Wartungen ausgemusterter Räder erscheinen wie alle anderen,
    /// ihr Name mit dem Zusatz „(ausgemustert)“.
    /// Ohne gewähltes Rad („Alle Equipments“) erscheinen nur die Wartungen der Equipments, die zur Aktivität passen
    /// (siehe <see cref="Wartungsaktivitaet.Passt"/>).
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="equipmentId">Nummer des gewählten Rads; <c>null</c> = alle Equipments der Aktivität.</param>
    /// <param name="aktivitaetId">Die gültige Aktivität (siehe <see cref="Wartungsaktivitaet.Bestimmen"/>); <c>null</c> = alle.</param>
    /// <returns>Die Zeilen in Anzeigereihenfolge.</returns>
    public static List<Wartungszeile> Ausfuehren(Datenbestand bestand, long? equipmentId, long? aktivitaetId = null)
    {
        Dictionary<long, Equipment> raeder = bestand.Equipments.ToDictionary(e => e.Id);
        return bestand.Wartungen
            .Where(w => w.Typ == WartungsTyp.Wartung
                && (equipmentId is null
                    ? Wartungsaktivitaet.Passt(w.EquipmentId is long id ? raeder.GetValueOrDefault(id) : null, aktivitaetId)
                    : w.EquipmentId == equipmentId))
            .OrderByDescending(w => w.Datum)
            .ThenByDescending(w => w.Id)
            .Select(w =>
            {
                Equipment? rad = w.EquipmentId is long id ? raeder.GetValueOrDefault(id) : null;
                Haltbarkeit haltbarkeit = BerechneHaltbarkeit.Ausfuehren(bestand, w);
                return new Wartungszeile(
                    w.Id,
                    w.Datum,
                    rad is null ? string.Empty : ErmittleWartungsraeder.Anzeigename(rad),
                    rad is null ? null : BerechneKmStand.Ausfuehren(bestand.Trainingseinheiten, rad, w.Datum),
                    haltbarkeit.Km,
                    haltbarkeit.Tage,
                    w.Taetigkeit,
                    w.Kommentar);
            })
            .ToList();
    }
}
