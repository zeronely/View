using System;
using UnityEngine;
using Object = UnityEngine.Object;


public static class LayerUtil
{
    public static readonly int Player = LayerMask.NameToLayer("Player");
    public static readonly int Monster = LayerMask.NameToLayer("Monster");
    public static readonly int Slot = LayerMask.NameToLayer("Slot");
    public static readonly int UI = LayerMask.NameToLayer("UI");
    public static readonly int Default = LayerMask.NameToLayer("Default");
    public static readonly int SkillActor = LayerMask.NameToLayer("SkillActor");
    public static readonly int FX = LayerMask.NameToLayer("Particle");
    // Player的Layer Mask
    public static readonly int PlayerMask = 1 << Player;
    // Monster的Layer Mask
    public static readonly int MonsterMask = 1 << Monster;
    // Slot的Layer Mask
    public static readonly int SlotMask = 1 << Slot;
    public static readonly int UIMask = 32;
    public static readonly int NothingMask = 0;

    public static void SetPlayerLayerRecursively(GameObject go)
    {
        SetLayerRecursively(go, Player);
    }
    public static void SetMonsterLayerRecursively(GameObject go)
    {
        SetLayerRecursively(go, Monster);
    }
    public static void SetUILayerRecursively(GameObject go)
    {
        SetLayerRecursively(go, UI);
    }

    public static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
        {
            SetLayerRecursively(t.gameObject, layer);
        }
    }
}
