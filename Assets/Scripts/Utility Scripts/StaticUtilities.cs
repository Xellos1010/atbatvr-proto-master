using System;
using System.Collections;

public static class StaticUtilities {

    

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }
}
