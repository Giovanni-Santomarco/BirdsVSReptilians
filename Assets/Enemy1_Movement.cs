using UnityEngine;

public class Enemy1_Movement : MonoBehaviour
{
    private Transform PlayerLocation;
    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 currentWalkDirection; // Memorizza la direzione attuale della pattuglia

    private bool isMoving; // La "Mente": il nemico ha intenzione di passeggiare?
    private float waitTime;
    private float distance;

    private Vector2 fleeDirection = Vector2.zero;
    private const float touchedDistance = 0.9f;
    private const float touchedDistanceWithPlayer = 2f;
    private const float allertDistance = 25f;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float chanceToChangeStateToMovement = 0.8f;
    [SerializeField] private float chanceToChangeStateToIdle = 0.5f;

    private bool allerted = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
            return Random.Range(0.5f, 1.5f);
        }
        return Random.Range(1.5f, 3.0f);
    }

    void RandomWalk()
    {
        if (isMoving == false)
        {
            if (Random.Range(0f, 1f) <= chanceToChangeStateToMovement)
            {
                isMoving = true;
                currentWalkDirection = RandomDirection();
            }
            else
            {
                //resta fermo, ma può cambiare direzione dove guardare
                Vector2 randomLookDirection = RandomDirection();
                FlipSprite(randomLookDirection.x);
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
                //continua a muoversi, ma cambia direzione di pattuglia
                currentWalkDirection = RandomDirection();
            }
        }

        //timer per la prossima decisione
        waitTime = Time.time + ComputeWaitTime();
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
        // 1. Logica di Pattuglia (si attiva solo a timer scaduto e se non è in allerta)
        if (!allerted && Time.time > waitTime)
        {
            RandomWalk();
        }

        // 2. Controllo della Fuga (valutiamo i dati raccolti da OnTriggerStay2D)
        if (fleeDirection != Vector2.zero)
        {
            // Hackeriamo la mente: Dimentica la passeggiata, scappa!
            currentWalkDirection = fleeDirection.normalized;
            isMoving = true;

            // Blocca il timer della pattuglia per 1 secondo
            waitTime = Time.time + 1f;
        }

        // 3. Logica Pathfinding Futura...

        // 4. Applicazione diretta del Movimento
        if (isMoving && currentWalkDirection != Vector2.zero)
        {
            moveInput = currentWalkDirection * speed;
            FlipSprite(moveInput.x);
        }
        else
        {
            moveInput = Vector2.zero;
        }

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
                FlipSprite(currentWalkDirection.x);
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
    }
}