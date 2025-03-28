using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace Scripts.LevelSystem.LevelObjects
{
    using System;
    using DG.Tweening;
    using UnityEngine;

    public class PointObject : MonoBehaviour
    {
        [SerializeField] TouchableObject _touchableObject;
        [SerializeField] Animator _animator;

        private void TouchableObject_Touched(int _)
        {
            _animator.SetTrigger("Touched");
        }



        private void TouchableObject_Resetted()
        {
        }

        private void Start()
        {
            _touchableObject.Touched += TouchableObject_Touched;
            _touchableObject.Resetted += TouchableObject_Resetted;
        }

        private void OnDestroy()
        {
            _touchableObject.Touched -= TouchableObject_Touched;
            _touchableObject.Resetted -= TouchableObject_Resetted;
        }
    }
}
