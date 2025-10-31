using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Vector3 playerPosition = new Vector3(0, -2.5f, 20); // starts slightly forward
    public float laneOffset = 2f;
    public int currentLane = 1; // 0 = left, 1 = middle, 2 = right
    public float jumpHeight = 3f;
    public float jumpSpeed = 4f;
    public bool isJumping = false;
    public float gravity = -9.8f;
    private float verticalVelocity = 0f;

    private bool isShaking = false;
    private float shakeTimer = 0f;
    private float shakeDuration = 0.2f;
    private float shakeMagnitude = 0.05f;
    private Vector3 originalLocalPos;

    private Quaternion targetRotation;
    private float rotationSpeed = 10f;


    private float groundY; // ← new variable for dynamic ground level

    [Header("Health")]
    public float maxHP = 100f;
    public float currentHP = 100f;
    public float regenRate = 1f;
    public Slider hpBar;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        playerPosition.z = 10f;
        groundY = playerPosition.y;
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleHealth();
        UpdatePerspective();
        CheckCollisions();
    }

    void HandleMovement()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0)
        {
            currentLane--;
            targetRotation = Quaternion.Euler(0, 0, 15f); // tilt left
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 2)
        {
            currentLane++;
            targetRotation = Quaternion.Euler(0, 0, -15f); // tilt right
        }

        float targetX = (currentLane - 1) * laneOffset;
        playerPosition.x = Mathf.Lerp(playerPosition.x, targetX, Time.deltaTime * 10f);

        // smooth tilt back toward upright
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        // when close to the target rotation, gradually reset upright
        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            targetRotation = Quaternion.identity;
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            verticalVelocity = Mathf.Sqrt(2 * jumpHeight * -gravity);
        }

        if (isJumping)
        {
            playerPosition.y += verticalVelocity * Time.deltaTime;
            verticalVelocity += gravity * Time.deltaTime;

            // ✅ use groundY instead of hardcoded -2.5f
            if (playerPosition.y <= groundY)
            {
                playerPosition.y = groundY;
                isJumping = false;
            }
        }
    }

    void HandleHealth()
    {
        currentHP += regenRate * Time.deltaTime;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        if (hpBar != null)
            hpBar.value = currentHP / maxHP;
    }

    void UpdatePerspective()
    {
        float perspective = CameraComponent.focalLenth / (CameraComponent.focalLenth + playerPosition.z);
        float adjustedY = playerPosition.y - 0.3f;

        transform.localScale = Vector3.one * perspective * 2f;
        transform.position = new Vector2(playerPosition.x, adjustedY) * perspective;
    }

    void CheckCollisions()
    {
        foreach (Obstacle obstacle in FindObjectsOfType<Obstacle>())
        {
            float zDistance = Mathf.Abs(obstacle.itemPosition.z - playerPosition.z);
            float xDistance = Mathf.Abs(obstacle.itemPosition.x - playerPosition.x);
            float yDistance = Mathf.Abs(obstacle.itemPosition.y - playerPosition.y);

            bool inRange = (zDistance < 2f && xDistance < 1f && yDistance < 1f);

            if (inRange && !isJumping)
            {
                // ✅ Only damage once per obstacle
                if (!obstacle.hasDamagedPlayer)
                {
                    TakeDamage(10f);
                    obstacle.hasDamagedPlayer = true;
                }
            }
        }
    }

    void TakeDamage(float dmg)
    {
        currentHP -= dmg;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (currentHP <= 0)
        {
            Debug.Log("Player Died!");
        }
    }
}
