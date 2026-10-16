using UnityEngine;

/// <summary>
/// Helper para el tramo demo de techo del Modo Arana.
/// Ponlo en un GameObject vacio, pulsa "Configurar como techo" en el
/// menu contextual y coloca el tramo 6-8u sobre una zona recta de Level1.
/// Reusa la capa Floor existente: no hace falta crear layers nuevos.
/// Requiere un Collider2D (lo crea si falta) + SurfaceEffector2D.
/// </summary>
[RequireComponent(typeof(SurfaceEffector2D))]
public class SpiderCeilingTrack : MonoBehaviour
{
    [Tooltip("Velocidad del SurfaceEffector del techo. Prueba con el mismo valor de la pista de suelo (10). Si empuja al reves, marca NegateSpeed.")]
    [SerializeField] private float ceilingSpeed = 10f;
    [Tooltip("Si al rodar boca abajo el jugador frena en vez de avanzar, activalo.")]
    [SerializeField] private bool negateSpeed;
    [Tooltip("Longitud del tramo demo si se usa BoxCollider2D.")]
    [SerializeField] private Vector2 trackSize = new Vector2(20f, 0.5f);

    void Start()
    {
        // Aplica el signo al entrar en Play para que negateSpeed mande
        // aunque el speed serializado del SurfaceEffector quedara en +10.
        ApplySpeed();
    }

    void OnValidate()
    {
        ApplySpeed();
    }

    /// <summary>
    /// Un clic: capa Floor + BoxCollider2D solido + Effector configurado.
    /// </summary>
    [ContextMenu("Configurar como techo")]
    public void ConfigureAsCeiling()
    {
        int floor = LayerMask.NameToLayer("Floor");
        if (floor >= 0)
            gameObject.layer = floor;

        var box = GetComponent<BoxCollider2D>();
        if (box == null)
            box = gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = false;
        box.usedByEffector = true;
        box.size = trackSize;
        box.offset = Vector2.zero;

        var eff = GetComponent<SurfaceEffector2D>();
        if (eff == null)
            eff = gameObject.AddComponent<SurfaceEffector2D>();
        eff.useFriction = true;
        eff.useBounce = true;
        eff.forceScale = 0.1f;
        ApplySpeed();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(box);
        UnityEditor.EditorUtility.SetDirty(eff);
#endif
    }

    private void ApplySpeed()
    {
        var eff = GetComponent<SurfaceEffector2D>();
        if (eff == null) return;
        eff.speed = negateSpeed ? -Mathf.Abs(ceilingSpeed) : Mathf.Abs(ceilingSpeed);
    }
}
