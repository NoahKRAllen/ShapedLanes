using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDelay : MonoBehaviour
{
    public void OnButtonClick(Button button)
    {
        StartCoroutine(DisableButtonRoutine(button));
    }

    private IEnumerator DisableButtonRoutine(Button button)
    {
        button.interactable = false;
        yield return new WaitForSeconds(0.5f);
        button.interactable = true;
    }
}