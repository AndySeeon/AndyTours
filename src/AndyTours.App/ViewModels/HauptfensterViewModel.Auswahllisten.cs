using System.Globalization;
using System.Windows;
using Microsoft.Data.Sqlite;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: die in den Einstellungen gepflegten Auswahllisten – Trainingsbereiche,
/// Aktivitäten und Kategorien – lesen, prüfen und ändern.
/// </summary>
/// <remarks>
/// Das Hauptfenster hält die Listen, weil es den Datenbestand hält: Ein Umbenennen oder Zusammenführen
/// ändert auch die Einheiten, und Werkzeugleiste, Filter, Tabelle und Summen müssen danach neu aufgebaut
/// werden. Das Einstellungsfenster erhält dafür die Schnittstelle <see cref="IAuswahllistenSpeicher"/>,
/// die hier ausdrücklich (nur über die Schnittstelle erreichbar) umgesetzt ist.
/// <para>
/// Ablauf jeder Änderung: zuerst in einer Transaktion in die Datenbank schreiben (über <see cref="Schreibe"/>),
/// erst nach Erfolg den Bestand im Speicher ändern und die Anzeige aktualisieren. Schlägt das Schreiben
/// fehl, erscheint eine Fehlermeldung, und alles bleibt beim vorherigen Stand.
/// </para>
/// </remarks>
public partial class HauptfensterViewModel : IAuswahllistenSpeicher
{
    /// <summary>Fehlertext, wenn eine Änderung einer Auswahlliste nicht gespeichert werden konnte.</summary>
    private const string Speicherfehler = "Die Auswahlliste konnte nicht gespeichert werden";

    /// <summary>Die gepflegte Liste der Trainingsbereiche in der Reihenfolge des Benutzers.</summary>
    private List<string> _trainingsbereiche = [];

    /// <summary>
    /// Solange <c>true</c>, löst ein Wechsel der gewählten Aktivität keinen Neuaufbau aus
    /// (während die Aktivitätsliste der Werkzeugleiste neu gefüllt wird).
    /// </summary>
    private bool _aktivitaetenWerdenGefuellt;

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

    /// <summary>
    /// Füllt die Aktivitätsauswahl der Werkzeugleiste in der festgelegten Reihenfolge neu und wählt eine
    /// Aktivität aus.
    /// </summary>
    /// <remarks>
    /// Die Auswahl wird über die Id gehalten, damit sie ein Umbenennen übersteht. Während des Füllens
    /// wird nichts neu berechnet; wer die Tabelle neu braucht, ruft danach <see cref="AktualisiereSummen"/>.
    /// </remarks>
    /// <param name="gewaehlteId">Id der zu wählenden Aktivität; <c>null</c> oder unbekannt = „alle Aktivitäten“.</param>
    private void FuelleAktivitaeten(long? gewaehlteId)
    {
        string gewaehlt = _bestand.Aktivitaeten.FirstOrDefault(a => a.Id == gewaehlteId)?.Name ?? AlleAktivitaeten;
        _aktivitaetenWerdenGefuellt = true;
        try
        {
            Aktivitaeten.Clear();
            Aktivitaeten.Add(AlleAktivitaeten);
            foreach (Aktivitaet aktivitaet in _bestand.Aktivitaeten)
            {
                Aktivitaeten.Add(aktivitaet.Name);
            }

            // Die Auswahlliste der Oberfläche hat die Wahl beim Leeren verloren – auch bei gleichem Wert neu melden
            AusgewaehlteAktivitaet = gewaehlt;
            OnPropertyChanged(nameof(AusgewaehlteAktivitaet));
        }
        finally
        {
            _aktivitaetenWerdenGefuellt = false;
        }
    }

    /// <summary>Id der in der Werkzeugleiste gewählten Aktivität; <c>null</c> bei „alle Aktivitäten“.</summary>
    private long? GewaehlteAktivitaetId() => _bestand.Aktivitaeten.FirstOrDefault(a => a.Name == AusgewaehlteAktivitaet)?.Id;

