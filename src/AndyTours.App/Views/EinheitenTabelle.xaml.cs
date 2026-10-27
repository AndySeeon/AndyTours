using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AndyTours.App.ViewModels;
using AndyTours.Core.Modell;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind der Tabelle „Trainingseinheiten“.
/// </summary>
/// <remarks>
/// Enthält nur Darstellung, keine Fachlogik (die liegt in Core und im <see cref="HauptfensterViewModel"/>):
/// <list type="bullet">
/// <item>Startsortierung nach Datum und Startzeit, die neueste Einheit zuerst.</item>
/// <item>Kontextmenü der Spaltenüberschriften zum Ein- und Ausblenden; es bleibt beim Anklicken offen.
/// Die Wahl selbst verwaltet das ViewModel (<see cref="HauptfensterViewModel.AusgeblendeteSpalten"/>),
/// hier wird sie nur angewendet.</item>
/// <item>Reihenfolge und Breiten der Spalten: beim Laden aus <see cref="HauptfensterViewModel.Spaltenanordnung"/>
/// angewendet, nach dem Verschieben oder Verbreitern sowie beim Beenden (<see cref="MerkeAnordnung"/>)
/// über das ViewModel gespeichert.</item>
/// <item>Summenzeile: je Tabellenspalte eine Zelle, deren Breite und Sichtbarkeit an die Spalte gebunden
/// sind, die der Reihenfolge der Spalten folgt und beim waagrechten Blättern mitläuft. Die Spalten mit
/// Summe erhalten als Mindestbreite die Breite ihres Summentextes.</item>
/// <item>Bearbeiten: Die markierten Zeilen werden dem ViewModel gemeldet (für die Wählbarkeit der Befehle);
/// Doppelklick öffnet das Bearbeitungsfenster, Rechtsklick markiert die Zeile unter dem Mauszeiger, Entf
/// löscht (nicht während der Bearbeitung einer Zelle). Nach dem Speichern aus dem Fenster wird die
/// gespeicherte Zeile markiert und sichtbar gemacht.</item>
/// <item>Bearbeiten in der Zelle: Die Spalten sind nur lesend gebunden. Beim Beginn wird der Rohtext
/// (bzw. die Auswahlliste) eingesetzt, am Ende wird der Text über
/// <see cref="HauptfensterViewModel.AendereZelle"/> geprüft und gespeichert. Ein ungültiger Wert färbt
/// die Zelle rot und hält sie in Bearbeitung; nach dem Speichern wird die Tabelle erst nach dem Ende der
/// Bearbeitung neu aufgebaut.</item>
/// </list>
/// </remarks>
public partial class EinheitenTabelle : UserControl
{
    /// <summary>Bezeichnungen der Spalten im Kontextmenü (ausführlicher als die knappen Überschriften).</summary>
    private static readonly Dictionary<Einheitenspalte, string> Bezeichnungen = new()
    {
        [Einheitenspalte.Datum] = "Datum",
        [Einheitenspalte.Startzeit] = "Startzeit",
        [Einheitenspalte.Name] = "Name",
        [Einheitenspalte.Aktivitaet] = "Aktivität",
        [Einheitenspalte.Kategorien] = "Kategorien",
        [Einheitenspalte.Equipment] = "Equipment",
        [Einheitenspalte.Distanz] = "Distanz (km)",
        [Einheitenspalte.Hoehenmeter] = "Höhenmeter (hm)",
        [Einheitenspalte.Fahrzeit] = "Fahrzeit",
        [Einheitenspalte.DurchschnittKmh] = "Ø km/h",
        [Einheitenspalte.MaxKmh] = "max km/h",
        [Einheitenspalte.PulsDurchschnitt] = "Ø Puls",
        [Einheitenspalte.PulsMax] = "max Puls",
        [Einheitenspalte.Trittfrequenz] = "Trittfrequenz",
        [Einheitenspalte.Temperatur] = "Temperatur (°C)",
        [Einheitenspalte.MaxHoehe] = "Maximalhöhe",
        [Einheitenspalte.Trainingsbereich] = "Trainingsbereich",
        [Einheitenspalte.Kommentar] = "Kommentar",
    };

