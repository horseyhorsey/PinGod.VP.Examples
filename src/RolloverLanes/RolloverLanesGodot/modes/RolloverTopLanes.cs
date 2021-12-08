public class RolloverTopLanes : PinballSwitchLanesNode
{
    public override bool LaneSwitchActivated(int i)
    {
        var result = base.LaneSwitchActivated(i);
        return result;
    }

    /// <summary>
    /// Just reset the targets back
    /// </summary>
    /// <returns></returns>
    public override bool CheckLanes()
    {
        var isCompleted = base.CheckLanes();
        if (isCompleted)
        {
            //log completion
            pinGod.LogDebug(nameof(RolloverTopLanes), " LANES COMPLETED");

            //reset the lanes
            ResetLanesCompleted();

            //change the led color to green            
            _led_color = Godot.Colors.Green;

            //add million to the players score and some bonus
            pinGod.AddPoints(1000000);
            pinGod.AddBonus(100000);

            //updates this scene lamps / leds
            UpdateLamps();
        }

        return isCompleted;
    }
}
