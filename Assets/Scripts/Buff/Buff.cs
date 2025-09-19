using UnityEngine;

public class Buff : ScriptableObject
{
    public string buffName;
    [TextArea] public string buffDescription;
    public Sprite buffIcon;

    // Called when buff is selected by player
    public virtual void Apply(PlayerController player)
    {
        // Ghi đè ở các buff con
    }
}
