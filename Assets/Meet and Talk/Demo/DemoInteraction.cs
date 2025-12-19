using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MeetAndTalk.Demo
{
    public class DemoInteraction : MonoBehaviour
    {
        public string InteractionText;
        public UnityEvent OnInteraction;

        public void Interaction()
        {
            OnInteraction.Invoke();
        }
    }
}
