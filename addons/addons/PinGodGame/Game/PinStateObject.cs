using Newtonsoft.Json;
using System.Collections.Generic;

public class PinStateObject
{
    public byte Num { get; set; }
    public byte State { get; set; }
    public int Colour { get; set; } = 255;
    public PinStateObject(byte num, byte state = 0, int color = 0)
    {
        Num = num;
        State = state;
        Colour = color;
    }    
}

public class PinStates : Dictionary<string, PinStateObject>
{
    /// <summary>
    /// Gets all states with the number [,2]
    /// </summary>
    /// <returns></returns>
    public byte[,] GetStates()
    {
        if (this.Count <= 0) return null;        
        byte[,] arr = new byte[this.Count, 2];
        int i = 0;
        foreach (var item in this.Values)
        {
            arr[i, 0] = item.Num;
            arr[i, 1] = item.State;
            i++;
        }
        return arr;
    }

    /// <summary>
    /// Run through all but only assign what we have in dictionary
    /// </summary>
    /// <param name="stateCount"></param>
    /// <returns></returns>
    public byte[] GetStatesArray(int stateCount = 32)
    {
        byte[] arr = new byte[stateCount];
        foreach (var item in this.Values)
        {
            if(item.Num*2 <= arr.Length)
            {
                arr[item.Num*2] = item.Num;
                arr[item.Num*2+1] = item.State;
            }
        }
        return arr;
    }

    public int[] GetLedStatesArray(int stateCount = 64)
    {
        int[] arr = new int[stateCount];
        foreach (var item in this.Values)
        {
            if (item.Num * 3 <= arr.Length)
            {
                arr[item.Num * 3] = item.Num;
                arr[item.Num * 3 + 1] = item.State;
                arr[item.Num * 3 + 2] = item.Colour;
            }
        }

        return arr;
    }

    /// <summary>
    /// Gets all states with the number [num,3]
    /// </summary>
    /// <returns>[num, state, colour]</returns>
    public int[,] GetLedStates()
    {
        if (this.Count <= 0) return null;

        int[,] arr = new int[this.Count, 3];
        int i = 0;
        foreach (var item in this.Values)
        {
            arr[i, 0] = item.Num;
            arr[i, 1] = item.State;
            arr[i, 2] = item.Colour;
            i++;
        }
        return arr;
    }

    public string GetStatesJson()
    {
        if (this.Keys.Count > 0)
        {
            var states = this.GetStates();
            if (states != null)
                return JsonConvert.SerializeObject(states);
        }

        return string.Empty;
    }

    public string GetLedStatesJson()
    {
        if (this.Keys.Count > 0)
        {
            var states = this.GetLedStates();
            if (states != null)
                return JsonConvert.SerializeObject(states);
        }

        return string.Empty;
    }
}