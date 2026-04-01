using UnityEngine;

public class Marksman_Movement : Enemy_Movement
{
    private bool isAimLocked = false;   //serve per fare in modo che se sto mirando terrò la mira ferma


    public void setIsAimLocked(bool state)
    {
        isAimLocked = state;
    }

    //Update is called once per frame
    protected override void Update()
    {
        //per sincronizzare la posizione reale con quella dell'agent in modo continuo
        agent.nextPosition = transform.position;

        //determino in quale degli stati mi trovo:
        //1) Patrol -> RandomWalk, quando non è stato allertato dal player
        //2) Chase -> non varrà per il cecchino che preferirà rimanere lontano
        //3) Combat -> appena entra nella combatDistance fino a quando il player si allontana troppo (loseAggroDistance) sarà in questa fase
        //4) Flee -> se la sua collisione entra in contatto con quella di un nemico o quella del player, si allontanerà
        DetermineState();

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (Time.time > waitTime)
                {
                    HandlePatrol();
                }
                return;
            case EnemyState.Combat:
                if (Time.time > waitTime)
                {
                    HandleCombat();
                }
                return;
            case EnemyState.Flee:
                HandleFlee();
                return;
        }
    }


    protected override void DetermineState()
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
        if (PlayerLocation == null)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        //dunque calcolo la distanza all'inizio di ogni frame
        distanceToPlayer = Vector2.Distance(transform.position, PlayerLocation.position);

        //se sto camminando e vedo o sento il nemico (perché è troppo vicino) allora entro in combattimento
        if (currentState == EnemyState.Patrol)
        {
            if (hasLineOfSight() || distanceToPlayer < combatDistance)
            {
                ChangeState(EnemyState.Combat);
                return;
            }
        }

        // se il marksman entra in modalità combattimento non torna in modalità pattuglia
    }


    protected override void HandleCombat()
    {
        if (isMoving == false)
        {
            if (distanceToPlayer < combatDistance)
            {
                isMoving = true;
                currentWalkDirection = (transform.position - PlayerLocation.position).normalized;
            }
        }
        else
        {
            isMoving = false;
            currentWalkDirection = Vector2.zero;
        }

        moveInput = isMoving ? currentWalkDirection * speed : Vector2.zero;
        lookTarget = PlayerLocation.position;
        

        waitTime = Time.time + ComputeWaitTime();

        ApplyMovement();
        UpdateAim();
    }


    protected override void UpdateAim()
    {
        //se sto mirando e quindi per sparare non sposto la mira
        if (isAimLocked)
        {
            return;
        }
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




}
