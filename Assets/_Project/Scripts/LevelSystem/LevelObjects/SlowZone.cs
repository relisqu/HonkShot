using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class SlowZone : MonoBehaviour
    {
        [Header("Slow Settings")]
        [SerializeField] private float _dragInZone = 15f;
        [SerializeField] private float _enterVelocityCut = 0.5f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _zoneVisual;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private Color _zoneColor = new Color(0.2f, 0.5f, 0.1f, 0.3f);

        private readonly Dictionary<Rigidbody2D, int> _overlapCounts = new();
        private readonly Dictionary<Rigidbody2D, float> _originalDrags = new();

        private void Awake()
        {
            if (_zoneVisual)
                _zoneVisual.color = _zoneColor;

            foreach (var col in GetComponentsInChildren<Collider2D>())
            {
                col.isTrigger = true;

                if (!col.GetComponent<SlowZoneCollider>())
                    col.gameObject.AddComponent<SlowZoneCollider>().Init(this);
            }

        }

        private void OnEnable()
        {
            
            if(_particleSystem)
                _particleSystem.Emit((int)_particleSystem.emission.rateOverTimeMultiplier*10);
        }

        public void BodyEnterCollider(Rigidbody2D rb)
        {
            if (!_overlapCounts.TryGetValue(rb, out int count))
            {
                _originalDrags[rb] = rb.linearDamping;
                rb.linearDamping = _dragInZone;
                rb.linearVelocity *= _enterVelocityCut;
            }

            _overlapCounts[rb] = count + 1;
        }

        public void BodyExitCollider(Rigidbody2D rb)
        {
            if (!_overlapCounts.TryGetValue(rb, out int count)) return;

            count--;
            if (count <= 0)
            {
                _overlapCounts.Remove(rb);
                if (_originalDrags.TryGetValue(rb, out float originalDrag))
                {
                    if (rb) rb.linearDamping = originalDrag;
                    _originalDrags.Remove(rb);
                }
            }
            else
            {
                _overlapCounts[rb] = count;
            }
        }

        private void OnDisable()
        {
            foreach (var kvp in _originalDrags)
            {
                if (kvp.Key)
                    kvp.Key.linearDamping = kvp.Value;
            }
            _overlapCounts.Clear();
            _originalDrags.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.5f, 0.1f, 0.3f);
            foreach (var col in GetComponentsInChildren<Collider2D>())
            {
                if (col is BoxCollider2D box)
                {
                    Gizmos.matrix = col.transform.localToWorldMatrix;
                    Gizmos.DrawCube(box.offset, box.size);
                    Gizmos.DrawWireCube(box.offset, box.size);
                    Gizmos.matrix = Matrix4x4.identity;
                }
                else if (col is CircleCollider2D circle)
                {
                    Gizmos.DrawSphere(col.transform.position + (Vector3)circle.offset,
                        circle.radius * col.transform.lossyScale.x);
                }
                else if (col is PolygonCollider2D)
                {
                    Gizmos.DrawWireSphere(col.transform.position, 0.5f);
                }
            }
        }
    }

    public class SlowZoneCollider : MonoBehaviour
    {
        private SlowZone _zone;

        public void Init(SlowZone zone)
        {
            _zone = zone;
        }

        private void Awake()
        {
            if (!_zone)
                _zone = GetComponentInParent<SlowZone>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.attachedRigidbody) return;
            _zone.BodyEnterCollider(other.attachedRigidbody);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.attachedRigidbody) return;
            _zone.BodyExitCollider(other.attachedRigidbody);
        }
    }
}
