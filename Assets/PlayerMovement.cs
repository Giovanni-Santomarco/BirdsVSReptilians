using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Impostazioni")]
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private Animator animator; // 1. Riferimento all'Animator
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Cerchiamo l'Animator sul Player
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        rb.linearVelocity = moveInput * speed;

        // 2. Logica Animazione
        // Se la "lunghezza" del vettore movimento è maggiore di 0, ci stiamo muovendo
        bool isMoving = moveInput.magnitude > 0;

        // Diciamo all'Animator di cambiare stato
        animator.SetBool("isWalking", isMoving);

        // 3. Girare la tartaruga (Opzionale ma carino)
        // Se vado a destra (x > 0), scala normale. Se vado a sinistra (x < 0), specchia.
        if (moveInput.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // --- I TUOI INPUT (Quelli che abbiamo fatto prima) ---
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // ... qui sotto lascia pure gli altri tuoi metodi (OnFire, OnInteract ecc.) ...
}