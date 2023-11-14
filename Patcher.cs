using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace LCExtraPlayers
{
    public static class Patcher  
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly)
        {
            foreach (TypeDefinition type in assembly.MainModule.Types)
            {
                if (type.FullName == "StartOfRound")
                {
                    customLogToConsole("Patching StartOfRound");
                    PatchStartOfRound(type);
                    customLogToConsole("Patched StartOfRound");

                }
                else if (type.FullName == "DressGirlAI")
                {
                    customLogToConsole("Patching DressGirlAI");
                    PatchDressGirlAI(type);
                    customLogToConsole("Patched DressGirlAI");

                }
                else if (type.FullName == "EnemyAI")
                {
                    customLogToConsole("Patching EnemyAI");
                    PatchEnemyAI(type);
                    customLogToConsole("Patched EnemyAI");
                }
                else if (type.FullName == "GameNetworkManager")
                {
                    customLogToConsole("Patching GameNetworkManager");
                    PatchGameNetworkManager(type);
                    customLogToConsole("Patched GameNetworkManager");
                }
                else if (type.FullName == "QuickMenuManager")
                {
                    customLogToConsole("Patching QuickMenuManager");
                    PatchQuickMenuManager(type);
                    customLogToConsole("Patched QuickMenuManager");
                }
                else if (type.FullName == "RoundManager")
                {
                    customLogToConsole("Patching RoundManager");
                    PatchRoundManager(type);
                    customLogToConsole("Patched RoundManager");
                }
                else if (type.FullName == "SpringManAI")
                {
                    customLogToConsole("Patching SpringManAI");
                    PatchSpringManAI(type);
                    customLogToConsole("Patched SpringManAI");
                }
            }
            
            //assembly.Write("new.dll"); // Creates a new compiled version of the assembly
            // Patcher code here
        }

        public static void PatchStartOfRound(TypeDefinition StartOfRound)
        {
            if (StartOfRound == null)
            {
                return;
            }
            // Handling Methods
            foreach(MethodDefinition method in StartOfRound.Methods)
            {

                if (method.Name == "SyncShipUnlockablesServerRpc")
                {
                    var processor = method.Body.GetILProcessor();
                    
                    Instruction targetInstruction = method.Body.Instructions[51];
                    Instruction otherTargetInsturction = method.Body.Instructions[70];

                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                    processor.Replace(otherTargetInsturction, newInstruction);

                }
                else if (method.Name == "OnClientConnect")
                {
                    var processor = method.Body.GetILProcessor();
                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
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

                    Instruction newInstruction = processor.Create(OpCodes.Ldc_I4_S, ((sbyte)10));
                    processor.Replace(targetInstruction, newInstruction);
                    processor.Replace(otherTargetInsturction, newInstruction);
                }
            } 

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
