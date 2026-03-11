using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Enemy1_Movement : MonoBehaviour
{
    private Transform PlayerLocation;
    private Rigidbody2D rb;
    private Animator animator;
    public Transform armTransform;
    public Transform Enemy1_WeaponHolder;

    private Vector2 moveInput;
    private Vector2 currentWalkDirection; // Memorizza la direzione attuale della pattuglia
    private Vector2 currentLookDirection = Vector2.right;

    private bool isMoving; //il nemico ha intenzione di passeggiare?
    private float waitTime;
    private float nextPathUpdateTime;   //uso una variabile differente da waitTime per essere più veloce con le transizioni da Patrol a Chase e da Chase a Combat
    private const float pathUpdateTime = 0.2f;
    private float distance;

    private Vector2 fleeDirection = Vector2.zero;
    private const float allertDistance = 12f;   //distanza da cui inizia l'aggro
    private const float loseAggroDistance = 16f; // Distanza a cui smette di inseguire
    private const float combatDistance = 5f;

    private NavMeshAgent agent;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float chanceToChangeStateToMovement = 0.8f;
    [SerializeField] private float chanceToChangeStateToIdle = 0.5f;

    private bool allerted = false;
    private bool combatMode = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.updatePosition = false;

        InitializeStartingMovement();
    }

    void InitializeStartingMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerLocation = player.transform;
        }
        else
        {
            Debug.LogWarning("Player non trovato");
        }

        // Stato iniziale: Completamente fermo
        isMoving = false;
        currentWalkDirection = Vector2.zero;
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

    float ComputeWaitTime()
    {
        if (isMoving)
        {
            if (!combatMode)
            {
                return Random.Range(0.5f, 1.5f);
            }
            return Random.Range(1f, 2f);    //se sono in movimento mentre combatto mi muovo più spesso
        }
        if (!combatMode)
        {
            return Random.Range(1.5f, 3.0f);
        }
        return Random.Range(0.5f, 1f);  //se sono fermo mentre combatto
    }

    //void RandomWalk()
    //{
    //    if (isMoving == false)
    //    {
    //        if (Random.Range(0f, 1f) <= chanceToChangeStateToMovement)
    //        {
    //            isMoving = true;
    //            currentWalkDirection = RandomDirection();
    //        }
    //        else
    //        {
    //            //resta fermo, ma può cambiare direzione dove guardare
    //            Vector2 randomLookDirection = RandomDirection();
    //            HandleAiming(randomLookDirection - (Vector2)transform.position);
    //            //FlipSprite(randomLookDirection.x);
    //        }
    //    }
    //    else
    //    {
    //        if (Random.Range(0f, 1f) <= chanceToChangeStateToIdle)
    //        {
    //            isMoving = false;
    //            currentWalkDirection = Vector2.zero;
    //        }
    //        else
    //        {
    //            //continua a muoversi, ma cambia direzione di pattuglia
    //            currentWalkDirection = RandomDirection();
    //        }
    //    }

    //    //timer per la prossima decisione
    //    waitTime = Time.time + ComputeWaitTime();
    //}


    void RandomWalk()
    {
        if (isMoving == false)
        {
            if (Random.Range(0f, 1f) <= chanceToChangeStateToMovement)
            {
                isMoving = true;
                currentWalkDirection = RandomDirection();
                currentLookDirection = currentWalkDirection; // Guarda verso dove inizia a camminare
            }
            else
            {
                // Resta fermo, ma cambia direzione dello sguardo a caso
                currentLookDirection = RandomDirection();
            }
        }
        else
        {
            if (Random.Range(0f, 1f) <= chanceToChangeStateToIdle)
            {
                isMoving = false;
                currentWalkDirection = Vector2.zero; // Si ferma fisicamente
                                                     // Quando si ferma, mantiene l'ultimo currentLookDirection (quindi guarda dritto)
            }
            else
            {
                // Continua a muoversi cambiando strada
                currentWalkDirection = RandomDirection();
                currentLookDirection = currentWalkDirection; // Guarda verso la nuova strada
            }
        }

        //timer per la prossima decisionez
        waitTime = Time.time + ComputeWaitTime();
    }


    Vector2 RandomDirectionInCombatMode()
    {
        //più lontano siamo dal nemico più la possiblità di andare verso di lui sono alte
        //però se siamo troppo vicino (shootingDistance o oltre) non mi muovo più verso il player
        float sidewaysProb = 0.5f;
        //float forwardOrBackwardProb = 0.67f;

        if(Random.Range(0, 1f) < sidewaysProb)
        {
            Vector2 strafeDirection = Vector2.Perpendicular(transform.position - PlayerLocation.position).normalized;

            if(Random.Range(0, 1f) < 0.5f)
            {
                strafeDirection = -strafeDirection;
            }

            return strafeDirection;
        }
        else
        {
            Vector2 forwarOrBackwards = (Vector2)(PlayerLocation.position - transform.position).normalized;

            float roll = Random.Range(0f, 1f);

            float f1 = (distance / (loseAggroDistance / 2));
            float f2;
            if(distance != 0)
            {
                f2 = ((loseAggroDistance / 2) / distance);
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
            if(roll <= p1 && distance > combatDistance && distance < loseAggroDistance)
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


    void CombatMovement()
    {
        if(isMoving == false)
        {
            if(Random.Range(0f, 1f) <= chanceToChangeStateToMovement)
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
        waitTime = Time.time + ComputeWaitTime();
    }


    private bool hasLineOfSight()
    //ritorna false se tra il nemico e il player c'è un muro, altrimenti torna true
    {
        if(PlayerLocation != null && distance < allertDistance)
        {
            Vector2 directionToPlayer = (PlayerLocation.position - transform.position).normalized;

            int wallLayer = LayerMask.GetMask("Wall");

            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distance, wallLayer);

            if(hit.collider != null)
            {
                return false;
            }
            return true;
        }

        return false;
    }


    void HandleAiming(Vector2 targetDirection)
    {
        // --- GESTIONE DELLO SGUARDO ---
        Vector2 directionFromCenter = targetDirection - (Vector2)transform.position;
        FlipSprite(directionFromCenter.x);

        Vector2 aimDirection = targetDirection - (Vector2)armTransform.position;
        //distanza dalla spalla alla y del muzzle dell'arma
        Transform muzzle = null;
        for (int i = 0; i < Enemy1_WeaponHolder.childCount; i++)
        {
            Transform arma = Enemy1_WeaponHolder.GetChild(i);
            if (arma.gameObject.activeInHierarchy && arma.childCount > 0)
            {
                muzzle = arma.GetChild(0);
                break;
            }
        }
        float y = 0f;
        if (muzzle != null)
        {
            //ricorda che funziona solo se l'arma ha coordinate (0,0,0)
            y = Enemy1_WeaponHolder.localPosition.y * Enemy1_WeaponHolder.lossyScale.y + muzzle.localPosition.y * muzzle.lossyScale.y;
        }
        else
        {
            y = Enemy1_WeaponHolder.localPosition.y * Enemy1_WeaponHolder.lossyScale.y;
        }

        float diagonale = (aimDirection).magnitude;

        //distanza da gomito al mouse
        float x = Mathf.Sqrt(Mathf.Max(0, (diagonale * diagonale) - (y * y)));

        float offsetAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        float gomitoToMouse;

        //Debug.DrawRay(armTransform.position, direction * 10, Color.red);
        // Verde: Direzione Reale Sparo (Mano)
        //Debug.DrawRay(Enemy1_WeaponHolder.position, Enemy1_WeaponHolder.right * (10) * transform.localScale.x, Color.green);

        Debug.Log(x);

        if (transform.localScale.x < 0)
        {
            //dobbiamo aggiungere l'offset invece di sottrarlo dato che siamo nel semisfero sinistro
            gomitoToMouse = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - offsetAngle * (-1);
            // Se guardiamo a sinistra, dobbiamo invertire la logica della rotazione
            armTransform.rotation = Quaternion.Euler(0, 0, gomitoToMouse + 180);
        }
        else
        {
            gomitoToMouse = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - offsetAngle;
            armTransform.rotation = Quaternion.Euler(0, 0, gomitoToMouse);
        }

    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        // Se chi è entrato nel nostro spazio vitale è un altro nemico...
        if (collision.CompareTag("Enemy") && collision.gameObject != this.gameObject)
        {
            // Calcola la direzione per allontanarci da lui
            Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
            fleeDirection += pushDirection;
        }
        // oppure se è il Player
        else if (collision.CompareTag("Player"))
        {
            Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
            fleeDirection += pushDirection;
        }
    }

    // Metodo dedicato per girare la grafica del nemico
    void FlipSprite(float directionX)
    {
        if (directionX > 0)
        {
            transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
        }
        else if (directionX < 0)
        {
            transform.localScale = new Vector3(-1.7f, 1.7f, 1.7f);
        }
    }

    void Update()
    {
        //per sincronizzare la posizione reale con quella dell'agent in modo continuo
        agent.nextPosition = transform.position;

        // 0. controllo della distanza con il Player
        if (PlayerLocation != null)
        {
            distance = Vector2.Distance(this.transform.position, PlayerLocation.position);

            //è allertato se siamo nella zona di allerta e se ci vede, ma se siamo troppo vicini si allerta comunque
            if((distance < allertDistance && hasLineOfSight()) || distance < combatDistance)
            {
                allerted = true;
            }
            else if(distance > loseAggroDistance)
            {
                allerted = false;   //se il player fugge via lontano, tornerò il mio randomWalk
                combatMode = false;
            }
        }

        //Calcoliamo la Direzione in cui vogliamo andare (Pattuglia o Inseguimento)
        Vector2 targetDirection = Vector2.zero;

        // 1. Logica Pathfinding (per avvicinarmi al player fino alla distanza di combattimento)
        if (allerted && PlayerLocation!=null && !combatMode)
        {
            if (Time.time >= nextPathUpdateTime)
            {
                agent.SetDestination(PlayerLocation.position);
                HandleAiming((Vector2)PlayerLocation.position);

                //nel caso più nemici sono stati aggrati nello stesso tempo cerco di non uccidere la cpu
                nextPathUpdateTime = Time.time + pathUpdateTime + Random.Range(-0.05f, 0.05f);
            }
            targetDirection = agent.desiredVelocity.normalized;
            isMoving = true;

            if(distance < combatDistance)
            {
                combatMode = true;
            }
        }
        else
        {   
            // Logica di Combattimento (mi muovo random ma posso anche sparare)
            if (allerted && combatMode && Time.time > waitTime && PlayerLocation != null)
            {
                CombatMovement();
                HandleAiming((Vector2)PlayerLocation.position);

                //tocca capire come fare la meccanica di shooting
            }
            else
            {
                // 2. Logica di Pattuglia (si attiva solo a timer scaduto e se non è in allerta)
                if (!allerted && Time.time > waitTime && !combatMode)
                {
                    RandomWalk();
                    HandleAiming((Vector2)transform.position + currentLookDirection * 5f); //se mi muovo randomicamente guardo miro verso dove mi sto muovendo
                }
            }
            targetDirection = currentWalkDirection;
        }

        // 3. Controllo della Fuga (valutiamo i dati raccolti da OnTriggerStay2D)
        if (fleeDirection != Vector2.zero)
        {
            // dimentica il randomWalk e comincio a scappare
            targetDirection = fleeDirection.normalized;
            currentWalkDirection = targetDirection;
            isMoving = true;

            // Blocca il timer della pattuglia per 1 secondo
            waitTime = Time.time + 1f;
        }

        // 4. Applicazione diretta del Movimento
        if (isMoving && targetDirection != Vector2.zero)
        {
            moveInput = targetDirection * speed;
        }
        else
        {
            moveInput = Vector2.zero;
        }

        //da fare in una funzione dipendente da HandleAiming
        // --- GESTIONE DELLO SGUARDO (Sempre attiva) ---
        //if (allerted && PlayerLocation != null)
        //{
        //    // Se è allertato, guarda SEMPRE il player, anche se è fermo
        //    Vector2 d = PlayerLocation.position - transform.position;
        //    FlipSprite(d.x);
        //}
        //else if (isMoving && targetDirection != Vector2.zero)
        //{
        //    // Se sta solo pattugliando, guarda dove cammina
        //    FlipSprite(moveInput.x);
        //}

        // Muovi fisicamente il nemico
        rb.linearVelocity = moveInput;
        animator.SetBool("isMoving", isMoving);

        // Azzeriamo la forza di fuga alla fine del frame
        // Se nel frame successivo c'è ancora qualcuno nel trigger, OnTriggerStay2D la ricalcolerà.
        // Se non c'è più nessuno, rimarrà a zero e il nemico smetterà di fuggire.
        fleeDirection = Vector2.zero;
    }


    // 5. Gestione del Rimbalzo sui muri tramite i Layer
    private void OnCollisionStay2D(Collision2D collision)
    {
        int wallLayerIndex = LayerMask.NameToLayer("Wall");

        if (collision.gameObject.layer == wallLayerIndex)
        {
            // Otteniamo la "normale", ovvero la freccia che esce perpendicolare dal muro
            Vector2 wallNormal = collision.contacts[0].normal;

            // Il Vector2.Dot ci dà un numero negativo se le due direzioni si "scontrano" (puntano una verso l'altra)
            // Se è minore di 0, significa che il nemico sta cercando di camminare DENTRO il muro
            if (Vector2.Dot(currentWalkDirection, wallNormal) < 0)
            {
                // Rimbalziamo!
                currentWalkDirection = Vector2.Reflect(currentWalkDirection, wallNormal).normalized;
                if (!combatMode && !allerted && PlayerLocation!=null)
                {
                    HandleAiming(currentWalkDirection * 15);
                }
                else
                {
                    HandleAiming((Vector2)PlayerLocation.position);
                }
                //FlipSprite(currentWalkDirection.x);
            }
            // Se invece il Dot è maggiore di 0 (es. sta scivolando via o allontanandosi), 
            // la funzione lo ignora e gli lascia continuare la sua strada liberamente.
        }
    }


    // --- DEBUG VISIVO NELLA SCENA ---
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
}