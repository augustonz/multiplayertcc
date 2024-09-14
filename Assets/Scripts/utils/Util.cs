using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Util {

    public static bool IsEqualLists<T>(List<T> list1, List<T> list2) {
        if (list1.Count != list2.Count) return false;
        for (int i = 0; i < list1.Count; i++)
        {
            if (!list1[i].Equals(list2[i])) return false;
        }
        return true;
    }

    public static bool IsEqualArrays<T>(T[] list1, T[] list2) {
        if (list1.Length != list2.Length) return false;
        for (int i = 0; i < list1.Length; i++)
        {
            if (!list1[i].Equals(list2[i])) return false;
        }
        return true;
    }

    public static void RemoveFromArray<T>(ref T[] array, T element) {
        array = array.Where(val => !val.Equals(element)).ToArray();
    }

    public static string formatMatchTime(float matchTimer) {
        string minutes = ((int)matchTimer/60).ToString();
        if (minutes.Length==1) minutes = "0" + minutes;
        matchTimer=matchTimer%60;
        string seconds = ((int)matchTimer/1).ToString();
        if (seconds.Length==1) seconds = "0" + seconds;
        matchTimer=matchTimer%1;
        string points;
        if (matchTimer==0) {
            points = "000";
        } else {
            if (matchTimer != 0) {
                string decimals = matchTimer.ToString().Split(".")[1];
                if (decimals.Length<3) {
                    points = decimals;
                    while (points.Length<3) {
                        points+="0";
                    }
                } else {
                    points = decimals.Substring(0,3);
                }
            } else {
                points = "000";
            }
        }
        return $"{minutes}:{seconds}.{points}";
    }

    public static Material getPlayerMaterialFromColor(string color) {

        int colorNum = ColorToColorNum(color);
        Material playerMaterial = Resources.Load<Material>($"Materials/PlayerMaterials/player{colorNum}");
        return playerMaterial;
    }

    public static string ColorToString(Color color) {
        float r = color.r;
        float g = color.g;
        float b = color.b;
        float limit = 0.8f;
        if (r>limit && g>limit) return "yellow";
        else if (r>limit && b>limit) return "magenta";
        else if (g>limit && b>limit) return "cyan";
        else if (r>limit) return "red";
        else if (g>limit) return "green";
        else if (b>limit) return "blue";
        return "blue";
    }


    private static int ColorToColorNum(string color) {
        switch(color) {
            case "blue":
                return 1;
            case "red":
                return 2;
            case "green":
                return 3;
            case "yellow":
                return 4;
            case "magenta":
                return 5;
            case "cyan":
                return 6;
        }
        return 1;
    }
}