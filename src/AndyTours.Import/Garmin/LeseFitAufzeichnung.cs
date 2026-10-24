using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AndyTours.Core.Modell;
using Dynastream.Fit;

namespace AndyTours.Import.Garmin;

/// <summary>
/// Liest eine Aufzeichnung (FIT-Datei) eines Garmin-Radcomputers mit dem Garmin-FIT-SDK.
/// </summary>
public static partial class LeseFitAufzeichnung
{
    /// <summary>
    /// Nummer der von Garmin nicht dokumentierten Nachricht mit dem nach der Tour gewählten Rad.
    /// </summary>
    /// <remarks>
    /// Seit der Software 31.33 des Edge 1040 steht im Feld 0 dieser Nachricht eine Kennung des Rads
    /// (z. B. „7af22db6-d829-4395-9e42-a200867c4abf“). Das SDK kennt die Nachricht nicht; sie wird deshalb
    /// über ihre Nummer gelesen. Fehlt sie, bleibt die Rad-Kennung leer.
    /// </remarks>
    public const ushort RadwahlNachricht = 526;

    /// <summary>ANT+-Gerätetypen der Sensoren, an denen ein Rad erkannt wird (Geschwindigkeit und Trittfrequenz).</summary>
    private static readonly HashSet<byte> Radsensoren = [AntplusDeviceType.BikeSpeedCadence, AntplusDeviceType.BikeCadence, AntplusDeviceType.BikeSpeed];

