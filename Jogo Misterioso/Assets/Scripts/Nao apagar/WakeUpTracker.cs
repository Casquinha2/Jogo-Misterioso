// WakeUpTracker.cs
using System.Collections.Generic;   // <<< sem isso o HashSet não existe
public static class WakeUpTracker
{
    public static readonly HashSet<string> Shown = new HashSet<string>();
}
