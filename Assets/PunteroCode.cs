using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class PunteroCode : MonoBehaviour
{
    public ReceptorPython receptor;

    [Header("Multiplicadores (Sensibilidad)")]
    public float multiplicadorX = 20f; 
    public float multiplicadorY = 15f; 

    [Header("Offsets (Ajuste de posición)")]
    public float offsetX = 0f;
    public float offsetY = 0f;

    [Header("Inversión de Ejes")]
    public bool invertirX = false;
    public bool invertirY = true; 

    // --- API DE WINDOWS ---
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    // Esta función permite leer el teclado a nivel de Sistema Operativo
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private const int VK_F = 0x46; // Código virtual de la tecla 'F'

    private bool teclaPresionadaEnFrameAnterior = false;

    void Update()
    {
        ManejarMovimiento();
        ManejarTecladoGlobal();
    }

    void ManejarMovimiento()
    {
        if (receptor == null || receptor.estadoActual.Trim() == "Sin mano") 
        {
            transform.position = new Vector3(0, 0, 0);
            return; 
        }

        float xCentrado = receptor.posX - 0.5f;
        float yCentrado = receptor.posY - 0.5f;

        if (invertirX) xCentrado *= -1;
        if (invertirY) yCentrado *= -1;

        float finalX = (xCentrado * multiplicadorX) + offsetX;
        float finalY = (yCentrado * multiplicadorY) + offsetY;

        transform.position = new Vector3(finalX, finalY, 0);
    }

    void ManejarTecladoGlobal()
    {
        // GetAsyncKeyState devuelve un valor cuyo bit más significativo es 1 si la tecla está pulsada
        bool estaSiendoPulsada = (GetAsyncKeyState(VK_F) & 0x8000) != 0;

        // Simulamos el "wasPressedThisFrame" manualmente para no hacer 60 clics por segundo
        if (estaSiendoPulsada && !teclaPresionadaEnFrameAnterior)
        {
            //EjecutarClickEnWindows();
            Debug.Log("F detectada globalmente (Incluso fuera de foco)");
        }

        teclaPresionadaEnFrameAnterior = estaSiendoPulsada;
    }

    public void EjecutarClickEnWindows()
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        int windowsX = (int)screenPos.x;
        int windowsY = Screen.height - (int)screenPos.y;

        // 1. Mover cursor
        SetCursorPos(windowsX, windowsY);
        // 2. Click
        mouse_event(MOUSEEVENTF_LEFTDOWN, windowsX, windowsY, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, windowsX, windowsY, 0, 0);
    }


}