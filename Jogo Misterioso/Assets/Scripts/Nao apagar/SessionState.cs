using System.Collections.Generic;

public static class SessionState
{
    // vai durar enquanto o jogo rodar, zerado só no restart do app
    public static HashSet<string> solvedPuzzles = new HashSet<string>();
}
