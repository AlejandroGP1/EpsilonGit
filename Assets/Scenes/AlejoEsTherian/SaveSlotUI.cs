using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Botones de slots")]
    public Button[] slotButtons; // Arrastra aquí los 3 botones en el Inspector

    void Start()
    {
        // Asigna el listener a cada botón según su índice
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i + 1; // Slots 1, 2 y 3
            slotButtons[i].onClick.AddListener(() => OnSlotSelected(slotIndex));

            // Opcional: muestra si el slot tiene datos
            UpdateButtonLabel(i, slotIndex);
        }
    }

    void OnSlotSelected(int slot)
{
    if (SaveManager.Instance == null)
    {
        Debug.LogError("SaveManager no encontrado en la escena.");
        return;
    }

    if (SaveManager.Instance.SlotExists(slot))
    {
        GameData data = SaveManager.Instance.LoadGame(slot);
        Debug.Log($"Cargando slot {slot}: Nivel {data.level}");
    }
    else
    {
        Debug.Log($"Slot {slot} vacío — creando nueva partida");
    }
}

    void UpdateButtonLabel(int buttonIndex, int slot)
    {
        // Si el botón tiene un TextMeshPro hijo, actualiza su texto
        var label = slotButtons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = SaveManager.Instance.SlotExists(slot)
                ? $"Slot {slot} — Continuar"
                : $"Slot {slot} — Nueva partida";
        }
    }
}