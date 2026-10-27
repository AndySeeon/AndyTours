using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Eine gelesene Zeile aus <c>workouts.csv</c>, bevor die Namen in Datenbanknummern umgesetzt sind.
/// </summary>
/// <remarks>
/// Das Original verweist auf Aktivität, Equipment und Kategorien über deren <b>Namen</b>.
/// AndyTours verwendet Nummern (Ids). Diese Klasse hält die Namen so lange fest, bis
/// <see cref="UebernehmeAltdaten"/> die passenden Stammdaten gefunden oder ergänzt hat.
/// </remarks>
public class AltWorkout
{
    /// <summary>Zeilennummer in der Datei (1-basiert, inklusive Kopfzeile) – für Meldungen im Bericht.</summary>
    public int Zeilennummer { get; set; }

    /// <summary>Die Trainingseinheit mit allen Kennwerten; Verweise (Ids) sind noch nicht gesetzt.</summary>
    public Trainingseinheit Einheit { get; set; } = new();

    /// <summary>Name der Aktivität, z. B. „Bike".</summary>
    public string AktivitaetName { get; set; } = string.Empty;

    /// <summary>Name des Equipments, z. B. „Genius/Bike"; leer, wenn keines angegeben ist.</summary>
    public string EquipmentName { get; set; } = string.Empty;

    /// <summary>Namen der Kategorien (im Original durch Komma getrennt).</summary>
    public List<string> KategorieNamen { get; set; } = new();

    /// <summary>Angegebene Anhänge mit ihrem Pfad, wie er in der Datei steht.</summary>
    public List<(AnhangTyp Typ, string Pfad)> Anhaenge { get; set; } = new();
}
