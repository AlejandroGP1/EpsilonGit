using UnityEngine;
using System.Collections;
using System.Diagnostics; // Necesario para Process.Start

public class AbreNavegador : MonoBehaviour
{
    [Header("Configuración")]
    public string urlParaAbrir = "https://www.google.com";
    public bool esDirectorio = false; // El nuevo booleano
    public float retraso = 1.0f;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(EsperarYAbrir());
    }

    // Reemplaza tu función IEnumerator EsperarYAbrir por esta:

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
                // Para archivos locales o carpetas
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
            // PARA WEBS: Intentamos la forma estándar de Unity
            // Si esta falla, usamos la de sistema como respaldo
            try 
            {
                Application.OpenURL(urlParaAbrir);
                UnityEngine.Debug.Log("<color=cyan>Navegador:</color> Abriendo URL: " + urlParaAbrir);
            }
            catch 
            {
                // Respaldo si Application.OpenURL falla
                Process.Start(new ProcessStartInfo(urlParaAbrir) { UseShellExecute = true });
            }
        }
    }
}