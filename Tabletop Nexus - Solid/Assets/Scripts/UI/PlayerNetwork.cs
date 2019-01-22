using UnityEngine;
using UnityEngine.UI;

public class PlayerNetwork : MonoBehaviour {

    [SerializeField]
    Text playerNameTextField;
    public static PlayerNetwork Instance;
    public string PlayerName { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void SetPlayerName()
    {
        PlayerName = playerNameTextField.text;
    }
}
