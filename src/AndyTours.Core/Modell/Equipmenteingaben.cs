namespace AndyTours.Core.Modell;

/// <summary>
/// Die Eingaben eines Equipments, wie sie im Fenster „Equipment“ stehen.
/// </summary>
/// <remarks>
/// Anschaffungsdatum und Anfangskilometer sind Text, weil der Benutzer dort auch Ungültiges tippen kann;
/// geprüft und umgewandelt wird mit <see cref="Berechnungen.EquipmentEingaben.AlsEquipment"/>. Aktivität
/// und Standardkategorie werden aus Listen gewählt. Ein leeres Feld bedeutet „nicht erfasst“.
/// <para>
/// Nicht enthalten sind die Werte, die AndyTours nicht pflegt (Gewicht, Radumfang, Widerstandswerte,
/// Kettenblätter, Ritzel, Bild); sie bleiben beim Speichern unverändert erhalten.
/// </para>
/// <para>
/// Die Gerätezuordnung erscheint als Liste der gemerkten Rad-Kennungen und Sensoren
/// (<see cref="Radkennungen"/>); im Fenster lassen sich nur einzelne entfernen, gemerkt werden sie beim Import.
/// </para>
/// </remarks>
public record Equipmenteingaben
{
    /// <summary>Name, z. B. „Genius“.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Nummer der Aktivität; <c>null</c> = keine (das Equipment passt zu jeder Aktivität).</summary>
    public long? AktivitaetId { get; init; }

    /// <summary>Name der Standardkategorie; leer = keine.</summary>
    public string StandardKategorie { get; init; } = string.Empty;

    /// <summary>Anschaffungsdatum, z. B. „13.04.2016“; leer = unbekannt.</summary>
    public string Anschaffungsdatum { get; init; } = string.Empty;

    /// <summary>Kilometerstand bei der Anschaffung, z. B. „1200“; leer = 0.</summary>
    public string Anfangskilometer { get; init; } = string.Empty;

    /// <summary><c>true</c>, wenn das Equipment Standard seiner Aktivität sein soll.</summary>
    public bool IstStandard { get; init; }

    /// <summary><c>true</c>, wenn das Equipment ausgemustert ist.</summary>
    public bool Ausgemustert { get; init; }

    /// <summary>Die gemerkten Rad-Kennungen und Sensoren (Gerätezuordnung); leer = keine.</summary>
    public IReadOnlyList<Radkennung> Radkennungen { get; init; } = [];

    /// <summary>
    /// Vergleicht alle Eingaben, die Rad-Kennungen der Reihe nach.
    /// </summary>
    /// <remarks>
    /// Ohne diese Methode verglichen Datensätze die Liste nur als Verweis: Zwei gleiche Eingaben mit je einer
    /// eigenen Liste wären dann verschieden.
    /// </remarks>
    /// <param name="other">Die anderen Eingaben.</param>
    /// <returns><c>true</c>, wenn alle Werte gleich sind.</returns>
    public virtual bool Equals(Equipmenteingaben? other) =>
        other is not null
        && Name == other.Name
        && AktivitaetId == other.AktivitaetId
        && StandardKategorie == other.StandardKategorie
        && Anschaffungsdatum == other.Anschaffungsdatum
        && Anfangskilometer == other.Anfangskilometer
        && IstStandard == other.IstStandard
        && Ausgemustert == other.Ausgemustert
        && Radkennungen.SequenceEqual(other.Radkennungen);

    /// <summary>Hashwert passend zu <see cref="Equals(Equipmenteingaben?)"/>.</summary>
    /// <returns>Der Hashwert.</returns>
    public override int GetHashCode() =>
        HashCode.Combine(Name, AktivitaetId, StandardKategorie, Anschaffungsdatum, Anfangskilometer, IstStandard, Ausgemustert, Radkennungen.Count);
}
