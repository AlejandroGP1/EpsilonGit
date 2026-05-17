using UnityEngine;

public class AccountUI : MonoBehaviour
{
    public void SelectAccount(string username)
    {
        bool success = AccountManager.Instance.SelectAccount(username);

        if (success)
        {
            Debug.Log("Cuenta seleccionada: " + username);

            // Aquí puedes cambiar de escena si quieres
            // SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("No se pudo seleccionar la cuenta");
        }
    }
}