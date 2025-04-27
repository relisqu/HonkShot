using Scripts.LevelSystem.LevelGeneration;
using UnityEngine;

namespace Scripts.Camera
{
    public class LevelFollow : MonoBehaviour
    {
        void LateUpdate()
        {
            if (!LevelManager.Instance.CurrentRoom) return;
            transform.position = LevelManager.Instance.CurrentRoom.transform.position;
        }
    }
}