    /// <summary>Erzeugt das ViewModel des Bereichs „Auswahllisten“ im Einstellungsfenster.</summary>
    private AuswahllistenViewModel ErstelleAuswahllistenViewModel() => new(this, WaehleZusammenfuehrungsziel);

    /// <summary>
    /// Öffnet das Fenster „Zusammenführen mit …“ und liefert das gewählte Ziel.
    /// </summary>
    /// <param name="ursprung">Der Eintrag, der entfernt werden soll.</param>
    /// <param name="ziele">Die übrigen Einträge derselben Liste.</param>
    /// <returns>Das Ziel oder <c>null</c> bei „Abbrechen“.</returns>
    private static string? WaehleZusammenfuehrungsziel(string ursprung, IReadOnlyList<string> ziele)
    {
        var viewModel = new ZusammenfuehrenViewModel(ursprung, ziele);
        var fenster = new ZusammenfuehrenFenster(viewModel)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow,
        };
        return fenster.ShowDialog() == true ? viewModel.Ergebnis : null;
    }

    /// <inheritdoc/>
    IReadOnlyList<Auswahllistenzeile> IAuswahllistenSpeicher.Lesen(Auswahlliste liste) => liste switch
    {
        Auswahlliste.Trainingsbereich => _trainingsbereiche.Select(b => new Auswahllistenzeile(b, b)).ToList(),
        Auswahlliste.Aktivitaet => _bestand.Aktivitaeten
            .Select(a => new Auswahllistenzeile(a.Name, a.IstHaupt ? $"{a.Name} (Hauptaktivität)" : a.Name))
            .ToList(),
        Auswahlliste.Kategorie => _bestand.Kategorien
            .OrderBy(k => k.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(k => new Auswahllistenzeile(k.Name, k.IstStandard ? $"{k.Name} (Standard)" : k.Name))
            .ToList(),
        _ => throw new ArgumentOutOfRangeException(nameof(liste), liste, null),
    };

    /// <inheritdoc/>
    /// <remarks>Bei Kategorien zählen nur die Einheiten; die Rückfrage nennt keine Equipments.</remarks>
    (int Einheiten, int Equipments) IAuswahllistenSpeicher.ZaehleVerwendung(Auswahlliste liste, string name) => liste switch
    {
        Auswahlliste.Trainingsbereich => (ZaehleTrainingsbereich.Ausfuehren(_bestand.Trainingseinheiten, name), 0),
        Auswahlliste.Aktivitaet => FindeAktivitaet(name) is { } a ? ZaehleVerwendung.Aktivitaet(_bestand, a.Id) : (0, 0),
        Auswahlliste.Kategorie => FindeKategorie(name) is { } k ? (ZaehleVerwendung.Kategorie(_bestand, k.Id).Einheiten, 0) : (0, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(liste), liste, null),
    };

    /// <inheritdoc/>
    string? IAuswahllistenSpeicher.ErmittleLoeschsperre(Auswahlliste liste, string name)
    {
        switch (liste)
        {
            case Auswahlliste.Trainingsbereich:
                int anzahl = ZaehleTrainingsbereich.Ausfuehren(_bestand.Trainingseinheiten, name);
                return anzahl == 0
                    ? null
                    : string.Format(CultureInfo.GetCultureInfo("de-DE"), "‚{0}‘ wird in {1:N0} {2} verwendet und kann nicht gelöscht werden.",
                        name, anzahl, anzahl == 1 ? "Einheit" : "Einheiten");
            case Auswahlliste.Aktivitaet:
                return FindeAktivitaet(name) is { } a ? ErmittleLoeschsperre.Aktivitaet(_bestand, a.Id) : null;
            case Auswahlliste.Kategorie:
                return FindeKategorie(name) is { } k ? ErmittleLoeschsperre.Kategorie(_bestand, k.Id) : null;
            default:
                throw new ArgumentOutOfRangeException(nameof(liste), liste, null);
        }
    }

    /// <inheritdoc/>
    bool IAuswahllistenSpeicher.Hinzufuegen(Auswahlliste liste, string name)
    {
        string neu = name.Trim();
        switch (liste)
        {
            case Auswahlliste.Trainingsbereich:
                return SpeichereTrainingsbereiche([.. _trainingsbereiche, neu], null);

            case Auswahlliste.Aktivitaet:
            {
                var aktivitaet = new Aktivitaet { Name = neu };
                bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
                {
                    aktivitaet.Id = AktivitaetenTabelle.Einfuegen(verbindung, transaktion, aktivitaet);
                    AktivitaetenReihenfolgeEinstellung.Speichern(verbindung, transaktion, [.. _bestand.Aktivitaeten.Select(a => a.Id), aktivitaet.Id]);
                });
                if (gespeichert)
                {
                    _bestand.Aktivitaeten.Add(aktivitaet);
                    FuelleAktivitaeten(GewaehlteAktivitaetId());
                }

                return gespeichert;
            }

            case Auswahlliste.Kategorie:
            {
                var kategorie = new Kategorie { Name = neu };
                bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
                    kategorie.Id = KategorienTabelle.Einfuegen(verbindung, transaktion, kategorie));
                if (gespeichert)
                {
                    _bestand.Kategorien.Add(kategorie);
                }

                return gespeichert;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(liste), liste, null);
        }
    }

    /// <inheritdoc/>
    bool IAuswahllistenSpeicher.Umbenennen(Auswahlliste liste, string alt, string neu)
    {
        string neuerName = neu.Trim();
        switch (liste)
        {
            case Auswahlliste.Trainingsbereich:
            {
                List<string> eintraege = [.. _trainingsbereiche];
                int stelle = eintraege.IndexOf(alt);
                if (stelle < 0)
                {
                    return false;
                }

                eintraege[stelle] = neuerName;
                bool verwendet = ZaehleTrainingsbereich.Ausfuehren(_bestand.Trainingseinheiten, alt) > 0;
                return SpeichereTrainingsbereiche(eintraege, verwendet ? (alt, neuerName) : null);
            }

            case Auswahlliste.Aktivitaet:
            {
                if (FindeAktivitaet(alt) is not { } aktivitaet)
                {
                    return false;
                }

                long? gewaehlt = GewaehlteAktivitaetId();
                bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
                    AktivitaetenTabelle.Aendern(verbindung, transaktion,
                        new Aktivitaet { Id = aktivitaet.Id, Name = neuerName, IstHaupt = aktivitaet.IstHaupt }));
                if (gespeichert)
                {
                    aktivitaet.Name = neuerName;
                    FuelleAktivitaeten(gewaehlt);
                    AktualisiereSummen();
                }

                return gespeichert;
            }

            case Auswahlliste.Kategorie:
            {
                if (FindeKategorie(alt) is not { } kategorie)
                {
                    return false;
                }

                bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
                {
                    KategorienTabelle.Aendern(verbindung, transaktion,
                        new Kategorie { Id = kategorie.Id, Name = neuerName, IstStandard = kategorie.IstStandard });
                    EquipmentsTabelle.ErsetzeStandardKategorie(verbindung, transaktion, alt, neuerName);
                });
                if (gespeichert)
                {
                    BenenneKategorieUm.Ausfuehren(_bestand, kategorie.Id, neuerName);
                    Filter.ErsetzeKategorie(alt, neuerName);
                    AktualisiereSummen();
                }

                return gespeichert;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(liste), liste, null);
        }
    }

    /// <inheritdoc/>
    bool IAuswahllistenSpeicher.Loeschen(Auswahlliste liste, string name)
    {
        switch (liste)
        {
            case Auswahlliste.Trainingsbereich:
                return SpeichereTrainingsbereiche(_trainingsbereiche.Where(b => b != name).ToList(), null);

            case Auswahlliste.Aktivitaet:
            {
                if (FindeAktivitaet(name) is not { } aktivitaet)
                {
                    return false;
                }

                long? gewaehlt = GewaehlteAktivitaetId();
                bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
                {
                    AktivitaetenTabelle.Loeschen(verbindung, transaktion, aktivitaet.Id);
                    AktivitaetenReihenfolgeEinstellung.Speichern(verbindung, transaktion,
                        _bestand.Aktivitaeten.Where(a => a.Id != aktivitaet.Id).Select(a => a.Id));
                });
                if (gespeichert)
                {
                    _bestand.Aktivitaeten.Remove(aktivitaet);
                    FuelleAktivitaeten(gewaehlt);
                }

                return gespeichert;
            }

            case Auswahlliste.Kategorie:
            {
                if (FindeKategorie(name) is not { } kategorie)
                {
                    return false;
                }

                bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
                    KategorienTabelle.Loeschen(verbindung, transaktion, kategorie.Id));
                if (gespeichert)
                {
                    _bestand.Kategorien.Remove(kategorie);
                }

                return gespeichert;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(liste), liste, null);
        }
    }

    /// <inheritdoc/>
    bool IAuswahllistenSpeicher.Verschieben(Auswahlliste liste, IReadOnlyList<string> reihenfolge)
    {
        switch (liste)
        {
            case Auswahlliste.Trainingsbereich:
                return SpeichereTrainingsbereiche(reihenfolge.ToList(), null);

            case Auswahlliste.Aktivitaet:
            {
                List<Aktivitaet> geordnet = reihenfolge.Select(FindeAktivitaet).OfType<Aktivitaet>().ToList();
                long? gewaehlt = GewaehlteAktivitaetId();
                bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
                    AktivitaetenReihenfolgeEinstellung.Speichern(verbindung, transaktion, geordnet.Select(a => a.Id)));
                if (gespeichert)
                {
                    _bestand.Aktivitaeten = OrdneAktivitaeten.Ausfuehren(_bestand.Aktivitaeten, geordnet.Select(a => a.Id).ToList());
                    FuelleAktivitaeten(gewaehlt);
                }

                return gespeichert;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(liste), liste, null);
        }
    }

    /// <inheritdoc/>
    bool IAuswahllistenSpeicher.SetzeHauptaktivitaet(string name)
    {
        if (FindeAktivitaet(name) is not { } haupt)
        {
            return false;
        }

        bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
        {
            foreach (Aktivitaet a in _bestand.Aktivitaeten)
            {
                AktivitaetenTabelle.Aendern(verbindung, transaktion, new Aktivitaet { Id = a.Id, Name = a.Name, IstHaupt = a.Id == haupt.Id });
            }
        });
        if (gespeichert)
        {
            SetzeHauptaktivitaet.Ausfuehren(_bestand.Aktivitaeten, haupt.Id);
        }

        return gespeichert;
    }

    /// <inheritdoc/>
    bool IAuswahllistenSpeicher.SchalteStandardUm(string name)
    {
        if (FindeKategorie(name) is not { } kategorie)
        {
            return false;
        }

        bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
            KategorienTabelle.Aendern(verbindung, transaktion,
                new Kategorie { Id = kategorie.Id, Name = kategorie.Name, IstStandard = !kategorie.IstStandard }));
        if (gespeichert)
        {
            kategorie.IstStandard = !kategorie.IstStandard;
        }

        return gespeichert;
    }

    /// <inheritdoc/>
    bool IAuswahllistenSpeicher.Zusammenfuehren(Auswahlliste liste, string ursprung, string ziel)
    {
        const string Fehler = "Die Einträge konnten nicht zusammengeführt werden";
        switch (liste)
        {
            case Auswahlliste.Aktivitaet:
            {
                if (FindeAktivitaet(ursprung) is not { } von || FindeAktivitaet(ziel) is not { } nach || von == nach)
                {
                    return false;
                }

                long? gewaehlt = GewaehlteAktivitaetId();
                bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
                {
                    AktivitaetenTabelle.FuehreZusammen(verbindung, transaktion, von.Id, nach.Id);
                    AktivitaetenReihenfolgeEinstellung.Speichern(verbindung, transaktion,
                        _bestand.Aktivitaeten.Where(a => a.Id != von.Id).Select(a => a.Id));
                }, Fehler);
                if (gespeichert)
                {
                    FuehreAktivitaetenZusammen.Ausfuehren(_bestand, von.Id, nach.Id);

                    // War der Ursprung gewählt, zeigt die Werkzeugleiste danach das Ziel
                    FuelleAktivitaeten(gewaehlt == von.Id ? nach.Id : gewaehlt);
                    AktualisiereSummen();
                }

                return gespeichert;
            }

            case Auswahlliste.Kategorie:
            {
                if (FindeKategorie(ursprung) is not { } von || FindeKategorie(ziel) is not { } nach || von == nach)
                {
                    return false;
                }

                bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
                {
                    KategorienTabelle.FuehreZusammen(verbindung, transaktion, von.Id, nach.Id);
                    EquipmentsTabelle.ErsetzeStandardKategorie(verbindung, transaktion, von.Name, nach.Name);
                }, Fehler);
                if (gespeichert)
                {
                    FuehreKategorienZusammen.Ausfuehren(_bestand, von.Id, nach.Id);
                    Filter.ErsetzeKategorie(von.Name, nach.Name);
                    AktualisiereSummen();
                }

                return gespeichert;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(liste), liste, null);
        }
    }

    /// <summary>Sucht eine Aktivität über ihren Namen.</summary>
    private Aktivitaet? FindeAktivitaet(string name) => _bestand.Aktivitaeten.FirstOrDefault(a => a.Name == name);

    /// <summary>Sucht eine Kategorie über ihren Namen.</summary>
    private Kategorie? FindeKategorie(string name) => _bestand.Kategorien.FirstOrDefault(k => k.Name == name);

    /// <summary>
    /// Führt Schreibschritte in einer gemeinsamen Transaktion aus – entweder alle oder keiner.
    /// </summary>
    /// <param name="schreiben">Die Schreibschritte.</param>
    /// <param name="fehlertext">Einleitung der Fehlermeldung.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    private bool SchreibeInTransaktion(Action<SqliteConnection, SqliteTransaction> schreiben, string fehlertext = Speicherfehler) =>
        Schreibe(verbindung =>
        {
            using SqliteTransaction transaktion = verbindung.BeginTransaction();
            schreiben(verbindung, transaktion);
            transaktion.Commit();
        }, fehlertext);

    /// <summary>
    /// Speichert die geänderte Liste der Trainingsbereiche; beim Umbenennen eines verwendeten Eintrags
    /// zusätzlich die Einheiten.
    /// </summary>
    /// <remarks>
    /// Liste und Einheiten werden in einer Transaktion geschrieben. Erst danach ändern sich die Liste und
    /// die Einheiten im Speicher; ein auf den alten Namen gesetzter Filter wird umgestellt, und Tabelle,
    /// Filterlisten und Summen werden neu aufgebaut.
    /// </remarks>
    /// <param name="eintraege">Die neuen Einträge in ihrer Reihenfolge.</param>
    /// <param name="umbenennung">Alter und neuer Name, wenn Einheiten mit umbenannt werden sollen; sonst <c>null</c>.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    private bool SpeichereTrainingsbereiche(List<string> eintraege, (string Alt, string Neu)? umbenennung)
    {
        bool gespeichert = SchreibeInTransaktion((verbindung, transaktion) =>
        {
            AuswahllisteEinstellung.Speichern(verbindung, transaktion, Auswahlliste.Trainingsbereich, eintraege);
            if (umbenennung is { } u)
            {
                TrainingseinheitenTabelle.ErsetzeTrainingsbereich(verbindung, transaktion, u.Alt, u.Neu);
            }
        });
        if (!gespeichert)
        {
            return false;
        }

        _trainingsbereiche = eintraege;
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
