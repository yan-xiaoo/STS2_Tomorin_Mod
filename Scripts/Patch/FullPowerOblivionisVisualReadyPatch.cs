using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2_Tomorin_Mod.Enemy;

namespace STS2_Tomorin_Mod.Patch;

[HarmonyPatch(typeof(NCreatureVisuals), "_Ready")]
public static class FullPowerOblivionisVisualReadyPatch
{
    private const string VisualRootName = "FullPowerOblvns";
    private const string SpineNodeName = "Visuals";
    private const string SkeletonDataPath =
        "res://STS2_Tomorin_Mod/Enemies/FullPowerOblvns/fullPowerOblvns.tres";
    private static readonly System.Reflection.MethodInfo? SetSpineBodyMethod =
        AccessTools.PropertySetter(typeof(NCreatureVisuals), nameof(NCreatureVisuals.SpineBody));

    [HarmonyPostfix]
    private static void EnsureSkeletonDataAfterReady(NCreatureVisuals __instance)
    {
        EnsureSkeletonData(__instance);
    }

    [HarmonyFinalizer]
    private static Exception? SuppressEarlySpineReadyException(NCreatureVisuals __instance, Exception? __exception)
    {
        if (__exception is not NullReferenceException)
            return __exception;

        if (__instance.Name.ToString() != VisualRootName)
            return __exception;

        EnsureSkeletonData(__instance);
        Log.Warn("[TomorinMod] Ignored early Spine skeleton readiness error for FullPowerOblvns visuals.");
        return null;
    }

    private static void EnsureSkeletonData(NCreatureVisuals visuals)
    {
        if (visuals.Name.ToString() != VisualRootName)
            return;

        var spine = visuals.SpineBody ?? CreateSpineWrapperFromChild(visuals);
        if (spine == null)
        {
            Log.Warn("[TomorinMod] FullPowerOblvns visuals has no SpineSprite child to initialize.");
            return;
        }

        if (spine.IsAnimationStateReady())
            return;

        var skeletonResource = ResourceLoader.Load(SkeletonDataPath);
        if (skeletonResource == null)
        {
            Log.Error("[TomorinMod] Failed to load FullPowerOblvns Spine skeleton data: " + SkeletonDataPath);
            return;
        }

        var skeletonData = new MegaSkeletonDataResource(Variant.From(skeletonResource));
        spine.SetSkeletonDataRes(skeletonData);
        Log.Warn("[TomorinMod] Re-applied FullPowerOblvns Spine skeleton data.");
    }

    private static MegaSprite? CreateSpineWrapperFromChild(NCreatureVisuals visuals)
    {
        var spineNode = visuals.GetNodeOrNull<Node>(SpineNodeName);
        if (spineNode == null)
            return null;

        var spine = new MegaSprite(Variant.From(spineNode));
        SetSpineBodyMethod?.Invoke(visuals, [spine]);
        return spine;
    }
}

[HarmonyPatch(typeof(NCreature), "_Ready")]
public static class FullPowerOblivionisCreatureReadyPatch
{
    [ThreadStatic] private static bool _isInitializingFullPowerOblivionis;
    private static readonly System.Reflection.MethodInfo? ConnectSpineAnimatorSignalsMethod =
        AccessTools.Method(typeof(NCreature), "ConnectSpineAnimatorSignals");
    private static readonly System.Reflection.MethodInfo? ImmediatelySetIdleMethod =
        AccessTools.Method(typeof(NCreature), "ImmediatelySetIdle");

    public static bool IsInitializingFullPowerOblivionis => _isInitializingFullPowerOblivionis;

    [HarmonyPrefix]
    private static void MarkFullPowerOblivionisReady(NCreature __instance)
    {
        _isInitializingFullPowerOblivionis = __instance.Entity?.Monster is FullPowerOblivionis;
    }

    [HarmonyFinalizer]
    private static Exception? ClearFullPowerOblivionisReady(NCreature __instance, Exception? __exception)
    {
        var isFullPowerOblivionis = _isInitializingFullPowerOblivionis ||
                                    __instance.Entity?.Monster is FullPowerOblivionis;
        _isInitializingFullPowerOblivionis = false;

        if (!isFullPowerOblivionis || !IsEarlyAnimationStateException(__exception))
            return __exception;

        RunWhenSpineReady(__instance);
        Log.Warn("[TomorinMod] Delayed FullPowerOblvns animation setup until Spine is ready.");
        return null;
    }

    private static bool IsEarlyAnimationStateException(Exception? exception)
    {
        return exception is InvalidOperationException &&
               exception.Message.StartsWith("GetAnimationState() was called before", StringComparison.Ordinal);
    }

    private static void RunWhenSpineReady(NCreature creatureNode)
    {
        var spine = creatureNode.Visuals?.SpineBody;
        if (spine == null)
            return;

        creatureNode.RunWhenSpineReady(spine, _ =>
        {
            try
            {
                ConnectSpineAnimatorSignalsMethod?.Invoke(creatureNode, null);
                ImmediatelySetIdleMethod?.Invoke(creatureNode, null);
                Log.Warn("[TomorinMod] Finished delayed FullPowerOblvns animation setup.");
            }
            catch (Exception e)
            {
                Log.Error("[TomorinMod] Failed delayed FullPowerOblvns animation setup: " + e);
            }
        });
    }
}

[HarmonyPatch(typeof(MegaSprite), nameof(MegaSprite.HasAnimation))]
public static class FullPowerOblivionisHasAnimationPatch
{
    [HarmonyFinalizer]
    private static Exception? SuppressEarlyHasAnimationException(
        MegaSprite __instance, Exception? __exception, ref bool __result)
    {
        if (__exception is not NullReferenceException)
            return __exception;

        if (!FullPowerOblivionisCreatureReadyPatch.IsInitializingFullPowerOblivionis)
            return __exception;

        __result = false;
        Log.Warn("[TomorinMod] Ignored early Spine animation readiness error for FullPowerOblvns visuals.");
        return null;
    }
}