    /// <summary>
    /// Liest die Datei und liefert ihre Werte.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Datum und Startzeit in Ortszeit: Startzeit der Sitzung plus Zeitverschiebung aus der Aktivität
    /// (<c>LocalTimestamp</c> minus <c>Timestamp</c>); fehlt diese, gilt die Zeitzone des Rechners.</item>
    /// <item>Fahrzeit: gestoppte Zeit (<c>TotalTimerTime</c>) ohne Pausen, auf ganze Sekunden abgeschnitten
    /// wie in der Anzeige des Edge.</item>
    /// <item>Distanz in km und Höchstgeschwindigkeit in km/h auf zwei Nachkommastellen gerundet, Höhen auf ganze
    /// Meter abgeschnitten (wie in BikeXperience).</item>
    /// <item>Starthöhe: Höhe des ersten Messpunkts; höchster Punkt aus der Sitzung, sonst aus den Messpunkten.</item>
    /// <item>Fehlen in der Sitzung der niedrigste Puls oder die Durchschnittstemperatur (z. B. beim Edge 1000),
    /// werden sie aus den Messpunkten ermittelt.</item>
    /// <item>Profil: Profilname der Sitzung, sonst der Sportname.</item>
    /// <item>Sensoren: Seriennummern der angeschlossenen Geschwindigkeits- und Trittfrequenzsensoren.</item>
    /// <item>Gerät: Produktname aus der Dateikennung, z. B. „Edge 1040“.</item>
    /// </list>
    /// </remarks>
    /// <param name="pfad">Pfad der FIT-Datei; der Dateiname wird als Name der Aufzeichnung übernommen.</param>
    /// <returns>Die Aufzeichnung und <c>null</c>, oder <c>null</c> und der Grund, warum sie nicht lesbar ist.</returns>
    public static (Aufzeichnung? Aufzeichnung, string? Fehler) Ausfuehren(string pfad)
    {
        FileIdMesg? dateikennung = null;
        SessionMesg? sitzung = null;
        ActivityMesg? aktivitaet = null;
        string sportname = string.Empty;
        string radKennung = string.Empty;
        List<string> sensoren = [];
        float? ersteHoehe = null;
        float? hoechsteHoehe = null;
        byte? niedrigsterPuls = null;
        long temperaturSumme = 0;
        int temperaturAnzahl = 0;

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
            verteiler.FileIdMesgEvent += (_, e) => dateikennung ??= (FileIdMesg)e.mesg;
            verteiler.SessionMesgEvent += (_, e) => sitzung ??= (SessionMesg)e.mesg;
            verteiler.ActivityMesgEvent += (_, e) => aktivitaet ??= (ActivityMesg)e.mesg;
            verteiler.SportMesgEvent += (_, e) => sportname = sportname.Length > 0 ? sportname : ((SportMesg)e.mesg).GetNameAsString() ?? string.Empty;
            verteiler.RecordMesgEvent += (_, e) =>
            {
                var punkt = (RecordMesg)e.mesg;
                if ((punkt.GetEnhancedAltitude() ?? punkt.GetAltitude()) is float hoehe)
                {
                    ersteHoehe ??= hoehe;
                    hoechsteHoehe = Math.Max(hoechsteHoehe ?? hoehe, hoehe);
                }

                if (punkt.GetHeartRate() is byte puls && puls > 0)
                {
                    niedrigsterPuls = Math.Min(niedrigsterPuls ?? puls, puls);
                }

                if (punkt.GetTemperature() is sbyte temperatur)
                {
                    temperaturSumme += temperatur;
                    temperaturAnzahl++;
                }
            };
            verteiler.DeviceInfoMesgEvent += (_, e) =>
            {
                var geraet = (DeviceInfoMesg)e.mesg;
                if (geraet.GetDeviceIndex() is byte index && index != 0
                    && geraet.GetAntplusDeviceType() is byte typ && Radsensoren.Contains(typ)
                    && geraet.GetSerialNumber() is uint nummer && nummer != 0
                    && !sensoren.Contains(nummer.ToString(System.Globalization.CultureInfo.InvariantCulture)))
                {
                    sensoren.Add(nummer.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            };
            decoder.MesgEvent += (_, e) =>
            {
                if (e.mesg.Num == RadwahlNachricht && radKennung.Length == 0)
                {
                    radKennung = AlsText(e.mesg.GetFieldValue(0));
                }
            };

            decoder.Read(datei);
        }
        catch (Exception fehler) when (fehler is FitException or IOException or UnauthorizedAccessException or InvalidCastException)
        {
            return (null, fehler.Message);
        }

        if (sitzung is null)
        {
            return (null, "keine Tourdaten enthalten");
        }

        System.DateTime start = Ortszeit(sitzung.GetStartTime(), aktivitaet);
        var aufzeichnung = new Aufzeichnung
        {
            Dateiname = Path.GetFileName(pfad),
            Geraet = Geraetename(dateikennung),
            Datum = DateOnly.FromDateTime(start),
            Startzeit = TimeOnly.FromDateTime(start),
            Fahrzeit = TimeSpan.FromSeconds(Math.Floor(sitzung.GetTotalTimerTime() ?? 0)),
            DistanzKm = Math.Round((sitzung.GetTotalDistance() ?? 0) / 1000.0, 2),
            Hoehenmeter = sitzung.GetTotalAscent() ?? 0,
            MaxHoehe = (int)Math.Floor(sitzung.GetEnhancedMaxAltitude() ?? sitzung.GetMaxAltitude() ?? hoechsteHoehe ?? 0),
            StartHoehe = (int)Math.Floor(ersteHoehe ?? 0),
            MaxGeschwindigkeit = Math.Round((sitzung.GetEnhancedMaxSpeed() ?? sitzung.GetMaxSpeed() ?? 0) * 3.6, 2),
            HfDurchschnitt = sitzung.GetAvgHeartRate() ?? 0,
            HfMax = sitzung.GetMaxHeartRate() ?? 0,
            HfMin = sitzung.GetMinHeartRate() ?? niedrigsterPuls ?? 0,
            Trittfrequenz = sitzung.GetAvgCadence() ?? 0,
            Temperatur = sitzung.GetAvgTemperature()
                ?? (temperaturAnzahl > 0 ? (int)Math.Round((double)temperaturSumme / temperaturAnzahl, MidpointRounding.AwayFromZero) : 0),
            LeistungWatt = sitzung.GetAvgPower() ?? 0,
            Profil = sitzung.GetSportProfileNameAsString() is { Length: > 0 } profil ? profil : sportname,
            RadKennung = radKennung,
            Sensoren = sensoren,
        };
        return (aufzeichnung, null);
    }

    /// <summary>
    /// Rechnet die Startzeit (UTC) in Ortszeit um.
    /// </summary>
    /// <remarks>
    /// Der Edge speichert in der Aktivität zusätzlich die Ortszeit; der Unterschied ist die Zeitverschiebung
    /// am Ort der Tour. Ohne diese Angabe wird die Zeitzone des Rechners verwendet.
    /// </remarks>
    private static System.DateTime Ortszeit(Dynastream.Fit.DateTime startUtc, ActivityMesg? aktivitaet)
    {
        System.DateTime utc = System.DateTime.SpecifyKind(startUtc.GetDateTime(), DateTimeKind.Utc);
        if (aktivitaet?.GetLocalTimestamp() is uint ortszeit && aktivitaet.GetTimestamp() is Dynastream.Fit.DateTime zeitpunkt)
        {
            long verschiebung = (long)ortszeit - zeitpunkt.GetTimeStamp();
            return System.DateTime.SpecifyKind(utc.AddSeconds(verschiebung), DateTimeKind.Unspecified);
        }

        return utc.ToLocalTime();
    }

    /// <summary>
    /// Liefert den Produktnamen aus der Dateikennung, z. B. „Edge 1040“ für Produkt 3843.
    /// </summary>
    /// <remarks>
    /// Das SDK führt die Garmin-Produkte als Konstanten (<see cref="GarminProduct"/>); der Name der ersten
    /// Konstante mit passendem Wert wird lesbar gemacht („Edge1040“ → „Edge 1040“).
    /// </remarks>
    private static string Geraetename(FileIdMesg? dateikennung)
    {
        if (dateikennung?.GetProduct() is ushort produkt)
        {
            string? name = typeof(GarminProduct)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(f => f.IsLiteral && f.GetValue(null) is ushort wert && wert == produkt)?.Name;
            if (name is not null)
            {
                return BuchstabeVorZiffer().Replace(name, "$1 $2");
            }
        }

        return "Garmin-Gerät";
    }

    /// <summary>Wandelt einen Feldwert (Text oder Bytes) in Text um.</summary>
    private static string AlsText(object? wert) => wert switch
    {
        byte[] bytes => Encoding.UTF8.GetString(bytes).TrimEnd('\0').Trim(),
        string text => text.TrimEnd('\0').Trim(),
        _ => string.Empty,
    };

    /// <summary>Stelle zwischen einem Buchstaben und einer folgenden Ziffer.</summary>
    [GeneratedRegex("([A-Za-z])([0-9])")]
    private static partial Regex BuchstabeVorZiffer();
}
