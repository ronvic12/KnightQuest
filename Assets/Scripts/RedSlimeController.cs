using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimationController), typeof(Collider2D))]
public class RedSlimeController : MonoBehaviour
{
    public PatrolPath path;
    public AudioClip ouch;
    internal PatrolPath.Mover mover;
    internal AnimationController control;
    internal Collider2D _collider;
    internal AudioSource _audio;
    SpriteRenderer spriteRenderer;
    public GameObject knight;
    public float detectionZone;
    public Bounds Bounds => _collider.bounds;
    public float followRange = 0.0f;
    public float minJumpInterval;
    public float maxJumpInterval;
    public GameObject _psystem;
    private float knightToSlimeDist;
    private float knightToSlimeInitDist;
    private Vector3 knightPostOutrunPosition = new Vector3(0, 0, 0);
    private int damage = 50;
    private Vector3 initialSlimePosition;
    private bool gotChasedAtLeastOnce;
    private float timeBeforeJump = 0.0f;
    private float jumpInterval = 0.0f;

    // for pausing when attacked
    private float originalSpeed;
    public float speed;
    private float dazedTime;
    public float startDazeTime;
    public float health = 100.0f;

    void Start()
    {
        initialSlimePosition = gameObject.transform.position;
        jumpInterval = Random.Range(minJumpInterval, maxJumpInterval);
        originalSpeed = control.maxSpeed;
    }

    void Awake()
    {
        control = GetComponent<AnimationController>();
        _collider = GetComponent<Collider2D>();
        _audio = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.gameObject.GetComponent<KnightController>();
        // Debug.Log(player);
        if (player != null) 
        {
            _psystem.transform.position = this.gameObject.transform.position;
            player.DoDamage(damage);
           _psystem.GetComponent<ParticleSystem>().Play();
            Debug.Log(_psystem.GetComponent<ParticleSystem>());
        }
    }

    void Update()
    {
        // pausing mechanism when the enemy got attacked 
        if(dazedTime <= 0)
        {
            control.maxSpeed = originalSpeed;
        }
        else
        {
            control.maxSpeed = 0;
            dazedTime -= Time.deltaTime;
        }

        timeBeforeJump += Time.deltaTime;
        
        if (path != null)
        {
            if (mover == null)
            {
                mover = path.CreateMover(control.maxSpeed * 0.5f);
            }

            knightToSlimeDist = Vector3.Distance(knight.transform.position, gameObject.transform.position);
            knightToSlimeInitDist = Vector3.Distance(knight.transform.position, initialSlimePosition);

            // Debug.Log(knightToSlimeDist);
            // Debug.Log(knightPostOutrunPosition.magnitude);

            if (knightToSlimeDist < followRange && knightToSlimeInitDist < detectionZone)
            {
                path.transform.position = knight.transform.position;
                gotChasedAtLeastOnce = true;
            }
            else if (knightToSlimeDist > followRange && knightToSlimeInitDist > detectionZone && gotChasedAtLeastOnce)
            {
                knightPostOutrunPosition = knight.transform.position;
            }

            if (knightPostOutrunPosition.magnitude > 0.0f && gotChasedAtLeastOnce)
            {
                gotChasedAtLeastOnce = false;
                path.transform.position = initialSlimePosition;
                knightPostOutrunPosition = new Vector3(0,0,0);
            }

            if(timeBeforeJump > jumpInterval)
            {
                timeBeforeJump = 0;
                jumpInterval = Random.Range(minJumpInterval, maxJumpInterval);
                Debug.Log("RED JUMP");
                control.jump = true;
            }
            control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1, 1);

            if(health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        dazedTime = startDazeTime;
        health -= damage;
        // need animator here. (Its animators job).
        Debug.Log("damage Taken!");
    }
}