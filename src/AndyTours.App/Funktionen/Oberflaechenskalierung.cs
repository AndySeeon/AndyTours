using System.Windows;
using System.Windows.Media;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Vergrößert die ganze Oberfläche einschließlich der Schrift in der gewählten Stufe (Spezifikation 035).
/// </summary>
/// <remarks>
/// <para>
/// Alle Fenster teilen sich eine Vergrößerung (<see cref="ScaleTransform"/>), die als
/// <see cref="FrameworkElement.LayoutTransform"/> auf ihrem Inhalt liegt. Eine neue Stufe ändert nur deren
/// Faktor; alle offenen Fenster folgen dadurch sofort, ohne Neustart. Gemerkte Spaltenbreiten,
/// Fensterpositionen und Kartenausschnitte bleiben in Einheiten von 100 % und wachsen von selbst mit.
/// </para>
/// <para>
/// Verbindlich für jedes Fenster (docs/adr/0007-skalierung-der-oberflaeche.md): Es ruft nach
/// <c>InitializeComponent</c> <see cref="Anmelden"/> auf; Kartensteuerelemente werden mit
/// <see cref="Ausnehmen"/> ausgenommen.
/// </para>
/// </remarks>
public static class Oberflaechenskalierung
{
    /// <summary>Die gemeinsame Vergrößerung aller Fensterinhalte.</summary>
    private static readonly ScaleTransform Vergroesserung = new(1, 1);

    /// <summary>Die Gegenvergrößerung (Kehrwert) für ausgenommene Elemente wie Karten.</summary>
    private static readonly ScaleTransform Gegenvergroesserung = new(1, 1);

    /// <summary>
    /// Die offenen angemeldeten Fenster, deren feste Größen bei einer neuen Stufe nachgeführt werden, jeweils mit
    /// ihrer Höchstgröße aus dem XAML (bei 100 %, unendlich, wenn keine gesetzt ist).
    /// </summary>
    /// <remarks>
    /// Die Höchstgröße wird gemerkt, weil <see cref="Begrenze"/> sie mit dem Arbeitsbereich überschreibt; so lässt
    /// sie sich bei jeder Stufe neu aus dem Ausgangswert berechnen (z. B. <c>MaxWidth="600"</c> für den Zeilenumbruch).
    /// </remarks>
    private static readonly Dictionary<Window, Size> FensterMitFesterGroesse = [];

    /// <summary>Der aktuelle Faktor, z. B. 1,25 bei 125 %.</summary>
    public static double Faktor { get; private set; } = 1;

    /// <summary>
    /// Meldet ein Fenster an: Sein Inhalt bekommt die gemeinsame Vergrößerung, feste Größenangaben werden mit
    /// der Stufe umgerechnet, und das Fenster wird höchstens so groß wie der Arbeitsbereich des Bildschirms.
    /// </summary>
    /// <remarks>
    /// Aufzurufen direkt nach <c>InitializeComponent</c>, also bevor das Fenster seine Position berechnet
    /// (z. B. mittig über dem Besitzer). Die Vergrößerung liegt auf dem Inhalt, nicht auf dem Fenster, weil
    /// WPF ein Fenster selbst nicht transformieren kann. Aufklappende Elemente (Menüs, Auswahllisten,
    /// Kontextmenüs, Tooltips) übernehmen die Vergrößerung von dem Element, an dem sie aufklappen.
    /// </remarks>
    /// <param name="fenster">Das Fenster.</param>
    /// <param name="groesseUmrechnen">
    /// <c>false</c> für das Hauptfenster: Es behält seine gemerkte Größe und Position, nur sein Inhalt wächst.
    /// </param>
    public static void Anmelden(Window fenster, bool groesseUmrechnen = true)
    {
        if (fenster.Content is FrameworkElement inhalt)
        {
            inhalt.LayoutTransform = Vergroesserung;
        }

        if (!groesseUmrechnen)
        {
            return;
        }

        // Die Größenangaben im XAML gelten für 100 %; beim Öffnen in der aktuellen Stufe umrechnen
        FensterMitFesterGroesse[fenster] = new Size(fenster.MaxWidth, fenster.MaxHeight);
        RechneGroesseUm(fenster, Faktor);
        Begrenze(fenster);
        fenster.Closed += (_, _) => FensterMitFesterGroesse.Remove(fenster);
    }

