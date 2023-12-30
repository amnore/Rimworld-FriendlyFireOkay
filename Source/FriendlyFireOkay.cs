using RimWorld;
using Verse;
using HarmonyLib;
using CombatExtended;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace FriendlyFireOkay
{
    [HarmonyPatch]
    [StaticConstructorOnStartup]
    public class FriendlyFireOkay
    {
        static ConstructorInfo damageInfoConstructor = AccessTools.Constructor(typeof(DamageInfo), new Type[] { typeof(DamageDef), typeof(float), typeof(float), typeof(float), typeof(Thing), typeof(BodyPartRecord), typeof(ThingDef), typeof(DamageInfo.SourceCategory), typeof(Thing), typeof(bool), typeof(bool) });

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Faction), "Notify_MemberTookDamage")]
        static bool Faction_MemberTookDamage_Prefix(Pawn member, DamageInfo dinfo)
        {
            System.Func<Thing, string> toStr = (o) => o == null ? "(null)" : o.ToString();
            Log.Message($"Damage: from {toStr(dinfo.Instigator)}, to {toStr(member)}, intended target {toStr(dinfo.IntendedTarget)}, amount {dinfo.Amount}, def {dinfo.Def}");
            return true;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(BulletCE), nameof(BulletCE.Impact))]
        static IEnumerable<CodeInstruction> BulletCE_Impact_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
          var instigatorGuiltyGetter = AccessTools.PropertyGetter(typeof(ProjectileCE), "InstigatorGuilty");
          var intendedTargetThingGetter = AccessTools.PropertyGetter(typeof(ProjectileCE), "intendedTargetThing");
          Log.Message($"Impact: {instigatorGuiltyGetter}, {intendedTargetThingGetter}");

            /* instrucitons to match
             * IL_00f9: ldnull
             * IL_00fa: ldarg.0
             * IL_00fb: call instance bool CombatExtended.ProjectileCE::get_InstigatorGuilty()
             * IL_0100: ldc.i4.1
             * IL_007e: call instance void ['Assembly-CSharp']Verse.DamageInfo::.ctor(class ['Assembly-CSharp']Verse.DamageDef, float32, float32, float32, class ['Assembly-CSharp']Verse.Thing, class ['Assembly-CSharp']Verse.BodyPartRecord, class ['Assembly-CSharp']Verse.ThingDef, valuetype ['Assembly-CSharp']Verse.DamageInfo/SourceCategory, class ['Assembly-CSharp']Verse.Thing, bool, bool)
             */
            return new CodeMatcher(instructions)
                .MatchStartForward(new CodeMatch[]{
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Call, instigatorGuiltyGetter),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Call, damageInfoConstructor)})
                .RemoveInstruction()
                .Insert(new CodeInstruction[]{
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, intendedTargetThingGetter)})
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(SecondaryDamage), nameof(SecondaryDamage.GetDinfo), new Type[] { typeof(DamageInfo) })]
        static IEnumerable<CodeInstruction> SecondaryDamage_GetDinfo_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
          var instigatorGuiltyGetter = AccessTools.PropertyGetter(typeof(DamageInfo), "InstigatorGuilty");
          var intendedTargetGetter = AccessTools.PropertyGetter(typeof(DamageInfo), "IntendedTarget");
          Log.Message($"GetDinfo: {instigatorGuiltyGetter}, {intendedTargetGetter}");

            /* instrucitons to match
             * IL_0075: ldnull
             * IL_0076: ldarga.s primaryDinfo
             * IL_0078: call instance bool ['Assembly-CSharp']Verse.DamageInfo::get_InstigatorGuilty()
             * IL_007d: ldc.i4.1
             * IL_007e: call instance void ['Assembly-CSharp']Verse.DamageInfo::.ctor(class ['Assembly-CSharp']Verse.DamageDef, float32, float32, float32, class ['Assembly-CSharp']Verse.Thing, class ['Assembly-CSharp']Verse.BodyPartRecord, class ['Assembly-CSharp']Verse.ThingDef, valuetype ['Assembly-CSharp']Verse.DamageInfo/SourceCategory, class ['Assembly-CSharp']Verse.Thing, bool, bool)
             */
            return new CodeMatcher(instructions)
                .MatchStartForward(new CodeMatch[]{
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(OpCodes.Ldarga_S, (byte)1),
                    new CodeMatch(OpCodes.Call, instigatorGuiltyGetter),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Call, damageInfoConstructor)})
                .RemoveInstruction()
                .Insert(new CodeInstruction[]{
                    new CodeInstruction(OpCodes.Ldarga_S, (byte)1),
                    new CodeInstruction(OpCodes.Call, intendedTargetGetter)})
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ArmorUtilityCE), "GetDeflectDamageInfo")]
        static IEnumerable<CodeInstruction> ArmorUtilityCE_GetDeflectDamageInfo_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
          var instigatorGuiltyGetter = AccessTools.PropertyGetter(typeof(DamageInfo), "InstigatorGuilty");
          var intendedTargetGetter = AccessTools.PropertyGetter(typeof(DamageInfo), "IntendedTarget");
          Log.Message($"GetDeflectDamageInfo: {instigatorGuiltyGetter}, {intendedTargetGetter}");

            /* instrucitons to match
             * IL_01ec: ldnull
             * IL_01ed: ldarga.s dinfo
             * IL_01ef: call instance bool ['Assembly-CSharp']Verse.DamageInfo::get_InstigatorGuilty()
             * IL_01f4: ldc.i4.1
             * IL_01f5: call instance void ['Assembly-CSharp']Verse.DamageInfo::.ctor(class ['Assembly-CSharp']Verse.DamageDef, float32, float32, float32, class ['Assembly-CSharp']Verse.Thing, class ['Assembly-CSharp']Verse.BodyPartRecord, class ['Assembly-CSharp']Verse.ThingDef, valuetype ['Assembly-CSharp']Verse.DamageInfo/SourceCategory, class ['Assembly-CSharp']Verse.Thing, bool, bool)
             */
            return new CodeMatcher(instructions)
                .MatchStartForward(new CodeMatch[]{
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(OpCodes.Ldarga_S, (byte)0),
                    new CodeMatch(OpCodes.Call, instigatorGuiltyGetter),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Call, damageInfoConstructor)})
                .RemoveInstruction()
                .Insert(new CodeInstruction[]{
                    new CodeInstruction(OpCodes.Ldarga_S, (byte)0),
                    new CodeInstruction(OpCodes.Call, intendedTargetGetter)})
                .InstructionEnumeration();
        }

        static FriendlyFireOkay()
        {
            var harmony = new Harmony("amnore.friendlyfireokay");
            harmony.PatchAll();
            Log.Message("FriendlyFireOkay loaded");
        }
    }
}
