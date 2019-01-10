using System;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SwizzyMeads
{
    [HarmonyPatch(typeof(StardewValley.Object))]
    [HarmonyPatch("performObjectDropInAction")]
    [HarmonyPatch(new Type[] { typeof(StardewValley.Item), typeof(bool), typeof(StardewValley.Farmer) })]
    static class Object_PerformObjectDropInAction_Patcher
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            System.Diagnostics.Debug.WriteLine("Starting code search!");
            var instructionsList = instructions.ToList();
            var insertionIndex = -1;
            var skip = 0;
            var instructionsToInsert = new List<CodeInstruction>();

            for (int i = 0; i < instructionsList.Count; i++)
            {
                if ((instructionsList[i].opcode == OpCodes.Ldstr) && (instructionsList[i].operand as String == "Mead"))
                {
                    if (skip == 1)
                    {
                        System.Diagnostics.Debug.WriteLine("Found Mead!");
                        insertionIndex = i;
                        instructionsList[i].operand = " Mead";
                        break;
                    }
                    skip++;
                }
            }
            System.Diagnostics.Debug.WriteLine("Augmenting Instructions!");
            // ldloc.0      // object1
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0));

            // callvirt     instance string StardewValley.Item::get_Name()
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(StardewValley.Item), "get_Name")));

            if (insertionIndex != - 1)
            {
                instructionsList.InsertRange(insertionIndex, instructionsToInsert);
                instructionsToInsert.Clear();
                // call         string [mscorlib]System.String::Concat(string, string)
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(String), "Concat", new Type[] { typeof(String), typeof(String) })));
                instructionsList.InsertRange(insertionIndex + 3, instructionsToInsert);
            }

            for(int i = 0; i < instructionsList.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("IL CODE: " + instructionsList[i].opcode + "  " + instructionsList[i].operand);
            }

            return instructionsList;
        }
    }
}
