using System.Collections.Generic;

public static class Machine 
{
    public static readonly Coils Coils = new Coils() { };    
    public static readonly Lamps Lamps = new Lamps() { };
    public static readonly Leds Leds = new Leds() { };
    public static readonly Switches Switches = new Switches() { };
}

public class Coils : PinStates { }
public class Switches : Dictionary<string, Switch> { }
public class Lamps : PinStates { }
public class Leds : PinStates { }