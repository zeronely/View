using System;
using Spate;

public static class SoundAPI
{
    public static void PlayButton(bool toSelect)
    {
        if (toSelect)
            PlayButtonSelect();
        else
            PlayButtonUnSelect();
    }

    public static void PlayButtonSelect()
    {
        SoundManager.GetSePlayer().Play("se_button_select");
    }

    public static void PlayButtonUnSelect()
    {
        SoundManager.GetSePlayer().Play("se_button_unselect");
    }

    public static void PlayTabSelect()
    {
        SoundManager.GetSePlayer().Play("se_tab_select");
    }
}
