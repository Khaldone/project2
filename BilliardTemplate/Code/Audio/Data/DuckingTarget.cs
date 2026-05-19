using System;
using System.Collections.Generic;

[Serializable]
public struct DuckingTarget
{
    public string BusName;
    public float DuckAmount;
    public DuckingReductionMode ReductionMode;
    public bool UseNameFilter;
    public List<string> FilterNames;
    public bool IsWhitelist;
    
    public bool ShouldDuck(string soundName)
    {
        if (!UseNameFilter || FilterNames == null || FilterNames.Count == 0)
        {
            return true;
        }

        bool nameMatches = false;
        foreach (var filterName in FilterNames)
        {
            if (soundName.Contains(filterName))
            {
                nameMatches = true;
                break;
            }
        }
    
        return IsWhitelist ? nameMatches : !nameMatches;
    }
}