using UnityEngine;
using TMPro;
using System;
using System.Globalization;
using UnityEngine.UI; // Necesario para forzar el LayoutRebuilder

public class RelojSistema : MonoBehaviour
{
    [Header("Referencias de Texto")]
    public TextMeshProUGUI textoHora;       // Ahora en formato 12h (1:05)
    public TextMeshProUGUI textoDiaNumero;
    public TextMeshProUGUI textoAmPm;
    public TextMeshProUGUI textoDiaNombre;

    private CultureInfo culturaEspanol = new CultureInfo("es-ES");
    private float tiempoSiguienteRefresco = 0f;

    void Update()
    {
        if (Time.time >= tiempoSiguienteRefresco)
        {
            ActualizarReloj();
            tiempoSiguienteRefresco = Time.time + 0.5f; 
        }
    }

    void ActualizarReloj()
    {
        DateTime ahora = DateTime.Now;

        // "h:mm" -> Formato 12 horas (sin el 0 inicial, ej: 1:05)
        // "hh:mm" -> Formato 12 horas (con el 0 inicial, ej: 01:05)
        ActualizarComponenteTexto(textoHora, ahora.ToString("h:mm"));
        
        ActualizarComponenteTexto(textoDiaNumero, ahora.Day.ToString());
        
        // AM/PM siempre en mayúsculas
        ActualizarComponenteTexto(textoAmPm, ahora.ToString("tt", CultureInfo.InvariantCulture).ToUpper());
        
        string diaAbreviado = ahora.ToString("ddd", culturaEspanol).ToLower().Replace(".", "").Replace("é", "e");
        ActualizarComponenteTexto(textoDiaNombre, diaAbreviado);
    }

    private void ActualizarComponenteTexto(TextMeshProUGUI tmp, string nuevoContenido)
    {
        if (tmp == null) return;

        if (tmp.text != nuevoContenido)
        {
            tmp.text = nuevoContenido;

            // 1. Forzar actualización de la malla de la fuente
            tmp.ForceMeshUpdate(true);

            // 2. HARDCODE FIX: Forzar al sistema de UI a recalcular el objeto
            // Esto simula lo que pasa cuando mueves el objeto en el editor manualmente
            RectTransform rect = tmp.GetComponent<RectTransform>();
            LayoutRebuilder.MarkLayoutForRebuild(rect);

            // 3. Opcional: Si el texto está dentro de un grupo (Layout Group), 
            // forzamos también al padre.
            if (rect.parent != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rect.parent as RectTransform);
            }
        }
    }
}