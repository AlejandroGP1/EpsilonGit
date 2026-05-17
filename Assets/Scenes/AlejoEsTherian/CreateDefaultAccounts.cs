using UnityEngine;

public class CreateDefaultAccounts : MonoBehaviour
{
    void Start()
    {
        CreateAccounts();
    }

    private void CreateAccounts()
    {
        CreateIfNotExists("Archivo1");
        CreateIfNotExists("Archivo2");
        CreateIfNotExists("Archivo3");
    }

    private void CreateIfNotExists(string username)
    {
        bool created = AccountManager.Instance.CreateAccount(username);

        if (created)
        {
            Debug.Log("Cuenta creada: " + username);
        }
        else
        {
            Debug.Log("La cuenta ya existe: " + username);
        }
    }
}