using UnityEngine;
using TMPro; // IMPORTANTE: Necesitas esta librería para usar TextMeshPro
using System.Collections;
using System.Collections.Generic;

public class AppYoutube : MonoBehaviour, IAppControlable
{
    public string NombreApp => "youtube";
    public enum EstadoApp { Cerrado, Esperando, Abierto }

    [Header("Configuración de Estados")]
    public EstadoApp estadoMenuInterno = EstadoApp.Cerrado;
    public bool modoNavegacion = true; 
    private bool estaContando = false; // Bloqueo para la cuenta atrás

    [Header("Referencias Sistema")]
    public SimuladorTeclado simulador;
    public AbreNavegadorConMenu navegadorHelper;
    public Animator animatorMiniMenu;
    public CargadorCircular cargador;
    public PunteroCode punteroCode;

    [Header("UI y Proximidad")]
    public TextMeshProUGUI textoUI; // Arrastra tu objeto de texto aquí
    public Transform transformFlecha; 
    public Transform objModoNavegacion;
    public Transform objMicrofono;
    public Transform objAddPestaña;

    [Header("Sprites Modo Navegación")]
    public Sprite modoClick;
    public Sprite modoTeclado;
    private SpriteRenderer srModoNav;
    private bool ultimoEstadoNavegacion;

    [Header("Debug")]
    public string objetoMasCercano;

    private Coroutine temporizadorEntrada;

    // --- LÓGICA DE GESTOS ---
    public bool SoportaGesto(string gesto) 
    {
        // Si estamos contando, bloqueamos todos los gestos
        if (estaContando) return false;

        if (estadoMenuInterno == EstadoApp.Abierto) return gesto == "Ok" || gesto == "Rock";
        
        if (estadoMenuInterno == EstadoApp.Cerrado || estadoMenuInterno == EstadoApp.Esperando)
        {
            if (modoNavegacion) 
            {
                cargador.ActivarClickThrough();
                return gesto == "Indice Arriba" || gesto == "Indice Abajo" || gesto == "Rock" || gesto == "Ok";
            }
            else 
            {
                cargador.DesactivarClickThrough();
                return gesto == "Palma Abierta" || gesto == "Indice Arriba"|| gesto == "Indice a la Izquierda" || 
                       gesto == "Indice a la Derecha" || gesto == "Rock";
            }
        }
        return false;
    }

    void Start() {
        if (objModoNavegacion != null) srModoNav = objModoNavegacion.GetComponent<SpriteRenderer>();
        ultimoEstadoNavegacion = modoNavegacion;
        ActualizarVisualizacionModo();
    }

    void Update() {
        if (modoNavegacion != ultimoEstadoNavegacion) {
            ultimoEstadoNavegacion = modoNavegacion;
            ActualizarVisualizacionModo();
        }

        // Si el menú está abierto y NO estamos contando, actualizamos el texto y la proximidad
        if (estadoMenuInterno == EstadoApp.Abierto && transformFlecha != null && !estaContando) {
            CalcularObjetoCercano();
            if(textoUI != null) textoUI.text = objetoMasCercano;
        }
    }

    private void CalcularObjetoCercano() {
        float distNav = Vector3.Distance(transformFlecha.position, objModoNavegacion.position);
        float distMic = Vector3.Distance(transformFlecha.position, objMicrofono.position);
        float distAdd = Vector3.Distance(transformFlecha.position, objAddPestaña.position);

        if (distNav < distMic && distNav < distAdd) objetoMasCercano = "Navegacion";
        else if (distMic < distNav && distMic < distAdd) objetoMasCercano = "Microfono";
        else objetoMasCercano = "Pestaña";
    }

    public void EjecutarAccion(string gesto) {
        // Bloqueo total si está contando
        if (estaContando) return;

        if (gesto == "Rock") {
            if (temporizadorEntrada != null) StopCoroutine(temporizadorEntrada);
            if (estadoMenuInterno == EstadoApp.Abierto) CerrarMenuInteractivo();
            else AbrirMenuInteractivo();
            return;
        }

        if (estadoMenuInterno == EstadoApp.Abierto && gesto == "Ok") {
            switch (objetoMasCercano) {
                case "Navegacion": AccionAlternarNavegacion(); break;
                case "Microfono": StartCoroutine(RutinaMicrofono()); break; // Iniciamos rutina
                case "Pestaña": AccionAñadirPestaña(); break;
            }
            return; 
        }

        if (estadoMenuInterno != EstadoApp.Abierto) {
            if (modoNavegacion) {
                if (gesto == "Ok") punteroCode?.EjecutarClickEnWindows();
                else if (gesto == "Indice Arriba") simulador?.EnviarScroll(1360);
                else if (gesto == "Indice Abajo") simulador?.EnviarScroll(-1360);
            } else {
                if (gesto == "Indice a la Izquierda") simulador?.EnviarTecla(0x25); 
                if (gesto == "Indice Arriba") simulador?.EnviarTecla(0x46);
                if (gesto == "Indice a la Derecha") simulador?.EnviarTecla(0x27);  
                if (gesto == "Palma Abierta") simulador?.EnviarTecla(0x20); 
            }
        }
    }

