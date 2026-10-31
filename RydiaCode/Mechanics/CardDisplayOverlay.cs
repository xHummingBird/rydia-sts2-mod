using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Rydia.RydiaCode.Mechanics;

public static class CardDisplayOverlay
{
    private const string NodeName = "EidolonCard_UI";
    private const string ScenePath = "res://Rydia/scenes/card_display.tscn";

    public static void Ensure(NCard card)
    {
        var model = card.Model;
        var body = card.Body;

        if (model == null || body == null)
            return;

        var node = body.GetNodeOrNull<Control>(NodeName);

        if (node == null)
        {
            var scene = GD.Load<PackedScene>(ScenePath);

            if (scene == null)
                return;

            node = scene.Instantiate<Control>();
            node.Name = NodeName;
            node.MouseFilter = Control.MouseFilterEnum.Ignore;

            body.AddChild(node);
        }

        node.Visible = model is IEidolonCard;
        node.Position = new Vector2(95f, -225f);
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class EidolonCardUiUpdate
{
    public static void Postfix(NCard __instance)
    {
        CardDisplayOverlay.Ensure(__instance);
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class EidolonCardUiReady
{
    public static void Postfix(NCard __instance)
    {
        Callable.From(() =>
        {
            CardDisplayOverlay.Ensure(__instance);
        }).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class EidolonCardUiModelChanged
{
    public static void Postfix(NCard __instance)
    {
        __instance.ModelChanged += _ =>
        {
            Callable.From(() =>
            {
                CardDisplayOverlay.Ensure(__instance);

                Callable.From(() =>
                {
                    CardDisplayOverlay.Ensure(__instance);
                }).CallDeferred();

            }).CallDeferred();
        };
    }
}