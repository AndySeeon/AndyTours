using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Übernimmt einen in der Tabelle „Trainingseinheiten“ geänderten Zellwert in die Einheit.
/// </summary>
public static class UebernimmZellwert
{
    /// <summary>
    /// Setzt den Text einer Zelle in die Eingaben der Einheit ein und prüft das Ergebnis.
    /// </summary>
    /// <remarks>
    /// Die Zelle wird genau wie das entsprechende Feld im Bearbeitungsfenster behandelt: Die Einheit wird
    /// in Eingaben umgewandelt (<see cref="EinheitEingaben.AlsEingaben"/>), das eine Feld ersetzt und alles
    /// über <see cref="EinheitEingaben.AlsEinheit"/> geprüft. So gelten in Tabelle und Fenster dieselben Regeln.
    /// <para>
    /// Aktivität und Equipment kommen als Name aus der Auswahlliste und werden über den Datenbestand in
    /// ihre Nummer übersetzt; „(kein)“ oder ein leerer Text bedeutet „kein Equipment“. Ein unbekannter Name
    /// gilt als Fehler. Ø km/h (berechnet) und Kategorien (mehrere) lassen sich in der Zelle nicht ändern.
    /// </para>
    /// </remarks>
    /// <param name="einheit">Die Einheit der bearbeiteten Zeile.</param>
    /// <param name="spalte">Die bearbeitete Spalte.</param>
    /// <param name="text">Der eingetragene Text bzw. der gewählte Listeneintrag.</param>
    /// <param name="bestand">Der Datenbestand (für Aktivitäten und Equipments).</param>
    /// <returns>Die geänderte Einheit oder das fehlerhafte Feld.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Die Spalte ist nicht bearbeitbar.</exception>
    public static Eingabeergebnis Ausfuehren(Trainingseinheit einheit, Einheitenspalte spalte, string? text, Datenbestand bestand)
    {
        Einheiteneingaben eingaben = EinheitEingaben.AlsEingaben(einheit);
        string wert = text ?? string.Empty;

        switch (spalte)
        {
            case Einheitenspalte.Aktivitaet:
                // Name → Nummer; ein unbekannter Name ergibt „keine Aktivität“ und damit den Pflichtfeld-Fehler
                eingaben = eingaben with { AktivitaetId = bestand.Aktivitaeten.FirstOrDefault(a => a.Name == wert)?.Id };
                break;

            case Einheitenspalte.Equipment:
                if (wert.Length == 0 || wert == Eingabelisten.KeinEquipment)
                {
                    eingaben = eingaben with { EquipmentId = null };
                }
                else if (bestand.Equipments.FirstOrDefault(e => e.Name == wert) is Equipment equipment)
                {
                    eingaben = eingaben with { EquipmentId = equipment.Id };
                }
                else
                {
                    return new Eingabeergebnis(null, new HashSet<Eingabefeld> { Eingabefeld.Equipment });
                }

                break;

            default:
                eingaben = MitText(eingaben, spalte, wert);
                break;
        }

        return EinheitEingaben.AlsEinheit(eingaben, einheit);
    }

    /// <summary>Ersetzt das Textfeld, das zur Spalte gehört.</summary>
    private static Einheiteneingaben MitText(Einheiteneingaben eingaben, Einheitenspalte spalte, string wert) => spalte switch
    {
        Einheitenspalte.Datum => eingaben with { Datum = wert },
        Einheitenspalte.Startzeit => eingaben with { Startzeit = wert },
        Einheitenspalte.Name => eingaben with { Name = wert },
        Einheitenspalte.Distanz => eingaben with { Distanz = wert },
        Einheitenspalte.Hoehenmeter => eingaben with { Hoehenmeter = wert },
        Einheitenspalte.Fahrzeit => eingaben with { Fahrzeit = wert },
        Einheitenspalte.MaxKmh => eingaben with { MaxKmh = wert },
        Einheitenspalte.PulsDurchschnitt => eingaben with { PulsDurchschnitt = wert },
        Einheitenspalte.PulsMax => eingaben with { PulsMax = wert },
        Einheitenspalte.Trittfrequenz => eingaben with { Trittfrequenz = wert },
        Einheitenspalte.Temperatur => eingaben with { Temperatur = wert },
        Einheitenspalte.MaxHoehe => eingaben with { MaxHoehe = wert },
        Einheitenspalte.Trainingsbereich => eingaben with { Trainingsbereich = wert },
        _ => throw new ArgumentOutOfRangeException(nameof(spalte), spalte, "Diese Spalte lässt sich nicht in der Tabelle bearbeiten."),
    };

    /// <summary>
    /// Liefert den Text, mit dem die Bearbeitung einer Zelle beginnt (ohne Tausenderpunkt, ohne Stellenverlust).
    /// </summary>
    /// <param name="einheit">Die Einheit der Zeile.</param>
    /// <param name="spalte">Die Spalte.</param>
    /// <param name="bestand">Der Datenbestand (für Aktivitäts- und Equipmentnamen).</param>
    /// <returns>Der Eingabetext, z. B. „1234,5“.</returns>
    public static string Rohtext(Trainingseinheit einheit, Einheitenspalte spalte, Datenbestand bestand)
    {
        Einheiteneingaben e = EinheitEingaben.AlsEingaben(einheit);
        return spalte switch
        {
            Einheitenspalte.Datum => e.Datum,
            Einheitenspalte.Startzeit => e.Startzeit,
            Einheitenspalte.Name => e.Name,
            Einheitenspalte.Aktivitaet => bestand.Aktivitaeten.FirstOrDefault(a => a.Id == einheit.AktivitaetId)?.Name ?? string.Empty,
            Einheitenspalte.Equipment => bestand.Equipments.FirstOrDefault(q => q.Id == einheit.EquipmentId)?.Name ?? Eingabelisten.KeinEquipment,
            Einheitenspalte.Distanz => e.Distanz,
            Einheitenspalte.Hoehenmeter => e.Hoehenmeter,
            Einheitenspalte.Fahrzeit => e.Fahrzeit,
            Einheitenspalte.MaxKmh => e.MaxKmh,
            Einheitenspalte.PulsDurchschnitt => e.PulsDurchschnitt,
            Einheitenspalte.PulsMax => e.PulsMax,
            Einheitenspalte.Trittfrequenz => e.Trittfrequenz,
            Einheitenspalte.Temperatur => e.Temperatur,
            Einheitenspalte.MaxHoehe => e.MaxHoehe,
            Einheitenspalte.Trainingsbereich => e.Trainingsbereich,
            _ => string.Empty,
        };
    }
}
