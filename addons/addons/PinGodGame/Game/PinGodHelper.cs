using System;
using System.Collections.Generic;

/// <summary>
/// Helper class to hold methods useful for pinball games.
/// </summary>
public static class PinGodHelper
{
    /// <summary>
    /// Creates a list of bonus strings from a total bonus. 58000 would return 13 frames (inc. zero), takes 8 off to get to 50K then 5 more times with all zeros left.
    /// </summary>
    /// <param name="totalBonus"></param>
    /// <param name="formatScore"></param>
    /// <returns></returns>
    public static IEnumerable<string> CreateBonusZeroCountdown(long totalBonus, bool formatScore = true)
    {
        long bonus = totalBonus;

        List<string> _bonusTexts = new List<string>();
        while (bonus > 0)
        {
            var zeroCount = GetZeros(bonus);
            var nonZeroBonus = zeroCount.Item2;
            var bonusString = "";
            while (nonZeroBonus % 10 != 0)
            {
                if (nonZeroBonus > 0)
                {
                    nonZeroBonus--;
                    bonusString = nonZeroBonus.ToString();
                    var zeroStr = "".PadRight(zeroCount.Item1, '0');
                    bonusString = bonusString.Insert(bonusString.Length, zeroStr);
                    if (formatScore)
                        _bonusTexts.Add(long.Parse(bonusString).ToScoreString());
                    else
                        _bonusTexts.Add(bonusString);
                }
            }

            if (!string.IsNullOrWhiteSpace(bonusString))
                bonus = long.Parse(bonusString);
        }

        return _bonusTexts;
    }

    /// <summary>
    /// Gets the count of zeros in a number and returns the number without zeros. Eg: 58000, 3 zeros and 58 returned
    /// </summary>
    /// <param name="num"></param>
    /// <returns>Item1 count, Item2 number stripped of zeros</returns>
    public static Tuple<int, int> GetZeros(long num)
    {
        int count = 0;
        while (num > 0 && num % 10 == 0)
        {
            num = num / 10;
            count++;
        }

        return new Tuple<int, int>(count, (int)num);
    }
}