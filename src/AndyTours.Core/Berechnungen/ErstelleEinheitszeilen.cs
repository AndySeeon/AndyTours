using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erzeugt die Zeilen der Tabelle „Trainingseinheiten“ aus den angezeigten Einheiten.
/// </summary>
public static class ErstelleEinheitszeilen
{
    /// <summary>
    /// Wandelt jede Einheit in eine <see cref="Einheitszeile"/> um.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Aktivität, Equipment und Kategorien werden über den Datenbestand in Namen übersetzt;
    /// unbekannte Nummern ergeben einen leeren Text. Ausgemustertes Equipment trägt den Zusatz „(ausgemustert)“
    /// (siehe <see cref="EquipmentAnzeigename"/>); die Spalte sortiert nach diesem Text.</item>
    /// <item>Werte, die als 0 gespeichert sind, gelten als „nicht erfasst“ und werden <c>null</c>:
    /// Distanz, Fahrzeit, max km/h, Ø und max Puls, Trittfrequenz, Temperatur (0 °C gilt in den
    /// Altdaten als „nicht aufgezeichnet“) und Maximalhöhe. Höhenmeter bleiben als Zahl, denn 0 hm
    /// ist bei einer Flachtour ein echter Wert.</item>
    /// <item>Die Bestwerte werden über <see cref="ErmittleBestwerte"/> aus genau diesen Einheiten bestimmt;
    /// alle Einheiten mit dem Höchstwert werden markiert (bei Gleichstand also mehrere).</item>
    /// </list>
    /// Die Reihenfolge der Einheiten bleibt erhalten; sortiert wird in der Tabelle.
    /// </remarks>
    /// <param name="einheiten">Die angezeigten Einheiten (aktuelle Auswahl).</param>
    /// <param name="bestand">Der Datenbestand mit Aktivitäten, Equipments und Kategorien.</param>
    /// <returns>Eine Zeile je Einheit.</returns>
    public static List<Einheitszeile> Ausfuehren(IReadOnlyCollection<Trainingseinheit> einheiten, Datenbestand bestand)
    {
        Dictionary<long, string> aktivitaeten = bestand.Aktivitaeten.ToDictionary(a => a.Id, a => a.Name);
        Dictionary<long, string> equipments = bestand.Equipments.ToDictionary(e => e.Id, EquipmentAnzeigename.Bilden);
        Dictionary<long, string> kategorien = bestand.Kategorien.ToDictionary(k => k.Id, k => k.Name);
        Bestwerte best = ErmittleBestwerte.Ausfuehren(einheiten);

        var zeilen = new List<Einheitszeile>(einheiten.Count);
        foreach (Trainingseinheit e in einheiten)
        {
            double? durchschnitt = BerechneDurchschnittsgeschwindigkeit.Ausfuehren(e.DistanzKm, e.Fahrzeit);
            double? distanz = e.DistanzKm > 0 ? e.DistanzKm : null;
            TimeSpan? fahrzeit = e.Fahrzeit > TimeSpan.Zero ? e.Fahrzeit : null;
            double? maxKmh = e.MaxGeschwindigkeit > 0 ? e.MaxGeschwindigkeit : null;

            zeilen.Add(new Einheitszeile
            {
                Id = e.Id,
                Datum = e.Datum,
                Startzeit = e.Startzeit,
                Name = e.Name,
                Aktivitaet = aktivitaeten.GetValueOrDefault(e.AktivitaetId, string.Empty),
                Kategorien = string.Join(", ", e.KategorieIds
                    .Select(id => kategorien.GetValueOrDefault(id, string.Empty))
                    .Where(name => name.Length > 0)
                    .Order(StringComparer.CurrentCultureIgnoreCase)),
                Equipment = e.EquipmentId is long equipmentId ? equipments.GetValueOrDefault(equipmentId, string.Empty) : string.Empty,
                DistanzKm = distanz,
                Hoehenmeter = e.Hoehenmeter,
                Fahrzeit = fahrzeit,
                DurchschnittKmh = durchschnitt,
                MaxKmh = maxKmh,
                HfDurchschnitt = OhneNull(e.HfDurchschnitt),
                HfMax = OhneNull(e.HfMax),
                Trittfrequenz = OhneNull(e.Trittfrequenz),
                Temperatur = OhneNull(e.Temperatur),
                MaxHoehe = OhneNull(e.MaxHoehe),
                Trainingsbereich = e.Trainingsbereich,

                // Bestwert-Merker: vorhandener Wert gleich dem Höchstwert der Auswahl
                IstBestDistanz = distanz is not null && distanz == best.DistanzKm,
                IstBestHoehenmeter = e.Hoehenmeter > 0 && e.Hoehenmeter == best.Hoehenmeter,
                IstBestFahrzeit = fahrzeit is not null && fahrzeit == best.Fahrzeit,
                IstBestDurchschnittKmh = durchschnitt is not null && durchschnitt == best.DurchschnittKmh,
                IstBestMaxKmh = maxKmh is not null && maxKmh == best.MaxKmh,
            });
        }

        return zeilen;
    }

    /// <summary>0 bedeutet „nicht erfasst“ → <c>null</c>.</summary>
    private static int? OhneNull(int wert) => wert != 0 ? wert : null;
}
