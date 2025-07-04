using System.Collections.Generic;

public static class SessionState
{
    // IDs de interactables/puzzles já resolvidos nesta sessão (int)
    public static HashSet<int> solvedInteractables = new HashSet<int>();

    // IDs únicos de puzzles já resolvidos (string)
    public static HashSet<string> solvedPuzzles = new HashSet<string>();
}
