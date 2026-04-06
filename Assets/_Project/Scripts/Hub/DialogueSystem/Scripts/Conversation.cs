using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Conversation", fileName = "New Convo")]
public class Conversation : ScriptableObject
{
    public CharacterDatabase characterDatabase;
    public List<ConversationNode> ConversationNodes = new();
}

[Serializable]
public class ConversationNode
{
    public string SpeakerNameKey;
    public string ConvoLineKey;
}