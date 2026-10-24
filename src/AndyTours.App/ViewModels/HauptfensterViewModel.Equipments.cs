using System.Windows;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: die Equipments der Auswahlliste „Equipment“ anzeigen, im Fenster
/// „Equipment“ anlegen und bearbeiten sowie löschen.
/// </summary>
/// <remarks>
/// Ablauf wie bei den übrigen Auswahllisten (siehe <c>HauptfensterViewModel.Auswahllisten.cs</c>): zuerst
/// in einer Transaktion in die Datenbank schreiben, erst nach Erfolg den Bestand im Speicher ändern und
/// Tabelle, Filter und Summen neu aufbauen. Die Einheiten verweisen über die Nummer auf ihr Equipment;
/// ein neuer Name erscheint deshalb ohne weitere Änderung überall.
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>
    /// Liefert die Zeilen der Liste „Equipment“: zuerst die nicht ausgemusterten, dann die ausgemusterten,
    /// jeweils alphabetisch, mit Aktivität und den Zusätzen „(Standard)“ bzw. „(ausgemustert)“,
    /// z. B. „Genius – Bike (Standard)“.
    /// </summary>
    /// <returns>Die Zeilen.</returns>
    private List<Auswahllistenzeile> LeseEquipments() => _bestand.Equipments
        .OrderBy(e => e.Ausgemustert)
        .ThenBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
        .Select(e => new Auswahllistenzeile(e.Name, Anzeige(e)))
        .ToList();

    /// <summary>Anzeigetext einer Zeile der Liste „Equipment“.</summary>
    /// <param name="equipment">Das Equipment.</param>
    /// <returns>Name, Aktivität und Zusätze.</returns>
    private string Anzeige(Equipment equipment)
    {
        string text = equipment.Name;
        if (_bestand.Aktivitaeten.FirstOrDefault(a => a.Id == equipment.AktivitaetId) is { } aktivitaet)
        {
            text += $" – {aktivitaet.Name}";
        }

        if (ErmittleStandardEquipment.IstWirksam(equipment))
        {
            text += " (Standard)";
        }

        if (equipment.Ausgemustert)
        {
            text += " (ausgemustert)";
        }

        return text;
    }

    /// <inheritdoc/>
    string? IAuswahllistenSpeicher.BearbeiteEquipment(string? name)
    {
        // Ein neues Equipment gehört zunächst zur Hauptaktivität; im Fenster lässt sich das ändern
        Equipment basis = name is null
            ? new Equipment { AktivitaetId = _bestand.Aktivitaeten.FirstOrDefault(a => a.IstHaupt)?.Id }
            : FindeEquipment(name) ?? new Equipment();
        string? gespeichert = null;

        var viewModel = new EquipmentBearbeitenViewModel(
            EquipmentEingaben.AlsEingaben(basis),
            _bestand.Aktivitaeten.Select(a => new Auswahleintrag(a.Id, a.Name)),
            _bestand.Kategorien.Select(k => k.Name).OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase),
            basis.Id == 0 ? "Neues Equipment" : "Equipment bearbeiten",
            eingaben => EquipmentEingaben.AlsEquipment(eingaben, basis, _bestand).Hinweis,
            eingaben => SpeichereEquipment(eingaben, basis, out gespeichert));
        var fenster = new EquipmentBearbeitenFenster(viewModel)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow,
        };
        return fenster.ShowDialog() == true ? gespeichert : null;
    }

    /// <summary>
    /// Speichert die Eingaben des Fensters „Equipment“: das Equipment selbst und, wenn es Standard wird,
    /// die anderen Equipments derselben Aktivität ohne Standard – alles in einer Transaktion.
    /// </summary>
    /// <param name="eingaben">Die Eingaben des Fensters.</param>
    /// <param name="basis">Das bisherige Equipment bzw. ein neues (Nummer 0).</param>
    /// <param name="name">Der gespeicherte Name.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    private bool SpeichereEquipment(Equipmenteingaben eingaben, Equipment basis, out string? name)
    {
        name = null;
        if (EquipmentEingaben.AlsEquipment(eingaben, basis, _bestand).Equipment is not Equipment equipment)
        {
            return false; // das Fenster sperrt „OK“ bei ungültigen Eingaben; nur zur Sicherheit
        }

        List<Equipment> aenderungen = UebernimmEquipment.ErmittleAenderungen(_bestand, equipment);
        bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
        {
            foreach (Equipment geaendert in aenderungen)
            {
                if (geaendert.Id == 0)
                {
                    geaendert.Id = EquipmentsTabelle.Einfuegen(verbindung, transaktion, geaendert);
                }
                else
                {
                    EquipmentsTabelle.Aendern(verbindung, transaktion, geaendert);
                }
            }
        });
        if (!gespeichert)
        {
            return false;
        }

        UebernimmEquipment.Einsetzen(_bestand, aenderungen);
        if (basis.Id != 0 && basis.Name != equipment.Name)
        {
            Filter.ErsetzeEquipment(basis.Name, equipment.Name);
        }

        AktualisiereSummen();
        name = equipment.Name;
        return true;
    }

    /// <summary>Löscht ein Equipment, das die Löschsperre freigegeben hat.</summary>
    /// <param name="name">Name des Equipments.</param>
    /// <returns><c>true</c>, wenn gelöscht wurde.</returns>
    private bool LoescheEquipment(string name)
    {
        if (FindeEquipment(name) is not { } equipment)
        {
            return false;
        }

        bool geloescht = SchreibeInTransaktion((verbindung, transaktion) => EquipmentsTabelle.Loeschen(verbindung, transaktion, equipment.Id));
        if (geloescht)
        {
            _bestand.Equipments.Remove(equipment);
        }

        return geloescht;
    }

    /// <summary>Sucht ein Equipment nach seinem Namen.</summary>
    private Equipment? FindeEquipment(string name) => _bestand.Equipments.FirstOrDefault(e => e.Name == name);
}
