using System.IO;
using UnityEngine;

public class JsonStateMachineLoader : MonoBehaviour
{
    [Header("JSON Dosya Adı (StreamingAssets içindeki dosya)")]
    [Tooltip("Örnek: bossStateMachine.json")]
    public string jsonFileName = "bossStateMachine.json";

    private MyStateMachine.FromJson.DynamicStateMachine _machine;

    void Start()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"State machine JSON dosyası bulunamadı: {filePath}");
            return;
        }

        _machine = new MyStateMachine.FromJson.DynamicStateMachine();
        _machine.LoadFromJsonFile(filePath);
    }

    void Update()
    {
        _machine?.Update();
    }
}
