using UnityEngine;

public abstract class Buff : ScriptableObject
{
    public string buffName;
    public string description;
    public Sprite icon;

    // Called when buff is selected by player
    public abstract void Apply(PlayerController player);
}
