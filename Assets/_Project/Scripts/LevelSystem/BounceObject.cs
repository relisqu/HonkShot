using Scripts.Audio;
using Scripts.Player;
using UnityEngine;

namespace Scripts.LevelSystem
{
    public class BounceObject : MonoBehaviour
    {
        [SerializeField] private float _speedToSoundCoeff;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out BounceObject _) ||
                other.gameObject.TryGetComponent(out LevelSolidObject _) ||
                other.gameObject.TryGetComponent(out PlayerMovement _) )
            {
                var soundVolume = (_rigidbody2D.velocity.magnitude + 0.1f) * _speedToSoundCoeff;
                soundVolume = Mathf.Clamp01(soundVolume);
                AudioManager.Instance.PlayOneShot(SoundChanelType.Environment, "ballCollision", soundVolume);
            }
        }
    }
}