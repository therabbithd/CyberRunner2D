using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Modo Arana estilo Geometry Dash para CyberRunner2D (Legacy 2D Physics).
/// Pulsa la accion "Spider" (W / UpArrow por defecto; Espacio es salto)
/// para invertir rb.gravityScale y salir disparado hacia el techo. Vuelve
/// a pulsar para caer al suelo. Voltea solo los visuales (eje Y) para no romper
/// los colliders del root fisico que rota libremente con torque.
/// Requiere: Rigidbody2D Dynamic en el mismo GameObject.
/// Deteccion: OverlapCircle en groundCheck / ceilingCheck contra la capa Floor.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class SpiderGravityController : MonoBehaviour
{
    [Header("Gravedad")]
    [Tooltip("Valor positivo base. Debe coincidir con el gravityScale del Rigidbody2D en escena (1.5 en Level1).")]
    [SerializeField] private float normalGravity = 1.5f;
    [Tooltip("Impulso vertical aplicado al invertir, para pegar al jugador al techo/suelo estilo GD. Alto para vuelo casi-teletransporte.")]
    [SerializeField] private float snapImpulse = 38f;
    [Tooltip("Tiempo minimo entre inversiones para evitar spam.")]
    [SerializeField] private float toggleCooldown = 0.2f;
    [Tooltip("Frena el giro al pegar el cambio para aterrizar estable sin perder los flips por completo.")]
    [SerializeField] private float angularDampOnToggle = 0.5f;
    [Tooltip("Velocidad vertical maxima tras el impulso. Alta para vuelo casi-teletransporte; el clamp + Continuous evitan tunneling (<1u por paso fisico).")]
    [SerializeField] private float maxSnapSpeed = 48f;
    [Tooltip("Tiempo de inmunidad del CrashDetector tras invertir. Cubre el vuelo rapido hasta la otra superficie + el impacto de aterrizaje.")]
    [SerializeField] private float crashGraceTime = 1.2f;
    [Tooltip("Distancia mundo desde el centro a los puntos de contacto. La tabla esta en local Y=-1.57; 1.75 deja el circulo del check solapando la superficie al apoyarse (con 1.85 el check quedaba justo por debajo y grounded era casi siempre false).")]
    [SerializeField] private float checkDistance = 1.75f;

    [Header("Muerte al caer hacia arriba")]
    [Tooltip("Con la gravedad invertida el jugador 'cae' hacia arriba. Si no hay techo que lo pare y se sale por arriba del nivel, muere (antes salia volando infinitamente).")]
    [SerializeField] private bool killWhenEscapingTop = true;
    [Tooltip("Margen por encima del punto mas alto del nivel (capa Floor) antes de morir. Evita matar durante el vuelo normal al techo.")]
    [SerializeField] private float topEscapeMargin = 6f;

    [Header("Deteccion suelo / techo (OverlapCircle)")]
    [Tooltip("Vacio hijo bajo la tabla. Si esta vacio se autocrea en (0,-1.85,0): justo bajo la CapsuleCollider2D (offset -1.28, tamano Y 0.58 -> base en -1.57). El valor antiguo -0.7 quedaba DENTRO de la tabla y grounded casi nunca era true.")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("Vacio hijo sobre la tabla. Si esta vacio se autocrea en (0,+1.85,0), espejo del suelo para el techo.")]
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private float checkRadius = 0.35f;
    [Tooltip("Reusamos la capa Floor existente tanto para suelo como para techo.")]
    [SerializeField] private LayerMask groundMask;

    [Header("Visuales (volteo Y)")]
    [Tooltip("Hijo que contiene los sprites del personaje + tabla. Solo se voltea este, NUNCA el root fisico.")]
    [SerializeField] private Transform visualsRoot;
    [Tooltip("Si es true y no hay visualsRoot asignado, se busca automaticamente el primer hijo con SpriteRenderer.")]
    [SerializeField] private bool autoFindVisuals = true;

    [Header("Input")]
    [Tooltip("Nombre de la accion en InputSystem_Actions (mapa Player).")]
    [SerializeField] private string spiderActionName = "Spider";
    [Tooltip("Fallback por si la accion no existe: tambien escucha W / UpArrow con el nuevo Input System. (Espacio queda reservado al salto.)")]
    [SerializeField] private bool useFallbackKeys = true;

    // Estado publico (solo lectura) para HUD, audio o tests.
    public bool IsInverted { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsCeilinged { get; private set; }

    /// <summary>
    /// True durante crashGraceTime tras cada inversion. CrashDetector debe ignorar
    /// los triggers de Floor en esta ventana (es el vuelo hacia la otra superficie).
    /// </summary>
    public bool IsCrashImmune => Time.time - lastToggleTime < crashGraceTime;

    public event System.Action<bool> OnGravitySwitched;

    private Rigidbody2D rb;
    private PlayerController player;
    private CrashDetector crashDetector; // Muerte unificada (particulas + recarga)
    private Collider2D boardCollider; // tabla (solida, rueda en suelo/techo)
    private CircleCollider2D headCollider; // cabeza (trigger que mata en CrashDetector)
    private Vector2 baseBoardOffset;
    private Vector2 baseHeadOffset;
    private InputAction spiderAction;
    private float lastToggleTime = -10f;
    private float baseVisualScaleY = 1f;
    private float baseSnapImpulse;
    private float baseMaxSnapSpeed;
    private float topEscapeY = float.PositiveInfinity; // Punto mas alto del nivel + margen
    private bool escapedTop;
    private bool suppressFlipCountOnce;
    private readonly System.Collections.Generic.List<ContactPoint2D> contactBuffer = new System.Collections.Generic.List<ContactPoint2D>(8);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerController>();
        crashDetector = GetComponent<CrashDetector>();
        // Anti-tunneling: el impulso del snap (12) con Discrete atraviesa el
        // techo de 0.5. Forzar Continuous aunque la escena diga Discrete.
        if (rb != null && rb.collisionDetectionMode == CollisionDetectionMode2D.Discrete)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        // Guarda los offsets base para el espejo vertical (solo se toca Y,
        // X se conserva para no cambiar el sentido de marcha).
        boardCollider = GetComponent<BoxCollider2D>();
        if (boardCollider == null) boardCollider = GetComponent<CapsuleCollider2D>();
        headCollider = GetComponent<CircleCollider2D>();
        if (boardCollider != null)
            baseBoardOffset = boardCollider.offset;
        if (headCollider != null)
            baseHeadOffset = headCollider.offset;
        EnsureChecksExist();
        if (autoFindVisuals && visualsRoot == null)
            visualsRoot = FindVisualsChild();
        if (visualsRoot != null)
            baseVisualScaleY = Mathf.Abs(visualsRoot.localScale.y) < 0.001f ? 1f : visualsRoot.localScale.y;
        // Guards para revertir el power-up "Spider" (velocidad de subida).
        baseSnapImpulse = snapImpulse;
        baseMaxSnapSpeed = maxSnapSpeed;
    }

    void Start()
    {
        // Respeta el valor de escena si difiere del serializado.
        if (rb != null && rb.gravityScale > 0f && !Mathf.Approximately(rb.gravityScale, normalGravity))
            normalGravity = rb.gravityScale;
        ApplyGravityScale();
        ApplyVisualFlip();
        ApplyColliderMirror();
        ComputeTopEscapeLimit();
    }

    void OnEnable()
    {
        TrySubscribeInputAction();
    }

    void OnDisable()
    {
        UnsubscribeInputAction();
    }

    void Update()
    {
        if (!useFallbackKeys) return;
        // Si la accion "Spider" existe, ella ya gestiona el input (OnSpiderPerformed).
        // Este fallback solo usa el NUEVO Input System para no lanzar InvalidOperationException
        // cuando el proyecto tiene Active Input Handling = Input System Package.
        if (spiderAction != null) return;

        var kb = Keyboard.current;
        if (kb != null && (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame))
            TryToggle();
    }

    void FixedUpdate()
    {
        UpdateContactFlags();
        CheckTopEscape();
        // El techo esta a ~32u y es un Edge sin grosor: sin tope, la gravedad
        // acelera el vuelo hasta ~33 u/s y atraviesa el Edge aunque sea
        // Continuous (0.66u por frame). Clampear en vuelo lo deja en 0.28u.
        if (IsInverted && !IsCeilinged && rb != null)
        {
            Vector2 v = rb.linearVelocity;
            if (Mathf.Abs(v.y) > maxSnapSpeed)
            {
                v.y = Mathf.Sign(v.y) * maxSnapSpeed;
                rb.linearVelocity = v;
            }
        }
        else if (!IsInverted && !IsGrounded && rb != null)
        {
            Vector2 v2 = rb.linearVelocity;
            if (Mathf.Abs(v2.y) > maxSnapSpeed)
            {
                v2.y = Mathf.Sign(v2.y) * maxSnapSpeed;
                rb.linearVelocity = v2;
            }
        }
    }

    // ------------------------------------------------------------------

    private void TrySubscribeInputAction()
    {
        if (InputSystem.actions == null) return;
        InputAction action = InputSystem.actions.FindAction(spiderActionName, false);
        if (action == null) return; // fallback por teclas cubre este caso
        spiderAction = action;
        spiderAction.performed += OnSpiderPerformed;
        spiderAction.Enable();
    }

    private void UnsubscribeInputAction()
    {
        if (spiderAction != null)
        {
            spiderAction.performed -= OnSpiderPerformed;
            spiderAction = null;
        }
    }

    private void OnSpiderPerformed(InputAction.CallbackContext ctx)
    {
        // Solo el flanco de pulsacion dispara el cambio (el asset usa Button sin Hold).
        TryToggle();
    }

    /// <summary>
    /// Punto de entrada del input. Solo permite invertir si hay contacto
    /// con suelo O con techo (estilo GD: sin vuelo infinito) y respeta cooldown.
    /// Llamable tambien desde UI tactil: spiderController.TryToggle().
    /// </summary>
    public void TryToggle()
    {
        if (!enabled || rb == null) return;
        if (player != null && !player.CanControlPlayer1) return;
        if (Time.time - lastToggleTime < toggleCooldown) return;
        UpdateContactFlags();
        if (!IsGrounded && !IsCeilinged) return;
        ToggleGravity();
    }

    /// <summary>
    /// Inversion inmediata: cambia gravityScale, ESPEJA los colliders en
    /// vertical (offset Y negado, X intacta) para que la tabla mire a la
    /// nueva superficie sin cambiar el sentido de marcha, anula velocidad Y,
    /// aplica impulso limitado hacia la nueva "abajo" y voltea visuales +
    /// torque. NO se rota rb.rotation: un giro de 180 en el plano voltea
    /// tambien la X y el personaje queda mirando atras (giro horizontal).
    /// Sin el espejo, la cabeza (trigger que mata) llegaba primera al techo
    /// y el jugador moria atravesandolo.
    /// </summary>
    public void ToggleGravity()
    {
        lastToggleTime = Time.time;
        IsInverted = !IsInverted;

        ApplyGravityScale();

        // Espejo vertical de colliders: la tabla (local Y-) pasa arriba y la
        // cabeza (local Y+) pasa abajo, pero ambas conservan su X (el frente
        // sigue siendo el frente). Igual que el flip de los visuales (scale.y).
        ApplyColliderMirror();

        // Viaje rapido y vertical hacia la otra superficie, con tope anti-tunneling.
        Vector2 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;
        float dir = IsInverted ? 1f : -1f; // invertido => subir (+Y)
        rb.AddForce(new Vector2(0f, dir * snapImpulse), ForceMode2D.Impulse);
        v = rb.linearVelocity;
        if (Mathf.Abs(v.y) > maxSnapSpeed)
        {
            v.y = Mathf.Sign(v.y) * maxSnapSpeed;
            rb.linearVelocity = v;
        }

        // Estabiliza sin congelar: conserva parte del giro para no matar los flips.
        rb.angularVelocity *= angularDampOnToggle;

        // El torque de PlayerController debe espejarse o los controles se sienten al reves.
        if (player != null)
            player.TorqueDirection = IsInverted ? -1 : 1;

        ApplyVisualFlip();
        suppressFlipCountOnce = true;
        OnGravitySwitched?.Invoke(IsInverted);
    }

    private void ApplyGravityScale()
    {
        if (rb == null) return;
        rb.gravityScale = IsInverted ? -Mathf.Abs(normalGravity) : Mathf.Abs(normalGravity);
        // gravityScale negativo con sleep puede dejar el cuerpo dormido: despertarlo.
        rb.WakeUp();
    }

    private void ApplyVisualFlip()
    {
        if (visualsRoot == null) return;
        Vector3 s = visualsRoot.localScale;
        float mag = Mathf.Abs(baseVisualScaleY) < 0.001f ? Mathf.Abs(s.y) : Mathf.Abs(baseVisualScaleY);
        s.y = IsInverted ? -mag : mag;
        visualsRoot.localScale = s;
    }

    /// <summary>
    /// Espeja tabla y cabeza solo en Y segun IsInverted, partiendo siempre
    /// de los offsets base (no se acumula). La capsula de la tabla es
    /// simetrica, asi que basta con mover offsets; el giro libre por torque
    /// sigue funcionando igual porque todo es espacio local.
    /// </summary>
    private void ApplyColliderMirror()
    {
        float sign = IsInverted ? -1f : 1f;
        if (boardCollider != null)
            boardCollider.offset = new Vector2(baseBoardOffset.x, baseBoardOffset.y * sign);
        if (headCollider != null)
            headCollider.offset = new Vector2(baseHeadOffset.x, baseHeadOffset.y * sign);
    }

    private void UpdateContactFlags()
    {
        // Puntos en ESPACIO MUNDO (arriba/abajo fijos), no en hijos que rotan
        // con el torque del jugador. Con hijos rotantes, al girar 90 grados
        // los checks quedaban a los lados y grounded/ceilinged fallaban.
        // Se reposicionan los Transforms existentes para no romper Gizmos.
        Vector2 center = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 groundPoint = center + Vector2.down * checkDistance;
        Vector2 ceilingPoint = center + Vector2.up * checkDistance;
        if (groundCheck != null)
            groundCheck.position = groundPoint;
        if (ceilingCheck != null)
            ceilingCheck.position = ceilingPoint;
        bool grounded = Physics2D.OverlapCircle(groundPoint, checkRadius, groundMask) != null;
        bool ceiled = Physics2D.OverlapCircle(ceilingPoint, checkRadius, groundMask) != null;

        // Refuerzo con los contactos reales del Rigidbody: el punto fijo a
        // checkDistance del centro falla cuando la tabla va inclinada (flips,
        // rampas) o el jugador se apoya en una roca (capa distinta a Floor),
        // dejando el modo arana muerto en Level1. Un contacto con normal hacia
        // arriba = superficie debajo (suelo); hacia abajo = superficie arriba
        // (techo). Es independiente del giro del root y de la capa del suelo.
        if (rb != null)
        {
            contactBuffer.Clear();
            rb.GetContacts(contactBuffer);
            for (int i = 0; i < contactBuffer.Count; i++)
            {
                Collider2D other = contactBuffer[i].collider;
                if (other == null || other.isTrigger || other.attachedRigidbody == rb) continue;
                float ny = contactBuffer[i].normal.y;
                if (ny >= 0.5f) grounded = true;
                else if (ny <= -0.5f) ceiled = true;
            }
        }

        IsGrounded = grounded;
        IsCeilinged = ceiled;
    }

    /// <summary>
    /// Calcula el techo real del nivel: el punto mas alto entre los colliders
    /// solidos de la capa Floor (suelo, techo y rocas son la misma capa).
    /// Se usa para matar al jugador si se sale por arriba al caer hacia arriba.
    /// Se ignora el jugador (tiene Rigidbody2D) y los triggers (zonas de muerte).
    /// </summary>
    private void ComputeTopEscapeLimit()
    {
        if (!killWhenEscapingTop) return;

        int floor = LayerMask.NameToLayer("Floor");
        if (floor < 0) return;

        float maxY = float.NegativeInfinity;
        var colliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        foreach (var col in colliders)
        {
            if (col == null || col.isTrigger) continue;
            if (col.gameObject.layer != floor) continue;
            if (col.attachedRigidbody != null) continue; // superficie estatica
            if (col.bounds.max.y > maxY) maxY = col.bounds.max.y;
        }

        if (maxY > float.NegativeInfinity)
            topEscapeY = maxY + topEscapeMargin;
    }

    /// <summary>
    /// Mata al jugador cuando, con la gravedad invertida (cae hacia arriba),
    /// supera el techo del nivel sin una superficie que lo pare. Evita el vuelo
    /// infinito hacia arriba al salir de la zona con techo.
    /// </summary>
    private void CheckTopEscape()
    {
        if (!killWhenEscapingTop || escapedTop || rb == null) return;
        if (!IsInverted) return; // Solo "cae hacia arriba" con la gravedad invertida
        if (rb.position.y <= topEscapeY) return;

        escapedTop = true;
        Debug.Log("Player escaped the top (fell upwards)!");
        if (crashDetector != null)
            crashDetector.Kill();
        else if (player != null)
            player.CanControlPlayer1 = false;
    }

    private void EnsureChecksExist()
    {
        // La tabla (CapsuleCollider2D) tiene offset Y -1.28 y tamano Y 0.58:
        // su base esta en local Y = -1.57. Los checks van justo fuera (+ radio).
        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -1.85f, 0f);
            groundCheck = go.transform;
        }
        if (ceilingCheck == null)
        {
            GameObject go = new GameObject("CeilingCheck");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.85f, 0f);
            ceilingCheck = go.transform;
        }
        // Si la mascara viene vacia (valor por defecto del inspector), apunta a Floor.
        if (groundMask.value == 0)
            groundMask = LayerMask.GetMask("Floor");
    }

    private Transform FindVisualsChild()
    {
        // Primer hijo con SpriteRenderer (Dino/Frog). No toca colliders del root.
        foreach (Transform child in transform)
        {
            if (child == groundCheck || child == ceilingCheck) continue;
            if (child.GetComponentInChildren<SpriteRenderer>(true) != null)
                return child;
        }
        return null;
    }

    /// <summary>
    /// PlayerController.CalculateFlips() puede llamar esto para ignorar
    /// el salto de 180 grados del snap y no regalar puntos.
    /// Devuelve true una sola vez tras cada toggle.
    /// </summary>
    public bool ConsumeSuppressFlipCount()
    {
        if (!suppressFlipCountOnce) return false;
        suppressFlipCountOnce = false;
        return true;
    }

    /// <summary>
    /// Power-up "Spider": acelera el impulso de subida al invertir la gravedad.
    /// Sube snapImpulse y tambien maxSnapSpeed, porque el clamp de vuelo
    /// recortaria el impulso si solo creciera el primero.
    /// </summary>
    public void AddSnapSpeed(float amount)
    {
        snapImpulse += amount;
        maxSnapSpeed += amount;
    }

    /// <summary>
    /// Revierte AddSnapSpeed sin bajar de los valores base (soporta varios
    /// recogibles activos a la vez sin dejar el impulso por debajo del original).
    /// </summary>
    public void RemoveSnapSpeed(float amount)
    {
        snapImpulse = Mathf.Max(baseSnapImpulse, snapImpulse - amount);
        maxSnapSpeed = Mathf.Max(baseMaxSnapSpeed, maxSnapSpeed - amount);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.yellow;
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        Gizmos.color = IsCeilinged ? Color.cyan : Color.red;
        if (ceilingCheck != null)
            Gizmos.DrawWireSphere(ceilingCheck.position, checkRadius);
    }
}
