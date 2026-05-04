using UnityEngine;

public class CercaniaIcon : MonoBehaviour
{
    [Header("Referencias")]
    public Transform puntero;        // Arrastra aquí el objeto que representa la mano/puntero

    [Header("Configuración")]
    public bool Activo = true;        // Solo funciona si está activado
    public float radioDeteccion = 5f; // A qué distancia empieza a reaccionar
    public float escalaMinima = 1f;   // Escala normal
    public float escalaMaxima = 2f;   // Escala cuando el puntero está encima
    public float suavizado = 10f;     // Qué tan fluida es la animación

    private Vector3 escalaOriginal;

    void Start()
    {
        escalaOriginal = transform.localScale;
    }

    void Update()
    {
        // Si no está activo o no hay puntero, volvemos a la escala original
        if (!Activo || puntero == null)
        {
            RegresarAEscalaOriginal();
            return;
        }

        // 1. Calculamos la distancia entre este objeto y el puntero
        float distancia = Vector3.Distance(transform.position, puntero.position);

        // 2. Si el puntero está dentro del rango de sensibilidad
        if (distancia < radioDeteccion)
        {
            // Creamos un factor de 0 a 1 (1 es estar encima, 0 es estar en el borde del radio)
            float factorProximidad = 1f - (distancia / radioDeteccion);
            
            // Calculamos la nueva escala basada en el factor (puedes elevarlo al cuadrado para un efecto más "marcado")
            float nuevaEscalaTamo = Mathf.Lerp(escalaMinima, escalaMaxima, factorProximidad);
            Vector3 escalaObjetivo = escalaOriginal * nuevaEscalaTamo;

            // 3. Aplicamos la escala suavemente
            transform.localScale = Vector3.Lerp(transform.localScale, escalaObjetivo, Time.deltaTime * suavizado);
        }
        else
        {
            RegresarAEscalaOriginal();
        }
    }

    void RegresarAEscalaOriginal()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, escalaOriginal * escalaMinima, Time.deltaTime * suavizado);
    }
}