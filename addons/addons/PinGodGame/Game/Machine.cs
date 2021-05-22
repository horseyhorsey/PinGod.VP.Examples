using Newtonsoft.Json;
using System.Collections.Generic;

public static class Machine 
{
    public static readonly Switches Switches = new Switches()
    {
        {"coin",        new Switch(2)},
        {"exit",        new Switch(4)},
        {"down",        new Switch(5)},
        {"up",          new Switch(6)},
        {"enter",       new Switch(7)},
        {"flipper_l",   new Switch(11)},
        {"flipper_r",   new Switch(9)},
        {"slam_tilt",   new Switch(16)},
        {"tilt",        new Switch(17)},
        {"start",       new Switch(19)},
        {"plunger_lane",new Switch(20)},
        {"outlane_l",   new Switch(21)},
        {"inlane_l",    new Switch(22)},
        {"inlane_r",    new Switch(23)},
        {"outlane_r",   new Switch(24)},
        {"sling_l",     new Switch(25)},
        {"sling_r",     new Switch(26)},
        {"trough_1",    new Switch(81)},
        {"trough_2",    new Switch(82)},
        {"trough_3",    new Switch(83)},
        {"trough_4",    new Switch(84)},
    };

    public static readonly Lamps Lamps = new Lamps()
    {
        {"shoot_again",        new PinStateObject(1)}
    };

    public static readonly Leds Leds = new Leds()
    {
        {"shoot_again",        new PinStateObject(1)}
    };

    public static readonly Coils Coils = new Coils()
    {
        {"died",            new PinStateObject(0)},
        {"trough",          new PinStateObject(1)},
        {"flippers",        new PinStateObject(2)},
        {"auto_plunger",    new PinStateObject(3)}      
    };
}

public class Coils : PinStates { }
public class Switches : Dictionary<string, Switch> { }
public class Lamps : PinStates { }
public class Leds : PinStates { }