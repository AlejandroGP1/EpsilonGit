using UnityEngine;

public class PunteroMenu : MonoBehaviour
{
    [Header("Configuración de Objetivo")]
    public Transform objetivo;
    public float velocidadRotacion = 10f;
    
    [Header("Ajustes de Reposo")]
    // Definimos el punto exacto que mencionas
    public Vector2 puntoReposo = new Vector2(-8f, 6.5f);
    
    [Tooltip("Distancia máxima al punto de reposo para que la flecha se ponga en Z=0")]
    public float umbralDistancia = 0.5f; 

    void Update()
    {
        if (objetivo == null) return;

        Quaternion rotacionObjetivo;

        // 1. Calculamos la distancia actual entre el objetivo y nuestro punto de reposo (-8, 6.5)
        float distanciaAlPunto = Vector2.Distance(objetivo.position, puntoReposo);

        // 2. Comprobar si está dentro del umbral
        if (distanciaAlPunto <= umbralDistancia)
        {
            // Si está cerca de (-8, 6.5), la flecha mira hacia arriba (Z = 0)
            rotacionObjetivo = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            // 3. Cálculo para seguir al objeto si está fuera del umbral
            Vector3 direccion = objetivo.position - transform.position;
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            
            // Ajuste de -90 porque la flecha apunta hacia arriba por defecto
            float anguloFinal = angulo - 90f;
            rotacionObjetivo = Quaternion.Euler(0, 0, anguloFinal);
        }

        // 4. Aplicar la rotación de forma suave o instantánea
        if (velocidadRotacion > 0)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);
        }
        else
        {
            transform.rotation = rotacionObjetivo;
        }
    }

    // Dibujamos una pequeña esfera en el editor para que veas dónde está el punto de reposo y su umbral
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(puntoReposo.x, puntoReposo.y, 0), umbralDistancia);
    }
}