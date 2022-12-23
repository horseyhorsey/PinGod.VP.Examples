using System.Collections.Generic;

/// <summary>
/// Static machine class which holds the machine item <see cref="PinStates"/> like lamps, leds, coils, switches 
/// </summary>
public static class Machine 
{
    public static readonly Coils Coils = new Coils() { };    
    public static readonly Lamps Lamps = new Lamps() { };
    public static readonly Leds Leds = new Leds() { };
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