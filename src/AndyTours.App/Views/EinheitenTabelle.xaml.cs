using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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
        }

        _viewModel = DataContext as HauptfensterViewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
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
