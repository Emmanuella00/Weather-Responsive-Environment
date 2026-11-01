using System.Globalization;

/// <summary>Outcome of validating the two coordinate text fields.</summary>
public readonly struct CoordinateValidation
{
    public bool IsValid { get; }
    public double Latitude { get; }
    public double Longitude { get; }
    public string LatitudeError { get; }     // null when the latitude is fine
    public string LongitudeError { get; }    // null when the longitude is fine
    public string Message { get; }           // everything to show the user, one line per problem

    public CoordinateValidation(bool isValid, double latitude, double longitude,
                                string latitudeError, string longitudeError, string message)
    {
        IsValid = isValid;
        Latitude = latitude;
        Longitude = longitude;
        LatitudeError = latitudeError;
        LongitudeError = longitudeError;
        Message = message;
    }
}

/// <summary>
/// Checks custom coordinates before any request is sent: not empty, a real number,
/// and inside range (latitude -90..90, longitude -180..180). A comma is accepted as the
/// decimal separator. Words such as NaN or Infinity are rejected as "not a number".
/// </summary>
public static class CoordinateValidator
{
    public static CoordinateValidation Validate(string latitudeText, string longitudeText)
    {
        string latRaw = (latitudeText ?? "").Trim();
        string lonRaw = (longitudeText ?? "").Trim();

        bool latOk = TryRead("Latitude", latRaw, 90, out double lat, out string latError);
        bool lonOk = TryRead("Longitude", lonRaw, 180, out double lon, out string lonError);

        string message;
        if (latRaw.Length == 0 && lonRaw.Length == 0)
            message = "Enter a latitude and a longitude.";
        else if (latError != null && lonError != null)
            message = latError + "\n" + lonError;
        else
            message = latError ?? lonError ?? "";

        return new CoordinateValidation(latOk && lonOk, lat, lon, latError, lonError, message);
    }

    static bool TryRead(string label, string raw, double limit, out double value, out string error)
    {
        value = 0;
        error = null;

        if (raw.Length == 0)
        {
            error = "Enter a " + label.ToLowerInvariant() + ".";
            return false;
        }

        if (!LooksNumeric(raw) ||
            !double.TryParse(raw.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
            double.IsNaN(value) || double.IsInfinity(value))
        {
            value = 0;
            error = label + " must be a number.";
            return false;
        }

        if (value < -limit || value > limit)
        {
            string text = limit.ToString(CultureInfo.InvariantCulture);
            error = label + " must be between -" + text + " and " + text + ".";
            return false;
        }

        return true;
    }

    // Optional sign, digits, at most one '.' or ','. No exponents, no words.
    static bool LooksNumeric(string s)
    {
        int i = (s[0] == '+' || s[0] == '-') ? 1 : 0;
        int digits = 0;
        int separators = 0;

        for (; i < s.Length; i++)
        {
            char c = s[i];
            if (c >= '0' && c <= '9') digits++;
            else if (c == '.' || c == ',') { if (++separators > 1) return false; }
            else return false;
        }

        return digits > 0;
    }
}
