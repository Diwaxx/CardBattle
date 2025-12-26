using TMPro;
using UnityEngine;

[System.Serializable]
public class SpriteTextDisplay : MonoBehaviour
{
    public TMP_Text textComponent;
    public TMP_SpriteAsset spriteAsset;

    public void Initialize()
    {
        if (textComponent != null && spriteAsset != null)
        {
            textComponent.spriteAsset = spriteAsset;
        }
    }

    public void SetText(string input)
    {
        if (textComponent == null) return;

        string spriteText = ConvertToSpriteText(input);
        textComponent.text = spriteText;
        textComponent.gameObject.SetActive(true);
    }

    private string ConvertToSpriteText(string input)
    {
        string result = "";

        foreach (char c in input)
        {
            switch (c)
            {
                case '0': result += "<sprite name=\"0\">"; break;
                case '1': result += "<sprite name=\"1\">"; break;
                case '2': result += "<sprite name=\"2\">"; break;
                case '3': result += "<sprite name=\"3\">"; break;
                case '4': result += "<sprite name=\"4\">"; break;
                case '5': result += "<sprite name=\"5\">"; break;
                case '6': result += "<sprite name=\"6\">"; break;
                case '7': result += "<sprite name=\"7\">"; break;
                case '8': result += "<sprite name=\"8\">"; break;
                case '9': result += "<sprite name=\"9\">"; break;
                case '+': result += "<sprite name=\"plus\">"; break;
                case '-': result += "<sprite name=\"minus\">"; break;
                default: result += c; break;
            }
        }

        Debug.Log($"Converted '{input}' to '{result}'");
        return result;
    }
}