using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class Enemy_Movement : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Combat, Flee }
    [Header("Stato Attuale")]
    private EnemyState currentState = EnemyState.Patrol;
    public EnemyState previousState = EnemyState.Patrol; //traccia cosa stavamo facendo

    [Header("Riferimenti")]
    public Transform armTransform;
    public Transform Enemy_WeaponHolder;
    private Transform PlayerLocation;
    private Rigidbody2D rb;
    private Animator animator;
    private NavMeshAgent agent;

    [Header("Parametri Scala")]
    private const float scale = 1.7f;

    [Header("Parametri Movimento")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float chanceToChangeStateToMovement = 0.8f;
    [SerializeField] private float chanceToChangeStateToIdle = 0.5f;

    [Header("Parametri IA")]
    private const float allertDistance = 12f;
    private const float loseAggroDistance = 16f;
    private const float combatDistance = 5f;
    private const float pathUpdateTime = 0.2f;

    private float distanceToPlayer;
    private float waitTime;
    private float nextPathUpdateTime;   //uso una variabile differente da waitTime per essere più veloce con le transizioni da Patrol a Chase e da Chase a Combat
    private float aimUpdateInFleeState;
    private float weaponOffsetY;

    private Vector2 moveInput;
    private Vector2 lookTarget;
    private Vector2 currentWalkDirection;
    private Vector2 fleeDirection = Vector2.zero;   //è diverso da Vector2.zero se non ci sono collisioni con altri nemici o con il player
    private bool isMoving;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.updatePosition = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerLocation = player.transform;
        }

        PrecalculateWeaponOffset(); //nel caso in cui i nemici dovessero avere più armi questo si dovrà calcolare magari nel momento in cui si fa il cambio d'arma o ad ogni frame

        //prima mossa iniziale è il randomWalk
        HandlePatrol();
    }

    void Update()
    {
        //per sincronizzare la posizione reale con quella dell'agent in modo continuo
        agent.nextPosition = transform.position;

        //determino in quale degli stati mi trovo:
        //1) Patrol -> RandomWalk, quando non è stato allertato dal player
        //2) Chase -> il nemico comincia ad avvicinarsi al player fino a quando non si trova all'interno della combatDistance (entra nella combat fase)
        //3) Combat -> appena entra nella combatDistance fino a quando il player si allontana troppo (loseAggroDistance) sarà in questa fase
        //4) Flee -> se la sua collisione entra in contatto con quella di un nemico o quella del player, si allontanerà
        DetermineState();

        switch (currentState)
        {
            case EnemyState.Patrol:
                if(Time.time > waitTime)
                {
                    HandlePatrol();
                }
                return;
            case EnemyState.Chase:
                if(Time.time > nextPathUpdateTime)
                {
                    HandleChase();
                }
                return;
            case EnemyState.Combat:
                if(Time.time > waitTime)
                {
                    HandleCombat();
                }
                return;
            case EnemyState.Flee:
                HandleFlee();
                return;
        }
    }


    private void DetermineState()
    {
        //nel caso il vettore fleeDirection non è il vettore nullo signifaca abbiamo colliso con il Player o con un altro nemico
        if (fleeDirection != Vector2.zero)
        {
            ChangeState(EnemyState.Flee);
            return;
        }

        if (currentState == EnemyState.Flee)
        {
            ChangeState(previousState);
            return;
        }

        //se il player non c'è si muove randomicamente
        if(PlayerLocation == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        //dunque calcolo la distanza alll'inizio di ogni frame
        distanceToPlayer = Vector2.Distance(transform.position, PlayerLocation.position);

        //se sto camminando e vedo o sento il nemico (perché è troppo vicino) allora entro in chase
        if (currentState == EnemyState.Patrol)
        {
            if ((distanceToPlayer < allertDistance && hasLineOfSight()) || distanceToPlayer < combatDistance)
            {
                ChangeState(EnemyState.Chase);
                return;
            }
        }
        else if (currentState == EnemyState.Chase)
        {
            //se sono in chase e sono abbastanza vicino al player allora entro in combattimento
            if(distanceToPlayer < combatDistance)
            {
                ChangeState(EnemyState.Combat);
            }
            //se sono in chase ma il player fugge torno a pattugliare
            else if (distanceToPlayer > loseAggroDistance)
            {
                ChangeState(EnemyState.Patrol);
            }
        }
        else if (currentState == EnemyState.Combat)
        {
            //se sono in combattimento ma il nemico fugge allora torno a pattugliare
            if(distanceToPlayer > loseAggroDistance)
            {
                ChangeState(EnemyState.Patrol);
            }
        }
    }


    public bool hasLineOfSight()
    //ritorna false se tra il nemico e il player c'è un muro, altrimenti torna true
    {
        if (PlayerLocation != null)
        {
            Vector2 directionToPlayer = (PlayerLocation.position - transform.position).normalized;

            int wallLayer = LayerMask.GetMask("Wall");

            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, wallLayer);

            if (hit.collider != null)
            {
                return false;
            }
            return true;
        }

        return false;
    }


    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return; //non fare nulla se è già in questo stato

        previousState = currentState;
        currentState = newState;      
    }


    private void PrecalculateWeaponOffset()
    {
        //mi trovo la posizione del muzzle
        Transform muzzle = null;
        for (int i = 0; i < Enemy_WeaponHolder.childCount; i++)
        {
            Transform arma = Enemy_WeaponHolder.GetChild(i);
            if (arma.gameObject.activeInHierarchy && arma.childCount > 0)
            {
                muzzle = arma.GetChild(0);
                break;
            }
        }

        if (muzzle != null)
        {
            //ricorda che funziona solo se l'arma ha coordinate (0,0,0)
            weaponOffsetY = Enemy_WeaponHolder.localPosition.y * Enemy_WeaponHolder.lossyScale.y + muzzle.localPosition.y * muzzle.lossyScale.y;
        }
        else
        {
            weaponOffsetY = Enemy_WeaponHolder.localPosition.y * Enemy_WeaponHolder.lossyScale.y;
        }
    }


    private void HandlePatrol()
    {
        if (isMoving == false)
        {
            if (Random.Range(0f, 1f) <= chanceToChangeStateToMovement)
            {
                isMoving = true;
                currentWalkDirection = RandomDirection();
                lookTarget = (Vector2)transform.position + currentWalkDirection * 5f; //guarda verso dove inizia a camminare
            }
            else
            {
                // Resta fermo, ma cambia direzione dello sguardo a caso
                lookTarget = (Vector2)transform.position + RandomDirection() * 5f;
            }
        }
        else
        {
            if (Random.Range(0f, 1f) <= chanceToChangeStateToIdle)
            {
                isMoving = false;
                currentWalkDirection = Vector2.zero; //si ferma fisicamente
                                                     //quando si ferma, mantiene l'ultimo lookTarget (quindi guarda dritto)
            }
            else
            {
                //continua a muoversi cambiando strada
                currentWalkDirection = RandomDirection();
                lookTarget = (Vector2)transform.position + currentWalkDirection * 5f; //guarda verso la nuova strada
            }
        }

        //se mi muovo allora moveInput avrà come valore currentWalkDirection.normalized * speed altrimenti se sono fermo non mi muoverò
        moveInput = isMoving ? currentWalkDirection.normalized * speed : Vector2.zero;

        //timer per la prossima decisione
        waitTime = Time.time + ComputeWaitTime();

        //aggiorno il movimento e la fisica
        ApplyMovement();
        UpdateAim();
    }


    private void HandleChase()
    {
        agent.SetDestination(PlayerLocation.position);

        currentWalkDirection = agent.desiredVelocity.normalized;

        moveInput = currentWalkDirection.normalized * speed;
        isMoving = true;
        lookTarget = PlayerLocation.position;

        //nel caso più nemici sono stati aggrati nello stesso tempo cerco di non uccidere la cpu
        nextPathUpdateTime = Time.time + pathUpdateTime + Random.Range(-0.05f, 0.05f);

        ApplyMovement();
        UpdateAim();
    }


    private void HandleCombat()
    {
        if (isMoving == false)
        {
            if (Random.Range(0f, 1f) <= chanceToChangeStateToMovement)
            {
                isMoving = true;
                currentWalkDirection = RandomDirectionInCombatMode();
            }
        }
        else
        {
            if (Random.Range(0f, 1f) <= chanceToChangeStateToIdle)
            {
                isMoving = false;
                currentWalkDirection = Vector2.zero;
            }
            else
            {
                //continua a muoversi, ma cambia direzione
                currentWalkDirection = RandomDirectionInCombatMode();
            }
        }

        moveInput = isMoving ? currentWalkDirection * speed : Vector2.zero;
        lookTarget = PlayerLocation.position;

        waitTime = Time.time + ComputeWaitTime();

        ApplyMovement();
        UpdateAim();
    }


    private void HandleFlee()
    {
        currentWalkDirection = fleeDirection.normalized;
        moveInput = currentWalkDirection * speed;
        isMoving = true;

        if (Time.time > aimUpdateInFleeState)
        {
            //se prima di scappare non stavo combattendo o non ero in chase non guardo il player
            if (previousState == EnemyState.Patrol || PlayerLocation == null)
            {
                lookTarget = (Vector2)transform.position + currentWalkDirection * 5f;
            }
            else
            {
                //devo fare in modo che quando sono in combattimento continuo a guardare il player
                lookTarget = PlayerLocation.position;
            }

            UpdateAim();
            aimUpdateInFleeState = Time.time + 0.5f; //faccio questo perché altrimenti avviene che nella fuga ci sta un update della mira troppo veloce
        }

        waitTime = Time.time + 0.6f;

        //devo fare in modo che torno nello stato giusto dopo essere scappato

        ApplyMovement();

        // Azzeriamo la forza di fuga alla fine del frame
        // Se nel frame successivo c'è ancora qualcuno nel trigger, OnTriggerStay2D la ricalcolerà.
        // Se non c'è più nessuno, rimarrà a zero e il nemico smetterà di fuggire.
        fleeDirection = Vector2.zero;
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        // Se chi è entrato nel nostro spazio vitale è un altro nemico o il player
        if ((collision.CompareTag("Enemy") && collision.gameObject != this.gameObject) || collision.CompareTag("Player"))
        {
            // Calcola la direzione per allontanarci da lui
            Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
            fleeDirection += pushDirection;
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Vector2 wallNormal = collision.contacts[0].normal;
            if (Vector2.Dot(currentWalkDirection, wallNormal) < 0)
            {
                currentWalkDirection = Vector2.Reflect(currentWalkDirection, wallNormal).normalized;

                if(currentState == EnemyState.Patrol || PlayerLocation == null)
                {
                    lookTarget = (Vector2)transform.position + currentWalkDirection * 5f;
                }
                else
                {
                    lookTarget = PlayerLocation.position;
                }
                moveInput = currentWalkDirection * speed;

                // Urto contro un muro? Aggiorna istantaneamente la fisica!
                ApplyMovement();
                UpdateAim();
            }
        }
    }


    private float ComputeWaitTime()
    {
        if (isMoving)
        {
            if (currentState == EnemyState.Combat)
            {
                return Random.Range(0.5f, 1.5f);     //se sono in movimento mentre combatto mi muovo più spesso
            }
            return Random.Range(1f, 2f);
        }
        if (currentState != EnemyState.Combat)
        {
            return Random.Range(1.5f, 3.0f);
        }
        return Random.Range(0.5f, 1f);  //se sono fermo mentre combatto aspetto molto meno
    }


    Vector2 RandomDirection()
    {
        int choice = Random.Range(0, 8);
        switch (choice)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            case 3: return Vector2.right;
            case 4: return new Vector2(1, 1).normalized;
            case 5: return new Vector2(1, -1).normalized;
            case 6: return new Vector2(-1, 1).normalized;
            default: return new Vector2(-1, -1).normalized;
        }
    }


    Vector2 RandomDirectionInCombatMode()
    {
        //più lontano siamo dal nemico più la possiblità di andare verso di lui sono alte
        //però se siamo troppo vicino (shootingDistance o oltre) non mi muovo più verso il player
        float sidewaysProb = 0.4f;

        if (Random.Range(0, 1f) < sidewaysProb)
        {
            Vector2 strafeDirection = Vector2.Perpendicular(transform.position - PlayerLocation.position).normalized;

            if (Random.Range(0, 1f) < 0.5f)
            {
                strafeDirection = -strafeDirection;
            }

            return strafeDirection;
        }
        else
        {
            Vector2 forwarOrBackwards = (Vector2)(PlayerLocation.position - transform.position).normalized;

            float roll = Random.Range(0f, 1f);

            float f1 = (distanceToPlayer / (loseAggroDistance / 2));
            float f2;
            if (distanceToPlayer != 0)
            {
                f2 = ((loseAggroDistance / 2) / distanceToPlayer);
            }
            else
            {
                f2 = 1f;
            }

            float normalisingFactor = f1 + f2;

            float p1 = f1 / normalisingFactor;
            //float p2 = f2 / normalisingFactor;
            //se siamo troppo vicini vale la funzione distance/(loseAggroDistance / 2) per andare avanti
            //e (loseAggroDistance/2)/distance per andare indietro

            //mi muovo verso il player (solo se non siamo troppo vicini al player)
            if (roll <= p1 && distanceToPlayer > combatDistance && distanceToPlayer < loseAggroDistance)
            {
                //prova ad usare agent.setDestination() per andare verso il player
                agent.SetDestination(PlayerLocation.position);

                return agent.desiredVelocity.normalized;
            }
            else
            {
                //mi allontano dal player
                return -forwarOrBackwards;
            }

        }
    }


    private void ApplyMovement()
    {
        // Si occupa SOLO di far camminare/scivolare il nemico
        rb.linearVelocity = moveInput;
        animator.SetBool("isMoving", isMoving);
    }

    private void UpdateAim()
    {
        // Si occupa SOLO di girare il corpo e ruotare il braccio
        Vector2 directionFromCenter = lookTarget - (Vector2)transform.position;
        FlipSprite(directionFromCenter.x);

        Vector2 aimDirection = lookTarget - (Vector2)armTransform.position;

        float diagonale = (aimDirection).magnitude;

        //distanza da gomito al mouse
        float x = Mathf.Sqrt(Mathf.Max(0, (diagonale * diagonale) - (weaponOffsetY * weaponOffsetY)));

        float offsetAngle = Mathf.Atan2(weaponOffsetY, x) * Mathf.Rad2Deg;

        if (transform.localScale.x < 0)
        {
            float gomitoToTarget = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg + offsetAngle;
            armTransform.rotation = Quaternion.Euler(0, 0, gomitoToTarget + 180);
        }
        else
        {
            float gomitoToTarget = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - offsetAngle;
            armTransform.rotation = Quaternion.Euler(0, 0, gomitoToTarget);
        }
    }


    void FlipSprite(float directionX)
    {
        if (directionX > 0)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else if (directionX < 0)
        {
            transform.localScale = new Vector3(-scale, scale, scale);
        }
    }


    private void OnDrawGizmosSelected()
    {
        // 1. Disegniamo un cerchio giallo enorme per l'allerta futura
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, allertDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, combatDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseAggroDistance);
    }

    internal EnemyState getCurrentState()
    {
        return currentState;
    }
}
