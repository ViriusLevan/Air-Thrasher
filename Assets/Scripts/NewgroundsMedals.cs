using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewgroundsMedals : MonoBehaviour
{
    public io.newgrounds.core ngio_core; 

    // Start is called before the first frame update
    void Start()
    {
        // this will call NgioReady when the core has properly initialized.
        ngio_core.onReady(NgioReady);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // call this method whenever you want to unlock a medal.
    void unlockMedal(int medal_id)
    {
        // create the component
        io.newgrounds.components.Medal.unlock medal_unlock = new io.newgrounds.components.Medal.unlock();

        // set required parameters
        medal_unlock.id = medal_id;

        // call the component on the server, and tell it to fire onMedalUnlocked() when it's done.
        medal_unlock.callWith(ngio_core, onMedalUnlocked);
    }

    void onLoginChecked(bool is_logged_in)
    {
        // do something
    }

    void checkLogin()
    {
        ngio_core.checkLogin(onLoginChecked);
    }

    void NgioReady()
    {
        io.newgrounds.components.App.logView logview = 
            new io.newgrounds.components.App.logView();
        logview.callWith(ngio_core);
    }

    // this will get called whenever a medal gets unlocked via unlockMedal()
    void onMedalUnlocked(io.newgrounds.results.Medal.unlock result)
    {
        io.newgrounds.objects.medal medal = result.medal;
        Debug.Log("Medal Unlocked: " + medal.name + 
            " (" + medal.value + " points)");
    }
}
