using AndyTours.Core.Modell;
using Dynastream.Fit;

namespace AndyTours.Import.Garmin;

/// <summary>
/// Liest den Verlauf einer Tour (alle Messpunkte) aus einer FIT-Datei – die Grundlage von Höhenprofil und Streckenkarte.
/// </summary>
public static class LeseFitVerlauf
{
    /// <summary>Ungültig-Wert eines Positionsfelds in der FIT-Datei: Der Messpunkt hat keine Position.</summary>
    private const int KeinePosition = int.MaxValue;

    /// <summary>
    /// Liest die Messpunkte der Datei in ihrer Reihenfolge.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Fahrzeit ohne Pausen: Die Zeit läuft nur, solange die Zeitmessung des Edge läuft. Pausen erkennt die
    /// Funktion an den Ereignissen „Start“ und „Stopp“ der Zeitmessung (auch die automatische Pause erzeugt sie);
    /// fehlen solche Ereignisse, läuft die Zeit durchgehend. So endet die Fahrzeit wie die gestoppte Zeit.</item>
    /// <item>Distanz in km; ein Messpunkt ohne Distanz übernimmt die letzte bekannte.</item>
    /// <item>Höhe in m (genauere Höhe, sonst einfache Höhe), Geschwindigkeit in km/h auf zwei Nachkommastellen.</item>
    /// <item>Ein Puls von 0 gilt als „nicht erfasst“ (der Brustgurt liefert noch keinen Wert).</item>
    /// <item>Position in Grad; die FIT-Datei speichert sie in Semicircles (2³¹ entsprechen 180°). Fehlt Breite
    /// oder Länge oder trägt sie den Ungültig-Wert der FIT-Datei (z. B. im Tunnel oder auf der Rolle), bleibt die
    /// Position leer.</item>
    /// <item>Messpunkte ohne Zeitstempel werden übergangen.</item>
    /// </list>
    /// Prüfung und Fehlertexte entsprechen <see cref="LeseFitAufzeichnung"/>.
    /// </remarks>
    /// <param name="pfad">Pfad der FIT-Datei.</param>
    /// <returns>Die Messpunkte und <c>null</c>, oder <c>null</c> und der Grund, warum die Datei nicht lesbar ist.</returns>
    public static (List<Verlaufspunkt>? Verlauf, string? Fehler) Ausfuehren(string pfad)
    {
        List<Verlaufspunkt> verlauf = [];
        double distanzKm = 0;
        TimeSpan fahrzeit = TimeSpan.Zero;
        bool zeitLaeuft = true;
        uint? letzterZeitpunkt = null;

        // Zählt die Zeit seit dem letzten Zeitpunkt, wenn die Zeitmessung läuft
        void ZaehleBis(uint zeitpunkt)
        {
            if (zeitLaeuft && letzterZeitpunkt is uint letzter && zeitpunkt > letzter)
            {
                fahrzeit += TimeSpan.FromSeconds(zeitpunkt - letzter);
            }

            letzterZeitpunkt = zeitpunkt;
        }

        try
        {
            using FileStream datei = System.IO.File.OpenRead(pfad);
            var decoder = new Decode();
            bool istFit = decoder.IsFIT(datei);
            datei.Position = 0;
            if (!istFit || !decoder.CheckIntegrity(datei))
            {
                return (null, "keine gültige FIT-Datei");
            }

            datei.Position = 0;
            var verteiler = new MesgBroadcaster();
            decoder.MesgEvent += verteiler.OnMesg;
            verteiler.EventMesgEvent += (_, e) =>
            {
                var ereignis = (EventMesg)e.mesg;
                if (ereignis.GetEvent() != Event.Timer || ereignis.GetTimestamp() is not Dynastream.Fit.DateTime zeit)
                {
                    return;
                }

                ZaehleBis(zeit.GetTimeStamp());
                if (ereignis.GetEventType() is EventType.Start)
                {
                    zeitLaeuft = true;
                }
                else if (ereignis.GetEventType() is EventType.Stop or EventType.StopAll or EventType.StopDisable or EventType.StopDisableAll)
                {
                    zeitLaeuft = false;
                }
            };
            verteiler.RecordMesgEvent += (_, e) =>
            {
                var punkt = (RecordMesg)e.mesg;
                if (punkt.GetTimestamp() is not Dynastream.Fit.DateTime zeit)
                {
                    return;
                }

                ZaehleBis(zeit.GetTimeStamp());
                if (punkt.GetDistance() is float meter)
                {
                    distanzKm = meter / 1000.0;
                }

                verlauf.Add(new Verlaufspunkt
                {
                    DistanzKm = distanzKm,
                    Fahrzeit = fahrzeit,
                    Hoehe = (punkt.GetEnhancedAltitude() ?? punkt.GetAltitude()) is float hoehe ? hoehe : null,
                    Puls = punkt.GetHeartRate() is byte puls && puls > 0 ? puls : null,
                    Geschwindigkeit = (punkt.GetEnhancedSpeed() ?? punkt.GetSpeed()) is float mps ? Math.Round(mps * 3.6, 2) : null,
                    Trittfrequenz = punkt.GetCadence() is byte trittfrequenz ? trittfrequenz : null,
                    Temperatur = punkt.GetTemperature() is sbyte temperatur ? temperatur : null,
                    Position = punkt.GetPositionLat() is int breite && breite != KeinePosition
                        && punkt.GetPositionLong() is int laenge && laenge != KeinePosition
                        ? new Geoposition(InGrad(breite), InGrad(laenge))
                        : null,
                });
            };

            decoder.Read(datei);
        }
        catch (Exception fehler) when (fehler is FitException or IOException or UnauthorizedAccessException or InvalidCastException)
        {
            return (null, fehler.Message);
        }

        return (verlauf, null);
    }

    /// <summary>Rechnet Semicircles der FIT-Datei in Grad um.</summary>
    private static double InGrad(int semicircles) => semicircles * (180.0 / (1L << 31));
}
