using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Flipped;

[BepInPlugin("Meadow.Flipped", "Flipped", "1.0")]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance;
    public static float height = 1;
    public static bool flipped = true;
    public static Animal localAnimal;
    private void Awake()
    {
        // Plugin startup logic
        Instance = this;
        Harmony.CreateAndPatchAll(typeof(Plugin));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
    // before the game tells the server our position and rotation, let's modify it to flip the player
    [HarmonyPatch(typeof(Fuse),nameof(Fuse.Move))]
    [HarmonyPrefix]
    public static void MovePacketHook(Fuse __instance, ref string data){
        if (!flipped) return;
        string[] splitData = data.Split(',');
        splitData[1] = (float.Parse(splitData[1])+height).ToString();
        Quaternion rotation = new Quaternion(float.Parse(splitData[3]),float.Parse(splitData[4]),float.Parse(splitData[5]),float.Parse(splitData[6]));
        rotation *= Quaternion.Euler(0,0,180);
        splitData[3] = rotation.x.ToString();
        splitData[4] = rotation.y.ToString();
        splitData[5] = rotation.z.ToString();
        splitData[6] = rotation.w.ToString();
        data = splitData.Join(delimiter: ",");
    }
    // the above modification only works for other players, so this applies the flipped transform to the local player's model
    [HarmonyPatch(typeof(Animal),nameof(Animal.OnSpawn))]
    [HarmonyPostfix]
    public static void PostAnimalSpawn(Animal __instance){
        if (!__instance.IsLocalPlayer) return;
        if (!flipped) return;
        localAnimal = __instance;
        Transform RigRoot = __instance.Animator.transform.GetChild(0);
        RigRoot.localRotation = Quaternion.Euler(0, 0, 180);
        RigRoot.localPosition = new Vector3(0,height,0);
    }

    void Update(){
        if (localAnimal == null) return;
        if (Input.GetKey(KeyCode.LeftBracket)){
            height -= 0.01f;
            UpdateLocalAnimalTransform();
        }
        if (Input.GetKey(KeyCode.RightBracket)){
            height += 0.01f;
            UpdateLocalAnimalTransform();
        }
        if (Input.GetKeyDown(KeyCode.Backslash)){
            flipped = !flipped;
            UpdateLocalAnimalTransform();
        }
    }

    static void UpdateLocalAnimalTransform(){
        Transform RigRoot = localAnimal.Animator.transform.GetChild(0);
        if (!flipped){
            RigRoot.localRotation = Quaternion.Euler(0,0,0);
            RigRoot.localPosition = Vector3.zero;
        } else {
            RigRoot.localRotation = Quaternion.Euler(0,0,180);
            RigRoot.localPosition = new Vector3(0,height,0);
        }
    }
}
