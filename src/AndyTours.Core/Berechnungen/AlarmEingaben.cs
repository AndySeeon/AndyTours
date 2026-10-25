using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wandelt zwischen einem Wartungsalarm und den Eingaben des Alarmfensters um und prüft die Eingaben.
/// </summary>
public static class AlarmEingaben
{
    /// <summary>Füllt die Felder des Alarmfensters aus einem Alarm.</summary>
    /// <param name="alarm">Der Alarm (bei „Neuer Alarm“ der vorbereitete neue).</param>
    /// <returns>Die Eingaben; ein Intervall von 0 (neuer Alarm) bleibt leer.</returns>
    public static Alarmeingaben AlsEingaben(Wartung alarm) => new()
    {
        EquipmentId = alarm.EquipmentId,
        Taetigkeit = alarm.Taetigkeit.Trim(),
        Intervall = alarm.AlarmKm > 0 ? Eingabetext.AlsText((int)Math.Round(alarm.AlarmKm)) : string.Empty,
        Eingeschaltet = alarm.Aktiv,
    };

    /// <summary>
    /// Prüft die Eingaben und bildet daraus den Alarm.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Rad und Tätigkeit sind Pflicht; das Intervall muss eine ganze Zahl größer als 0 sein.</item>
    /// <item>Je Rad und Tätigkeit gibt es höchstens einen Alarm (Tätigkeit ohne Unterschied der Schreibweise);
    /// der bearbeitete Alarm selbst zählt nicht. Ein Treffer markiert die Tätigkeit, und der Hinweis nennt den
    /// vorhandenen Alarm.</item>
    /// <item>Nummer und Datum (Tag des Anlegens) kommen aus <paramref name="basis"/> und bleiben beim
    /// Bearbeiten erhalten; der Typ ist immer „Alarm“, ein Kommentar bleibt unverändert.</item>
    /// </list>
    /// </remarks>
    /// <param name="eingaben">Die Eingaben des Fensters.</param>
    /// <param name="basis">Der bearbeitete Alarm (bei „Neu“ der vorbereitete mit dem heutigen Datum).</param>
    /// <param name="bestand">Der Datenbestand mit den übrigen Alarmen und den Rädern (für die Doppelprüfung).</param>
    /// <returns>Der Alarm oder die fehlerhaften Felder samt Hinweis.</returns>
    public static Alarmergebnis AlsAlarm(Alarmeingaben eingaben, Wartung basis, Datenbestand bestand)
    {
        HashSet<Alarmfeld> fehler = [];
        string? hinweis = null;

        if (eingaben.EquipmentId is null)
        {
            fehler.Add(Alarmfeld.Rad);
        }

        string taetigkeit = eingaben.Taetigkeit.Trim();
        if (taetigkeit.Length == 0)
        {
            fehler.Add(Alarmfeld.Taetigkeit);
        }

        if (!Eingabetext.LeseGanzzahl(eingaben.Intervall, out int? intervall) || intervall is not > 0)
        {
            fehler.Add(Alarmfeld.Intervall);
        }

        // Schon ein Alarm für dieses Rad und diese Tätigkeit?
        if (eingaben.EquipmentId is long radId && taetigkeit.Length > 0
            && bestand.Wartungen.FirstOrDefault(w => w.Typ == WartungsTyp.Alarm && w.Id != basis.Id && w.EquipmentId == radId
                && string.Equals(w.Taetigkeit.Trim(), taetigkeit, StringComparison.OrdinalIgnoreCase)) is Wartung vorhanden)
        {
            fehler.Add(Alarmfeld.Taetigkeit);
            string rad = bestand.Equipments.FirstOrDefault(e => e.Id == radId)?.Name ?? string.Empty;
            hinweis = string.Format(CultureInfo.GetCultureInfo("de-DE"),
                "Für ‚{0}‘ gibt es schon den Alarm ‚{1}‘ (alle {2:N0} km).", rad, vorhanden.Taetigkeit.Trim(), vorhanden.AlarmKm);
        }

        if (fehler.Count > 0)
        {
            return new Alarmergebnis(null, fehler, hinweis);
        }

        var alarm = new Wartung
        {
            Id = basis.Id,
            Datum = basis.Datum,
            EquipmentId = eingaben.EquipmentId,
            Taetigkeit = taetigkeit,
            Kommentar = basis.Kommentar,
            Typ = WartungsTyp.Alarm,
            AlarmKm = intervall!.Value,
            Aktiv = eingaben.Eingeschaltet,
        };
        return new Alarmergebnis(alarm, fehler, null);
    }
}
