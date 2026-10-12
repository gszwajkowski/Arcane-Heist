using UnityEngine;
using UnityEngine.AI;

public enum GuardState
{
    Patrol,
    Suspicious,
    Alert
}

[RequireComponent(typeof(NavMeshAgent))]
public class BankGuardAI : MonoBehaviour, IDamageable
{
    [Header("Detection Cone")]
    [SerializeField] private float viewDistance = 14f;
    [SerializeField] private float viewAngle = 55f; // Half-angle (110° total cone)
    [SerializeField] private float detectionRate = 45f; // Rate per second in LOS
    [SerializeField] private float detectionDecay = 20f;
    [SerializeField] private LayerMask occlusionMask; // Obstacle / Wall layers

    [Header("Combat & Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackDamage = 12f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Patrol Configuration")]
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private float waypointTolerance = 0.8f;
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 4.2f;

    public GuardState CurrentState { get; private set; } = GuardState.Patrol;
    public float DetectionMeter { get; private set; } = 0f;

    private float currentHealth;
    private int currentWaypointIndex = 0;
    private float nextAttackTime = 0f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private IDamageable playerDamageable;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerDamageable = playerObj.GetComponent<IDamageable>();
        }

        agent.speed = patrolSpeed;
        SetNextWaypoint();

        HeistManager.OnPhaseChanged += HandleGlobalPhaseChanged;
    }

    private void OnDestroy()
    {
        HeistManager.OnPhaseChanged -= HandleGlobalPhaseChanged;
    }

    private void Update()
    {
        if (CurrentState != GuardState.Alert)
        {
            EvaluateLineOfSight();
        }

        switch (CurrentState)
        {
            case GuardState.Patrol:
                HandlePatrol();
                break;

            case GuardState.Suspicious:
                HandleSuspicious();
                break;

            case GuardState.Alert:
                HandleCombat();
                break;
        }
    }

    private void EvaluateLineOfSight()
    {
        if (playerTransform == null) return;

        Vector3 eyeOrigin = transform.position + Vector3.up * 1.6f;
        Vector3 targetPoint = playerTransform.position + Vector3.up * 1.0f;
        Vector3 dirToPlayer = (targetPoint - eyeOrigin).normalized;
        float distToPlayer = Vector3.Distance(eyeOrigin, targetPoint);

        bool canSee = false;

        if (distToPlayer <= viewDistance)
        {
            if (Vector3.Angle(transform.forward, dirToPlayer) <= viewAngle)
            {
                if (!Physics.Raycast(eyeOrigin, dirToPlayer, distToPlayer, occlusionMask))
                {
                    canSee = true;
                }
            }
        }

        if (canSee)
        {
            DetectionMeter += detectionRate * Time.deltaTime;
            CurrentState = GuardState.Suspicious;
            agent.isStopped = true;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0f, dirToPlayer.z)), Time.deltaTime * 5f);

            if (DetectionMeter >= 100f)
            {
                TriggerAlert();
            }
        }
        else
        {
            DetectionMeter = Mathf.Max(0f, DetectionMeter - (detectionDecay * Time.deltaTime));
            if (DetectionMeter <= 0f && CurrentState == GuardState.Suspicious)
            {
                CurrentState = GuardState.Patrol;
                agent.isStopped = false;
            }
        }
    }

    private void HandlePatrol()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            SetNextWaypoint();
        }
    }

    private void SetNextWaypoint()
    {
        if (patrolWaypoints.Length > 0 && patrolWaypoints[currentWaypointIndex] != null)
        {
            agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
        }
    }

    private void HandleSuspicious()
    {
        // Guard stays stationary while analyzing the visual contact
    }

    private void HandleCombat()
    {
        if (playerTransform == null) return;

        agent.SetDestination(playerTransform.position);

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            playerDamageable?.TakeDamage(attackDamage);
        }
    }

    public void TriggerAlert()
    {
        CurrentState = GuardState.Alert;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        HeistManager.Instance.TripAlarm();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (CurrentState != GuardState.Alert)
        {
            TriggerAlert();
        }

        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void HandleGlobalPhaseChanged(HeistPhase phase)
    {
        if (phase == HeistPhase.Loud && CurrentState != GuardState.Alert)
        {
            TriggerAlert();
        }
    }
}
