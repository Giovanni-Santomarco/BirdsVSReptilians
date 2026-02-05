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

    // --- NUOVA LOGICA UNIFICATA ---
    void HandleAimingAndTurning()
    {
        // A. Troviamo dove è il mouse nel mondo
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Camera.main.nearClipPlane;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // B. GIRIAMO IL CORPO (Flip)
        // Se il mouse è a destra del player (x > player.x) -> Guarda a destra
        // Se il mouse è a sinistra del player (x < player.x) -> Guarda a sinistra
        Vector3 direction = mouseWorldPosition - transform.position;

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // Faccia a Destra
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Faccia a Sinistra
        }

        // C. RUOTIAMO IL BRACCIO
        // Calcoliamo la direzione specifica dal perno del braccio al mouse
        Vector3 armDirection = mouseWorldPosition - armTransform.position;
        float angle = Mathf.Atan2(armDirection.y, armDirection.x) * Mathf.Rad2Deg;

        // Correggiamo l'angolo se siamo specchiati
        if (transform.localScale.x < 0)
        {
            // Se guardiamo a sinistra, dobbiamo invertire la logica della rotazione
            armTransform.rotation = Quaternion.Euler(0, 0, angle + 180);
        }
        else
        {
            armTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}