using UnityEngine;

public class AppTemperatura : MonoBehaviour, IAppControlable
{
    public string NombreApp => "temperatura";

    [Header("Referencias")]
    public Animator animatorMegaT;
    public CargadorCircular cargador;

    [Header("Estado")]
    public bool megaTActivo = false;
    public int MegaMenuActual = 0;
    public bool Lluvia = false;

    // =========================
    // GESTOS SOPORTADOS
    // =========================
    public bool SoportaGesto(string gesto)
    {
        // Siempre puede detectar cuernos
        if (gesto == "Rock")
            return true;

        // Solo detectar izquierda/derecha si MegaT está activo
        if (megaTActivo)
        {
            return gesto == "Indice a la Derecha" ||
                   gesto == "Indice a la Izquierda";
        }

        return false;
    }

    // =========================
    // EJECUTAR ACCIONES
    // =========================
    public void EjecutarAccion(string gesto)
    {
        // -------------------------
        // ACTIVAR / DESACTIVAR MegaT
        // -------------------------
        if (gesto == "Rock")
        {
            megaTActivo = !megaTActivo;

            if (animatorMegaT != null)
                animatorMegaT.SetBool("MegaT", megaTActivo);

            // ClickThrough
            if (megaTActivo)
                cargador?.DesactivarClickThrough();
            else
                cargador?.ActivarClickThrough();

            Debug.Log("MegaT cambiado a: " + megaTActivo);

            return;
        }

        // -------------------------
        // MENU DERECHA (Ir al menú 1 / Lluvia)
        // -------------------------
        if (megaTActivo && gesto == "Indice a la Derecha")
        {
            if (MegaMenuActual != 1) // Solo se ejecuta si no estamos ya en ese menú
            {
                MegaMenuActual = 1;
                animatorMegaT.SetBool("Lluvia", true);
                EjecutarMenuMegaT();
            }
        }

        // -------------------------
        // MENU IZQUIERDA (Ir al menú 0 / Normal)
        // -------------------------
        if (megaTActivo && gesto == "Indice a la Izquierda")
        {
            if (MegaMenuActual != 0) // Solo se ejecuta si no estamos ya en ese menú
            {
                MegaMenuActual = 0;
                animatorMegaT.SetBool("Lluvia", false);
                EjecutarMenuMegaT();
            }
        }
    }

    // =========================
    // MENU MEGAT
    // =========================
    private void EjecutarMenuMegaT()
    {
        Debug.Log("MegaMenuActual: " + MegaMenuActual);

        switch (MegaMenuActual)
        {
            case 0:
                Debug.Log("MENÚ 0");
                break;

            case 1:
                Debug.Log("MENÚ 1");
                break;
        }
    }

    // =========================
    // ESTADOS APP
    // =========================
    public void AlEntrar()
    {
        Debug.Log("Entró en App Temperatura");
    }

    public void AlSalir()
    {
        megaTActivo = false;

        if (animatorMegaT != null)
        {
            animatorMegaT.SetBool("MegaT", false);
            animatorMegaT.SetBool("LLuvia", false);
        }
        //cargador?.ActivarClickThrough();

        Debug.Log("Salió de App Temperatura");
    }
}