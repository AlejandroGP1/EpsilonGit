using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshPro

public class ControladorAudioGlobal : MonoBehaviour
{
    [Header("UI")]
    public Image imagenRadial;
    public TextMeshProUGUI textoPorcentaje; // Referencia al texto de UI

    [Header("Rango de Fill Image")]
    [Range(0f, 1f)] public float fillInicio = 0.636604f;
    [Range(0f, 1f)] public float fillFin = 0.859f;

    [Header("Estado Actual")]
    [Range(0f, 100f)] public float porcentaje = 0f;

    private const string KeyVolumen = "VolumenGuardado";

    void Start()
    {
        // 1. Cargar el valor guardado. Si no existe, usa el volumen actual del sistema
        float volumenGuardado = PlayerPrefs.GetFloat(KeyVolumen, AudioListener.volume * 100f);
        porcentaje = volumenGuardado;

        // 2. Aplicar inmediatamente al iniciar
        ActualizarTodo();
    }

    void Update()
    {
        // En el Update solo llamamos si necesitas control manual por Inspector, 
        // pero para optimizar podrías mover esto a una función que solo se llame al cambiar el valor.
        ActualizarTodo();
    }

    void ActualizarTodo()
    {
        AplicarFill();
        AplicarVolumen();
        ActualizarTexto();
    }

    void AplicarFill()
    {
        float t = Mathf.Clamp01(porcentaje / 100f);
        float fill = Mathf.Lerp(fillInicio, fillFin, t);

        if (imagenRadial != null)
        {
            imagenRadial.fillAmount = fill;
        }
    }

    void AplicarVolumen()
    {
        float nivelCeroUno = porcentaje / 100f;
        AudioListener.volume = nivelCeroUno;
    }

    void ActualizarTexto()
    {
        if (textoPorcentaje != null)
        {
            // Convertimos a int para que no tenga decimales y añadimos el símbolo %
            textoPorcentaje.text = Mathf.RoundToInt(porcentaje).ToString() + "%";
        }
    }

    // Esta función se asegura de guardar los datos cuando la app se cierra o pierde el foco
    private void OnApplicationQuit()
    {
        GuardarVolumen();
    }

    private void OnDisable()
    {
        GuardarVolumen();
    }

    public void GuardarVolumen()
    {
        PlayerPrefs.SetFloat(KeyVolumen, porcentaje);
        PlayerPrefs.Save();
    }
}