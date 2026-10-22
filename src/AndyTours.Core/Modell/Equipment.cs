namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Ausrüstungsgegenstand, mit dem Trainingseinheiten absolviert werden – meist ein Fahrrad,
/// bei der Aktivität „Laufen" ein Paar Laufschuhe.
/// </summary>
/// <remarks>
/// Entspricht im Original den INI-Abschnitten <c>[Equipment.&lt;Name&gt;]</c>.
/// Die Beiwerte (<see cref="CrCoeff"/> … <see cref="SinCoeff"/>) werden später für die
/// Leistungsberechnung (Phase 5, entspricht <c>datamodel.PowerCalc</c>) benötigt und deshalb
/// unverändert aus dem Original übernommen, auch wenn ihre genaue Bedeutung erst dort geklärt wird.
/// </remarks>
public class Equipment
{
    /// <summary>Eindeutige Nummer in der Datenbank.</summary>
    public long Id { get; set; }

    /// <summary>
    /// Name des Equipments, z. B. „Genius/Bike". Der Teil hinter einem Schrägstrich ist im
    /// Original ein Namenszusatz; manche Altdateien verwenden nur den Teil davor.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Aktivität, für die das Equipment verwendet wird (Verweis auf <see cref="Aktivitaet.Id"/>).</summary>
    public long? AktivitaetId { get; set; }

    /// <summary>Gewicht des Equipments in Kilogramm (geht in die Leistungsberechnung ein).</summary>
    public double GewichtKg { get; set; }

    /// <summary>
    /// Radumfang in Millimetern. Im Original heißt der Schlüssel <c>wheeldiameter</c>,
    /// die Werte (z. B. 2133) sind aber eindeutig Umfänge.
    /// </summary>
    public int RadumfangMm { get; set; }

    /// <summary>Rollwiderstandsbeiwert (Original: <c>crCoeff</c>).</summary>
    public double CrCoeff { get; set; }

    /// <summary>Beiwert aus dem Original (<c>cmCoeff</c>), Bedeutung wird in Phase 5 geklärt.</summary>
    public double CmCoeff { get; set; }

    /// <summary>Luftwiderstandsbeiwert c<sub>w</sub> (Original: <c>cwCoeff</c>).</summary>
    public double CwCoeff { get; set; }

    /// <summary>Luftwiderstandsfläche c<sub>w</sub>·A in m² (Original: <c>cwaCoeff</c>).</summary>
    public double CwaCoeff { get; set; }

    /// <summary>Beiwert aus dem Original (<c>sinCoeff</c>), Bedeutung wird in Phase 5 geklärt.</summary>
    public double SinCoeff { get; set; }

    /// <summary>Zähnezahlen der Kettenblätter, z. B. 39 und 53 (für den Ritzelrechner).</summary>
    public List<int> Kettenblaetter { get; set; } = new();

    /// <summary>Zähnezahlen der Ritzel (für den Ritzelrechner).</summary>
    public List<int> Ritzel { get; set; } = new();

    /// <summary>
    /// Gerätezuordnung aus dem Original (<c>assignment</c>, z. B. „Bike1"): Kennung, unter der
    /// ein Radcomputer dieses Rad führt. Wird für die automatische Zuordnung beim Import genutzt.
    /// </summary>
    public string Geraetezuordnung { get; set; } = string.Empty;

    /// <summary>Kategorie, die bei neuen Einheiten mit diesem Equipment vorgeschlagen wird (leer = keine).</summary>
    public string StandardKategorie { get; set; } = string.Empty;

    /// <summary><c>true</c>, wenn dieses Equipment bei neuen Einheiten vorausgewählt wird.</summary>
    public bool IstStandard { get; set; }

    /// <summary><c>true</c>, wenn das Equipment nicht mehr benutzt wird („ausgemustert").</summary>
    public bool Ausgemustert { get; set; }

    /// <summary>Anschaffungsdatum, falls bekannt.</summary>
    public DateOnly? Anschaffungsdatum { get; set; }

    /// <summary>Pfad zu einem Foto des Equipments (leer = keines).</summary>
    public string Bildpfad { get; set; } = string.Empty;

    /// <summary>
    /// Kilometerstand bei Übernahme in die Verwaltung (Original: <c>initDst</c>); wird zur
    /// Gesamtlaufleistung aus den Trainingseinheiten addiert.
    /// </summary>
    public double AnfangsdistanzKm { get; set; }
}
