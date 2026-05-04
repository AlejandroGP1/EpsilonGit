using UnityEngine;
using TMPro;

[System.Serializable]
public class SpriteConfig
{
    public Sprite sprite;
    public float escala = 1f; // escala uniforme
}

public class TocandoIcono : MonoBehaviour
{
    public string nombreSprite;
    public string AppActual;
    public GameObject IconoMinimizado;
    public GameObject LogoMini;

    public SpriteConfig[] listaSprites;

    public TextMeshProUGUI textoUI;

    private SpriteRenderer iconoRenderer;

    void Start()
    {
        if (IconoMinimizado != null)
        {
            iconoRenderer = IconoMinimizado.GetComponent<SpriteRenderer>();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        SpriteRenderer sr = other.GetComponent<SpriteRenderer>();

        if (sr != null && sr.sprite != null)
        {
            nombreSprite = sr.sprite.name;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        nombreSprite = "";
    }

    public void NuevoSpriteApp()
    {
        Debug.Log("flechitaV2_0 - Actualizando Icono segun la app abierta...");
        StartCoroutine(CambiarSpriteConDelay());
    }

    private System.Collections.IEnumerator CambiarSpriteConDelay()
{
    yield return new WaitForSeconds(2.642f);

    if (iconoRenderer == null) yield break;


    foreach (SpriteConfig config in listaSprites)
    {
        if (config.sprite != null && config.sprite.name == AppActual)
        {
            iconoRenderer.sprite = config.sprite;

            IconoMinimizado.transform.localScale = new Vector3(
                config.escala,
                config.escala,
                1f
            );

            Debug.Log("Sprite cambiado a: " + config.sprite.name + " con escala " + config.escala);
            yield break;
        }
    }

    Debug.LogWarning("No se encontró sprite para: " + AppActual);
}

    public void QuitarApp()
    {
        //Debug.Log("flechitaV2_0 - Quitando sprite del icono...");
        LogoMini.SetActive(true);
        if (iconoRenderer == null) return;

        iconoRenderer.sprite = null;

        // Opcional: resetear escala
        IconoMinimizado.transform.localScale = Vector3.one;
    }

    void Update()
    {
        if (textoUI != null)
        {
            textoUI.text = nombreSprite;
        }
    }
}