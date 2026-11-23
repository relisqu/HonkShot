using UnityEngine;

namespace Scripts.VFX
{
    [CreateAssetMenu(fileName = "HitFeedbackConfig", menuName = "Configs/Hit Feedback Config")]
    public class HitFeedbackConfig : ScriptableObject
    {
        [Header("Hit Strength (also drives PlayerHitJuice hit stop and shake)")]
        [Tooltip("Damage at or below this plays the lightest reaction.")]
        [SerializeField] private float _lightHitDamage = 10f;
        [Tooltip("Damage at or above this plays the strongest reaction.")]
        [SerializeField] private float _heavyHitDamage = 35f;
        [Tooltip("Hits below this damage (e.g. resting contact ticks) get only a light flash and wobble: no kick, spark, " +
                 "hit stop or shake. Kills always get the full player-side reaction.")]
        [SerializeField] private float _minFullReactionDamage = 7f;

        [Header("Flash")]
        [SerializeField] private Material _flashMaterial;
        [SerializeField] private float _flashDuration = 0.1f;

        [Header("Squash Wobble")]
        [Tooltip("Scale offset at the first wobble peak for a medium hit: +x wider, -y flatter.")]
        [SerializeField] private Vector3 _punch = new Vector3(0.45f, -0.38f, 0f);
        [Tooltip("Wobble strength multiplier for the lightest (x) and heaviest (y) hit.")]
        [SerializeField] private Vector2 _punchStrengthRange = new Vector2(0.75f, 1.5f);
        [SerializeField] private float _punchDuration = 0.35f;
        [SerializeField] private int _punchVibrato = 7;
        [SerializeField, Range(0f, 1f)] private float _punchElasticity = 0.8f;

        [Header("Knockback Kick")]
        [Tooltip("How far the sprite is kicked away from the damage source for the lightest (x) and heaviest (y) hit.")]
        [SerializeField] private Vector2 _kickDistanceRange = new Vector2(0.12f, 0.32f);
        [SerializeField] private float _kickDuration = 0.28f;
        [SerializeField] private int _kickVibrato = 3;
        [SerializeField, Range(0f, 1f)] private float _kickElasticity = 0.6f;

        [Header("Hit Spark")]
        [SerializeField] private GameObject _sparkPrefab;
        [Tooltip("Spark spawn distance from the visual's center towards the damage source.")]
        [SerializeField] private float _sparkOffset = 0.3f;
        [Tooltip("Spark size for the lightest (x) and heaviest (y) hit.")]
        [SerializeField] private Vector2 _sparkScaleRange = new Vector2(0.8f, 1.4f);

        [Header("Stun Tremble (stun length is set per enemy on EnemyHitStun)")]
        [SerializeField] private float _stunShakeStrength = 0.07f;
        [SerializeField] private int _stunShakeVibrato = 35;

        public float MinFullReactionDamage => _minFullReactionDamage;
        public Material FlashMaterial => _flashMaterial;
        public float FlashDuration => _flashDuration;
        public float PunchDuration => _punchDuration;
        public int PunchVibrato => _punchVibrato;
        public float PunchElasticity => _punchElasticity;
        public float KickDuration => _kickDuration;
        public int KickVibrato => _kickVibrato;
        public float KickElasticity => _kickElasticity;
        public GameObject SparkPrefab => _sparkPrefab;
        public float SparkOffset => _sparkOffset;
        public float StunShakeStrength => _stunShakeStrength;
        public int StunShakeVibrato => _stunShakeVibrato;

        public bool IsFullReaction(float damage) => damage >= _minFullReactionDamage;

        // 0 for the lightest hit, 1 for the heaviest.
        public float GetHitStrength(float damage)
        {
            return _heavyHitDamage > _lightHitDamage ? Mathf.InverseLerp(_lightHitDamage, _heavyHitDamage, damage) : 0.5f;
        }

        public Vector3 GetPunch(float strength) => _punch * Mathf.Lerp(_punchStrengthRange.x, _punchStrengthRange.y, strength);
        public float GetKickDistance(float strength) => Mathf.Lerp(_kickDistanceRange.x, _kickDistanceRange.y, strength);
        public float GetSparkScale(float strength) => Mathf.Lerp(_sparkScaleRange.x, _sparkScaleRange.y, strength);
    }
}
