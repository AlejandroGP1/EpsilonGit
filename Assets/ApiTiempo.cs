using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[Serializable]
public class RespuestaMeteo
{
    public CurrentData current;
    public DailyData daily;
}

[Serializable]
public class CurrentData
{
    public float temperature_2m;
}

[Serializable]
public class DailyData
{
    public string[] time;
    public float[] temperature_2m_max;
    public float[] temperature_2m_min;
    public int[] precipitation_probability_max;
}

[Serializable]
public class DatosDia
{
    public string diaSemana;
    public float tempMax;
    public float tempMin;
    public int probLluvia;
}

public class ApiTiempo : MonoBehaviour
{
    [Header("Configuración")]
    public string ciudad = "Madrid";
    public float latitud  = 40.4168f;
    public float longitud = -3.7038f;
    public bool usarFahrenheit = false;

    [Header("Intervalo de actualización (segundos)")]
    public float intervaloSegundos = 360f;

    // ── TMP: datos de HOY ────────────────────────────────────────────────────
    [Header("TMP – Hoy")]
    public TextMeshProUGUI tmpCiudad;
    public TextMeshProUGUI tmpCiudad2;
    public TextMeshProUGUI tmpTempActual;
    public TextMeshProUGUI tmpMaxMinHoy;   // max y min juntas con \n
    public TextMeshProUGUI tmpLluviaHoy;
    public TextMeshProUGUI tmpLluviaHoy2;
    

    // ── TMP: próximos 4 días ─────────────────────────────────────────────────
    [Header("TMP – Próximos 4 días (índice 0 = mañana)")]
    public TextMeshProUGUI[] tmpDiaSemana = new TextMeshProUGUI[4];
    public TextMeshProUGUI[] tmpDiaMaxMin = new TextMeshProUGUI[4]; // max y min juntas con \n
    public TextMeshProUGUI[] tmpDiaLluvia = new TextMeshProUGUI[4];

    [Header("Datos en bruto (solo lectura en runtime)")]
    public DatosDia[] proximosDias = new DatosDia[4];

    private static readonly string[] _nombresDias =
        { "DOM", "LUN", "MAR", "MIE", "JUE", "VIE", "SAB" };

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        InvokeRepeating(nameof(PedirDatos), 0f, intervaloSegundos);
    }

    void PedirDatos() => StartCoroutine(ObtenerDatosMeteo());

    IEnumerator ObtenerDatosMeteo()
    {
        string unidadParam = usarFahrenheit ? "fahrenheit" : "celsius";

        string url =
            "https://api.open-meteo.com/v1/forecast" +
            $"?latitude={latitud.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={longitud.ToString(CultureInfo.InvariantCulture)}" +
            "&current=temperature_2m" +
            "&daily=temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
            $"&temperature_unit={unidadParam}" +
            "&timezone=Europe%2FMadrid" +
            "&forecast_days=5";

        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[ApiTiempo] " + req.error);
            yield break;
        }

        string json = req.downloadHandler.text;
        Debug.Log("[ApiTiempo] JSON recibido: " + json);

        RespuestaMeteo datos = JsonUtility.FromJson<RespuestaMeteo>(json);

        if (datos == null || datos.daily == null)
        {
            Debug.LogError("[ApiTiempo] No se pudo deserializar el JSON.");
            yield break;
        }

        ActualizarUI(datos);
    }

    void ActualizarUI(RespuestaMeteo datos)
    {
        if (tmpCiudad != null)
        {
            tmpCiudad.text = ciudad.ToUpper();
            tmpCiudad2.text = ciudad.ToUpper();
        }

        if (tmpTempActual != null)
            tmpTempActual.text = $"{datos.current.temperature_2m:F0}º ";

        DailyData d = datos.daily;

        // ── HOY (índice 0) ───────────────────────────────────────────────────
        if (d.temperature_2m_max != null && d.temperature_2m_max.Length > 0)
        {
            if (tmpMaxMinHoy != null)
                tmpMaxMinHoy.text = $"{d.temperature_2m_max[0]:F0}º\n         {d.temperature_2m_min[0]:F0}º";

            if (tmpLluviaHoy != null && d.precipitation_probability_max != null
                                     && d.precipitation_probability_max.Length > 0)
                                     {
                tmpLluviaHoy.text = $"{d.precipitation_probability_max[0]} ";
                tmpLluviaHoy2.text = $"{d.precipitation_probability_max[0]} ";
                }
        }

        // ── PRÓXIMOS 4 DÍAS (índices 1..4) ───────────────────────────────────
        for (int i = 0; i < 4; i++)
        {
            int idx = i + 1;

            if (idx >= d.temperature_2m_max.Length) break;

            string diaNombre = "???";
            if (d.time != null && idx < d.time.Length
                && DateTime.TryParseExact(d.time[idx], "yyyy-MM-dd",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None, out DateTime fecha))
            {
                diaNombre = _nombresDias[(int)fecha.DayOfWeek];
            }

            float max    = d.temperature_2m_max[idx];
            float min    = d.temperature_2m_min[idx];
            int   lluvia = (d.precipitation_probability_max != null
                            && idx < d.precipitation_probability_max.Length)
                           ? d.precipitation_probability_max[idx] : 0;

            proximosDias[i] = new DatosDia
            {
                diaSemana  = diaNombre,
                tempMax    = max,
                tempMin    = min,
                probLluvia = lluvia
            };

            if (tmpDiaSemana[i] != null) tmpDiaSemana[i].text = diaNombre;
            if (tmpDiaMaxMin[i] != null) tmpDiaMaxMin[i].text = $"{max:F0}º\n         {min:F0}º";
            if (tmpDiaLluvia[i] != null) tmpDiaLluvia[i].text = $"{lluvia} ";
        }

        Debug.Log($"[ApiTiempo] UI actualizada a las {DateTime.Now:HH:mm:ss}");
    }
}