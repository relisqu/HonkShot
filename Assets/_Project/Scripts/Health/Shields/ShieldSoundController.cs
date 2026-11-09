using Scripts.Audio;
using UnityEngine;

namespace Scripts.Health
{
    /// <summary>
    /// Bolts sound onto a <see cref="ShieldController"/> the same way <see cref="ShieldVisualController"/>
    /// bolts on visuals. Keys are serialized so the player's shields and an enemy's can sound different.
    /// </summary>
    public class ShieldSoundController : MonoBehaviour
    {
        [SerializeField] private ShieldController _shieldController;
        [SerializeField] private SoundChanelType _channel = SoundChanelType.Player;

        [Tooltip("Played when a shield stopped a hit and survived.")]
        [SerializeField] private string _blockedSound = "shieldBlocked";

        [Tooltip("Played when a hit was heavy enough to break a shield.")]
        [SerializeField] private string _brokenSound = "shieldBroken";

        private void Awake()
        {
            if (!_shieldController)
                _shieldController = GetComponent<ShieldController>();
        }

        private void OnEnable()
        {
            if (!_shieldController) return;

            _shieldController.OnShieldBlocked += ShieldController_ShieldBlocked;
            _shieldController.OnShieldDamaged += ShieldController_ShieldBroken;
        }

        private void OnDisable()
        {
            if (!_shieldController) return;

            _shieldController.OnShieldBlocked -= ShieldController_ShieldBlocked;
            _shieldController.OnShieldDamaged -= ShieldController_ShieldBroken;
        }

        private void ShieldController_ShieldBlocked(Shield shield) => Play(_blockedSound);

        // Deliberately not OnShieldDestroyed: that also fires when shields are cleared or removed for
        // reasons other than a hit, and those must stay silent.
        private void ShieldController_ShieldBroken(Shield shield) => Play(_brokenSound);

        private void Play(string soundName)
        {
            if (string.IsNullOrEmpty(soundName) || !AudioManager.Instance) return;
            AudioManager.Instance.PlayOneShot(_channel, soundName);
        }
    }
}
