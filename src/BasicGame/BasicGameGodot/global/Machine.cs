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
        {"shoot_again",        1}
    };

    public static readonly Coils Coils = new Coils()
    {
        {"died",            0},
        {"trough",          1},
        {"flippers",        2},
        {"auto_plunger",    3},
        {"mball_saucer",    4},        
    };
}

public class Switches : Dictionary<string, Switch> { }
public class Lamps : Dictionary<string, byte> { }
public class Coils : Dictionary<string, byte> { }
