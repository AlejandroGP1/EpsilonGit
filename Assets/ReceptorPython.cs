using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization; 
using UnityEngine;

public class ReceptorPython : MonoBehaviour
{
    [Header("Configuración de Red")]
    public int port = 5052;

    [Header("Valores en Tiempo Real (Solo Lectura)")]
    public string estadoActual = "Sin mano";
    public string gestoActual = "Nada";
    public float posX;
    public float posY;

    // Herramientas internas
    Thread receiveThread;
    UdpClient client;

    void Start()
    {
        Application.targetFrameRate = 60;
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        try 
        {
            client = new UdpClient(port);
            while (true)
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(dataByte);

                // --- PROCESAMIENTO DE DATOS ---
                string[] partes = text.Split('|');

                // Si el paquete tiene los 4 datos que esperamos:
                if (partes.Length >= 4)
                {
                    estadoActual = partes[0];
                    gestoActual = partes[1];
                    
                    // Convertimos texto a número usando "InvariantCulture" (ignora comas/puntos locales)
                    posX = float.Parse(partes[2], CultureInfo.InvariantCulture);
                    posY = float.Parse(partes[3], CultureInfo.InvariantCulture);
                }
            }
        }
        catch (Exception e) 
        {
            Debug.LogWarning("Error en el hilo de recepción: " + e.Message); 
        }
    }

    // Cerramos todo al salir
    void OnApplicationQuit()
    {
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Abort();
        if (client != null) client.Close();
    }
}