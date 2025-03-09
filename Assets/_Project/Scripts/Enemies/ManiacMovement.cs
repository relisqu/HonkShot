using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ManiacMovement : MonoBehaviour
{

    [SerializeField] private Transform _target;
    [SerializeField] private float _movementSpeed = 200f;
    [SerializeField] private float _nextWaypointDistance = 3f;

    private Path _path;
    private int _currentWaypoint = 0;
    private bool _reachedEndOfPath = false;

    [SerializeField] Seeker _seeker;
    [SerializeField] Rigidbody2D _rb;
    // Start is called before the first frame update
    void Start()
    {

        InvokeRepeating("UpdatePath", 0f, .5f);
        
    }

    private void UpdatePath()
    {
        if(_seeker.IsDone())
            _seeker.StartPath(_rb.position, _target.position, OnPathComplete);
    }
    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            _currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_path == null) return;

        if(_currentWaypoint >= _path.vectorPath.Count)
        {
            _reachedEndOfPath = true;
            return;
        }
        else
        {
            _reachedEndOfPath = false;
        }

        Vector2 dirction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rb.position).normalized;
        Vector2 force = dirction * _movementSpeed * Time.deltaTime;

        _rb.AddForce(force);

        float distance = Vector2.Distance(_rb.position, _path.vectorPath[_currentWaypoint]);

        if(distance < _nextWaypointDistance)
        {
            _currentWaypoint++;
        }
    }
}
