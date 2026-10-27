namespace AndyTours.Core.Modell;

/// <summary>
/// Die Eingaben einer Trainingseinheit, wie sie im Bearbeitungsfenster stehen.
/// </summary>
/// <remarks>
/// Zahlen, Datum und Zeiten sind Text, weil der Benutzer dort auch Ungültiges tippen kann; geprüft und
/// umgewandelt wird mit <see cref="Berechnungen.EinheitEingaben.AlsEinheit"/>. Aktivität und Equipment
/// werden aus Listen gewählt und stehen deshalb als Nummer da. Ein leeres Feld bedeutet „nicht erfasst“.
/// <para>
/// Zwei Eingaben sind gleich, wenn alle Felder gleich sind; die Kategorien werden dabei ohne Rücksicht
/// auf ihre Reihenfolge verglichen. Daran erkennt das Fenster, ob etwas geändert wurde.
/// </para>
/// </remarks>
public record Einheiteneingaben
{
    /// <summary>Datum, z. B. „24.10.2026“.</summary>
    public string Datum { get; init; } = string.Empty;

    /// <summary>Startzeit, z. B. „9:30“.</summary>
    public string Startzeit { get; init; } = string.Empty;

    /// <summary>Name der Tour.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Nummer der Aktivität; <c>null</c> = noch keine gewählt.</summary>
    public long? AktivitaetId { get; init; }

    /// <summary>Nummern der gewählten Kategorien.</summary>
    public IReadOnlyList<long> KategorieIds { get; init; } = [];

    /// <summary>Nummer des Equipments; <c>null</c> = „(kein)“.</summary>
    public long? EquipmentId { get; init; }

    /// <summary>Distanz in km.</summary>
    public string Distanz { get; init; } = string.Empty;

    /// <summary>Höhenmeter.</summary>
    public string Hoehenmeter { get; init; } = string.Empty;

    /// <summary>Fahrzeit „h:mm:ss“.</summary>
    public string Fahrzeit { get; init; } = string.Empty;

    /// <summary>Höchstgeschwindigkeit in km/h.</summary>
    public string MaxKmh { get; init; } = string.Empty;

    /// <summary>Durchschnittlicher Puls.</summary>
    public string PulsDurchschnitt { get; init; } = string.Empty;

    /// <summary>Höchster Puls.</summary>
    public string PulsMax { get; init; } = string.Empty;

    /// <summary>Niedrigster Puls.</summary>
    public string PulsMin { get; init; } = string.Empty;

    /// <summary>Durchschnittliche Trittfrequenz.</summary>
    public string Trittfrequenz { get; init; } = string.Empty;

    /// <summary>Durchschnittstemperatur in °C.</summary>
    public string Temperatur { get; init; } = string.Empty;

    /// <summary>Durchschnittliche Leistung in Watt.</summary>
    public string Leistung { get; init; } = string.Empty;

    /// <summary>Körpergewicht in kg.</summary>
    public string Koerpergewicht { get; init; } = string.Empty;

    /// <summary>Erholungspuls.</summary>
    public string Erholungspuls { get; init; } = string.Empty;

    /// <summary>Höhe des Startpunkts in Metern.</summary>
    public string StartHoehe { get; init; } = string.Empty;

    /// <summary>Höchster Punkt der Tour in Metern.</summary>
    public string MaxHoehe { get; init; } = string.Empty;

    /// <summary>Kürzel des Trainingsbereichs; leer = keiner.</summary>
    public string Trainingsbereich { get; init; } = string.Empty;

    /// <summary>Freier Kommentar.</summary>
    public string Kommentar { get; init; } = string.Empty;

    /// <summary>
    /// Vergleicht alle Felder; die Kategorien ohne Rücksicht auf die Reihenfolge.
    /// </summary>
    /// <param name="other">Die anderen Eingaben.</param>
    /// <returns><c>true</c>, wenn alle Felder gleich sind.</returns>
    public virtual bool Equals(Einheiteneingaben? other) =>
        other is not null
        && Datum == other.Datum && Startzeit == other.Startzeit && Name == other.Name
        && AktivitaetId == other.AktivitaetId && EquipmentId == other.EquipmentId
        && Distanz == other.Distanz && Hoehenmeter == other.Hoehenmeter && Fahrzeit == other.Fahrzeit
        && MaxKmh == other.MaxKmh && PulsDurchschnitt == other.PulsDurchschnitt && PulsMax == other.PulsMax
        && PulsMin == other.PulsMin && Trittfrequenz == other.Trittfrequenz && Temperatur == other.Temperatur
        && Leistung == other.Leistung && Koerpergewicht == other.Koerpergewicht && Erholungspuls == other.Erholungspuls
        && StartHoehe == other.StartHoehe && MaxHoehe == other.MaxHoehe && Trainingsbereich == other.Trainingsbereich
        && Kommentar == other.Kommentar
        && KategorieIds.Order().SequenceEqual(other.KategorieIds.Order());

    /// <summary>Hashwert aus einigen Feldern; gleiche Eingaben ergeben denselben Wert.</summary>
    /// <returns>Der Hashwert.</returns>
    public override int GetHashCode() => HashCode.Combine(Datum, Name, AktivitaetId, KategorieIds.Count);
}
