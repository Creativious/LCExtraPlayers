using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace LCExtraPlayers
{
    public static class Patcher  
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };
        public static bool oldPlanetHasTime;

        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly)
        {
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.FullName == "StartOfRound")
                {
                    PatchStartOfRound(type, assembly);

                }
                else if (type.FullName == "DressGirlAI")
                {
                    PatchDressGirlAI(type);

                }
                else if (type.FullName == "EnemyAI")
                {
                    PatchEnemyAI(type);
                }
                else if (type.FullName == "GameNetworkManager")
                {
                    PatchGameNetworkManager(type);
                }
                else if (type.FullName == "QuickMenuManager")
                {
                    PatchQuickMenuManager(type);
                }
                else if (type.FullName == "RoundManager")
                {
                    PatchRoundManager(type);
                }
                else if (type.FullName == "SpringManAI")
                {
                    PatchSpringManAI(type);
                }
                else if (type.FullName == "HUDManager")
                {
                    PatchHUDManager(type, assembly);
                }
                else if (type.Name == "PlayerControllerB")
                {
                    PatchPlayerControllerB(type);
                }
            }
            
            assembly.Write("new.dll"); // Creates a new compiled version of the assembly
            // Patcher code here
        }

        public static void PatchStartOfRound(TypeDefinition StartOfRound, AssemblyDefinition assembly)
        {
            if (StartOfRound == null)
            {
                return;
            }
            
            MethodInfo resizeMethodInfo = typeof(Patcher).GetMethod("ResizeArray");
            MethodReference resizeArrayMethodReference = assembly.MainModule.ImportReference(resizeMethodInfo);
            FieldInfo oldPlanetHasTimeFieldInfo = typeof(Patcher).GetField("oldPlanetHasTime");
            FieldReference oldPlanetHasTimeFieldReference = assembly.MainModule.ImportReference(oldPlanetHasTimeFieldInfo);
            
            // Handling Methods
            foreach(MethodDefinition method in StartOfRound.Methods)
            {

                if (method.Name == "SyncShipUnlockablesServerRpc")
                {
                    var processor = method.Body.GetILProcessor();
                    
                    Instruction targetInstruction = method.Body.Instructions[51];
                    Instruction otherTargetInsturction = method.Body.Instructions[70];

                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    Instruction newNewInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)50));
                    processor.Replace(targetInstruction, newNewInstruction);
                    processor.Replace(otherTargetInsturction, newInstruction);

                }
                else if (method.Name == "OnClientConnect")
                {
                    var processor = method.Body.GetILProcessor();
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)11));
                    Instruction targetInstruction = method.Body.Instructions[39];
                    processor.Replace(targetInstruction, newInstruction);
                }
                else if (method.Name == "SyncShipUnlockablesClientRpc")
                {
                    var processor = method.Body.GetILProcessor();
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    Instruction targetInstruction = method.Body.Instructions[578];
                    processor.Replace(targetInstruction, newInstruction);
                }
                else if (method.Name == ".ctor")
                {
                    var processor = method.Body.GetILProcessor();

                    Instruction targetInstruction = method.Body.Instructions[1];
                    Instruction otherTargetInsturction = method.Body.Instructions[5];

                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)50));
                    processor.Replace(targetInstruction, newInstruction);
                    processor.Replace(otherTargetInsturction, newInstruction);
                    
                }
                else if (method.Name == "Awake")
                {
                    var processor = method.Body.GetILProcessor();
                    Instruction startAfterInstruction = method.Body.Instructions[method.Body.Instructions.Count - 2];
                    Instruction[] instructions = new Instruction[105];
                    FieldReference allPlayerObjectsReference = getFieldReferenceFromNameAndType("allPlayerObjects", StartOfRound, assembly);
                    Instruction[] allPlayerObjectsInstructions = getInstructionsForResizing(processor, resizeArrayMethodReference, allPlayerObjectsReference);
                    int i = 0;
                    foreach(Instruction instruction in allPlayerObjectsInstructions)
                    {
                        instructions[i] = instruction;
                        i++;
                    }
                    
                    FieldReference allPlayerScriptsReference = getFieldReferenceFromNameAndType("allPlayerScripts", StartOfRound, assembly);
                    Instruction[] allPlayerScriptsInstructions = getInstructionsForResizing(processor, resizeArrayMethodReference, allPlayerScriptsReference);
                    foreach (Instruction instruction in allPlayerScriptsInstructions)
                    {
                        instructions[i] = instruction;
                        i++;
                    }

                    FieldReference playerSpawnPositionsReference = getFieldReferenceFromNameAndType("playerSpawnPositions", StartOfRound, assembly);
                    Instruction[] playerSpawnPositionsInstructions = getInstructionsForResizing(processor, resizeArrayMethodReference, playerSpawnPositionsReference);
                    foreach (Instruction instruction in playerSpawnPositionsInstructions)
                    {
                        instructions[i] = instruction;
                        i++;
                    }

                    FieldDefinition gameStatsDefinition = StartOfRound.Fields.First(fd => fd.Name == "gameStats");
                    FieldReference gameStatsReference = getFieldReferenceFromNameAndType("gameStats", StartOfRound, assembly);
                    TypeDefinition EndOfGameStatsDefinition = gameStatsDefinition.FieldType.Resolve();
                    FieldReference allPlayerStatsReference = getFieldReferenceFromNameAndType("allPlayerStats", EndOfGameStatsDefinition, assembly);
                    Instruction[] allPlayerStatsInstructions = getInstructionsForResizing(processor, resizeArrayMethodReference, allPlayerStatsReference);
                    int x = 0;
                    foreach (Instruction instruction in allPlayerStatsInstructions)
                    {
                        instructions[i] = instruction;
                        if (x == 1)
                        {
                            instructions[i+1] = processor.Create(OpCodes.Ldfld, gameStatsReference);
                            i++;
                        }
                        i++;
                        x++;
                    }
                    TypeDefinition playerStatsTypeDefinition = assembly.MainModule.Types.First(td => td.Name == "PlayerStats");
                    TypeReference playerStatsTypeReference = assembly.MainModule.ImportReference(playerStatsTypeDefinition);
                    MethodDefinition playerStatsTypeCtorDefinition = playerStatsTypeDefinition.Methods.First(md => md.Name == ".ctor");
                    MethodReference playerStatsTypeCtorReference = assembly.MainModule.ImportReference(playerStatsTypeCtorDefinition);

                    Instruction[] extraAwakeInstructions = getInstructionsForMoreAwakeStuff(processor, gameStatsReference, allPlayerStatsReference, playerSpawnPositionsReference, playerStatsTypeCtorReference);
                    foreach (Instruction instruction in extraAwakeInstructions)
                    {
                        instructions[i] = instruction;
                        i++;
                    }

                    for (i = 0; i < ((int)instructions.Count()); i++)
                    {
                        if (i == 0)
                        {
                            processor.InsertAfter(startAfterInstruction, instructions[0]);
                        }
                        else
                        {
                            processor.InsertAfter(instructions[i-1], instructions[i]);
                        }
                    }

                }
                
            } 

        }
        public static FieldReference getFieldReferenceFromNameAndType(String name, TypeDefinition type, AssemblyDefinition assembly)
        {
            FieldDefinition fieldDefinition = type.Fields.First(fd => fd.Name == name);
            FieldReference fieldReference = assembly.MainModule.ImportReference(fieldDefinition);
            return fieldReference;
        }
        public static Instruction[] getInstructionsForResizing(ILProcessor processor, MethodReference resizeReference, FieldReference arrayReference)
        {
            Instruction[] instructions = new Instruction[5];
            instructions[0] = processor.Create(OpCodes.Ldarg_0);
            instructions[1] = processor.Create(OpCodes.Ldarg_0);
            instructions[2] = processor.Create(OpCodes.Ldfld, arrayReference);
            instructions[3] = processor.Create(OpCodes.Call, resizeReference);
            instructions[4] = processor.Create(OpCodes.Stfld, arrayReference);
            return instructions;
        }

        public static Instruction[] getInstructionsForMoreAwakeStuff(ILProcessor processor, FieldReference gameStatsReference, FieldReference playerStatsReference, FieldReference playerSpawnPositionsReference, MethodReference playerStatsTypeCtorReference)
        {
            Instruction[] instructions = new Instruction[84];
            instructions[0] = processor.Create(OpCodes.Ldarg_0);
            instructions[1] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[2] = processor.Create(OpCodes.Ldc_I4_4);
            instructions[3] = processor.Create(OpCodes.Ldarg_0);
            instructions[4] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[5] = processor.Create(OpCodes.Ldc_I4_0);
            instructions[6] = processor.Create(OpCodes.Ldelem_I4);
            instructions[7] = processor.Create(OpCodes.Stelem_I4);

            instructions[8] = processor.Create(OpCodes.Ldarg_0);
            instructions[9] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[10] = processor.Create(OpCodes.Ldc_I4_5);
            instructions[11] = processor.Create(OpCodes.Ldarg_0);
            instructions[12] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[13] = processor.Create(OpCodes.Ldc_I4_0);
            instructions[14] = processor.Create(OpCodes.Ldelem_I4);
            instructions[15] = processor.Create(OpCodes.Stelem_I4);

            instructions[16] = processor.Create(OpCodes.Ldarg_0);
            instructions[17] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[18] = processor.Create(OpCodes.Ldc_I4_6);
            instructions[19] = processor.Create(OpCodes.Ldarg_0);
            instructions[20] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[21] = processor.Create(OpCodes.Ldc_I4_0);
            instructions[22] = processor.Create(OpCodes.Ldelem_I4);
            instructions[23] = processor.Create(OpCodes.Stelem_I4);

            instructions[24] = processor.Create(OpCodes.Ldarg_0);
            instructions[25] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[26] = processor.Create(OpCodes.Ldc_I4_7);
            instructions[27] = processor.Create(OpCodes.Ldarg_0);
            instructions[28] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[29] = processor.Create(OpCodes.Ldc_I4_0);
            instructions[30] = processor.Create(OpCodes.Ldelem_I4);
            instructions[31] = processor.Create(OpCodes.Stelem_I4);

            instructions[32] = processor.Create(OpCodes.Ldarg_0);
            instructions[33] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[34] = processor.Create(OpCodes.Ldc_I4_8);
            instructions[35] = processor.Create(OpCodes.Ldarg_0);
            instructions[36] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[37] = processor.Create(OpCodes.Ldc_I4_0);
            instructions[38] = processor.Create(OpCodes.Ldelem_I4);
            instructions[39] = processor.Create(OpCodes.Stelem_I4);

            instructions[40] = processor.Create(OpCodes.Ldarg_0);
            instructions[41] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[42] = processor.Create(OpCodes.Ldc_I4_S, ((sbyte) 9));
            instructions[43] = processor.Create(OpCodes.Ldarg_0);
            instructions[44] = processor.Create(OpCodes.Ldfld, playerSpawnPositionsReference);
            instructions[45] = processor.Create(OpCodes.Ldc_I4_0);
            instructions[46] = processor.Create(OpCodes.Ldelem_I4);
            instructions[47] = processor.Create(OpCodes.Stelem_I4);

            instructions[48] = processor.Create(OpCodes.Ldarg_0);
            instructions[49] = processor.Create(OpCodes.Ldfld, gameStatsReference);
            instructions[50] = processor.Create(OpCodes.Ldfld, playerStatsReference);
            instructions[51] = processor.Create(OpCodes.Ldc_I4_4);
            instructions[52] = processor.Create(OpCodes.Newobj, playerStatsTypeCtorReference);
            instructions[53] = processor.Create(OpCodes.Stelem_Ref);

            instructions[54] = processor.Create(OpCodes.Ldarg_0);
            instructions[55] = processor.Create(OpCodes.Ldfld, gameStatsReference);
            instructions[56] = processor.Create(OpCodes.Ldfld, playerStatsReference);
            instructions[57] = processor.Create(OpCodes.Ldc_I4_5);
            instructions[58] = processor.Create(OpCodes.Newobj, playerStatsTypeCtorReference);
            instructions[59] = processor.Create(OpCodes.Stelem_Ref);

            instructions[60] = processor.Create(OpCodes.Ldarg_0);
            instructions[61] = processor.Create(OpCodes.Ldfld, gameStatsReference);
            instructions[62] = processor.Create(OpCodes.Ldfld, playerStatsReference);
            instructions[63] = processor.Create(OpCodes.Ldc_I4_6);
            instructions[64] = processor.Create(OpCodes.Newobj, playerStatsTypeCtorReference);
            instructions[65] = processor.Create(OpCodes.Stelem_Ref);

            instructions[66] = processor.Create(OpCodes.Ldarg_0);
            instructions[67] = processor.Create(OpCodes.Ldfld, gameStatsReference);
            instructions[68] = processor.Create(OpCodes.Ldfld, playerStatsReference);
            instructions[69] = processor.Create(OpCodes.Ldc_I4_7);
            instructions[70] = processor.Create(OpCodes.Newobj, playerStatsTypeCtorReference);
            instructions[71] = processor.Create(OpCodes.Stelem_Ref);

            instructions[72] = processor.Create(OpCodes.Ldarg_0);
            instructions[73] = processor.Create(OpCodes.Ldfld, gameStatsReference);
            instructions[74] = processor.Create(OpCodes.Ldfld, playerStatsReference);
            instructions[75] = processor.Create(OpCodes.Ldc_I4_8);
            instructions[76] = processor.Create(OpCodes.Newobj, playerStatsTypeCtorReference);
            instructions[77] = processor.Create(OpCodes.Stelem_Ref);

            instructions[78] = processor.Create(OpCodes.Ldarg_0);
            instructions[79] = processor.Create(OpCodes.Ldfld, gameStatsReference);
            instructions[80] = processor.Create(OpCodes.Ldfld, playerStatsReference);
            instructions[81] = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)9));
            instructions[82] = processor.Create(OpCodes.Newobj, playerStatsTypeCtorReference);
            instructions[83] = processor.Create(OpCodes.Stelem_Ref);


            // OpCodes.Nop after this
            return instructions;
        }
        public static T[] ResizeArray<T>(T[] oldArray)
        {
            T[] array = new T[10];
            oldArray.CopyTo(array, 0);
            return array;
        }
        public static void PatchDressGirlAI(TypeDefinition DressGirlAI)
        {
            if (DressGirlAI == null)
            {
                return;
            }
            // Handling Methods
            foreach (MethodDefinition method in DressGirlAI.Methods)
            {
                if (method.Name == "ChoosePlayerToHaunt")
                {
                    var processor = method.Body.GetILProcessor();

                    Instruction targetInstruction = method.Body.Instructions[85];
                    Instruction otherTargetInsturction = method.Body.Instructions[87];
                    Instruction otherOtherTargetInstruction = method.Body.Instructions[181];

                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                    processor.Replace(otherTargetInsturction, newInstruction);
                    processor.Replace(otherOtherTargetInstruction, newInstruction);
                }
            }
        }
        public static void PatchEnemyAI(TypeDefinition EnemyAI)
        {
            if (EnemyAI == null)
            {
                return;
            }
            foreach (MethodDefinition method in EnemyAI.Methods)
            {
                if (method.Name == "GetClosestPlayer")
                {
                    var processor = method.Body.GetILProcessor();

                    Instruction targetInstruction = method.Body.Instructions[105];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                }
            }
        }
        public static void PatchGameNetworkManager(TypeDefinition GameNetworkManager)
        {
            if (GameNetworkManager == null)
            {
                return;
            }
            foreach (MethodDefinition method in GameNetworkManager.Methods)
            {
                if (method.Name == "LobbyDataIsJoinable")
                {
                    var processor = method.Body.GetILProcessor();

                    Instruction targetInstruction = method.Body.Instructions[94];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);

                }
                else if (method.Name == "ConnectionApproval")
                {
                    var processor = method.Body.GetILProcessor();

                    Instruction targetInstruction = method.Body.Instructions[51];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                }
            }
        }
        public static void PatchQuickMenuManager(TypeDefinition QuickMenuManager)
        {
            if (QuickMenuManager == null)
            {
                return;
            }
            foreach (MethodDefinition method in QuickMenuManager.Methods)
            {
                if (method.Name == "AddUserToPlayerList")
                {
                    var processor = method.Body.GetILProcessor();

                    Instruction targetInstruction = method.Body.Instructions[4];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                }
            }
        }
        public static void PatchRoundManager(TypeDefinition RoundManager)
        {
            if (RoundManager == null)
            {
                return;
            }
            foreach(MethodDefinition method in RoundManager.Methods)
            {
                if (method.Name == ".ctor")
                {
                    var processor = method.Body.GetILProcessor();

                    Instruction targetInstruction = method.Body.Instructions[28];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                }
            }
        }
        public static void PatchHUDManager(TypeDefinition HUDManager, AssemblyDefinition assembly)
        {
            if (HUDManager == null)
            {
                return;
            }
            MethodInfo resizeMethodInfo = typeof(Patcher).GetMethod("ResizeArray");
            MethodReference resizeArrayMethodReference = assembly.MainModule.ImportReference(resizeMethodInfo);
            foreach (MethodDefinition method in HUDManager.Methods)
            {
                if (method.Name == "SyncAllPlayerLevelsServerRpc")
                {
                    var processor = method.Body.GetILProcessor();
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)11));
                    Instruction targetInstruction = method.Body.Instructions[71];
                    processor.Replace(targetInstruction, newInstruction);
                    targetInstruction = method.Body.Instructions[106];
                    processor.Replace(targetInstruction, newInstruction);
                }
                else if (method.Name == "Awake")
                {
                    var processor = method.Body.GetILProcessor();
                    FieldReference playerLevelsFieldReference = getFieldReferenceFromNameAndType("playerLevels", HUDManager, assembly);
                    TypeDefinition PlayerLevelDefinition = assembly.MainModule.Types.First(td => td.Name == "PlayerLevel");
                    FieldReference playerLevelCtorReference = getFieldReferenceFromNameAndType(".ctor", PlayerLevelDefinition, assembly); // make a new method for methods reference instead, cuz I hate this :(
                    Instruction firstInstruction = method.Body.Instructions.First();

                    Instruction[] playerLevelsResizeInstructions = getInstructionsForResizing(processor, resizeArrayMethodReference, playerLevelsFieldReference); // 5 instructions
                    Instruction[] instructions = new Instruction[35];
                    int i = 0;
                    instructions[i] = playerLevelsResizeInstructions[i];
                    i++;
                    instructions[i] = playerLevelsResizeInstructions[i];
                    i++;
                    instructions[i] = playerLevelsResizeInstructions[i];
                    i++;
                    instructions[i] = playerLevelsResizeInstructions[i];
                    i++;
                    instructions[i] = playerLevelsResizeInstructions[i];
                    i++;

                    instructions[i] = processor.Create(OpCodes.Ldarg_0);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldfld, playerLevelsFieldReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldc_I4_4);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Newobj, playerLevelCtorReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Stelem_Ref);
                    i++;

                    instructions[i] = processor.Create(OpCodes.Ldarg_0);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldfld, playerLevelsFieldReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldc_I4_5);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Newobj, playerLevelCtorReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Stelem_Ref);
                    i++;

                    instructions[i] = processor.Create(OpCodes.Ldarg_0);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldfld, playerLevelsFieldReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldc_I4_6);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Newobj, playerLevelCtorReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Stelem_Ref);
                    i++;

                    instructions[i] = processor.Create(OpCodes.Ldarg_0);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldfld, playerLevelsFieldReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldc_I4_7);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Newobj, playerLevelCtorReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Stelem_Ref);
                    i++;

                    instructions[i] = processor.Create(OpCodes.Ldarg_0);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldfld, playerLevelsFieldReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldc_I4_8);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Newobj, playerLevelCtorReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Stelem_Ref);
                    i++;

                    instructions[i] = processor.Create(OpCodes.Ldarg_0);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldfld, playerLevelsFieldReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Ldc_I4_S, ((sbyte) 9));
                    i++;
                    instructions[i] = processor.Create(OpCodes.Newobj, playerLevelCtorReference);
                    i++;
                    instructions[i] = processor.Create(OpCodes.Stelem_Ref);
                    i++;


                    for (i = 0; i < ((int)instructions.Count()); i++)
                    {
                        
                        if (i == 0)
                        {
                            processor.InsertAfter(firstInstruction, instructions[0]);
                        }
                        else
                        {
                            processor.InsertAfter(instructions[i - 1], instructions[i]);
                        }
                    }



                }
                //else if (method.Name == "FillEndGameStats")
                //{
                //    var i = 0;
                //    var processor = method.Body.GetILProcessor();
                //    foreach (Instruction instruction in method.Body.Instructions)
                //    {
                //        Instruction inst = method.Body.Instructions[i];
                //        if (inst.OpCode == OpCodes.Blt)
                //        {
                //            inst.OpCode = OpCodes.Bgt;
                //            processor.Replace(method.Body.Instructions[i], inst);
                //        }
                //        i++;
                //    }

                //}
            }
        }
        public static void PatchSpringManAI(TypeDefinition SpringManAI)
        {
            if (SpringManAI == null)
            {
                return;
            }
            foreach (MethodDefinition method in SpringManAI.Methods)
            {
                if (method.Name == "DoAIInterval")
                {
                    var processor = method.Body.GetILProcessor();

                    Instruction targetInstruction = method.Body.Instructions[81];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                }
                else if (method.Name == "Update")
                {
                    var processor = method.Body.GetILProcessor();

                    Instruction targetInstruction = method.Body.Instructions[110];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                }
            }
        }
        public static void PatchPlayerControllerB(TypeDefinition PlayerControllerB)
        {
            if (PlayerControllerB == null)
            {
                return;
            }
            foreach(MethodDefinition method in PlayerControllerB.Methods)
            {
                if (method.Name == "SpectateNextPlayer")
                {
                    var processor = method.Body.GetILProcessor();
                    Instruction targetInstruction = method.Body.Instructions[59];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)9));
                    processor.Replace(targetInstruction, newInstruction);
                }
                else if (method.Name == "SendNewPlayerValuesServerRpc")
                {
                    var processor = method.Body.GetILProcessor();
                    Instruction targetInstruction = method.Body.Instructions[111];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)9));
                    processor.Replace(targetInstruction, newInstruction);
                }
                else if (method.Name == ".ctor")
                {
                    var processor = method.Body.GetILProcessor();
                    Instruction targetInstruction = method.Body.Instructions[31];
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                }
            }
        }

        public static void logMethodInstructions(MethodDefinition method)
        {
            customLogToConsole("");
            customLogToConsole("Start instructions: ");
            customLogToConsole("");
            foreach (Instruction instruction in method.Body.Instructions)
            {
                customLogToConsole(instruction.OpCode.Name + " | " + instruction.Operand);
            }
            customLogToConsole("");
            customLogToConsole("End Instructions");
        }

        public static void customLogToConsole(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        // Called before patching occurs
        public static void Initialize()
        {
            System.Diagnostics.Trace.WriteLine("LCExtraPlayers Patcher loading");
        }

        // Called after preloader has patched all assemblies and loaded them in
        // At this point it is fine to reference patched assemblies
        public static void Finish()
        {
            System.Diagnostics.Trace.WriteLine("LCExtraPlayers Patcher loaded");
        }

    }
}
