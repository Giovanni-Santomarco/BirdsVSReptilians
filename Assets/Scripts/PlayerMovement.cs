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
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Camera.main.nearClipPlane;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        //dato che sto usando cineMachine con z = - 10 devo mettere sullo stesso piano gli assi z
        mouseWorldPosition.z = armTransform.position.z;

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

        //distanza dalla spalla alla y del muzzle dell'arma
        Transform muzzle = null;
        for(int i = 0; i < WeaponHolder.childCount; i++)
        {
            Transform arma = WeaponHolder.GetChild(i);
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
            y = WeaponHolder.localPosition.y * WeaponHolder.lossyScale.y + muzzle.localPosition.y * muzzle.lossyScale.y;
        }
        else
        {
            y = WeaponHolder.localPosition.y * WeaponHolder.lossyScale.y;
        }

        float diagonale = (mouseWorldPosition - armTransform.position).magnitude;

        //distanza da gomito al mouse
        float x = Mathf.Sqrt(Mathf.Max(0, (diagonale * diagonale) - (y * y)));

        float offsetAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        float gomitoToMouse;

        Debug.DrawRay(armTransform.position, direction * 10, Color.red);
        // Verde: Direzione Reale Sparo (Mano)
        Debug.DrawRay(WeaponHolder.position, WeaponHolder.right * (10) * transform.localScale.x, Color.green);

        Debug.Log(x);

        if (transform.localScale.x < 0)
        {
            gomitoToMouse = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - offsetAngle*(-1); 
            // Se guardiamo a sinistra, dobbiamo invertire la logica della rotazione
            armTransform.rotation = Quaternion.Euler(0, 0, gomitoToMouse + 180);
        }
        else
        {
            gomitoToMouse = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - offsetAngle;
            armTransform.rotation = Quaternion.Euler(0, 0, gomitoToMouse);
        }

    }
}