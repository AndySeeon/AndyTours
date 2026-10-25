using Dynastream.Fit;

namespace AndyTours.Tests.Garmin;

/// <summary>
/// Erzeugt FIT-Dateien für die Tests mit dem Garmin-FIT-SDK.
/// </summary>
/// <remarks>
/// Damit kommen keine persönlichen Aufzeichnungen ins Repository. Die Datei enthält Dateikennung,
/// optional die Sensoren, den Start der Zeitmessung, einige Messpunkte (optional mit Pause), die Sitzung,
/// die Aktivität mit Ortszeit und optional die undokumentierte Nachricht 526 mit der Rad-Kennung.
/// </remarks>
public static class FitTestdatei
{
    /// <summary>
    /// Name der xUnit-Testgruppe aller Testklassen, die FIT-Dateien schreiben und lesen.
    /// </summary>
    /// <remarks>
    /// Das Garmin-SDK verträgt es nicht, wenn mehrere Testklassen gleichzeitig FIT-Dateien schreiben und lesen:
    /// Gelegentlich ist eine Datei dann unlesbar („FIT decode error“). Tests einer Gruppe laufen
    /// nacheinander. Die App selbst liest Aufzeichnungen immer nacheinander und ist davon nicht betroffen.
    /// </remarks>
    public const string Testgruppe = "FIT-Dateien";

    /// <summary>
    /// Die Werte der zu erzeugenden Datei; alles Übrige ist fest.
    /// </summary>
    /// <param name="StartUtc">Startzeit in UTC.</param>
    /// <param name="VerschiebungStunden">Zeitverschiebung der Ortszeit gegenüber UTC.</param>
    /// <param name="RadKennung">Rad-Kennung für Nachricht 526; <c>null</c> = Nachricht fehlt.</param>
    /// <param name="Sensoren">ANT+-Gerätetyp und Seriennummer der Sensoren.</param>
    /// <param name="MitPulsUndTemperaturInSitzung">
    /// <c>false</c> = niedrigster Puls und Durchschnittstemperatur fehlen in der Sitzung (wie beim Edge 1000).
    /// </param>
    /// <param name="MitPause">
    /// <c>true</c> = die Zeitmessung steht zwischen dem zweiten und dritten Messpunkt 40 s still
    /// (Stopp nach 60 s, Start nach 100 s).
    /// </param>
    /// <param name="OhneHoehe"><c>true</c> = die Messpunkte enthalten keine Höhe (wie bei einer Fahrt auf der Rolle).</param>
    /// <param name="OhneGps"><c>true</c> = kein Messpunkt enthält eine Position (wie bei einer Fahrt auf der Rolle).</param>
    public record Werte(
        System.DateTime StartUtc,
        int VerschiebungStunden = 2,
        string? RadKennung = null,
        IReadOnlyList<(byte Typ, uint Nummer)>? Sensoren = null,
        bool MitPulsUndTemperaturInSitzung = true,
        bool MitPause = false,
        bool OhneHoehe = false,
        bool OhneGps = false);

    /// <summary>Position des ersten Messpunkts in Grad.</summary>
    public static readonly (double Breite, double Laenge) ErstePosition = (47.8, 12.1);

    /// <summary>Position des dritten Messpunkts in Grad; der zweite hat keine Position (wie im Tunnel).</summary>
    public static readonly (double Breite, double Laenge) DrittePosition = (47.81, 12.12);

