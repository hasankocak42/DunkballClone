using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Serialize
    [SerializeField] private GameObject _ground;
    [SerializeField] private ParticleSystem _groundParticle;
    [SerializeField] private ParticleSystem[] _afterPointParticle;
    [SerializeField] private Transform _camFollow;
    [SerializeField] private Transform _basket;
    [SerializeField] private Transform _moveBall;
    #endregion
    #region Private
    private Vector3 _followofset = new Vector3(0, 0, -1);
    private float groundForce = 5f;
    private float smoothSwipe = 3f;
    private float initialAngle = 75f;
    private Vector2 mouseFirstPosition;
    private Vector2 mouseLastPosition;
    private Vector3 ballFirstPosition;
    private Vector2 mousePosDiff;
    private Rigidbody ballRb;
    private Collider groundCollider;
    private bool isForce;
    private bool isShoot;
    private bool isJump;
    AudioSource audioSourse;
    #endregion
    void Start()
    {
        audioSourse = GetComponent<AudioSource>();
        isShoot = false;
        isJump = true;
        isForce = true;
        mousePosDiff = mouseLastPosition = mouseFirstPosition = Vector2.zero;
        ballFirstPosition = Vector3.zero;
        ballRb = GetComponentInChildren<Rigidbody>();
        _camFollow.position = new Vector3(_moveBall.position.x + _followofset.x, _camFollow.position.y, _moveBall.position.z + _followofset.z);
        groundCollider = _ground.gameObject.GetComponent<Collider>();
    }

    void Update()
    {
        MoveControl();
    }
    private void LateUpdate()
    {
        _camFollow.position = new Vector3(_moveBall.position.x + _followofset.x, _camFollow.position.y, _moveBall.position.z + _followofset.z);
        _moveBall.LookAt(_basket.position);
        _camFollow.LookAt(_basket.position);
    }

    private void MoveControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseFirstPosition = Input.mousePosition;
            ballFirstPosition = transform.position;
            if (groundCollider.bounds.size.z / 2.5 > Mathf.Abs(transform.position.z))
            {
                isShoot = true;
            }
            else
            {
                isShoot = false;
            }
        }
        if (Input.GetMouseButton(0))
        {
            mouseLastPosition = Input.mousePosition;
            mousePosDiff = mouseLastPosition - mouseFirstPosition;
            #region Move Control
            if (mousePosDiff.x < 0f)
            {
                ChangePosition(-_moveBall.right, smoothSwipe);


            }
            if (mousePosDiff.x > 0f)
            {
                ChangePosition(_moveBall.right, smoothSwipe);

            }
            if (mousePosDiff.y > 0f)
            {
                ChangePosition(_camFollow.forward, (smoothSwipe / 2));

            }
            if (mousePosDiff.y < 0f)
            {
                ChangePosition(-_camFollow.forward, smoothSwipe);

            }
            #endregion
        }
        if (Input.GetMouseButtonUp(0))
        {
            float findDiff;
            findDiff = transform.position.z - ballFirstPosition.z;
            if (mousePosDiff.y > (Screen.height / 5f) && isJump && isShoot)
            {
                isJump = false;
                ShootBall();
            }
            else if (findDiff < (groundCollider.bounds.size.z / 5f) && mousePosDiff.y > (Screen.height / 5f) && isJump)
            {
                isJump = false;
                transform.rotation = Quaternion.AngleAxis(45, Vector3.left);
                ballRb.AddForce(transform.forward * 5f, ForceMode.Impulse);
            }
        }
    }

    void ChangePosition(Vector3 newPos, float smoothNess)
    {
        transform.Translate(newPos * smoothNess * Time.deltaTime, Space.World);
        ballRb.AddForce(newPos * Time.deltaTime, ForceMode.Impulse);
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.tag == "Ground")
        {
            audioSourse.Play();
            isJump = true;
            _groundParticle.transform.position = _moveBall.position;
            _groundParticle.gameObject.SetActive(true);
            _groundParticle.Play();
            if (isForce)
            {
                ballRb.velocity = ballRb.velocity / 2f;
                ballRb.AddForce(Vector3.up * groundForce, ForceMode.VelocityChange);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Point")
        {
            foreach (ParticleSystem particle in _afterPointParticle)
            {
                particle.gameObject.SetActive(true);
            }
            isForce = false;
            gameObject.GetComponent<SphereCollider>().material.bounciness = 0.5f;
            GameManager.isGameOver = true;
        }
    }
    void ShootBall()
    {
        if (Mathf.Abs(_basket.transform.position.x - transform.position.x) < 1f && Mathf.Abs(_basket.transform.position.z - transform.position.z) < 0.7f)
        {
            ballRb.AddForce(Vector3.back * ballRb.mass * 2f, ForceMode.VelocityChange);
        }
        else
        {
            #region Shoot 
            float gravity = Physics.gravity.magnitude;
            float angle = initialAngle * Mathf.Deg2Rad;
            Vector3 p = _basket.position;


            Vector3 planarTarget = new Vector3(p.x, 0, p.z);
            Vector3 planarPostion = new Vector3(_moveBall.position.x, 0, _moveBall.position.z);
            float distance = Vector3.Distance(planarTarget, planarPostion);


            float yOffset = _moveBall.position.y - p.y;
            float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
            Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

            float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > _moveBall.position.x ? 1 : -1);
            Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

            ballRb.velocity = finalVelocity;
            #endregion

        }
    }
}
