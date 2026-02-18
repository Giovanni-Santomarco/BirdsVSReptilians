using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class PlayerMovement : MonoBehaviour
{
    [Header("Impostazioni")]
    [SerializeField] private float speed = 5f;

    [Header("Aiming")]
    [SerializeField] private Transform armTransform;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    public Transform WeaponHolder;
    public float maxWristAngle = 20f;

    private InventoryManager im;

    private bool isTouchingWall = false;
    private Vector2 currentWallNormal;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        im = GetComponent<InventoryManager>();

    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        int wallLayerIndex = LayerMask.NameToLayer("Wall");

        if (collision.gameObject.layer == wallLayerIndex)
        {
            isTouchingWall = true;
            // Salviamo la direzione verso cui "guarda" la faccia del muro
            currentWallNormal = collision.contacts[0].normal;
        }
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        int wallLayerIndex = LayerMask.NameToLayer("Wall");

        if (collision.gameObject.layer == wallLayerIndex)
        {
            // Appena ci stacchiamo dal muro, spegniamo la correzione
            isTouchingWall = false;
            currentWallNormal = Vector2.zero;
        }
    }


    void Update()
    {
        // 2. ANIMAZIONE
        bool isMoving = moveInput.magnitude > 0;
        animator.SetBool("isWalking", isMoving);

        // 3. MIRA E ROTAZIONE DEL CORPO (Tutto gestito dal mouse)
        HandleAimingAndTurning();
        //HandleAimingAndTurning2();
    }

    // --- LOGICA INPUT ---
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }


    //gestisce tutta la fisica (movimento e collisioni)
    void FixedUpdate()
    {
        Vector2 actualMoveDirection = moveInput;

        // CORREZIONE DEL MURO
        if (isTouchingWall && Vector2.Dot(actualMoveDirection, currentWallNormal) < 0)
        {
            // Proiettiamo e normalizziamo il vettore temporaneo
            actualMoveDirection = Vector3.ProjectOnPlane(actualMoveDirection, currentWallNormal).normalized;
        }

        // Applichiamo il vettore calcolato e corretto al Rigidbody
        rb.linearVelocity = actualMoveDirection * speed;
    }


    void HandleAimingAndTurning()
    {
        // A. TROVIAMO IL MOUSE
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Camera.main.nearClipPlane;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // B. FLIP DEL CORPO
        Vector3 direction = mouseWorldPosition - transform.position;
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // Faccia a Destra
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f); // Faccia a Sinistra
        }

        // C. ROTAZIONE SPALLA (Movimento principale)
        // La spalla punta dritta al mouse (senza correzioni complesse)
        Vector3 armDirection = mouseWorldPosition - armTransform.position;
        float armAngle = Mathf.Atan2(armDirection.y, armDirection.x) * Mathf.Rad2Deg;

        // Correggiamo l'angolo se siamo specchiati
        if (transform.localScale.x < 0)
        {
            // Se guardiamo a sinistra, dobbiamo invertire la logica della rotazione
            armTransform.rotation = Quaternion.Euler(0, 0, armAngle + 180);
        }
        else
        {
            armTransform.rotation = Quaternion.Euler(0, 0, armAngle);
        }


        // D. ROTAZIONE POLSO 
        // 1. Calcoliamo l'angolo PERFETTO che dovrebbe avere la mano per colpire il mouse
        Vector3 handToMouse = mouseWorldPosition - WeaponHolder.position;
        float idealHandAngle = Mathf.Atan2(handToMouse.y, handToMouse.x) * Mathf.Rad2Deg;

        // 2. Calcoliamo la differenza tra l'angolo del braccio e l'angolo ideale della mano
        // Mathf.DeltaAngle gestisce automaticamente il passaggio da 360 a 0 gradi
        float angleDifference = Mathf.DeltaAngle(armAngle, idealHandAngle);

        // 3. Limitiamo la rotazione (Clamp) tra -20 e +20 gradi
        float wristRotation = Mathf.Clamp(angleDifference, -maxWristAngle, maxWristAngle);

        // 4. Applichiamo la rotazione LOCALE alla mano
        // Se siamo flippati, dobbiamo invertire la rotazione del polso
        if (transform.localScale.x < 0)
        {
            WeaponHolder.localRotation = Quaternion.Euler(0, 0, -wristRotation);
        }
        else
        {
            WeaponHolder.localRotation = Quaternion.Euler(0, 0, wristRotation);
        }

        // ---------------- DEBUG ----------------
        // Rosso: Direzione Spalla
        Debug.DrawRay(armTransform.position, armTransform.right * 10, Color.red);
        // Verde: Direzione Reale Sparo (Mano)
        Debug.DrawRay(WeaponHolder.position, WeaponHolder.right * 10, Color.green);
    }



    void HandleAimingAndTurning2()
    {
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Camera.main.nearClipPlane;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // B. FLIP DEL CORPO
        Vector3 direction = mouseWorldPosition - armTransform.position;
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // Faccia a Destra
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-0.6f, 0.6f, 0.6f); // Faccia a Sinistra
        }

        float gomito = WeaponHolder.localPosition.y * WeaponHolder.lossyScale.y;

        float shoulderToMouse = (mouseWorldPosition - armTransform.position).magnitude;

        float offsetAngle = Mathf.Asin(Mathf.Clamp(gomito / shoulderToMouse, -1f, 1f)) * Mathf.Rad2Deg;

        float gomitoToMouse = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - offsetAngle;

        Debug.DrawRay(armTransform.position, direction * 10, Color.red);
        // Verde: Direzione Reale Sparo (Mano)
        Debug.DrawRay(WeaponHolder.position, WeaponHolder.right * (10) * transform.localScale.x, Color.green);

        Debug.Log(offsetAngle);

        if (transform.localScale.x < 0)
        {
            // Se guardiamo a sinistra, dobbiamo invertire la logica della rotazione
            armTransform.rotation = Quaternion.Euler(0, 0, gomitoToMouse + 180);
        }
        else
        {
            armTransform.rotation = Quaternion.Euler(0, 0, gomitoToMouse);
        }






    }
}