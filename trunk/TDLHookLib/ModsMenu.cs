/* ModsMenu.cs by Piepieonline
 * Practically lifted from the World Delete Menu, but a few tweaks
 * 
 * Important: for modded menus, we must change the way we enable/disable the menu - due to how we add it to the game
 * Also, we use OnGUI, not TDLOnGUI
 */

using System;
using UnityEngine;

using TDLHookLib;

public class BodyMenuMods : IBodyMenu
{
    public static BodyMenuMods singleton;

    static BodyMenuMods()
    {
    }

    public BodyMenuMods()
    {
    }

    public override void EscapePressed()
    {
        StartMenu.singleton.SwitchPanes(StartMenu._paneState.NEWS);
    }

    private void Start()
    {
    }

    private void Awake()
    {
        BodyMenuMods.singleton = this;
    }

    public override void TDLOnGUI(Rect r)
    {
        // Ripped this from the world select screen, hence the default names
        
        Rect rect = r;

        // Label
        float labelSize = 2f * (float)(StartMenu.singleton.guiWorldListStyle.fontSize + 4);
        string ModsLabel = "MODS (In load order)";
        GUI.Label(new Rect(r.x + 1f, r.y + 1f, r.width, labelSize), string.Concat("<color=white>", ModsLabel, "</color>"), StartMenu.singleton.guiWorldListHeaderStyle);


        // Offset the outline box
        rect.y = rect.y + labelSize * 2;
        rect.height = rect.height - labelSize * 2;
        float single1 = (float)(StartMenu.singleton.guiButtonsMedium.fontSize * 2 + 16);
        single1 = single1 * 2f;
        rect.height = rect.height - single1;
        float single2 = rect.y + rect.height;
        
        
        GUI.DrawTexture(rect, StartMenu.singleton.boundsBackground);
        GUI.BeginGroup(rect);
        
        Vector2 scrollPos = GUILayout.BeginScrollView(StartMenu.singleton.worldListScrollPosition, new GUILayoutOption[] { GUILayout.Width(rect.width), GUILayout.Height(rect.height) });
        float single3 = (float)(StartMenu.singleton.guiWorldListStyle.fontSize + 4) * 4;
        float single4 = 0;

        foreach (Mod currMod in TDLPlugin.mods)
        {
            single4 = single4 + single3;
            if (GUI.Button(new Rect(0f, single4, rect.width, single3), currMod.GetGUIListString(), StartMenu.singleton.guiWorldListStyle))
            //if (GUILayout.Button(currMod.GetGUIListString(), TDLMenuCommon.Singleton.tdlStyleWorldList, new GUILayoutOption[0]))
            {
                SfxScripts.PlaySfx("menu_click");
                //Show mod info
            }
        }


        GUILayout.EndScrollView();
        GUI.EndGroup();

        /*
        for (int i = 0; i < IBodyMenu.worldNames.Count; i++)
        {
            string item = IBodyMenu.worldNames[i];
            bool flag = false;
            //flag = (i == IBodyMenu.selectedWorld ? GUI.Button(new Rect(0f, single4, rect.width, single3), item, StartMenu.singleton.guiWorldListSelectedStyle) : GUI.Button(new Rect(0f, single4, rect.width, single3), item, StartMenu.singleton.guiWorldListStyle));
            //if (flag)
            //{
            //    IBodyMenu.selectedWorld = i;
            //    num = 1;
            //    SfxScripts.PlaySfx("Audio/ui/mouse_click");
            //    IBodyMenu.selectedWorldData = Scenario.GetWorldDetails(IBodyMenu.worldNames[IBodyMenu.selectedWorld]);
            //}
            single4 = single4 + single3;
        }
        GUILayout.EndScrollView();
        GUI.EndGroup();
        */

        base.TDLOnGUI(r);
    }

    public override void Transition()
    {
        base.Transition();
        IBodyMenu.WorldSelectInit();
    }
}

/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace TDLHookLib
{
    class ModsMenu : MonoBehaviour
    {
        public static ModsMenu Singleton;
        public int guiDepth = 1;
        public static bool visible = false;

        private Vector2 modListPos = new Vector2();

        public static void activateGUI()
        {
            ModsMenu.Singleton.gameObject.GetComponent<ModsMenu>().enabled = true;
            ModsMenu.visible = true;
        }

        private void Awake()
        {
            ModsMenu.Singleton = this;
        }

        public static void deactivateGUI()
        {
            ModsMenu.visible = false;
            ModsMenu.Singleton.gameObject.GetComponent<ModsMenu>().enabled = false;
        }

        private void Start()
        {}

        private void OnGUI()
        {
            GUIStyle gUIStyle;
            GUIStyle gUIStyle1;
            GUIStyle gUIStyle2;
            if (!ModsMenu.visible)
            {
                return;
            }
            GUI.depth = this.guiDepth;
            OnGuiManagerScript.preMenu();
            if (TDLMenuCommon.Singleton.tdlSkin != null)
            {
                GUI.skin = TDLMenuCommon.Singleton.tdlSkin;
            }
            (new GUIStyle()).border = new RectOffset(20, 20, 20, 20);
            TDLMenuCommon.Singleton.fillMenuBackground();
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Space(20f);
            GUILayout.BeginVertical(new GUILayoutOption[0]);
            GUILayout.Space(100f);
            bool flag = TDLMenuCommon.Singleton.adjustForPopup(out gUIStyle, out gUIStyle1, out gUIStyle2);
            GUILayout.Label("Mods and Extras", gUIStyle1, new GUILayoutOption[0]);
            GUILayout.Space(20f);
            GUILayout.Label("Mods (In load order)", gUIStyle1, new GUILayoutOption[0]);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Space(5f);
            modListPos = GUILayout.BeginScrollView(modListPos, new GUILayoutOption[] { GUILayout.Width(600f), GUILayout.Height(200f) });
            foreach(Mod currMod in TDLPlugin.mods)
            {
                if (GUILayout.Button(currMod.GetGUIListString(), TDLMenuCommon.Singleton.tdlStyleWorldList, new GUILayoutOption[0]) && flag)
                {
                    SfxScripts.PlaySfx("menu_click");
                    //Show mod info
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
            if (GUILayout.Button("Back", gUIStyle, new GUILayoutOption[0]) && flag)
            {
                SfxScripts.PlaySfx("menu_click");
                ModsMenu.deactivateGUI();
                //MainMenu.activateGUI();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            TDLMenuCommon.Singleton.showTDLName(true);
            TDLMenuCommon.Singleton.showTDLFooter();
        }

        private void Update()
        {
            if(!ModsMenu.visible)
            {
                ModsMenu.Singleton.gameObject.GetComponent<ModsMenu>().enabled = false; //SetActive(false);
                return;
            }

            if (!TheDeadLinger.EscapeConsumed && Input.GetKeyUp(KeyCode.Escape))
            {
                TheDeadLinger.EscapeConsumed = true;
                ModsMenu.deactivateGUI();
                SfxScripts.PlaySfx("menu_click");
                //MainMenu.activateGUI();
            }
        }
    }
}
*/