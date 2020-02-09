using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameState;

public interface GameStateObserver
{
    void HandleEvent(GameStateEvent e);   
}
