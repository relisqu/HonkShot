using Scripts.LevelSystem.LevelGeneration;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts.Camera
{
    public class LevelFollow : MonoBehaviour
    {
        void LateUpdate()
        {
            transform.position = LevelManager.Instance.CurrentRoom.transform.position;
        }
    }
}