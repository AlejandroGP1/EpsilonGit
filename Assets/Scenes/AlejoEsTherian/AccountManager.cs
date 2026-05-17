using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class AccountManager : MonoBehaviour
{
    private static AccountManager instance;
    public static AccountManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AccountManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AccountManager");
                    instance = obj.AddComponent<AccountManager>();
                }
            }
            return instance;
        }
    }

    private const string AccountsKey = "Accounts";
    private List<string> accounts = new List<string>();
    private string currentAccount;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAccounts();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAccounts()
    {
        string accountsJson = PlayerPrefs.GetString(AccountsKey, "");
        if (!string.IsNullOrEmpty(accountsJson))
        {
            accounts = JsonUtility.FromJson<AccountList>(accountsJson).accounts;
        }
    }

    private void SaveAccounts()
    {
        AccountList accountList = new AccountList { accounts = accounts };
        string accountsJson = JsonUtility.ToJson(accountList);
        PlayerPrefs.SetString(AccountsKey, accountsJson);
        PlayerPrefs.Save();
    }

    public bool CreateAccount(string username)
    {
        if (string.IsNullOrEmpty(username) || accounts.Contains(username))
        {
            return false; // Account already exists or invalid name
        }
        accounts.Add(username);
        SaveAccounts();
        return true;
    }

    public bool SelectAccount(string username)
    {
        if (accounts.Contains(username))
        {
            currentAccount = username;
            return true;
        }
        return false;
    }

    public bool DeleteAccount(string username)
    {
        if (accounts.Contains(username))
        {
            accounts.Remove(username);
            SaveAccounts();
            // Optionally delete the data file
            string path = Application.persistentDataPath + "/" + username + ".json";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (currentAccount == username)
            {
                currentAccount = null;
            }
            return true;
        }
        return false;
    }

    public List<string> GetAccounts()
    {
        return new List<string>(accounts);
    }

    public string GetCurrentAccount()
    {
        return currentAccount;
    }

    [System.Serializable]
    private class AccountList
    {
        public List<string> accounts;
    }
}