using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void GoToSelection()
    {
        Menu.MENU.GoToSelection();
    }

    public void LoadLevel(eLevelType levelType)
    {
        TypeTransfer.TYPE = levelType;
        SceneManager.LoadScene(1);
    }

    public void LoadSnow()
    {
        LoadLevel(eLevelType.snow);
    }

    public void LoadField()
    {
        LoadLevel(eLevelType.field);
    }

    public void LoadMountain()
    {
        LoadLevel(eLevelType.mountain);
    }
}