    /// <summary>
    /// Schreibt die Datei.
    /// </summary>
    /// <remarks>
    /// Feste Werte: Edge 1040; gestoppte Zeit 5:40:07,785 bei 6:39:55 Gesamtzeit; 90.830,7 m; 1.206 hm;
    /// höchster Punkt 1.379,6 m; max. 14,509 m/s; Puls 130/154/75; Trittfrequenz 75; 18 °C; 180 W; Profil „MTB“.
    /// Drei Messpunkte nach 0, 60 und 120 s: 540,8 m / 0 bpm / 14 °C / 0 m / 5 m/s / 80 U/min,
    /// 900 m / 92 bpm / 15 °C / 500 m / 6 m/s / 85 U/min und 600 m / 140 bpm / 16 °C / 1.000 m / 7 m/s / 90 U/min.
    /// Positionen: erster Punkt <see cref="ErstePosition"/>, zweiter ohne, dritter <see cref="DrittePosition"/>.
    /// </remarks>
    /// <param name="pfad">Zieldatei.</param>
    /// <param name="werte">Die veränderlichen Werte.</param>
    public static void Schreibe(string pfad, Werte werte)
    {
        var start = new Dynastream.Fit.DateTime(werte.StartUtc);
        using var datei = new FileStream(pfad, FileMode.Create, FileAccess.ReadWrite);
        var encoder = new Encode(ProtocolVersion.V20);
        encoder.Open(datei);

        var kennung = new FileIdMesg();
        kennung.SetType(Dynastream.Fit.File.Activity);
        kennung.SetManufacturer(Manufacturer.Garmin);
        kennung.SetProduct(GarminProduct.Edge1040);
        kennung.SetSerialNumber(1234);
        kennung.SetTimeCreated(start);
        encoder.Write(kennung);

        byte index = 1;
        foreach ((byte typ, uint nummer) in werte.Sensoren ?? [])
        {
            var sensor = new DeviceInfoMesg();
            sensor.SetTimestamp(start);
            sensor.SetDeviceIndex(index++);
            sensor.SetSourceType(SourceType.Antplus);
            sensor.SetAntplusDeviceType(typ);
            sensor.SetSerialNumber(nummer);
            encoder.Write(sensor);
        }

        SchreibeZeitmessung(encoder, start, 0, EventType.Start);

        float[] hoehen = [540.8f, 900f, 600f];
        byte[] pulse = [0, 92, 140];
        sbyte[] temperaturen = [14, 15, 16];
        float[] distanzen = [0f, 500f, 1000f];
        float[] geschwindigkeiten = [5f, 6f, 7f];
        byte[] trittfrequenzen = [80, 85, 90];
        (double Breite, double Laenge)?[] positionen = [ErstePosition, null, DrittePosition];
        for (int i = 0; i < hoehen.Length; i++)
        {
            if (werte.MitPause && i == 2)
            {
                SchreibeZeitmessung(encoder, start, 60, EventType.StopAll);
                SchreibeZeitmessung(encoder, start, 100, EventType.Start);
            }

            var punkt = new RecordMesg();
            punkt.SetTimestamp(new Dynastream.Fit.DateTime(start.GetTimeStamp() + (uint)(i * 60)));
            if (!werte.OhneHoehe)
            {
                punkt.SetEnhancedAltitude(hoehen[i]);
            }

            punkt.SetHeartRate(pulse[i]);
            punkt.SetTemperature(temperaturen[i]);
            punkt.SetDistance(distanzen[i]);
            punkt.SetEnhancedSpeed(geschwindigkeiten[i]);
            punkt.SetCadence(trittfrequenzen[i]);
            if (!werte.OhneGps && positionen[i] is (double breite, double laenge))
            {
                punkt.SetPositionLat(InSemicircles(breite));
                punkt.SetPositionLong(InSemicircles(laenge));
            }

            encoder.Write(punkt);
        }

        if (werte.RadKennung is string radKennung)
        {
            // Textfeld aus dem Profil kopieren und als Feld 0 der unbekannten Nachricht 526 verwenden
            var feld = new Field(Profile.GetField(MesgNum.Sport, SportMesg.FieldDefNum.Name)) { Num = 0 };
            var radwahl = new Mesg("Radwahl", 526);
            radwahl.SetField(feld);
            radwahl.SetFieldValue(0, System.Text.Encoding.UTF8.GetBytes(radKennung + "\0"));
            encoder.Write(radwahl);
        }

        var sitzung = new SessionMesg();
        sitzung.SetTimestamp(new Dynastream.Fit.DateTime(start.GetTimeStamp() + 23995));
        sitzung.SetStartTime(start);
        sitzung.SetTotalTimerTime(20407.785f);
        sitzung.SetTotalElapsedTime(23995f);
        sitzung.SetTotalDistance(90830.7f);
        sitzung.SetTotalAscent(1206);
        sitzung.SetEnhancedMaxAltitude(1379.6f);
        sitzung.SetEnhancedMaxSpeed(14.509f);
        sitzung.SetAvgHeartRate(130);
        sitzung.SetMaxHeartRate(154);
        if (werte.MitPulsUndTemperaturInSitzung)
        {
            sitzung.SetMinHeartRate(75);
            sitzung.SetAvgTemperature(18);
        }

        sitzung.SetAvgCadence(75);
        sitzung.SetAvgPower(180);
        sitzung.SetSportProfileName("MTB");
        encoder.Write(sitzung);

        var aktivitaet = new ActivityMesg();
        var ende = new Dynastream.Fit.DateTime(start.GetTimeStamp() + 23995);
        aktivitaet.SetTimestamp(ende);
        aktivitaet.SetLocalTimestamp(ende.GetTimeStamp() + (uint)(werte.VerschiebungStunden * 3600));
        aktivitaet.SetNumSessions(1);
        encoder.Write(aktivitaet);

        encoder.Close();
    }

    /// <summary>Rechnet Grad in die Einheit der FIT-Datei um (Semicircles: 2³¹ entsprechen 180°).</summary>
    private static int InSemicircles(double grad) => (int)Math.Round(grad * (1L << 31) / 180.0);

    /// <summary>Schreibt ein Ereignis der Zeitmessung (Start oder Stopp) nach der angegebenen Anzahl Sekunden.</summary>
    private static void SchreibeZeitmessung(Encode encoder, Dynastream.Fit.DateTime start, uint sekunden, EventType art)
    {
        var ereignis = new EventMesg();
        ereignis.SetTimestamp(new Dynastream.Fit.DateTime(start.GetTimeStamp() + sekunden));
        ereignis.SetEvent(Event.Timer);
        ereignis.SetEventType(art);
        encoder.Write(ereignis);
    }
}
