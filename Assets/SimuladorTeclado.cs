using UnityEngine;
using System.Runtime.InteropServices;

public class SimuladorTeclado : MonoBehaviour
{
    // --- IMPORTACIÓN DE FUNCIONES DE WINDOWS ---
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    // --- CONSTANTES ---
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const int MOUSEEVENTF_WHEEL = 0x0800;
    
    // Teclas para Dictado
    private const byte VK_LWIN = 0x5B; 
    private const byte VK_H = 0x48;

    // --- MÉTODOS PÚBLICOS ---

    public void EnviarTecla(byte tecla) {
        keybd_event(tecla, 0, 0, 0); // Presionar
        keybd_event(tecla, 0, KEYEVENTF_KEYUP, 0); // Soltar
    }

    public void EnviarScroll(int cantidad) {
        // cantidad 120 = una muesca arriba, -120 = una muesca abajo
        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, cantidad, 0);
    }

    public void EnviarDictadoVoz() {
        // 1. Presionar Windows
        keybd_event(VK_LWIN, 0, 0, 0);
        
        // 2. Presionar H
        keybd_event(VK_H, 0, 0, 0);
        
        // 3. Soltar H
        keybd_event(VK_H, 0, KEYEVENTF_KEYUP, 0);
        
        // 4. Soltar Windows
        keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0);
    }
}