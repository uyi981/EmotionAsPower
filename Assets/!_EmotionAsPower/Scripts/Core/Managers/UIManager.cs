using Assets.__EmotionAsPower.Scripts.Gameplay.Building.BuidingChild;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>, ISetup
{
    private bool showUI = false;

    public GameObject loseGamePanel;

    public string homeScreenSceneName = "HomeScreen";

    public GameSavedNotification savedNotification;
    public BreedingUI breedingUI;
    public bool ShowUI => showUI;
    public void Setup()
    {
        showUI = true;
        if (savedNotification != null) {
            DataPersistenceManager.Instance.OnGameSaved += 
                () => savedNotification.gameObject.SetActive(true);
        }
    }

    public void ToggleLoseGamePanel(bool show)
    {
        loseGamePanel.SetActive(show);
    }

    public void BackToHome()
    {
        SceneManager.LoadSceneAsync(homeScreenSceneName);
    }
    public void OpenBreedingUI(BreedingBuilding breedingBuilding)
    {
        breedingUI.gameObject.SetActive(true);
        breedingUI.SetBreedingBuilding(breedingBuilding);
    }
    
}