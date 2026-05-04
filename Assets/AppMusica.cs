using UnityEngine;

public class AppMusica : MonoBehaviour, IAppControlable
{
    // Debe coincidir con el nombre del sprite en el inspector (ej. "musica")
    public string NombreApp => "musica"; 

    [Header("Referencias")]
    public BuscadorMusica buscadorMusica; // Arrastra aquí el objeto que tiene el script BuscadorMusica

    public bool SoportaGesto(string gesto) {
        // Definimos qué gestos activarán la carga del círculo
        return gesto == "Palma Abierta" || 
               gesto == "Indice a la Izquierda" || 
               gesto == "Indice a la Derecha";
    }

    public void EjecutarAccion(string gesto) {
        if (buscadorMusica == null) {
            Debug.LogWarning("AppMusica: No se ha asignado la referencia a BuscadorMusica.");
            return;
        }

        Debug.Log($"<color=blue>AppMusica:</color> Ejecutando acción para {gesto}");

        switch (gesto)
        {
            case "Palma Abierta":
                buscadorMusica.PlayPauseFisico();
                break;

            case "Indice a la Izquierda":
                buscadorMusica.AnteriorCancion();
                break;

            case "Indice a la Derecha":
                buscadorMusica.SiguienteCancion();
                break;
        }
    }

    public void AlEntrar() {
        Debug.Log("<color=blue>AppMusica:</color> Modo música activado.");
        // Aquí podrías añadir un mensaje en pantalla o sonido de bienvenida
    }

    public void AlSalir() {
        Debug.Log("<color=blue>AppMusica:</color> Saliendo del modo música.");
        // Opcional: podrías pausar la música al salir si quisieras:
        // buscadorMusica.audioSource.Pause();
    }
}