    /// <summary>
    /// Nimmt ein Element von der Vergrößerung aus, z. B. eine Karte: Es füllt den mitgewachsenen Platz,
    /// zeichnet aber in Originalgröße.
    /// </summary>
    /// <remarks>
    /// Die Gegenvergrößerung (Kehrwert der Stufe) hebt die Vergrößerung des Fensterinhalts für dieses Element
    /// genau auf. Das Kartenbild (Kacheln, Strecke, Beschriftungen) bleibt dadurch scharf und so groß wie bei
    /// 100 %, und Mausklicks treffen die Stelle unter dem Zeiger wie bisher.
    /// </remarks>
    /// <param name="element">Das auszunehmende Element, z. B. das Kartensteuerelement.</param>
    public static void Ausnehmen(FrameworkElement element)
    {
        element.LayoutTransform = Gegenvergroesserung;
    }

    /// <summary>
    /// Setzt eine neue Stufe; alle offenen Fenster folgen sofort.
    /// </summary>
    /// <remarks>
    /// Feste Fenstergrößen werden im Verhältnis neu/alt nachgeführt, damit der vergrößerte Inhalt hineinpasst
    /// und vom Benutzer veränderte Größen erhalten bleiben.
    /// </remarks>
    /// <param name="prozent">Die Stufe in Prozent, z. B. 125.</param>
    public static void Setze(int prozent)
    {
        double neu = prozent / 100.0;
        double verhaeltnis = neu / Faktor;
        if (verhaeltnis == 1)
        {
            return;
        }

        Faktor = neu;
        Vergroesserung.ScaleX = neu;
        Vergroesserung.ScaleY = neu;
        Gegenvergroesserung.ScaleX = 1 / neu;
        Gegenvergroesserung.ScaleY = 1 / neu;

        foreach (Window fenster in FensterMitFesterGroesse.Keys)
        {
            RechneGroesseUm(fenster, verhaeltnis);
            Begrenze(fenster);
        }
    }

    /// <summary>
    /// Multipliziert die gesetzten Größenangaben eines Fensters mit einem Faktor.
    /// </summary>
    /// <remarks>
    /// Nicht gesetzte Angaben (<c>NaN</c> bzw. 0) bleiben unberührt; bei <see cref="SizeToContent"/> ergibt
    /// sich die betreffende Richtung ohnehin aus dem (vergrößerten) Inhalt.
    /// </remarks>
    /// <param name="fenster">Das Fenster.</param>
    /// <param name="faktor">Der Faktor, z. B. 1,5.</param>
    private static void RechneGroesseUm(Window fenster, double faktor)
    {
        if (!double.IsNaN(fenster.Width))
        {
            fenster.Width *= faktor;
        }

        if (!double.IsNaN(fenster.Height))
        {
            fenster.Height *= faktor;
        }

        fenster.MinWidth *= faktor;
        fenster.MinHeight *= faktor;
    }

    /// <summary>
    /// Begrenzt ein Fenster auf den Arbeitsbereich des Bildschirms (ohne Taskleiste).
    /// </summary>
    /// <remarks>
    /// Reicht der Platz nicht, rollt der Inhalt der Dialoge, während ihre Schaltflächenleiste unten sichtbar
    /// bleibt. Eine eigene Höchstgröße des Fensters wächst mit der Stufe, aber nie über den Arbeitsbereich hinaus.
    /// Auch die Mindestgröße darf den Arbeitsbereich nicht überschreiten, sonst ließe sie sich nicht
    /// einhalten.
    /// </remarks>
    /// <param name="fenster">Das Fenster.</param>
    private static void Begrenze(Window fenster)
    {
        Rect bereich = SystemParameters.WorkArea;
        Size hoechstgroesse = FensterMitFesterGroesse[fenster];
        fenster.MaxWidth = Math.Min(hoechstgroesse.Width * Faktor, bereich.Width);
        fenster.MaxHeight = Math.Min(hoechstgroesse.Height * Faktor, bereich.Height);
        fenster.MinWidth = Math.Min(fenster.MinWidth, bereich.Width);
        fenster.MinHeight = Math.Min(fenster.MinHeight, bereich.Height);
        if (!double.IsNaN(fenster.Width))
        {
            fenster.Width = Math.Min(fenster.Width, bereich.Width);
        }

        if (!double.IsNaN(fenster.Height))
        {
            fenster.Height = Math.Min(fenster.Height, bereich.Height);
        }
    }
}