    /// <summary>Inhalt der Summenzeile: Spalte → Eigenschaft des ViewModels; alle anderen Zellen bleiben leer.</summary>
    private static readonly Dictionary<Einheitenspalte, string> Summen = new()
    {
        [Einheitenspalte.Name] = nameof(HauptfensterViewModel.SummeAnzahl),
        [Einheitenspalte.Distanz] = nameof(HauptfensterViewModel.SummeDistanz),
        [Einheitenspalte.Hoehenmeter] = nameof(HauptfensterViewModel.SummeHoehenmeter),
        [Einheitenspalte.Fahrzeit] = nameof(HauptfensterViewModel.SummeFahrzeit),
    };

    /// <summary>Innenabstand einer Summenzelle links und rechts.</summary>
    private const double Zellabstand = 4;

    /// <summary>Zuordnung der fachlichen Spalten zu den Spalten des DataGrid.</summary>
    private readonly Dictionary<Einheitenspalte, DataGridColumn> _spalten;

    /// <summary>Die Zellen der Summenzeile je Spalte.</summary>
    private readonly Dictionary<Einheitenspalte, TextBlock> _summenzellen = new();

    /// <summary>Das ViewModel, dessen Änderungen gerade beobachtet werden.</summary>
    private HauptfensterViewModel? _viewModel;

    /// <summary><c>true</c>, sobald die Startsortierung gesetzt ist (Loaded kommt bei jedem Karteikartenwechsel).</summary>
    private bool _sortiert;

    /// <summary><c>true</c>, während eine gespeicherte Anordnung angewendet wird – dann nicht zurückspeichern.</summary>
    private bool _wendeAnordnungAn;

    /// <summary><c>true</c>, solange eine Zelle bearbeitet wird – dann wirkt Entf im Eingabefeld statt zu löschen.</summary>
    private bool _zelleInBearbeitung;

    /// <summary>Die zuletzt bearbeitete Spalte; nach dem Neuaufbau wird die Zelle dieser Spalte wieder aktuell.</summary>
    private DataGridColumn? _letzteSpalte;

    /// <summary>
    /// Erzeugt die Tabelle, ordnet die Spalten zu und baut die Summenzeile auf.
    /// </summary>
    public EinheitenTabelle()
    {
        InitializeComponent();

        _spalten = new Dictionary<Einheitenspalte, DataGridColumn>
        {
            [Einheitenspalte.Datum] = SpalteDatum,
            [Einheitenspalte.Startzeit] = SpalteStartzeit,
            [Einheitenspalte.Name] = SpalteName,
            [Einheitenspalte.Aktivitaet] = SpalteAktivitaet,
            [Einheitenspalte.Kategorien] = SpalteKategorien,
            [Einheitenspalte.Equipment] = SpalteEquipment,
            [Einheitenspalte.Distanz] = SpalteDistanz,
            [Einheitenspalte.Hoehenmeter] = SpalteHoehenmeter,
            [Einheitenspalte.Fahrzeit] = SpalteFahrzeit,
            [Einheitenspalte.DurchschnittKmh] = SpalteDurchschnittKmh,
            [Einheitenspalte.MaxKmh] = SpalteMaxKmh,
            [Einheitenspalte.PulsDurchschnitt] = SpaltePulsDurchschnitt,
            [Einheitenspalte.PulsMax] = SpaltePulsMax,
            [Einheitenspalte.Trittfrequenz] = SpalteTrittfrequenz,
            [Einheitenspalte.Temperatur] = SpalteTemperatur,
            [Einheitenspalte.MaxHoehe] = SpalteMaxHoehe,
            [Einheitenspalte.Trainingsbereich] = SpalteTrainingsbereich,
            [Einheitenspalte.Kommentar] = SpalteKommentar,
        };

        BaueSummenzeile();

        // Ende des Ziehens am Spaltenrand = Breite geändert (Thumb-Ereignisse der Bildlaufleisten werden ausgefiltert)
        Tabelle.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(Tabelle_DragCompleted));

