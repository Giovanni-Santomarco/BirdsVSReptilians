using UnityEngine;
using UnityEngine.InputSystem;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. MOVIMENTO (I tasti muovono solo la posizione, non la rotazione)
        rb.linearVelocity = moveInput * speed;

        // 2. ANIMAZIONE
        bool isMoving = moveInput.magnitude > 0;
        animator.SetBool("isWalking", isMoving);

        // 3. MIRA E ROTAZIONE DEL CORPO (Tutto gestito dal mouse)
        HandleAimingAndTurning();
    }

    // --- LOGICA INPUT ---
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
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
            transform.localScale = new Vector3(1, 1, 1); // Faccia a Destra
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Faccia a Sinistra
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
}