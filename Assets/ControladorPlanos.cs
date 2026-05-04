using UnityEngine;

public class ControladorPlanos : MonoBehaviour
{
    [Header("Configuración de Planos")]
    public GameObject[] planos;

    [Tooltip("Cambia este número para activar el plano correspondiente (0, 1, 2...)")]
    public int idPlanoObjetivo = -1; 

    // Guardamos el ID que estaba activo en el frame anterior para detectar cambios
    private int idPlanoUltimoFrame = -1;

    void Update()
    {
        // Solo actuamos si el usuario (o el socket) cambió el número
        if (idPlanoObjetivo != idPlanoUltimoFrame)
        {
            ActualizarEstadoPlanos(idPlanoObjetivo);
            idPlanoUltimoFrame = idPlanoObjetivo;
        }
    }

    private void ActualizarEstadoPlanos(int nuevoID)
    {
        // 1. CERRAR el plano que estaba abierto antes
        if (idPlanoUltimoFrame >= 0 && idPlanoUltimoFrame < planos.Length)
        {
            EjecutarAnimacion(planos[idPlanoUltimoFrame], "ClosePlano");
        }

        // 2. ABRIR el nuevo plano si el ID es válido
        if (nuevoID >= 0 && nuevoID < planos.Length)
        {
            EjecutarAnimacion(planos[nuevoID], "OpenPlano");
        }
    }

    private void EjecutarAnimacion(GameObject objeto, string nombreAnim)
    {
        if (objeto == null) return;

        Animator anim = objeto.GetComponent<Animator>();
        if (anim != null)
        {
            // ResetTrigger y SetTrigger o Play según prefieras
            // anim.Play es más directo para estados sin transiciones
            anim.Play(nombreAnim);
        }
        else
        {
            Debug.LogWarning($"El objeto {objeto.name} no tiene Animator.");
        }
    }
}