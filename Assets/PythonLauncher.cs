using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonLauncher : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string scriptName = "VentanaCam.py";

    private Process pythonProcess;

    void Start()
    {
        // 1. Rutas de carpetas
        string pythonFolder = Path.Combine(Application.streamingAssetsPath, "Python");
        string scriptPath = Path.Combine(pythonFolder, scriptName);
        
        // 2. Ruta al ejecutable de Python PORTÁTIL
        string portablePythonPath = Path.Combine(pythonFolder, "PythonDist", "python.exe");

        // Verificaciones de archivos
        if (!File.Exists(portablePythonPath))
        {
            UnityEngine.Debug.LogError($"[PythonLauncher] No se encontró el motor en: {portablePythonPath}");
            return;
        }

        if (!File.Exists(scriptPath))
        {
            UnityEngine.Debug.LogError($"[PythonLauncher] No existe el script en: {scriptPath}");
            return;
        }

        // Lanzar el proceso
        LanzarPython(portablePythonPath, scriptPath);
    }

    private void LanzarPython(string pythonExe, string fullScriptPath)
    {
        string workingDir = Path.GetDirectoryName(fullScriptPath);

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonExe,
            // Ponemos comillas en las rutas por si el usuario tiene espacios en su nombre de carpeta
            Arguments = $"\"{fullScriptPath}\"",
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            pythonProcess = new Process();
            pythonProcess.StartInfo = psi;

            // Registro de errores en archivo externo para Debug en la Build
            pythonProcess.ErrorDataReceived += (s, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    string logPath = Path.Combine(Application.persistentDataPath, "python_log.txt");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] ERROR: {e.Data}\n");
                }
            };

            pythonProcess.Start();
            pythonProcess.BeginErrorReadLine();
            
            UnityEngine.Debug.Log("Python iniciado correctamente desde: " + pythonExe);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Fallo crítico al lanzar Python: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            try {
                pythonProcess.Kill();
                pythonProcess.Dispose();
                UnityEngine.Debug.Log("Proceso Python finalizado.");
            } catch (Exception e) {
                UnityEngine.Debug.LogWarning("Error al cerrar Python: " + e.Message);
            }
        }
    }
}