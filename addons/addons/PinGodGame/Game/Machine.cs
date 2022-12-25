using System.Collections.Generic;

/// <summary>
/// Static machine class which holds the machine item <see cref="PinStates"/> like lamps, leds, coils, switches 
/// </summary>
public static class Machine 
{
    /// <summary>
    /// Coil Pin States
    /// </summary>
    public static readonly Coils Coils = new Coils() { };    
    /// <summary>
    /// Lamp Pin States
    /// </summary>
    public static readonly Lamps Lamps = new Lamps() { };
    /// <summary>
    /// LED Pin States
    /// </summary>
    public static readonly Leds Leds = new Leds() { };
    /// <summary>
    /// Switch Pin States
    /// </summary>
    public static readonly Switches Switches = new Switches() { };
}

/// <summary>
/// Collection of string, <see cref="PinStateObject"/>
/// </summary>
public class Coils : PinStates { }

/// <summary>
/// Collection of string, <see cref="PinStateObject"/>
/// </summary>
public class Switches : Dictionary<string, Switch> { }

/// <summary>
/// Collection of string, <see cref="PinStateObject"/>
/// </summary>
public class Lamps : PinStates { }

/// <summary>
/// Collection of string, <see cref="PinStateObject"/>
/// </summary>
public class Leds : PinStates { }