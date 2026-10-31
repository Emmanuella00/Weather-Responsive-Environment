public enum WeatherErrorKind
{
    None,
    NoApiKey,
    Network,      
    Timeout,
    HttpError,    
    ParseError,   
    InvalidData   
}

public readonly struct WeatherError
{
    public WeatherErrorKind Kind { get; }
    public string Message { get; }      
    public int HttpStatus { get; }      

    public WeatherError(WeatherErrorKind kind, string message, int httpStatus = 0)
    {
        Kind = kind;
        Message = message;
        HttpStatus = httpStatus;
    }

    public override string ToString() => $"[{Kind}] {Message}" + (HttpStatus != 0 ? $" (HTTP {HttpStatus})" : "");
}
