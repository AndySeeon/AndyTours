using Microsoft.Data.Sqlite;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: die in den Einstellungen gepflegten Auswahllisten (zunächst die
/// Trainingsbereiche) laden, zählen und speichern.
/// </summary>
/// <remarks>
/// Das Hauptfenster hält die Listen, weil es den Datenbestand hält: Ein Umbenennen ändert auch die
/// Einheiten, und Tabelle, Filter und Summen müssen danach neu aufgebaut werden. Das Einstellungsfenster
/// erhält dafür die Funktionen <see cref="LeseAuswahlliste"/>, <see cref="ZaehleVerwendungen"/> und
/// <see cref="SpeichereAuswahlliste"/>.
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Die gepflegte Liste der Trainingsbereiche in der Reihenfolge des Benutzers.</summary>
    private List<string> _trainingsbereiche = [];

    /// <summary>
    /// Liest die Liste der Trainingsbereiche; wurde sie noch nie angelegt, wird sie aus den vorkommenden
    /// Kürzeln gebildet und sofort gespeichert.
    /// </summary>
    /// <remarks>
    /// Das greift beim ersten Start mit dieser Funktion, nach dem Wiederherstellen einer älteren Sicherung
    /// und in einem anderen Datenordner. Eine vom Benutzer geleerte Liste bleibt leer.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="bestand">Der eben geladene Datenbestand.</param>
    /// <returns>Die Liste.</returns>
    private static List<string> LadeTrainingsbereiche(SqliteConnection verbindung, Datenbestand bestand)
    {
        List<string>? liste = AuswahllisteEinstellung.Laden(verbindung, Auswahlliste.Trainingsbereich);
        if (liste is null)
        {
            liste = ErmittleTrainingsbereiche.Ausfuehren(bestand.Trainingseinheiten);
            AuswahllisteEinstellung.Speichern(verbindung, null, Auswahlliste.Trainingsbereich, liste);
        }

        return liste;
    }

    /// <summary>Erzeugt das ViewModel des Bereichs „Auswahllisten“ im Einstellungsfenster.</summary>
    private AuswahllistenViewModel ErstelleAuswahllistenViewModel() =>
        new(LeseAuswahlliste, ZaehleVerwendungen, SpeichereAuswahlliste);

    /// <summary>Liefert die aktuellen Einträge einer Liste.</summary>
    /// <param name="liste">Die Liste.</param>
    /// <returns>Die Einträge.</returns>
    private List<string> LeseAuswahlliste(Auswahlliste liste) => liste switch
    {
        Auswahlliste.Trainingsbereich => _trainingsbereiche,
        _ => throw new ArgumentOutOfRangeException(nameof(liste), liste, null),
    };

    /// <summary>Zählt die Einheiten, die einen Eintrag einer Liste verwenden.</summary>
    /// <param name="liste">Die Liste.</param>
    /// <param name="eintrag">Der Eintrag.</param>
    /// <returns>Die Anzahl der Einheiten.</returns>
    private int ZaehleVerwendungen(Auswahlliste liste, string eintrag) => liste switch
    {
        Auswahlliste.Trainingsbereich => ZaehleTrainingsbereich.Ausfuehren(_bestand.Trainingseinheiten, eintrag),
        _ => throw new ArgumentOutOfRangeException(nameof(liste), liste, null),
    };

    /// <summary>
    /// Speichert eine geänderte Liste; beim Umbenennen eines verwendeten Eintrags zusätzlich die Einheiten.
    /// </summary>
    /// <remarks>
    /// Liste und Einheiten werden in einer Transaktion geschrieben – entweder beides oder nichts. Erst danach
    /// ändern sich die Liste und die Einheiten im Speicher; ein auf den alten Namen gesetzter Filter wird
    /// umgestellt, und Tabelle, Filterlisten und Summen werden neu aufgebaut. Schlägt das Schreiben fehl,
    /// erscheint eine Fehlermeldung, und alles bleibt beim vorherigen Stand.
    /// </remarks>
    /// <param name="liste">Die Liste.</param>
    /// <param name="eintraege">Die neuen Einträge in ihrer Reihenfolge.</param>
    /// <param name="umbenennung">Alter und neuer Name, wenn Einheiten mit umbenannt werden sollen; sonst <c>null</c>.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    private bool SpeichereAuswahlliste(Auswahlliste liste, IReadOnlyList<string> eintraege, (string Alt, string Neu)? umbenennung)
    {
        bool gespeichert = Schreibe(verbindung =>
        {
            using SqliteTransaction transaktion = verbindung.BeginTransaction();
            AuswahllisteEinstellung.Speichern(verbindung, transaktion, liste, eintraege);
            if (umbenennung is { } u)
            {
                TrainingseinheitenTabelle.ErsetzeTrainingsbereich(verbindung, transaktion, u.Alt, u.Neu);
            }

            transaktion.Commit();
        }, "Die Auswahlliste konnte nicht gespeichert werden");
        if (!gespeichert)
        {
            return false;
        }

        _trainingsbereiche = eintraege.ToList();
        if (umbenennung is { } umbenannt && BenenneTrainingsbereichUm.Ausfuehren(_bestand.Trainingseinheiten, umbenannt.Alt, umbenannt.Neu) > 0)
        {
            if (string.Equals(Filter.Trainingsbereich, umbenannt.Alt.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                Filter.Trainingsbereich = umbenannt.Neu;
            }

            AktualisiereSummen();
        }

        return true;
    }
}
