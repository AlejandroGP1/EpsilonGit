using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class AppMapping
{
    public string nombreID;      
    public GameObject objetoApp; 
}

public class ControladorApps : MonoBehaviour
{
    [Header("Mapeo de Aplicaciones")]
    public List<AppMapping> listaMapeoApps;

    // Eliminamos el Update para evitar conflictos de "doble mando"

    public void CambiarApp(string nombreApp)
    {
        if (string.IsNullOrEmpty(nombreApp))
        {
            CerrarTodas();
            return;
        }

        bool encontrada = false;
        foreach (AppMapping mapeo in listaMapeoApps)
        {
            if (mapeo.objetoApp == null) continue;

            bool esEsta = string.Equals(mapeo.nombreID, nombreApp, StringComparison.OrdinalIgnoreCase);
            mapeo.objetoApp.SetActive(esEsta);

            if (esEsta) encontrada = true;
        }

        if (encontrada) Debug.Log($"<color=green>ControladorApps:</color> Panel de {nombreApp} activado.");
        else Debug.LogWarning($"No se encontró panel para: {nombreApp}");
    }

    public void CerrarTodas()
    {
        foreach (AppMapping mapeo in listaMapeoApps)
        {
            if (mapeo.objetoApp != null) mapeo.objetoApp.SetActive(false);
        }
    }
}