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
/// Kettenblätter, Ritzel, Bild, Gerätezuordnung); sie bleiben beim Speichern unverändert erhalten.
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
}