    // --- NUEVA RUTINA ---
    private IEnumerator RutinaMicrofono() {
        estaContando = true; // Bloqueamos el script
        // Hacemos el dictado de voz (opcional, si quieres activar el Win+H tras el Enter)
        simulador?.EnviarDictadoVoz();
        
        for (int i = 6; i > 0; i--) {
            if(i == 6) {   
                textoUI.text = "Asegurate de estar en un cuadro de texto";
                // Añade un tiempo de espera aquí para que el usuario pueda leerlo
                yield return new WaitForSeconds(2f); 
            }
            else {
                if(textoUI != null) textoUI.text = "" + i;
                yield return new WaitForSeconds(1f);
            }
        }
        if(textoUI != null) textoUI.text = "Ejecutando...";
        simulador?.EnviarTecla(0x0D); // Presiona ENTER (0x0D es el código de ENTER)
        
        

        estaContando = false; // Desbloqueamos
        CerrarMenuInteractivo();
    }

    // --- FUNCIONES Y LÓGICA DE ESTADO (MANTENER IGUAL) ---
    private void AccionAlternarNavegacion() {
        modoNavegacion = !modoNavegacion;
        CerrarMenuInteractivo();
    }
    
    private void AccionAñadirPestaña() {
        Application.OpenURL(navegadorHelper.urlParaAbrir);
        CerrarMenuInteractivo();
    }


    // --- MANTENIMIENTO DE ESTADOS ---

    public void AlEntrar() {
        estadoMenuInterno = EstadoApp.Esperando;
        if (animatorMiniMenu != null) animatorMiniMenu.SetBool("AbrirMiniMenu", false);
        
        // Al entrar, si modoNavegacion es true, activamos clickthrough
        if(modoNavegacion) cargador?.ActivarClickThrough();
        
        if (temporizadorEntrada != null) StopCoroutine(temporizadorEntrada);
        temporizadorEntrada = StartCoroutine(RetrasoApertura(4.0f));
    }

    public void AlSalir() {
        if (temporizadorEntrada != null) StopCoroutine(temporizadorEntrada);
        CerrarMenuInteractivo();
    }

    private IEnumerator RetrasoApertura(float tiempo) {
        yield return new WaitForSeconds(tiempo);
        if (estadoMenuInterno == EstadoApp.Esperando) AbrirMenuInteractivo();
    }

    private void AbrirMenuInteractivo() {
        estadoMenuInterno = EstadoApp.Abierto;
        if (animatorMiniMenu != null) animatorMiniMenu.SetBool("AbrirMiniMenu", true);
        navegadorHelper?.AbrirMiniMenu(); 
        cargador?.DesactivarClickThrough(); // Bloqueamos clicks externos para usar el menú
    }

    private void CerrarMenuInteractivo() {
        estadoMenuInterno = EstadoApp.Cerrado;
        if (animatorMiniMenu != null) animatorMiniMenu.SetBool("AbrirMiniMenu", false);
        navegadorHelper?.CerrarMiniMenu();

        // Si cerramos y la navegación está activa, devolvemos el control a Windows
        if (modoNavegacion) cargador?.ActivarClickThrough();
        else cargador?.DesactivarClickThrough();
    }

    private void OnDisable() {
        // 1. Desbloqueamos el script por si se apagó durante la cuenta atrás
        estaContando = false;
        
        // 2. Limpiamos el texto para que no se quede congelado en "3" al volver a entrar
        if (textoUI != null) textoUI.text = "";

        // 3. (Opcional pero recomendado) Forzamos el estado a cerrado
        estadoMenuInterno = EstadoApp.Cerrado;
    }

    private void ActualizarVisualizacionModo() {
    if (srModoNav == null) return;
    
    // Cambiamos el sprite según el booleano
    srModoNav.sprite = modoNavegacion ? modoClick : modoTeclado;
}

}