using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CargadorCircular : MonoBehaviour
{
    public enum EstadoMenu { Navegacion, ModoAudio, ModoApp }
    [Header("Estado Actual")]
    public EstadoMenu estadoActual = EstadoMenu.Navegacion;
    public bool menuAbierto = false;

    [Header("Referencias")]
    public ReceptorPython receptor;
    public Animator animatorMenu;
    public Image imagenProgreso;
    public ControladorAudioGlobal controladorAudio;
    public TocandoIcono tocandoIcono;
    public PunteroCode punteroCode;
    public ControladorApps controladorApps; 

    [Header("Gestos")]
    public string gestoAbrir = "Saludo";   // Para abrir menú
    public string gestoCerrar = "Back";     // Para volver/atrás
    public string gestoOk = "Ok";           // Para click o entrar
    public string gestoSalirApp = "Indice Abajo"; // Para cerrar programa desde fuera
    public string gestoPalma = "Palma Abierta";   // Para más usos dentro de la app
    public string gestoRock = "Rock";   // Para acceder a cosas del mini menu dentro de la app
    public float tiempoRequerido = 1.0f;

    [Header("Visualización Mano")]
    public GameObject manoObj;
    public Sprite manoCerrada, manoSaludo, manoIndice, manoOk, PalmaAbierta,manoRock;
    private SpriteRenderer manoRenderer;

    [Header("Permisos")]
    public bool clickThroughActivo = true;

    private float cronometro = 0f;
    private string gestoCargando = ""; // <-- NUEVO: Memoria del gesto actual
    private bool bloqueado = false;
    private IAppControlable appActiva;
    private List<IAppControlable> appsInstaladas = new List<IAppControlable>();

    void Start() {
        if (manoObj != null) manoRenderer = manoObj.GetComponent<SpriteRenderer>();
        appsInstaladas.AddRange(GetComponents<IAppControlable>());
    }

    void Update() {
        if (receptor == null || bloqueado) return;

        string gesto = receptor.gestoActual;
        ActualizarVisualizacionMano(gesto);

        // --- PREVENCIÓN DE SUPERPOSICIÓN ---
        // Si el gesto cambia a mitad de carga, reseteamos el cronómetro a 0.
        if (gesto != gestoCargando) {
            cronometro = 0f;
            gestoCargando = gesto;
        }

        bool esGestoValido = false;

        // --- MÁQUINA DE ESTADOS (Filtro de Gestos) ---
        switch (estadoActual)
        {
            case EstadoMenu.Navegacion:
                if (!menuAbierto) {
                    // MODO ESCRITORIO (Fuera de menú)
                    if (gesto == gestoOk && clickThroughActivo) { 
                        esGestoValido = true; 
                        if(LlegoAlLimite()) punteroCode?.EjecutarClickEnWindows(); 
                    }
                    else if (gesto == gestoAbrir) { 
                        esGestoValido = true; 
                        if(LlegoAlLimite()) AbrirMenuPrincipal(); 
                    }
                    else if (gesto == gestoSalirApp) { 
                        esGestoValido = true; 
                        if(LlegoAlLimite()) StartCoroutine(CerrarPrograma()); 
                    }
                } else {
                    // DENTRO DEL MENÚ PRINCIPAL
                    if (gesto == gestoOk) { 
                        esGestoValido = true; 
                        if(LlegoAlLimite()) EntrarApp(); 
                    }
                    else if (gesto == gestoCerrar) { 
                        esGestoValido = true; 
                        if(LlegoAlLimite()) CerrarMenuPrincipal(); 
                    }
                    else if (gesto == "Indice a la Izquierda") { 
                        esGestoValido = true; 
                        if(LlegoAlLimite()) IrAModoAudio(); 
                    }
                }
                break;

            case EstadoMenu.ModoAudio:
                if (gesto == gestoCerrar) { 
                    esGestoValido = true; 
                    if(LlegoAlLimite()) VolverAlMenuDesdeAudio(); 
                }
                else if (gesto == "Indice Arriba" || gesto == "Indice Abajo") { 
                    esGestoValido = true; 
                    ManejarVolumen(gesto); 
                }
                break;

            case EstadoMenu.ModoApp:
                if (gesto == gestoAbrir || gesto == gestoCerrar) { 
                    esGestoValido = true; 
                    if(LlegoAlLimite()) SalirDeApp(); 
                }
                else if (appActiva != null && appActiva.SoportaGesto(gesto)) {
                    esGestoValido = true;
                    if (LlegoAlLimite()) { appActiva.EjecutarAccion(gesto); cronometro = 0f; }
                }
                break;
        }

        // Gestión de carga
        if (esGestoValido) {
            cronometro += Time.deltaTime;
        } else {
            // Si el gesto se pierde por un momento, la barra baja rápido pero no instantáneamente
            cronometro = Mathf.Max(0, cronometro - Time.deltaTime * 5f);
        }
        
        ActualizarUI();
    }

    private bool LlegoAlLimite() {
        if (cronometro >= tiempoRequerido) { 
            cronometro = 0f; // Vaciamos la barra tras ejecutar la acción
            return true; 
        }
        return false;
    }

    // --- ACCIONES DE ESTADOS ---
    void AbrirMenuPrincipal() { 
        menuAbierto = true; 
        animatorMenu.SetBool("MainMenuActivo", true); 
        DesactivarClickThrough(); // Evita clicks en Windows al abrir el menú
    }
    
    void CerrarMenuPrincipal() { 
        menuAbierto = false; 
        animatorMenu.SetBool("MainMenuActivo", false); 
        ActivarClickThrough(); // Reactiva los clicks en Windows
    }
    
    void IrAModoAudio() { 
        estadoActual = EstadoMenu.ModoAudio; 
        animatorMenu.SetBool("ModoAudio", true); 
        menuAbierto = false; 
    }
    
    void VolverAlMenuDesdeAudio() { 
        estadoActual = EstadoMenu.Navegacion; 
        animatorMenu.SetBool("ModoAudio", false); 
        menuAbierto = true; 
    }

    void EntrarApp() {
        if (tocandoIcono == null) {
            Debug.LogError("<color=red>Cargador:</color> No hay referencia a TocandoIcono.");
            return;
        }

        if (controladorApps == null) {
            Debug.LogError("<color=red>Cargador:</color> No hay referencia a ControladorApps.");
            return;
        }

        // 1. Detectar qué estamos señalando
        string nombreApp = tocandoIcono.nombreSprite.ToLower();
        Debug.Log("<color=cyan>Cargador:</color> Intentando entrar en la App: " + nombreApp);

        if (string.IsNullOrEmpty(nombreApp)) {
            Debug.LogWarning("<color=yellow>Cargador:</color> El nombre de la app está vacío (¿estás señalando un icono?)");
            return;
        }

        // 2. Buscar en el ControladorApps si esa app existe en la lista de mapeo
        bool appEncontrada = false;
        foreach (var mapeo in controladorApps.listaMapeoApps) {
            if (mapeo.nombreID.ToLower() == nombreApp) {
                appEncontrada = true;
                Debug.Log("<color=green>Cargador:</color> App encontrada en ControladorApps: " + mapeo.nombreID);

                // Intentar obtener el script de lógica (IAppControlable) del objeto mapeado
                appActiva = mapeo.objetoApp.GetComponent<IAppControlable>();
                
                if (appActiva != null) {
                    Debug.Log("<color=green>Cargador:</color> Lógica IAppControlable detectada en el objeto.");
                } else {
                    Debug.LogWarning("<color=yellow>Cargador:</color> El objeto mapeado no tiene un script IAppControlable (lógica de gestos).");
                }

                // 3. Cambiar Estados y Sincronizar
                estadoActual = EstadoMenu.ModoApp;
                
                // Actualizar el sistema visual de iconos
                tocandoIcono.AppActual = nombreApp; 
                tocandoIcono.NuevoSpriteApp();
                Debug.Log("<color=cyan>Cargador:</color> Enviada orden de nuevo sprite a TocandoIcono.");

                // Activar Panel en ControladorApps
                controladorApps.CambiarApp(nombreApp);

                // 4. Animaciones y Eventos
                if (animatorMenu != null) {
                    animatorMenu.SetBool("App", true);
                    Debug.Log("<color=cyan>Cargador:</color> Animator 'App' -> True");
                }

                appActiva?.AlEntrar();
                
                CerrarMenuPrincipal();
                return;
            }
        }

        if (!appEncontrada) {
            Debug.LogError("<color=red>Cargador:</color> No existe ninguna App llamada '" + nombreApp + "' en la lista del ControladorApps.");
        }
}
    void SalirDeApp() {
    appActiva?.AlSalir();
    appActiva = null;
    estadoActual = EstadoMenu.Navegacion;

    // Apagamos los paneles en el controlador
    controladorApps?.CerrarTodas(); 

    // Limpiamos los iconos
    tocandoIcono.AppActual = "";
    tocandoIcono.QuitarApp();

    animatorMenu.SetBool("App", false);
    AbrirMenuPrincipal();
}

    void ManejarVolumen(string g) {
        float cambio = (g == "Indice Arriba") ? 0.3f : -0.3f;
        if(controladorAudio != null) controladorAudio.porcentaje = Mathf.Clamp(controladorAudio.porcentaje + cambio, 0f, 100f);
    }

    void ActualizarVisualizacionMano(string g) {
        if (manoRenderer == null) return;

        // 1. Por defecto, no hay ningún sprite (invisible)
        Sprite s = null; 

        // 2. Asignamos el sprite adecuado según el gesto
        if (g == gestoAbrir) s = manoSaludo;
        else if (g == gestoPalma) s = PalmaAbierta;
        else if (g == gestoRock) s = manoRock;
        else if (g == gestoCerrar) s = manoCerrada;
        else if (g == gestoOk) s = manoOk;
        else if (g.Contains("Indice")) s = manoIndice; // Solo mostramos el dedo si el string tiene "Indice"
        
        manoRenderer.sprite = s;

        // 3. Control de rotación
        if (g.Contains("Indice")) {
            float ang = g == "Indice Arriba" ? 90 : g == "Indice Abajo" ? 270 : g == "Indice a la Izquierda" ? 180 : 0;
            manoObj.transform.rotation = Quaternion.Euler(0, 0, ang);
        } else {
            manoObj.transform.rotation = Quaternion.identity;
        }
    }

    void ActualizarUI() {
        if (imagenProgreso) {
            float p = Mathf.Clamp01(cronometro / tiempoRequerido);
            imagenProgreso.fillAmount = p;
            Color c = imagenProgreso.color; c.a = p; imagenProgreso.color = c;
        }
    }

    // --- CONTROL DE PERMISOS ---
    public void ActivarClickThrough() => clickThroughActivo = true;
    public void DesactivarClickThrough() => clickThroughActivo = false;

    IEnumerator CerrarPrograma() {
        bloqueado = true;
        animatorMenu.SetBool("Cerrando", true);
        yield return new WaitForSeconds(4f);
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}