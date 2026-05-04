using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class BuscadorMusica : MonoBehaviour
{
    [Header("Configuración")]
    public string DirectorioMusica = @"C:\Users\User\Music\Canciones";
    public AudioSource audioSource;
    public TextMeshProUGUI textoNombreCancion;
    public Slider sliderProgreso;

    [Header("Objetos Físicos")]
    public GameObject objetoPrevio, objetoSiguiente, objetoPausa;
    public Sprite spritePlay, spritePausa;
    private SpriteRenderer srPausa;

    [Header("Estado")]
    public bool esAleatorio;
    public bool esBucle;
    private List<string> rutasCanciones = new List<string>();
    private int indiceActual = 0;
    private bool puedeAccionar = true;
    private bool estaArrastrandoSlider = false; // Para que el Update no pelee con tu dedo/ratón

    void Start() {
        if (Directory.Exists(DirectorioMusica)) 
            rutasCanciones.AddRange(Directory.GetFiles(DirectorioMusica, "*.mp3"));
        
        if (objetoPausa) srPausa = objetoPausa.GetComponent<SpriteRenderer>();

        // Configurar el Slider para que responda a cambios manuales
        if (sliderProgreso != null)
            sliderProgreso.onValueChanged.AddListener(OnSliderChanged);

        if (rutasCanciones.Count > 0) 
            StartCoroutine(CargarYReproducir(rutasCanciones[0]));
    }

    void Update() {
        // 1. Actualizar el progreso del slider visualmente
        ActualizarProgresoSlider();

        // 2. Auto-reproducir siguiente al terminar la canción
        if (!audioSource.isPlaying && audioSource.time == 0 && rutasCanciones.Count > 0 && audioSource.clip != null)
        {
            SiguienteCancion();
        }
    }

    // --- LÓGICA DEL SLIDER ---

    void ActualizarProgresoSlider() {
        if (sliderProgreso != null && audioSource.clip != null && !estaArrastrandoSlider) {
            sliderProgreso.maxValue = audioSource.clip.length;
            sliderProgreso.value = audioSource.time;
        }
    }

    // Llama a esto desde un EventTrigger "Pointer Down" en el Slider
    public void OnSliderPointerDown() => estaArrastrandoSlider = true;

    // Llama a esto desde un EventTrigger "Pointer Up" en el Slider
    public void OnSliderPointerUp() => estaArrastrandoSlider = false;

    public void OnSliderChanged(float valor) {
        // Solo cambiamos el tiempo del audio si el usuario es quien mueve el slider
        if (estaArrastrandoSlider) {
            audioSource.time = valor;
        }
    }

    // --- LÓGICA DE COLISIÓN FÍSICA ---

    private void OnTriggerEnter2D(Collider2D col) {
        if (!puedeAccionar) return;
        if (col.gameObject == objetoSiguiente) SiguienteCancion();
        else if (col.gameObject == objetoPrevio) AnteriorCancion();
        else if (col.gameObject == objetoPausa) PlayPauseFisico();
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown() { 
        puedeAccionar = false; 
        yield return new WaitForSeconds(0.5f); 
        puedeAccionar = true; 
    }

    public void PlayPauseFisico() {
        if (audioSource.isPlaying) { 
            audioSource.Pause(); 
            if(srPausa) srPausa.sprite = spritePlay; 
        }
        else { 
            audioSource.UnPause(); 
            if(srPausa) srPausa.sprite = spritePausa; 
        }
    }

    public void SiguienteCancion() {
        if (rutasCanciones.Count == 0) return;
        indiceActual = esAleatorio ? UnityEngine.Random.Range(0, rutasCanciones.Count) : (indiceActual + 1) % rutasCanciones.Count;
        StartCoroutine(CargarYReproducir(rutasCanciones[indiceActual]));
    }

    public void AnteriorCancion() {
        if (rutasCanciones.Count == 0) return;
        indiceActual = (indiceActual - 1 + rutasCanciones.Count) % rutasCanciones.Count;
        StartCoroutine(CargarYReproducir(rutasCanciones[indiceActual]));
    }

    IEnumerator CargarYReproducir(string ruta) {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + ruta, AudioType.MPEG)) {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.Play();
                
                if(srPausa) srPausa.sprite = spritePausa;
                if(textoNombreCancion) textoNombreCancion.text = Path.GetFileNameWithoutExtension(ruta);
                
                // Resetear slider para la nueva canción
                if(sliderProgreso) {
                    sliderProgreso.maxValue = audioSource.clip.length;
                    sliderProgreso.value = 0;
                }
            }
        }
    }
}