using Packlead.Domain.Exceptions;

namespace Packlead.Domain.ValueObjects;

// Clase de dominio para representar una ubicación
public sealed record Location
{
    public double Lat { get; }
    public double Lng { get; }

    public Location() : this(0, 0) { }

    public Location(double lat, double lng)
    {
        if (lat < -90 || lat > 90)
            throw new InvalidLocationException($"Latitude must be between -90 and 90. Received: {lat}");
        if (lng < -180 || lng > 180)
            throw new InvalidLocationException($"Longitude must be between -180 and 180. Received: {lng}");

        Lat = lat;
        Lng = lng;
    }
}