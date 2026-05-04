using UnityEngine;
using System.Collections;
using System.Diagnostics; // Necesario para Process.Start

public class AbreNavegadorConMenu : MonoBehaviour
{
    [Header("Configuración de Apertura")]
    public string urlParaAbrir = "https://www.google.com";
    public bool esDirectorio = false;
    public float retraso = 1.0f;

    [Header("Referencias de Animación")]
    public Animator animadorAro; // Aquí arrastras el objeto "AroPosibilidades"

    private void OnEnable()
    {
        StopAllCoroutines();
        
        // Al activar la App, abrimos el menú visual
        AbrirMiniMenu(); 
    }

    private void OnDisable()
    {
        // Al desactivar la App (por ejemplo, al volver al menú principal), 
        // nos aseguramos de que el menú visual se cierre.
        CerrarMiniMenu();
    }

    /// <summary>
    /// Cambia el booleano 'AbrirMiniMenu' a true.
    /// </summary>
    public void AbrirMiniMenu()
    {
        if (animadorAro != null)
        {
            animadorAro.SetBool("AbrirMiniMenu", true);
            UnityEngine.Debug.Log("<color=orange>Animación:</color> Menú Abierto.");
        }
    }

    /// <summary>
    /// Cambia el booleano 'AbrirMiniMenu' a false.
    /// </summary>
    public void CerrarMiniMenu()
    {
        if (animadorAro != null)
        {
            animadorAro.SetBool("AbrirMiniMenu", false);
            UnityEngine.Debug.Log("<color=red>Animación:</color> Menú Cerrado.");
        }
    }

    IEnumerator EsperarYAbrir()
    {
        yield return new WaitForSeconds(retraso);

        if (string.IsNullOrEmpty(urlParaAbrir))
        {
            UnityEngine.Debug.LogWarning("La ruta o URL está vacía.");
            yield break;
        }

        if (esDirectorio)
        {
            try
            {
                Process.Start(new ProcessStartInfo(urlParaAbrir) { UseShellExecute = true });
                UnityEngine.Debug.Log("<color=yellow>Sistema:</color> Ejecutando ruta local: " + urlParaAbrir);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Error al abrir local: " + e.Message);
            }
        }
        else
        {
            try 
            {
                Application.OpenURL(urlParaAbrir);
                UnityEngine.Debug.Log("<color=cyan>Navegador:</color> Abriendo URL: " + urlParaAbrir);
            }
            catch 
            {
                Process.Start(new ProcessStartInfo(urlParaAbrir) { UseShellExecute = true });
            }
        }
    }
}