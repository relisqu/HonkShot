using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Dialogue/Character")]
public class Character : ScriptableObject
{
    public string id;
    public Sprite portrait;

    void OnEnable()
    {
#if UNITY_EDITOR
        CharacterDatabase.AddCharacter(this);
#endif
    }
}
