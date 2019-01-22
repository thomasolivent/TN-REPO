using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour {

    public TutorialStep[] tutorialSteps;

    public GameObject panel;
    public Text tutorialText;
    public GameObject arrow;

    int stepInt = 0;

    void Awake()
    {
        UpdateTutorial(0);
    }

    public void EnablePanel()
    {
        panel.SetActive(true);
    }

    public void DisablePanel()
    {
        panel.SetActive(false);
    }

    public void NextStep(bool fwd) {
        switch (fwd)
        {
            case true:
                if (stepInt != tutorialSteps.Length - 1)
                {
                    stepInt++;
                }
                else
                {
                    stepInt = 0;
                }
                UpdateTutorial(stepInt);
                break;
            case false:
                if (stepInt != 0)
                {
                    stepInt--;
                }
                else if(stepInt == 0)
                {
                    stepInt = (tutorialSteps.Length) - 1;
                }
                UpdateTutorial(stepInt);
                break;
            default:
                break;
        }
    }

    public void UpdateTutorial(int id)
    {
        arrow.gameObject.SetActive(false);
        TutorialStep newStep = tutorialSteps[id];

        tutorialText.text = newStep.tutorialDialog;

        panel.GetComponent<RectTransform>().localPosition = newStep.panelPosDuringStep;

        if (newStep.tutorialObject != null)
        {
            arrow.gameObject.SetActive(true);
            Vector3 diff = newStep.tutorialObject.transform.position - panel.transform.position;
            float angle = Mathf.Atan2(diff.x, diff.y);

            arrow.transform.rotation = Quaternion.Euler(0, 0, -(Mathf.Rad2Deg * angle));
            arrow.transform.position = (panel.transform.position + newStep.tutorialObject.transform.position) / 2f;
        }
    }

    public void UpdateBool(int id)
    {
        TutorialStep newStep = tutorialSteps[id];
        newStep.wasSeen = true;
    }

    [Serializable]
    public class TutorialStep
    {
        public string tutorialDialog;
        public GameObject tutorialObject;
        public Vector2 panelPosDuringStep;
        public bool wasSeen;
    }
}
