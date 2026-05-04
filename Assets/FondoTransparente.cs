using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransparentWindow : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    
    // Estructuras para la API de Windows
    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    // Importación de funciones de Windows
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    // Constantes de Windows
    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    
    // Banderas de SetWindowPos para forzar el refresco
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_FRAMECHANGED = 0x0020; // <--- Esta es la clave
    private const uint SWP_SHOWWINDOW = 0x0040;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    private IntPtr hWnd;
    private bool lastClickThroughState = false;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        #if !UNITY_EDITOR
        hWnd = GetActiveWindow();

        // 1. Extender el marco al área cliente para transparencia real
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // 2. Estado inicial: Topmost y transparente al click
        SetClickThrough(true);
        #endif
    }

    void Update()
    {
        #if !UNITY_EDITOR
        bool sobreAlgo = RevisarSiHayContenidoBajoElMouse();
        
        // Solo llamamos a la API de Windows si el estado ha cambiado
        // Esto mejora mucho el rendimiento y estabilidad
        if (sobreAlgo == lastClickThroughState) 
        {
            SetClickThrough(!sobreAlgo);
        }
        #endif
    }

    private bool RevisarSiHayContenidoBajoElMouse()
    {
        // 1. Detectar UI (Canvas)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        // 2. Detectar el Hexágono (Requiere Collider2D)
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        
        return hit != null;
    }

    public void SetClickThrough(bool canClickThrough)
    {
        lastClickThroughState = !canClickThrough;

        if (canClickThrough)
        {
            // Transparente al click
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
        else
        {
            // Sólido al click
            SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        }

        // FORZAR REFRESCO: Esto hace que Windows sepa que debe dejar pasar el click AHORA
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
    }
}