using UnityEngine;
using System.IO;
public class DataPersistenceManager : MonoBehaviour
{
    private string GetUsername()
    {
        string username = AccountManager.Instance.GetCurrentAccount();
        if (string.IsNullOrEmpty(username))
        {
            // Default to "Jugador1" if no account selected
            username = "Jugador1";
        }
        return username;
    }

    private string GetPath()
    {
        return Application.persistentDataPath + "/" + GetUsername() + ".json";
    }
}
