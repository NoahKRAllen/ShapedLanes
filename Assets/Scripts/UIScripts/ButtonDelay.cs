using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDelay : MonoBehaviour
{
    [SerializeField] private Button myButton;

    public void OnButtonClick()
    {
        StartCoroutine(DisableButtonRoutine());
    }

    private IEnumerator DisableButtonRoutine()
    {
        myButton.interactable = false;
        yield return new WaitForSeconds(0.5f);
        myButton.interactable = true;
    }
}