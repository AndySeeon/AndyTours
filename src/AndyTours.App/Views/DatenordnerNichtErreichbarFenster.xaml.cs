using System.Windows;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind der Meldung „Datenordner nicht erreichbar“.
/// </summary>
/// <remarks>
/// Das Fenster ist eine reine Rückfrage ohne Fachlogik; deshalb kommt es ohne eigenes
/// ViewModel aus. Der Aufrufer zeigt es mit <c>ShowDialog()</c> an und liest danach
/// <see cref="Gewaehlt"/> aus.
/// </remarks>
public partial class DatenordnerNichtErreichbarFenster : Window
{
    /// <summary>Die möglichen Antworten des Benutzers.</summary>
    public enum Aktion
    {
        /// <summary>Den Ordner noch einmal prüfen (z. B. nachdem das NAS eingeschaltet wurde).</summary>
        ErneutVersuchen,

        /// <summary>Einen anderen Datenordner auswählen.</summary>
        AndererOrdner,

        /// <summary>Die App beenden.</summary>
        Beenden,
    }

    /// <summary>
    /// Erzeugt die Meldung.
    /// </summary>
    /// <param name="meldung">Anzuzeigender Text, z. B. mit dem nicht erreichbaren Pfad.</param>
    public DatenordnerNichtErreichbarFenster(string meldung)
    {
        InitializeComponent();
        Meldung.Text = meldung;
    }

    /// <summary>Gewählte Aktion; „Beenden“, wenn das Fenster über das X geschlossen wurde.</summary>
    public Aktion Gewaehlt { get; private set; } = Aktion.Beenden;

    /// <summary>Schaltfläche „Erneut versuchen“.</summary>
    private void ErneutVersuchen_Click(object sender, RoutedEventArgs e) => Schliessen(Aktion.ErneutVersuchen);

    /// <summary>Schaltfläche „Anderen Ordner wählen…“.</summary>
    private void AndererOrdner_Click(object sender, RoutedEventArgs e) => Schliessen(Aktion.AndererOrdner);

    /// <summary>Schaltfläche „Beenden“.</summary>
    private void Beenden_Click(object sender, RoutedEventArgs e) => Schliessen(Aktion.Beenden);

    /// <summary>Merkt sich die Antwort und schließt das Fenster.</summary>
    private void Schliessen(Aktion aktion)
    {
        Gewaehlt = aktion;
        DialogResult = true;
    }
}
