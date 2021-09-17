using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_EventText : MonoBehaviour
{
    [SerializeField]private float displayDuration;
    private int lastModifiedIndex;
    private float[] textTimers;
    [SerializeField]private TextMeshProUGUI[] eventTexts;
    [SerializeField]private Animator [] animatorEventTexts;


    // Start is called before the first frame update
    void Start()
    {
        textTimers = new float[3];
        textTimers[0] = textTimers[1] = textTimers[2] = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //Timer until moving texts to the left
        for (int i = 0; i < eventTexts.Length; i++)
        {
            if (textTimers[i]>-10f)
            {
                textTimers[i] -= Time.deltaTime;
                if (textTimers[i] <= 0) {
                    animatorEventTexts[i].
                        SetBool("Displaying",false);
                    textTimers[i] = -10f;
                    lastModifiedIndex = 
                        (i-1<0) ? eventTexts.Length-1 : i-1;
                }
            }
        }
    }

    public void DisplayAnEvent(string eventName, int whatColor) {
        int queuedIdx = lastModifiedIndex+1;
        if (lastModifiedIndex == eventTexts.Length - 1) {
            queuedIdx = 0;
        }
        if (whatColor == 2)//blue
        {
            eventTexts[queuedIdx].color = new Color(0, 117, 255, 255);
        }
        else if (whatColor == 1)//red
        {
            eventTexts[queuedIdx].color = new Color(241, 0, 0, 255);
        }
        else
        { //white
            eventTexts[queuedIdx].color = new Color(255, 255, 255, 255);
        }

        eventTexts[queuedIdx].text = eventName;
        textTimers[queuedIdx] = displayDuration;
        if (!animatorEventTexts[queuedIdx].
            GetBool("Displaying"))
        {
            animatorEventTexts[queuedIdx].
                SetBool("Displaying", true);
        }
        else {
            StartCoroutine(DisappearBeforeChanging(queuedIdx));
        }
        lastModifiedIndex = queuedIdx;
    }

    private IEnumerator DisappearBeforeChanging(int qIdx) {
        textTimers[qIdx] = 0;
        yield return new WaitForSeconds(0.1f);
        animatorEventTexts[qIdx].
                 SetBool("Displaying", true);
    }

    private IEnumerator WaitBeforeErasingText
        (TextMeshProUGUI tmpObj, int idx) {
        yield return new WaitForSeconds(0.1f);
        tmpObj.text = "";
        lastModifiedIndex = idx;
    }
}