        DataContextChanged += (_, _) => BeobachteViewModel();
        Loaded += (_, _) => SetzeStartsortierung();
    }

    /// <summary>
    /// Speichert Reihenfolge und Breiten der Spalten über das ViewModel.
    /// </summary>
    /// <remarks>
    /// Wird nach dem Verschieben und Verbreitern sowie vom Hauptfenster beim Schließen aufgerufen – Letzteres
    /// erfasst auch die automatische Anpassung per Doppelklick auf den Spaltenrand. Eine automatisch
    /// angepasste Spalte wird als „automatisch“ gemerkt, eine von Hand gezogene mit ihrer Breite.
    /// </remarks>
    public void MerkeAnordnung()
    {
        if (_viewModel is null || _wendeAnordnungAn)
        {
            return;
        }

        List<Spalteneinstellung> anordnung = _spalten
            .OrderBy(paar => paar.Value.DisplayIndex)
            .Select(paar => new Spalteneinstellung(paar.Key,
                paar.Value.Width.UnitType == DataGridLengthUnitType.Pixel ? paar.Value.Width.Value : null))
            .ToList();
        _viewModel.MerkeSpaltenanordnung(anordnung);
    }

    /// <summary>
    /// Erzeugt je Tabellenspalte eine Zelle der Summenzeile.
    /// </summary>
    /// <remarks>
    /// Breite und Sichtbarkeit jeder Zelle sind an die zugehörige Spalte gebunden – ändert der Benutzer
    /// die Spaltenbreite oder blendet er eine Spalte aus, folgt die Summenzeile. Rechts wird Platz in der
    /// Breite der senkrechten Bildlaufleiste gelassen, damit beide Bereiche gleich weit blättern können.
    /// </remarks>
    private void BaueSummenzeile()
    {
        foreach (Einheitenspalte spalte in Enum.GetValues<Einheitenspalte>())
        {
            DataGridColumn tabellenspalte = _spalten[spalte];
            var text = new TextBlock
            {
                Padding = new Thickness(Zellabstand, 0, Zellabstand, 0),
                TextAlignment = spalte == Einheitenspalte.Name ? TextAlignment.Left : TextAlignment.Right,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            if (Summen.TryGetValue(spalte, out string? eigenschaft))
            {
                text.SetBinding(TextBlock.TextProperty, new Binding(eigenschaft));
            }

            text.SetBinding(WidthProperty, new Binding(nameof(DataGridColumn.ActualWidth)) { Source = tabellenspalte });
            text.SetBinding(VisibilityProperty, new Binding(nameof(DataGridColumn.Visibility)) { Source = tabellenspalte });
            _summenzellen[spalte] = text;
        }

        OrdneSummenzeile();

        // Linker Rand der Tabelle (1 Pixel) und Platz für die senkrechte Bildlaufleiste
        Summenzeile.Margin = new Thickness(1, 3, SystemParameters.VerticalScrollBarWidth, 3);
    }

    /// <summary>Ordnet die Zellen der Summenzeile in der aktuellen Reihenfolge der Tabellenspalten an.</summary>
    private void OrdneSummenzeile()
    {
        Summenzeile.Children.Clear();
        foreach (Einheitenspalte spalte in _spalten.OrderBy(paar => paar.Value.DisplayIndex).Select(paar => paar.Key))
        {
            Summenzeile.Children.Add(_summenzellen[spalte]);
        }
    }

    /// <summary>
    /// Setzt für die Spalten mit Summe die Mindestbreite auf die Breite ihres Summentextes.
    /// </summary>
    /// <remarks>
    /// Die automatische Anpassung des DataGrid (Doppelklick auf den Spaltenrand) misst nur Überschrift und
    /// sichtbare Zellen; mit der Mindestbreite bleibt auch die Summe darunter vollständig lesbar, ebenso
    /// beim Schmalziehen von Hand. Gemessen wird in Fettschrift wie in der Summenzeile.
    /// </remarks>
    private void SetzeMindestbreiten()
    {
        if (_viewModel is null)
        {
            return;
        }

        var schrift = new Typeface(FontFamily, FontStyle, FontWeights.Bold, FontStretch);
        double pixelProDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        foreach ((Einheitenspalte spalte, string eigenschaft) in Summen)
        {
            string text = eigenschaft switch
            {
                nameof(HauptfensterViewModel.SummeAnzahl) => _viewModel.SummeAnzahl,
                nameof(HauptfensterViewModel.SummeDistanz) => _viewModel.SummeDistanz,
                nameof(HauptfensterViewModel.SummeHoehenmeter) => _viewModel.SummeHoehenmeter,
                _ => _viewModel.SummeFahrzeit,
            };
            var gemessen = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, schrift,
                FontSize, Brushes.Black, pixelProDip);

            // Textbreite + Innenabstand der Summenzelle + Rand der Tabellenzelle; aufgerundet
            _spalten[spalte].MinWidth = Math.Ceiling(gemessen.WidthIncludingTrailingWhitespace + 2 * Zellabstand + 2);
        }
    }

    /// <summary>Beim waagrechten Blättern der Tabelle die Summenzeile mitführen.</summary>
    private void Tabelle_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        SummenScroller.ScrollToHorizontalOffset(e.HorizontalOffset);
    }

    /// <summary>Spalte per Ziehen verschoben: Summenzeile nachziehen und die Anordnung speichern.</summary>
    private void Tabelle_ColumnReordered(object? sender, DataGridColumnEventArgs e)
    {
        OrdneSummenzeile();
        MerkeAnordnung();
    }

    /// <summary>Breite einer Spalte per Ziehen am Rand geändert: Anordnung speichern.</summary>
    private void Tabelle_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { TemplatedParent: DataGridColumnHeader })
        {
            MerkeAnordnung();
        }
    }

    /// <summary>
    /// Sortiert beim ersten Anzeigen nach Datum und Startzeit, die neueste Einheit zuerst.
    /// </summary>
    /// <remarks>
    /// Die Sortierung gehört zur Ansicht der Sammlung <see cref="HauptfensterViewModel.Einheitszeilen"/>;
    /// da diese Sammlung bestehen bleibt, gilt eine per Klick gewählte Sortierung bis zum Programmende.
    /// </remarks>
    private void SetzeStartsortierung()
    {
        if (_sortiert)
        {
            return;
        }

        _sortiert = true;
        Tabelle.Items.SortDescriptions.Clear();
        Tabelle.Items.SortDescriptions.Add(new SortDescription(nameof(Einheitszeile.Datum), ListSortDirection.Descending));
        Tabelle.Items.SortDescriptions.Add(new SortDescription(nameof(Einheitszeile.Startzeit), ListSortDirection.Descending));
        SpalteDatum.SortDirection = ListSortDirection.Descending;
    }

    /// <summary>Meldet sich beim (neuen) ViewModel an und übernimmt dessen Spaltenwahl, Anordnung und Summen.</summary>
    private void BeobachteViewModel()
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _viewModel.MarkiereEinheit -= ViewModel_MarkiereEinheit;
            _viewModel.MarkiereEinheiten -= ViewModel_MarkiereEinheiten;
        }

        _viewModel = DataContext as HauptfensterViewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.MarkiereEinheit += ViewModel_MarkiereEinheit;
            _viewModel.MarkiereEinheiten += ViewModel_MarkiereEinheiten;
        }

        WendeAnordnungAn();
        WendeSpaltenwahlAn();
        SetzeMindestbreiten();
    }

    /// <summary>Reagiert auf Änderungen im ViewModel, die die Spalten betreffen.</summary>
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(HauptfensterViewModel.AusgeblendeteSpalten):
                WendeSpaltenwahlAn();
                break;
            case nameof(HauptfensterViewModel.Spaltenanordnung):
                WendeAnordnungAn();
                break;
            case nameof(HauptfensterViewModel.SummeAnzahl) or nameof(HauptfensterViewModel.SummeDistanz)
                or nameof(HauptfensterViewModel.SummeHoehenmeter) or nameof(HauptfensterViewModel.SummeFahrzeit):
                SetzeMindestbreiten();
                break;
        }
    }

    /// <summary>
    /// Übernimmt Reihenfolge und Breiten aus <see cref="HauptfensterViewModel.Spaltenanordnung"/>.
    /// </summary>
    private void WendeAnordnungAn()
    {
        if (_viewModel is null)
        {
            return;
        }

        _wendeAnordnungAn = true;
        try
        {
            IReadOnlyList<Spalteneinstellung> anordnung = _viewModel.Spaltenanordnung;
            for (int i = 0; i < anordnung.Count; i++)
            {
                DataGridColumn spalte = _spalten[anordnung[i].Spalte];
                spalte.DisplayIndex = i;
                spalte.Width = anordnung[i].Breite is double breite ? new DataGridLength(breite) : DataGridLength.Auto;
            }

            OrdneSummenzeile();
        }
        finally
        {
            _wendeAnordnungAn = false;
        }
    }

    /// <summary>Blendet die Spalten des DataGrid entsprechend der Wahl im ViewModel ein oder aus.</summary>
    private void WendeSpaltenwahlAn()
    {
        IReadOnlySet<Einheitenspalte> ausgeblendet = _viewModel?.AusgeblendeteSpalten ?? new HashSet<Einheitenspalte>();
        foreach ((Einheitenspalte spalte, DataGridColumn tabellenspalte) in _spalten)
        {
            tabellenspalte.Visibility = ausgeblendet.Contains(spalte) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>
    /// Füllt das Kontextmenü der Spaltenüberschriften: ein Eintrag je Spalte in Standardreihenfolge, Häkchen = sichtbar.
    /// </summary>
    /// <remarks>
    /// Die Einträge bleiben beim Anklicken offen (<see cref="MenuItem.StaysOpenOnClick"/>), sodass mehrere
    /// Spalten nacheinander umgeschaltet werden können. Ein Klick ruft den Befehl
    /// <see cref="HauptfensterViewModel.SpalteUmschaltenCommand"/> auf, der die Wahl prüft und speichert;
    /// danach werden Häkchen und Sperre aller Einträge aktualisiert. Der letzte Eintrag „Übernehmen“
    /// schließt das Menü.
    /// </remarks>
    private void Spaltenmenue_Opened(object sender, RoutedEventArgs e)
    {
        var menue = (ContextMenu)sender;
        menue.Items.Clear();
        if (_viewModel is null)
        {
            return;
        }

        foreach (Einheitenspalte spalte in Enum.GetValues<Einheitenspalte>())
        {
            var eintrag = new MenuItem { Header = Bezeichnungen[spalte], Tag = spalte, StaysOpenOnClick = true };
            eintrag.Click += (_, _) =>
            {
                _viewModel.SpalteUmschaltenCommand.Execute(spalte);
                AktualisiereMenue(menue);
            };
            menue.Items.Add(eintrag);
        }

        // „Übernehmen“ schließt das Menü; die Spaltenwahl ist zu diesem Zeitpunkt schon angewendet und gespeichert
        var uebernehmen = new MenuItem { Header = "Übernehmen", FontWeight = FontWeights.Bold };
        uebernehmen.Click += (_, _) => menue.IsOpen = false;
        menue.Items.Add(new Separator());
        menue.Items.Add(uebernehmen);

        AktualisiereMenue(menue);
    }

    /// <summary>
    /// Meldet dem ViewModel die markierten Einheiten, damit „Bearbeiten“, „Kopieren“ und „Löschen“ richtig
    /// wählbar sind.
    /// </summary>
    /// <remarks>
    /// Das Ereignis steigt auch aus den Auswahllisten in bearbeiteten Zellen auf; diese werden übergangen.
    /// </remarks>
    private void Tabelle_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, Tabelle) || _viewModel is null)
        {
            return;
        }

        _viewModel.MarkierteEinheiten = Tabelle.SelectedItems.OfType<Einheitszeile>().Select(z => z.Id).ToList();
    }

    /// <summary>
    /// Markiert die Zeile einer Einheit (nach dem Speichern), holt sie in den sichtbaren Bereich und macht
    /// die Zelle der zuletzt bearbeiteten (sonst der ersten sichtbaren) Spalte zur aktuellen Zelle.
    /// </summary>
    /// <param name="id">Nummer der Einheit.</param>
    private void ViewModel_MarkiereEinheit(long id)
    {
        if (_viewModel?.Einheitszeilen.FirstOrDefault(z => z.Id == id) is not Einheitszeile zeile)
        {
            return;
        }

        Tabelle.SelectedItem = zeile;
        DataGridColumn? spalte = _letzteSpalte is { Visibility: Visibility.Visible }
            ? _letzteSpalte
            : Tabelle.Columns.Where(s => s.Visibility == Visibility.Visible).OrderBy(s => s.DisplayIndex).FirstOrDefault();
        if (spalte is not null)
        {
            Tabelle.CurrentCell = new DataGridCellInfo(zeile, spalte);
        }

        Tabelle.ScrollIntoView(zeile);
    }

    /// <summary>
    /// Markiert nach dem Zuordnen die Zeilen mehrerer Einheiten und holt die erste in den sichtbaren Bereich.
    /// </summary>
    /// <remarks>
    /// Wird erst nach dem Neuaufbau der Tabelle ausgelöst; die dabei geleerte Markierung wird so ersetzt.
    /// Das Ändern von <c>SelectedItems</c> meldet die neue Markierung über
    /// <see cref="Tabelle_SelectionChanged"/> an das ViewModel.
    /// </remarks>
    /// <param name="ids">Nummern der Einheiten, die noch angezeigt werden.</param>
    private void ViewModel_MarkiereEinheiten(IReadOnlyCollection<long> ids)
    {
        if (_viewModel is null)
        {
            return;
        }

        List<Einheitszeile> zeilen = _viewModel.Einheitszeilen.Where(z => ids.Contains(z.Id)).ToList();
        Tabelle.SelectedItems.Clear();
        foreach (Einheitszeile zeile in zeilen)
        {
            Tabelle.SelectedItems.Add(zeile);
        }

        // Die oberste markierte Zeile in der aktuellen Sortierung sichtbar machen
        if (Tabelle.Items.OfType<Einheitszeile>().FirstOrDefault(z => ids.Contains(z.Id)) is Einheitszeile erste)
        {
            Tabelle.ScrollIntoView(erste);
        }
    }

    /// <summary>
    /// Doppelklick auf eine Zeile: eine eben begonnene Zellbearbeitung verwerfen und das Bearbeitungsfenster öffnen.
    /// </summary>
    /// <remarks>
    /// Der zweite Klick des Doppelklicks trifft die gerade markierte Zelle und beginnt deshalb deren
    /// Bearbeitung; diese wird hier zurückgenommen.
    /// </remarks>
    private void Zeile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel is null || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        e.Handled = true;
        Tabelle.CancelEdit(DataGridEditingUnit.Row);
        if (_viewModel.EinheitBearbeitenCommand.CanExecute(null))
        {
            _viewModel.EinheitBearbeitenCommand.Execute(null);
        }
    }

    /// <summary>
    /// Rechtsklick auf eine nicht markierte Zeile markiert diese, damit sich das Rechtsklickmenü auf sie bezieht;
    /// bei einer schon markierten Zeile bleibt eine Mehrfachmarkierung erhalten.
    /// </summary>
    private void Tabelle_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (SucheVorfahr<DataGridRow>(e.OriginalSource as DependencyObject) is DataGridRow zeile && !zeile.IsSelected)
        {
            Tabelle.SelectedItem = zeile.Item;
        }
    }

    /// <summary>
    /// Entf löscht die markierten Einheiten (mit Rückfrage) – außer während der Bearbeitung einer Zelle, dort
    /// löscht Entf wie gewohnt Zeichen.
    /// </summary>
    private void Tabelle_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete || _zelleInBearbeitung || _viewModel is null)
        {
            return;
        }

        e.Handled = true;
        if (_viewModel.EinheitenLoeschenCommand.CanExecute(null))
        {
            _viewModel.EinheitenLoeschenCommand.Execute(null);
        }
    }

    /// <summary>
    /// Beginn der Bearbeitung einer Zelle: Rohtext bzw. Auswahlliste einsetzen.
    /// </summary>
    /// <remarks>
    /// Die Spalten sind nur lesend gebunden; das Eingabefeld zeigt deshalb zunächst den Anzeigetext
    /// (z. B. „Mo, 03.06.2024“ oder „1.234,5“). Er wird durch den Rohtext ersetzt („03.06.2024“, „1234,5“).
    /// Hat der Benutzer die Bearbeitung durch Lostippen begonnen, steht schon das getippte Zeichen im Feld
    /// und bleibt stehen.
    /// </remarks>
    private void Tabelle_PreparingCellForEdit(object? sender, DataGridPreparingCellForEditEventArgs e)
    {
        _zelleInBearbeitung = true;
        _letzteSpalte = e.Column;
        if (_viewModel is null || e.Row.Item is not Einheitszeile zeile || SpalteZu(e.Column) is not Einheitenspalte spalte)
        {
            return;
        }

        string rohtext = _viewModel.RohtextFuer(zeile.Id, spalte);
        if (e.EditingElement is TextBox feld)
        {
            if (e.EditingEventArgs is not TextCompositionEventArgs)
            {
                feld.Text = rohtext;
                feld.SelectAll();
            }
        }
        else if (FindeAuswahlliste(e.EditingElement) is ComboBox liste)
        {
            Eingabelisten listen = _viewModel.EingabelistenFuer(zeile.Id);
            IEnumerable<string> eintraege = spalte switch
            {
                Einheitenspalte.Aktivitaet => listen.Aktivitaeten.Select(a => a.Name).ToList(),
                Einheitenspalte.Equipment => listen.Equipments.Select(q => q.Name).ToList(),
                _ => listen.Trainingsbereiche,
            };
            liste.ItemsSource = eintraege;
            liste.SelectedItem = rohtext;
        }
    }

    /// <summary>
    /// Ende der Bearbeitung einer Zelle: Wert prüfen und sofort speichern.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Esc (Abbrechen): nichts tun – die Zelle zeigt wieder den alten Wert.</item>
    /// <item>Ungültiger Wert: Zelle rot färben und in Bearbeitung lassen.</item>
    /// <item>Gespeichert: Nach dem Ende der Bearbeitung wird die Tabelle neu aufgebaut (Ø km/h, Summen,
    /// Bestwerte, Sortierung) und die Zeile wieder markiert; während der laufenden Bearbeitung ist ein
    /// Neuaufbau nicht erlaubt.</item>
    /// <item>Nicht gespeichert (unverändert oder Schreibfehler, gemeldet vom ViewModel): Die Zelle zeigt
    /// wieder den Wert aus dem Datenbestand, also den alten.</item>
    /// </list>
    /// </remarks>
    private void Tabelle_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        HauptfensterViewModel? viewModel = _viewModel;
        if (e.EditAction != DataGridEditAction.Commit || viewModel is null
            || e.Row.Item is not Einheitszeile zeile || SpalteZu(e.Column) is not Einheitenspalte spalte)
        {
            _zelleInBearbeitung = false;
            return;
        }

        string text = e.EditingElement is TextBox feld
            ? feld.Text
            : FindeAuswahlliste(e.EditingElement)?.SelectedItem as string ?? string.Empty;
        switch (viewModel.AendereZelle(zeile.Id, spalte, text))
        {
            case Zellaenderung.Ungueltig:
                e.Cancel = true;
                MarkiereUngueltig(e.EditingElement);
                return;
            case Zellaenderung.Gespeichert:
                long id = zeile.Id;
                Dispatcher.InvokeAsync(() =>
                {
                    // Die Zeile ist noch in Bearbeitung – erst abschließen, dann neu aufbauen
                    if (!Tabelle.CommitEdit(DataGridEditingUnit.Row, true))
                    {
                        Tabelle.CancelEdit(DataGridEditingUnit.Row);
                    }

                    viewModel.ZeigeAenderung(id);
                });
                break;
        }

        _zelleInBearbeitung = false;
    }

    /// <summary>Färbt das Eingabefeld bzw. die Auswahlliste einer Zelle rot (ungültiger Wert).</summary>
    /// <param name="element">Das Bearbeitungselement der Zelle.</param>
    private static void MarkiereUngueltig(FrameworkElement element)
    {
        Control? feld = element as TextBox ?? (Control?)FindeAuswahlliste(element);
        if (feld is null)
        {
            return;
        }

        feld.BorderBrush = Brushes.Red;
        feld.BorderThickness = new Thickness(2);
        feld.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xE0, 0xE0));
        feld.ToolTip = "Ungültiger Wert – bitte berichtigen oder mit Esc abbrechen.";
    }

    /// <summary>
    /// Liefert die Auswahlliste im Bearbeitungselement einer Spalte mit Auswahlliste.
    /// </summary>
    /// <remarks>
    /// Bei Vorlagenspalten ist das Bearbeitungselement ein ContentPresenter, dessen einziges Kind die
    /// Auswahlliste aus der Vorlage „AuswahlBearbeiten“ ist; die Vorlage wird bei Bedarf zuerst angewendet.
    /// </remarks>
    /// <param name="element">Das Bearbeitungselement der Zelle.</param>
    /// <returns>Die Auswahlliste oder <c>null</c>.</returns>
    private static ComboBox? FindeAuswahlliste(FrameworkElement element)
    {
        if (element is ComboBox liste)
        {
            return liste;
        }

        element.ApplyTemplate();
        return VisualTreeHelper.GetChildrenCount(element) > 0 ? VisualTreeHelper.GetChild(element, 0) as ComboBox : null;
    }

    /// <summary>Liefert die fachliche Spalte zu einer Spalte des DataGrid.</summary>
    /// <param name="tabellenspalte">Spalte des DataGrid.</param>
    /// <returns>Die fachliche Spalte oder <c>null</c>.</returns>
    private Einheitenspalte? SpalteZu(DataGridColumn tabellenspalte)
    {
        foreach ((Einheitenspalte spalte, DataGridColumn kandidat) in _spalten)
        {
            if (ReferenceEquals(kandidat, tabellenspalte))
            {
                return spalte;
            }
        }

        return null;
    }

    /// <summary>
    /// Sucht vom angeklickten Element aus aufwärts das nächste Element des Typs <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>Textteile (z. B. Run) gehören nicht zum sichtbaren Baum; dort geht es über den logischen Baum weiter.</remarks>
    /// <typeparam name="T">Gesuchter Typ, z. B. <see cref="DataGridRow"/>.</typeparam>
    /// <param name="start">Das angeklickte Element.</param>
    /// <returns>Das gefundene Element oder <c>null</c>.</returns>
    private static T? SucheVorfahr<T>(DependencyObject? start) where T : DependencyObject
    {
        DependencyObject? aktuell = start;
        while (aktuell is not null and not T)
        {
            aktuell = aktuell is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(aktuell)
                : LogicalTreeHelper.GetParent(aktuell);
        }

        return aktuell as T;
    }

    /// <summary>
    /// Setzt Häkchen (= sichtbar) und Sperre der Menüeinträge; die letzte sichtbare Spalte ist gesperrt,
    /// damit die Tabelle nie ganz ohne Spalten dasteht.
    /// </summary>
    private void AktualisiereMenue(ContextMenu menue)
    {
        IReadOnlySet<Einheitenspalte> ausgeblendet = _viewModel?.AusgeblendeteSpalten ?? new HashSet<Einheitenspalte>();
        int sichtbar = Enum.GetValues<Einheitenspalte>().Count(s => !ausgeblendet.Contains(s));
        // Nur die Spalteneinträge (Tag = Spalte), nicht „Übernehmen“
        foreach (MenuItem eintrag in menue.Items.OfType<MenuItem>())
        {
            if (eintrag.Tag is not Einheitenspalte spalte)
            {
                continue;
            }

            bool istSichtbar = !ausgeblendet.Contains(spalte);
            eintrag.IsChecked = istSichtbar;
            eintrag.IsEnabled = !(istSichtbar && sichtbar == 1);
        }
    }
}
