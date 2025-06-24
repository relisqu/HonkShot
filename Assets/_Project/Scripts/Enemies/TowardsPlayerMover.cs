using System;
using UnityEngine;
using Scripts.PointSystem;
using UnityEngine.UI;

namespace Scripts.Enemies
{
    public class TowardsPlayerMover : MonoBehaviour
    {
        [Header("Chase Settings")] [SerializeField]
        private float _chaseRange = 5f;

        [SerializeField] private bool _usesFasterSpeedCloseToPlayer;


        [SerializeField] private float _minSpeedCoeff = 0.1f;
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private EnemyHealth _enemyHealth;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_enemyHealth && !_enemyHealth.IsAlive())
                return;

            if (!PointReceiver.Instance)
                return;

            Vector2 playerPos = PointReceiver.Instance.transform.position;
            Vector2 myPos = _rb ? _rb.position : (Vector2)transform.position;
            float dist = Vector2.Distance(myPos, playerPos);
            if (dist > _chaseRange)
                return;

            Vector2 dir = (playerPos - myPos).normalized;
            // Check for wall between enemy and player
            RaycastHit2D hit = Physics2D.Raycast(myPos, dir, (playerPos - myPos).magnitude, _obstacleMask);
            if (hit.collider)
                return;

            Vector2 targetPos;
            // Move towards player
            if (_usesFasterSpeedCloseToPlayer)
            {
                targetPos =
                    myPos + dir * (_moveSpeed * Time.deltaTime * (1 - dist / _chaseRange + _minSpeedCoeff));
            }
            else
            {
                targetPos = myPos + dir * (_moveSpeed * Time.deltaTime);
            }

            if (_rb)
                _rb.MovePosition(targetPos);
            else
                transform.position = targetPos;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _chaseRange);
        }
    }

}