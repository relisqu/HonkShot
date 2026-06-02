using System;
using DG.Tweening;
using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class ArrowObject : MonoBehaviour
    {
        [SerializeField] TouchableObject _touchableObject;
        [SerializeField] SpriteRenderer _arrowSpriteRenderer;
        [SerializeField] Color _touchedColor;

        [Tooltip("Sorting order applied to the arrow's SpriteRenderer in Start so it renders in front of the trail background.")]
        [SerializeField] int _sortingOrder = 1;

        private Color _defaultColor;

        private void TouchableObject_Touched(int _)
        {
            _arrowSpriteRenderer.DOColor(_touchedColor, 0.1f);
        }


        private void TouchableObject_Resetted()
        {
            _arrowSpriteRenderer.DOColor(_defaultColor, 0.2f);
        }

        private void Start()
        {
            _touchableObject.Touched += TouchableObject_Touched;
            _touchableObject.Resetted += TouchableObject_Resetted;
            _defaultColor = _arrowSpriteRenderer.color;
            _arrowSpriteRenderer.sortingOrder = _sortingOrder;
        }

        private void OnDestroy()
        {
            _touchableObject.Touched -= TouchableObject_Touched;
            _touchableObject.Resetted -= TouchableObject_Resetted;
        }
    }
}