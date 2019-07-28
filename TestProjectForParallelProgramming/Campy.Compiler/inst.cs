﻿
namespace Campy.Compiler
{
    using Campy.Graphs;
    using Campy.Meta;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;
    using Swigged.LLVM;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using Utils;

    #region INST definition
    /// <summary>
    /// Wrapper for CIL instructions that are implemented using Mono.Cecil.Cil.
    /// This class adds basic block graph structure on top of these instructions. There
    /// is no semantics encoded in the wrapper.
    /// </summary>
    public class INST
    {
        public Mono.Cecil.Cil.Instruction Instruction { get; set; }
        public Mono.Cecil.Cil.MethodBody Body { get; private set; }
        public static List<INST> CallInstructions { get; private set; } = new List<INST>();
        public override string ToString() { return Instruction.ToString(); }
        public Mono.Cecil.Cil.OpCode OpCode { get { return Instruction.OpCode; } }
        public int Offset { get { return Instruction.Offset; } }
        public object Operand { get { return Instruction.Operand; } }
        public static int instruction_id = 1;
        private static bool init = false;
        public BuilderRef Builder { get { return Block.LlvmInfo.Builder; } }
        public List<VALUE> LLVMInstructions { get; private set; }
        public CFG.Vertex Block { get; set; }
        public SequencePoint SeqPoint { get; set; }
        private static Dictionary<string, MetadataRef> debug_files = new Dictionary<string, MetadataRef>();
        private static Dictionary<string, MetadataRef> debug_compile_units = new Dictionary<string, MetadataRef>();
        private static Dictionary<string, MetadataRef> debug_methods = new Dictionary<string, MetadataRef>();
        private static Dictionary<string, MetadataRef> debug_blocks = new Dictionary<string, MetadataRef>();
        public static DIBuilderRef dib;
        private static bool done_this;
        public UInt32 TargetPointerSizeInBits = 64;
        delegate INST wrap_func(CFG.Vertex b, Mono.Cecil.Cil.Instruction i);
        static Dictionary<Mono.Cecil.Cil.Code, wrap_func> wrappers =
            new Dictionary<Mono.Cecil.Cil.Code, wrap_func>() {
                { Mono.Cecil.Cil.Code.Add,              i_add.factory },
                { Mono.Cecil.Cil.Code.Add_Ovf,          i_add_ovf.factory },
                { Mono.Cecil.Cil.Code.Add_Ovf_Un,       i_add_ovf_un.factory },
                { Mono.Cecil.Cil.Code.And,              i_and.factory },
                { Mono.Cecil.Cil.Code.Arglist,          i_arglist.factory },
                { Mono.Cecil.Cil.Code.Beq,              i_beq.factory },
                { Mono.Cecil.Cil.Code.Beq_S,            i_beq_s.factory },
                { Mono.Cecil.Cil.Code.Bge,              i_bge.factory },
                { Mono.Cecil.Cil.Code.Bge_S,            i_bge_s.factory },
                { Mono.Cecil.Cil.Code.Bge_Un,           i_bge_un.factory },
                { Mono.Cecil.Cil.Code.Bge_Un_S,         i_bge_un_s.factory },
                { Mono.Cecil.Cil.Code.Bgt,              i_bgt.factory },
                { Mono.Cecil.Cil.Code.Bgt_S,            i_bgt_s.factory },
                { Mono.Cecil.Cil.Code.Bgt_Un,           i_bgt_un.factory },
                { Mono.Cecil.Cil.Code.Bgt_Un_S,         i_bgt_un_s.factory },
                { Mono.Cecil.Cil.Code.Ble,              i_ble.factory },
                { Mono.Cecil.Cil.Code.Ble_S,            i_ble_s.factory },
                { Mono.Cecil.Cil.Code.Ble_Un,           i_ble_un.factory },
                { Mono.Cecil.Cil.Code.Ble_Un_S,         i_ble_un_s.factory },
                { Mono.Cecil.Cil.Code.Blt,              i_blt.factory },
                { Mono.Cecil.Cil.Code.Blt_S,            i_blt_s.factory },
                { Mono.Cecil.Cil.Code.Blt_Un,           i_blt_un.factory },
                { Mono.Cecil.Cil.Code.Blt_Un_S,         i_blt_un_s.factory },
                { Mono.Cecil.Cil.Code.Bne_Un,           i_bne_un.factory },
                { Mono.Cecil.Cil.Code.Bne_Un_S,         i_bne_un_s.factory },
                { Mono.Cecil.Cil.Code.Box,              i_box.factory },
                { Mono.Cecil.Cil.Code.Br,               i_br.factory },
                { Mono.Cecil.Cil.Code.Br_S,             i_br_s.factory },
                { Mono.Cecil.Cil.Code.Break,            i_break.factory },
                { Mono.Cecil.Cil.Code.Brfalse,          i_brfalse.factory },
                { Mono.Cecil.Cil.Code.Brfalse_S,        i_brfalse_s.factory },
                { Mono.Cecil.Cil.Code.Brtrue,           i_brtrue.factory },
                { Mono.Cecil.Cil.Code.Brtrue_S,         i_brtrue_s.factory },
                { Mono.Cecil.Cil.Code.Call,             i_call.factory },
                { Mono.Cecil.Cil.Code.Calli,            i_calli.factory },
                { Mono.Cecil.Cil.Code.Callvirt,         i_callvirt.factory },
                { Mono.Cecil.Cil.Code.Castclass,        i_castclass.factory },
                { Mono.Cecil.Cil.Code.Ceq,              i_ceq.factory },
                { Mono.Cecil.Cil.Code.Cgt,              i_cgt.factory },
                { Mono.Cecil.Cil.Code.Cgt_Un,           i_cgt_un.factory },
                { Mono.Cecil.Cil.Code.Ckfinite,         i_ckfinite.factory },
                { Mono.Cecil.Cil.Code.Clt,              i_clt.factory },
                { Mono.Cecil.Cil.Code.Clt_Un,           i_clt_un.factory },
                { Mono.Cecil.Cil.Code.Constrained,      i_constrained.factory },
                { Mono.Cecil.Cil.Code.Conv_I1,          i_conv_i1.factory },
                { Mono.Cecil.Cil.Code.Conv_I2,          i_conv_i2.factory },
                { Mono.Cecil.Cil.Code.Conv_I4,          i_conv_i4.factory },
                { Mono.Cecil.Cil.Code.Conv_I8,          i_conv_i8.factory },
                { Mono.Cecil.Cil.Code.Conv_I,           i_conv_i.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I1,      i_conv_ovf_i1.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I1_Un,   i_conv_ovf_i1_un.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I2,      i_conv_ovf_i2.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I2_Un,   i_conv_ovf_i2_un.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I4,      i_conv_ovf_i4.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I4_Un,   i_conv_ovf_i4_un.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I8,      i_conv_ovf_i8.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I8_Un,   i_conv_ovf_i8_un.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I,       i_conv_ovf_i.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_I_Un,    i_conv_ovf_i_un.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U1,      i_conv_ovf_u1.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U1_Un,   i_conv_ovf_u1_un.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U2,      i_conv_ovf_u2.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U2_Un,   i_conv_ovf_u2_un.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U4,      i_conv_ovf_u4.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U4_Un,   i_conv_ovf_u4_un.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U8,      i_conv_ovf_u8.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U8_Un,   i_conv_ovf_u8_un.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U,       i_conv_ovf_u.factory },
                { Mono.Cecil.Cil.Code.Conv_Ovf_U_Un,    i_conv_ovf_u_un.factory },
                { Mono.Cecil.Cil.Code.Conv_R4,          i_conv_r4.factory },
                { Mono.Cecil.Cil.Code.Conv_R8,          i_conv_r8.factory },
                { Mono.Cecil.Cil.Code.Conv_R_Un,        i_conv_r_un.factory },
                { Mono.Cecil.Cil.Code.Conv_U1,          i_conv_u1.factory },
                { Mono.Cecil.Cil.Code.Conv_U2,          i_conv_u2.factory },
                { Mono.Cecil.Cil.Code.Conv_U4,          i_conv_u4.factory },
                { Mono.Cecil.Cil.Code.Conv_U8,          i_conv_u8.factory },
                { Mono.Cecil.Cil.Code.Conv_U,           i_conv_u.factory },
                { Mono.Cecil.Cil.Code.Cpblk,            i_cpblk.factory },
                { Mono.Cecil.Cil.Code.Cpobj,            i_cpobj.factory },
                { Mono.Cecil.Cil.Code.Div,              i_div.factory },
                { Mono.Cecil.Cil.Code.Div_Un,           i_div_un.factory },
                { Mono.Cecil.Cil.Code.Dup,              i_dup.factory },
                { Mono.Cecil.Cil.Code.Endfilter,        i_endfilter.factory },
                { Mono.Cecil.Cil.Code.Endfinally,       i_endfinally.factory },
                { Mono.Cecil.Cil.Code.Initblk,          i_initblk.factory },
                { Mono.Cecil.Cil.Code.Initobj,          i_initobj.factory },
                { Mono.Cecil.Cil.Code.Isinst,           i_isinst.factory },
                { Mono.Cecil.Cil.Code.Jmp,              i_jmp.factory },
                { Mono.Cecil.Cil.Code.Ldarg,            i_ldarg.factory },
                { Mono.Cecil.Cil.Code.Ldarg_0,          i_ldarg_0.factory },
                { Mono.Cecil.Cil.Code.Ldarg_1,          i_ldarg_1.factory },
                { Mono.Cecil.Cil.Code.Ldarg_2,          i_ldarg_2.factory },
                { Mono.Cecil.Cil.Code.Ldarg_3,          i_ldarg_3.factory },
                { Mono.Cecil.Cil.Code.Ldarg_S,          i_ldarg_s.factory },
                { Mono.Cecil.Cil.Code.Ldarga,           i_ldarga.factory },
                { Mono.Cecil.Cil.Code.Ldarga_S,         i_ldarga_s.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4,           i_ldc_i4.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_0,         i_ldc_i4_0.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_1,         i_ldc_i4_1.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_2,         i_ldc_i4_2.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_3,         i_ldc_i4_3.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_4,         i_ldc_i4_4.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_5,         i_ldc_i4_5.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_6,         i_ldc_i4_6.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_7,         i_ldc_i4_7.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_8,         i_ldc_i4_8.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_M1,        i_ldc_i4_m1.factory },
                { Mono.Cecil.Cil.Code.Ldc_I4_S,         i_ldc_i4_s.factory },
                { Mono.Cecil.Cil.Code.Ldc_I8,           i_ldc_i8.factory },
                { Mono.Cecil.Cil.Code.Ldc_R4,           i_ldc_r4.factory },
                { Mono.Cecil.Cil.Code.Ldc_R8,           i_ldc_r8.factory },
                { Mono.Cecil.Cil.Code.Ldelem_Any,       i_ldelem_any.factory },
                { Mono.Cecil.Cil.Code.Ldelem_I1,        i_ldelem_i1.factory },
                { Mono.Cecil.Cil.Code.Ldelem_I2,        i_ldelem_i2.factory },
                { Mono.Cecil.Cil.Code.Ldelem_I4,        i_ldelem_i4.factory },
                { Mono.Cecil.Cil.Code.Ldelem_I8,        i_ldelem_i8.factory },
                { Mono.Cecil.Cil.Code.Ldelem_I,         i_ldelem_i.factory },
                { Mono.Cecil.Cil.Code.Ldelem_R4,        i_ldelem_r4.factory },
                { Mono.Cecil.Cil.Code.Ldelem_R8,        i_ldelem_r8.factory },
                { Mono.Cecil.Cil.Code.Ldelem_Ref,       i_ldelem_ref.factory },
                { Mono.Cecil.Cil.Code.Ldelem_U1,        i_ldelem_u1.factory },
                { Mono.Cecil.Cil.Code.Ldelem_U2,        i_ldelem_u2.factory },
                { Mono.Cecil.Cil.Code.Ldelem_U4,        i_ldelem_u4.factory },
                { Mono.Cecil.Cil.Code.Ldelema,          i_ldelema.factory },
                { Mono.Cecil.Cil.Code.Ldfld,            i_ldfld.factory },
                { Mono.Cecil.Cil.Code.Ldflda,           i_ldflda.factory },
                { Mono.Cecil.Cil.Code.Ldftn,            i_ldftn.factory },
                { Mono.Cecil.Cil.Code.Ldind_I1,         i_ldind_i1.factory },
                { Mono.Cecil.Cil.Code.Ldind_I2,         i_ldind_i2.factory },
                { Mono.Cecil.Cil.Code.Ldind_I4,         i_ldind_i4.factory },
                { Mono.Cecil.Cil.Code.Ldind_I8,         i_ldind_i8.factory },
                { Mono.Cecil.Cil.Code.Ldind_I,          i_ldind_i.factory },
                { Mono.Cecil.Cil.Code.Ldind_R4,         i_ldind_r4.factory },
                { Mono.Cecil.Cil.Code.Ldind_R8,         i_ldind_r8.factory },
                { Mono.Cecil.Cil.Code.Ldind_Ref,        i_ldind_ref.factory },
                { Mono.Cecil.Cil.Code.Ldind_U1,         i_ldind_u1.factory },
                { Mono.Cecil.Cil.Code.Ldind_U2,         i_ldind_u2.factory },
                { Mono.Cecil.Cil.Code.Ldind_U4,         i_ldind_u4.factory },
                { Mono.Cecil.Cil.Code.Ldlen,            i_ldlen.factory },
                { Mono.Cecil.Cil.Code.Ldloc,            i_ldloc.factory },
                { Mono.Cecil.Cil.Code.Ldloc_0,          i_ldloc_0.factory },
                { Mono.Cecil.Cil.Code.Ldloc_1,          i_ldloc_1.factory },
                { Mono.Cecil.Cil.Code.Ldloc_2,          i_ldloc_2.factory },
                { Mono.Cecil.Cil.Code.Ldloc_3,          i_ldloc_3.factory },
                { Mono.Cecil.Cil.Code.Ldloc_S,          i_ldloc_s.factory },
                { Mono.Cecil.Cil.Code.Ldloca,           i_ldloca.factory },
                { Mono.Cecil.Cil.Code.Ldloca_S,         i_ldloca_s.factory },
                { Mono.Cecil.Cil.Code.Ldnull,           i_ldnull.factory },
                { Mono.Cecil.Cil.Code.Ldobj,            i_ldobj.factory },
                { Mono.Cecil.Cil.Code.Ldsfld,           i_ldsfld.factory },
                { Mono.Cecil.Cil.Code.Ldsflda,          i_ldsflda.factory },
                { Mono.Cecil.Cil.Code.Ldstr,            i_ldstr.factory },
                { Mono.Cecil.Cil.Code.Ldtoken,          i_ldtoken.factory },
                { Mono.Cecil.Cil.Code.Ldvirtftn,        i_ldvirtftn.factory },
                { Mono.Cecil.Cil.Code.Leave,            i_leave.factory },
                { Mono.Cecil.Cil.Code.Leave_S,          i_leave_s.factory },
                { Mono.Cecil.Cil.Code.Localloc,         i_localloc.factory },
                { Mono.Cecil.Cil.Code.Mkrefany,         i_mkrefany.factory },
                { Mono.Cecil.Cil.Code.Mul,              i_mul.factory },
                { Mono.Cecil.Cil.Code.Mul_Ovf,          i_mul_ovf.factory },
                { Mono.Cecil.Cil.Code.Mul_Ovf_Un,       i_mul_ovf_un.factory },
                { Mono.Cecil.Cil.Code.Neg,              i_neg.factory },
                { Mono.Cecil.Cil.Code.Newarr,           i_newarr.factory },
                { Mono.Cecil.Cil.Code.Newobj,           i_newobj.factory },
                { Mono.Cecil.Cil.Code.No,               i_no.factory },
                { Mono.Cecil.Cil.Code.Nop,              i_nop.factory },
                { Mono.Cecil.Cil.Code.Not,              i_not.factory },
                { Mono.Cecil.Cil.Code.Or,               i_or.factory },
                { Mono.Cecil.Cil.Code.Pop,              i_pop.factory },
                { Mono.Cecil.Cil.Code.Readonly,         i_readonly.factory },
                { Mono.Cecil.Cil.Code.Refanytype,       i_refanytype.factory },
                { Mono.Cecil.Cil.Code.Refanyval,        i_refanyval.factory },
                { Mono.Cecil.Cil.Code.Rem,              i_rem.factory },
                { Mono.Cecil.Cil.Code.Rem_Un,           i_rem_un.factory },
                { Mono.Cecil.Cil.Code.Ret,              i_ret.factory },
                { Mono.Cecil.Cil.Code.Rethrow,          i_rethrow.factory },
                { Mono.Cecil.Cil.Code.Shl,              i_shl.factory },
                { Mono.Cecil.Cil.Code.Shr,              i_shr.factory },
                { Mono.Cecil.Cil.Code.Shr_Un,           i_shr_un.factory },
                { Mono.Cecil.Cil.Code.Sizeof,           i_sizeof.factory },
                { Mono.Cecil.Cil.Code.Starg,            i_starg.factory },
                { Mono.Cecil.Cil.Code.Starg_S,          i_starg_s.factory },
                { Mono.Cecil.Cil.Code.Stelem_Any,       i_stelem_any.factory },
                { Mono.Cecil.Cil.Code.Stelem_I1,        i_stelem_i1.factory },
                { Mono.Cecil.Cil.Code.Stelem_I2,        i_stelem_i2.factory },
                { Mono.Cecil.Cil.Code.Stelem_I4,        i_stelem_i4.factory },
                { Mono.Cecil.Cil.Code.Stelem_I8,        i_stelem_i8.factory },
                { Mono.Cecil.Cil.Code.Stelem_I,         i_stelem_i.factory },
                { Mono.Cecil.Cil.Code.Stelem_R4,        i_stelem_r4.factory },
                { Mono.Cecil.Cil.Code.Stelem_R8,        i_stelem_r8.factory },
                { Mono.Cecil.Cil.Code.Stelem_Ref,       i_stelem_ref.factory },
                { Mono.Cecil.Cil.Code.Stfld,            i_stfld.factory },
                { Mono.Cecil.Cil.Code.Stind_I1,         i_stind_i1.factory },
                { Mono.Cecil.Cil.Code.Stind_I2,         i_stind_i2.factory },
                { Mono.Cecil.Cil.Code.Stind_I4,         i_stind_i4.factory },
                { Mono.Cecil.Cil.Code.Stind_I8,         i_stind_i8.factory },
                { Mono.Cecil.Cil.Code.Stind_I,          i_stind_i.factory },
                { Mono.Cecil.Cil.Code.Stind_R4,         i_stind_r4.factory },
                { Mono.Cecil.Cil.Code.Stind_R8,         i_stind_r8.factory },
                { Mono.Cecil.Cil.Code.Stind_Ref,        i_stind_ref.factory },
                { Mono.Cecil.Cil.Code.Stloc,            i_stloc.factory },
                { Mono.Cecil.Cil.Code.Stloc_0,          i_stloc_0.factory },
                { Mono.Cecil.Cil.Code.Stloc_1,          i_stloc_1.factory },
                { Mono.Cecil.Cil.Code.Stloc_2,          i_stloc_2.factory },
                { Mono.Cecil.Cil.Code.Stloc_3,          i_stloc_3.factory },
                { Mono.Cecil.Cil.Code.Stloc_S,          i_stloc_s.factory },
                { Mono.Cecil.Cil.Code.Stobj,            i_stobj.factory },
                { Mono.Cecil.Cil.Code.Stsfld,           i_stsfld.factory },
                { Mono.Cecil.Cil.Code.Sub,              i_sub.factory },
                { Mono.Cecil.Cil.Code.Sub_Ovf,          i_sub_ovf.factory },
                { Mono.Cecil.Cil.Code.Sub_Ovf_Un,       i_sub_ovf_un.factory },
                { Mono.Cecil.Cil.Code.Switch,           i_switch.factory },
                { Mono.Cecil.Cil.Code.Tail,             i_tail.factory },
                { Mono.Cecil.Cil.Code.Throw,            i_throw.factory },
                { Mono.Cecil.Cil.Code.Unaligned,        i_unaligned.factory },
                { Mono.Cecil.Cil.Code.Unbox,            i_unbox.factory },
                { Mono.Cecil.Cil.Code.Unbox_Any,        i_unbox_any.factory },
                { Mono.Cecil.Cil.Code.Volatile,         i_volatile.factory },
                { Mono.Cecil.Cil.Code.Xor,              i_xor.factory },
          };

        static wrap_func[] wrappers_array;

        public virtual MethodReference CallTarget() { return null; }

        public virtual void DebuggerInfo()
        {
            if (Campy.Utils.Options.IsOn("debug_info_off"))
                return;

            COMPILER converter = COMPILER.Singleton;

            // Skip if no sequence point debugging information.
            //if (this.SeqPoint == null || this.SeqPoint.IsHidden)
            if (this.SeqPoint == null)
                return;

            if (!done_this)
            {
                done_this = true;
                dib = LLVM.CreateDIBuilder(RUNTIME.global_llvm_module);
            }
            var doc = SeqPoint.Document;
            string assembly_name = this.Block._method_reference.Module.FileName;
            string loc = Path.GetDirectoryName(Path.GetFullPath(doc.Url));
            string file_name = Path.GetFileName(doc.Url);
            MetadataRef file;
            if (!debug_files.ContainsKey(file_name))
            {
                file = LLVM.DIBuilderCreateFile(dib,
                    file_name, (uint)file_name.Length, loc, (uint)loc.Length);
                debug_files[file_name] = file;
            }
            else
            {
                file = debug_files[file_name];
            }

            string producer = "Campy Compiler";
            MetadataRef compile_unit;
            if (!debug_compile_units.ContainsKey(file_name))
            {
                compile_unit = LLVM.DIBuilderCreateCompileUnit(
                    dib,
                    DWARFSourceLanguage.DWARFSourceLanguageJava,
                    file, producer, (uint)producer.Length,
                    false, "", 0, 0, "", 0, DWARFEmissionKind.DWARFEmissionFull,
                    0, false, false);
                debug_compile_units[file_name] = compile_unit;
            }
            else
            {
                compile_unit = debug_compile_units[file_name];
            }

            ContextRef context_ref = LLVM.GetModuleContext(RUNTIME.global_llvm_module);
            var normalized_method_name = METAHELPER.RenameToLlvmMethodName(this.Block._method_reference.FullName);
            MetadataRef sub;
            if (!debug_methods.ContainsKey(normalized_method_name))
            {
                var sub_type = LLVM.DIBuilderCreateSubroutineType(
                    dib,
                    file, new MetadataRef[0], 0, DIFlags.DIFlagNoReturn);
                sub = LLVM.DIBuilderCreateFunction(dib, file,
                    normalized_method_name, (uint)normalized_method_name.Length,
                    normalized_method_name, (uint)normalized_method_name.Length,
                    file,
                    (uint) this.SeqPoint.StartLine,
                    sub_type,
                    true, true,
                    (uint) this.SeqPoint.StartLine, 0, false);

                debug_methods[normalized_method_name] = sub;
                LLVM.SetSubprogram(this.Block.LlvmInfo.MethodValueRef, sub);
            }
            else {
                sub = debug_methods[normalized_method_name];
            }

            MetadataRef lexical_scope;
            if (!debug_blocks.ContainsKey(this.Block.Name))
            {
                lexical_scope = LLVM.DIBuilderCreateLexicalBlock(
                    dib, sub, file,
                    (uint)this.SeqPoint.StartLine,
                    (uint)this.SeqPoint.StartColumn);
                debug_blocks[this.Block.Name] = lexical_scope;
            }
            else
            {
                lexical_scope = debug_blocks[this.Block.Name];
            }

            MetadataRef debug_location = LLVM.DIBuilderCreateDebugLocation(
                LLVM.GetModuleContext(RUNTIME.global_llvm_module),
                (uint)this.SeqPoint.StartLine,
                (uint)this.SeqPoint.StartColumn,
                lexical_scope,
                default(MetadataRef)
                );
            var dv = LLVM.MetadataAsValue(LLVM.GetModuleContext(RUNTIME.global_llvm_module), debug_location);
            LLVM.SetCurrentDebugLocation(Builder, dv);

            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine("Created debug loc " + dv);

        }

        public virtual void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            throw new Exception("Must have an implementation for CallClosure! The instruction is: "
                                + this.ToString());
        }

        public virtual unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            throw new Exception("Must have an implementation for Convert! The instruction is: "
                                + this.ToString());
        }

        protected INST(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
        {
            Instruction = i;
            if (i.OpCode.FlowControl == Mono.Cecil.Cil.FlowControl.Call)
            {
                INST.CallInstructions.Add(this);
            }
            Block = b;
        }

        static public INST Wrap(Mono.Cecil.Cil.Instruction i, CFG.Vertex block, SequencePoint sp)
        {
            // Wrap instruction with semantics, def/use/kill properties.
            Mono.Cecil.Cil.OpCode op = i.OpCode;
            INST wrapped_inst;
            if (!init)
            {
                int max = 0;
                foreach (var p in wrappers)
                {
                    max = max < (int)p.Key ? (int)p.Key : max;
                }
                wrappers_array = new wrap_func[max + 1];
                foreach (var p in wrappers)
                {
                    if (wrappers_array[(int)p.Key] != null) throw new Exception("Duplicate key?");
                    wrappers_array[(int)p.Key] = p.Value;
                }
                foreach (object item in Enum.GetValues(typeof(Mono.Cecil.Cil.Code)))
                {
                    var pp = (int)item;
                    if (wrappers_array[pp] == null) throw new Exception("Missing enum value for OpCode.");
                }
                for (int j = 0; j < max; ++j)
                {
                    for (int k = j+1; k < max; ++k)
                    {
                        if (wrappers_array[j] != null && wrappers_array[j] == wrappers_array[k]) throw new Exception("Duplicate in OpCode table.");
                    }
                }
                init = true;
            }
            var w = wrappers_array[(int)op.Code];
            wrapped_inst = w(block, i);
            wrapped_inst.SeqPoint = sp;
            return wrapped_inst;
        }
    }
    #endregion INST definition

    #region BinOp definition
    public class BinOp : INST
    {
        public BinOp(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var rhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(rhs);

            var lhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(lhs);

            var result = lhs;

            state._stack.Push(result);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var rhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(rhs);

            var lhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(lhs);

            var result = binaryOp(this.GetType(), lhs, rhs);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(result);

            state._stack.Push(result);
        }

        class BinaryInstTable
        {
            public System.Type Op;
            public Swigged.LLVM.Opcode Opcode;
            public bool IsOverflow;
            public bool IsUnsigned;

            public BinaryInstTable(System.Type ao, Swigged.LLVM.Opcode oc, bool aIsOverflow, bool aIsUnsigned)
            {
                Op = ao;
                Opcode = oc;
                IsOverflow = aIsOverflow;
                IsUnsigned = aIsUnsigned;
            }

            // Default constructor for invalid cases
            public BinaryInstTable()
            {
            }
        }

        static List<BinaryInstTable> IntMap = new List<BinaryInstTable>()
        {
            new BinaryInstTable(typeof(i_add), Opcode.Add, false, false), // ADD
            new BinaryInstTable(typeof(i_add_ovf), Opcode.Add, true, false), // ADD_OVF
            new BinaryInstTable(typeof(i_add_ovf_un), Opcode.Add, true, true), // ADD_OVF_UN
            new BinaryInstTable(typeof(i_and), Opcode.And, false, false), // AND
            new BinaryInstTable(typeof(i_div), Opcode.SDiv, false, false), // DIV
            new BinaryInstTable(typeof(i_div_un), Opcode.UDiv, false, true), // DIV_UN
            new BinaryInstTable(typeof(i_mul), Opcode.Mul, false, false), // MUL
            new BinaryInstTable(typeof(i_mul_ovf), Opcode.Mul, true, false), // MUL_OVF
            new BinaryInstTable(typeof(i_mul_ovf_un), Opcode.Mul, true, true), // MUL_OVF_UN
            new BinaryInstTable(typeof(i_or), Opcode.Or, false, false), // OR
            new BinaryInstTable(typeof(i_rem), Opcode.SRem, false, false), // REM
            new BinaryInstTable(typeof(i_rem_un), Opcode.SRem, false, true), // REM_UN
            new BinaryInstTable(typeof(i_sub), Opcode.Sub, false, false), // SUB
            new BinaryInstTable(typeof(i_sub_ovf), Opcode.Sub, true, false), // SUB_OVF
            new BinaryInstTable(typeof(i_sub_ovf_un), Opcode.Sub, true, true), // SUB_OVF_UN
            new BinaryInstTable(typeof(i_xor), Opcode.Xor, false, false) // XOR
        };

        static List<BinaryInstTable> FloatMap = new List<BinaryInstTable>()
        {
            new BinaryInstTable(typeof(i_add), Opcode.FAdd, false, false), // ADD
            new BinaryInstTable(), // ADD_OVF (invalid)
            new BinaryInstTable(), // ADD_OVF_UN (invalid)
            new BinaryInstTable(), // AND (invalid)
            new BinaryInstTable(typeof(i_div), Opcode.FDiv, false, false), // DIV
            new BinaryInstTable(), // DIV_UN (invalid)
            new BinaryInstTable(typeof(i_mul), Opcode.FMul, false, false), // MUL
            new BinaryInstTable(), // MUL_OVF (invalid)
            new BinaryInstTable(), // MUL_OVF_UN (invalid)
            new BinaryInstTable(), // OR (invalid)
            new BinaryInstTable(typeof(i_rem), Opcode.FRem, false, false), // REM
            new BinaryInstTable(), // REM_UN (invalid)
            new BinaryInstTable(typeof(i_sub), Opcode.FSub, false, false), // SUB
            new BinaryInstTable(), // SUB_OVF (invalid)
            new BinaryInstTable(), // SUB_OVF_UN (invalid)
            new BinaryInstTable(), // XOR (invalid)
        };


        TYPE binaryOpType(System.Type Opcode, TYPE Type1, TYPE Type2)
        {
            // Roughly follows ECMA-355, Table III.2.
            // If both types are floats, the result is the larger float type.
            if (Type1.isFloatingPointTy() && Type2.isFloatingPointTy())
            {
                UInt32 Size1a = Type1.getPrimitiveSizeInBits();
                UInt32 Size2a = Type2.getPrimitiveSizeInBits();
                return Size1a >= Size2a ? Type1 : Type2;
            }

            bool Type1IsInt = Type1.isIntegerTy();
            bool Type2IsInt = Type2.isIntegerTy();
            bool Type1IsPtr = Type1.isPointerTy();
            bool Type2IsPtr = Type2.isPointerTy();

            UInt32 Size1 =
                Type1IsPtr ? TargetPointerSizeInBits : Type1.getPrimitiveSizeInBits();
            UInt32 Size2 =
                Type2IsPtr ? TargetPointerSizeInBits : Type2.getPrimitiveSizeInBits();

            // If both types are integers, sizes must match, or one of the sizes must be
            // native int and the other must be smaller.
            if (Type1IsInt && Type2IsInt)
            {
                if (Size1 == Size2)
                {
                    return Type1;
                }
                if (Size1 > Size2)
                {
                    return Type1;
                }
                if (Size2 > Size1)
                {
                    return Type2;
                }
            }
            else
            {
                bool Type1IsUnmanagedPointer = false;
                bool Type2IsUnmanagedPointer = false;
                bool IsStrictlyAdd = (Opcode == typeof(i_add));
                bool IsAdd = IsStrictlyAdd || (Opcode == typeof(i_add_ovf)) ||
                             (Opcode == typeof(i_add_ovf_un));
                bool IsStrictlySub = (Opcode == typeof(i_sub));
                bool IsSub = IsStrictlySub || (Opcode == typeof(i_sub_ovf)) ||
                             (Opcode == typeof(i_sub_ovf_un));
                bool IsStrictlyAddOrSub = IsStrictlyAdd || IsStrictlySub;
                bool IsAddOrSub = IsAdd || IsSub;

                // If we see a mixture of int and unmanaged pointer, the result
                // is generally a native int, with a few special cases where we
                // preserve pointer-ness.
                if (Type1IsUnmanagedPointer || Type2IsUnmanagedPointer)
                {
                    // ptr +/- int = ptr
                    if (IsAddOrSub && Type1IsUnmanagedPointer && Type2IsInt &&
                        (Size1 >= Size2))
                    {
                        return Type1;
                    }
                    // int + ptr = ptr
                    if (IsAdd && Type1IsInt && Type2IsUnmanagedPointer && (Size2 >= Size1))
                    {
                        return Type2;
                    }
                    // Otherwise type result as native int as long as there's no truncation
                    // going on.
                    if ((Size1 <= TargetPointerSizeInBits) &&
                        (Size2 <= TargetPointerSizeInBits))
                    {
                        return new TYPE(TYPE.getIntNTy(LLVM.GetModuleContext(RUNTIME.global_llvm_module),
                            TargetPointerSizeInBits));
                    }
                }
                else if (Type1.isPointerTy())
                {
                    if (IsSub && Type2.isPointerTy())
                    {
                        // The difference of two managed pointers is a native int.
                        return new TYPE(TYPE.getIntNTy(LLVM.GetModuleContext(RUNTIME.global_llvm_module),
                            TargetPointerSizeInBits));
                    }
                    else if (IsStrictlyAddOrSub && Type2IsInt && (Size1 >= Size2))
                    {
                        // Special case for just strict add and sub: if Type1 is a managed
                        // pointer and Type2 is an integer, the result is Type1. We see the
                        // add case in some internal uses in reader base. We see the sub case
                        // in some IL stubs.
                        return Type1;
                    }
                }
            }

            // All other combinations are invalid.
            return null;
        }

        // Handle pointer + int by emitting a flattened LLVM GEP.
        VALUE genPointerAdd(VALUE Arg1, VALUE Arg2)
        {
            // Assume 1 is base and 2 is offset
            VALUE BasePtr = Arg1;
            VALUE Offset = Arg2;

            // Reconsider based on types.
            bool Arg1IsPointer = Arg1.T.isPointerTy();
            bool Arg2IsPointer = Arg2.T.isPointerTy();
            Debug.Assert(Arg1IsPointer || Arg2IsPointer);

            // Bail if both args are already pointer types.
            if (Arg1IsPointer && Arg2IsPointer)
            {
                return null;
            }

            // Swap base and offset if we got it wrong.
            if (Arg2IsPointer)
            {
                BasePtr = Arg2;
                Offset = Arg1;
            }

            // Bail if offset is not integral.
            TYPE OffsetTy = Offset.T;
            if (!OffsetTy.isIntegerTy())
            {
                return null;
            }

            // Build an LLVM GEP for the resulting address.
            // For now we "flatten" to byte offsets.

            TYPE CharPtrTy = new TYPE(
                TYPE.getInt8PtrTy(
                LLVM.GetModuleContext(RUNTIME.global_llvm_module),
                BasePtr.T.getPointerAddressSpace()));
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(CharPtrTy);

            VALUE BasePtrCast = new VALUE(LLVM.BuildBitCast(Builder, BasePtr.V, CharPtrTy.IntermediateTypeLLVM, "i"+instruction_id++));
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(BasePtrCast);

            VALUE ResultPtr = new VALUE(LLVM.BuildInBoundsGEP(Builder, BasePtrCast.V, new ValueRef[] {Offset.V}, ""));
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(ResultPtr);

            return ResultPtr;
        }

        // Handle pointer - int by emitting a flattened LLVM GEP.
        VALUE genPointerSub(VALUE Arg1, VALUE Arg2)
        {

            // Assume 1 is base and 2 is offset
            VALUE BasePtr = Arg1;
            VALUE Offset = Arg2;

            // Reconsider based on types.
            bool Arg1IsPointer = Arg1.T.isPointerTy();
            bool Arg2IsPointer = Arg2.T.isPointerTy();
            Debug.Assert(Arg1IsPointer);

            // Bail if both args are already pointer types.
            if (Arg1IsPointer && Arg2IsPointer)
            {
                return null;
            }

            // Bail if offset is not integral.
            TYPE OffsetTy = Offset.T;
            if (!OffsetTy.isIntegerTy())
            {
                return null;
            }

            // Build an LLVM GEP for the resulting address.
            // For now we "flatten" to byte offsets.
            TYPE CharPtrTy = new TYPE(TYPE.getInt8PtrTy(
                LLVM.GetModuleContext(RUNTIME.global_llvm_module), BasePtr.T.getPointerAddressSpace()));
            VALUE BasePtrCast = new VALUE(LLVM.BuildBitCast(Builder, BasePtr.V, CharPtrTy.IntermediateTypeLLVM, "i" + instruction_id++));
            VALUE NegOffset = new VALUE(LLVM.BuildNeg(Builder, Offset.V, "i" + instruction_id++));
            VALUE ResultPtr = new VALUE(LLVM.BuildGEP(Builder, BasePtrCast.V, new ValueRef[] { NegOffset.V }, "i" + instruction_id++));
            return ResultPtr;
        }

        // This method only handles basic arithmetic conversions for use in
        // binary operations.
        public VALUE convert(TYPE Ty, VALUE Node, bool SourceIsSigned)
        {
            TYPE SourceTy = Node.T;
            VALUE Result = null;

            if (Ty == SourceTy)
            {
                Result = Node;
            }
            else if (SourceTy.isIntegerTy() && Ty.isIntegerTy())
            {
                Result = new VALUE(LLVM.BuildIntCast(Builder, Node.V, Ty.IntermediateTypeLLVM, "i" + instruction_id++));//SourceIsSigned);
            }
            else if (SourceTy.isFloatingPointTy() && Ty.isFloatingPointTy())
            {
                Result = new VALUE(LLVM.BuildFPCast(Builder, Node.V, Ty.IntermediateTypeLLVM, "i" + instruction_id++));
            }
            else if (SourceTy.isPointerTy() && Ty.isIntegerTy())
            {
                Result = new VALUE(LLVM.BuildPtrToInt(Builder, Node.V, Ty.IntermediateTypeLLVM, "i" + instruction_id++));
            }
            else
            {
                Debug.Assert(false);
            }

            return Result;
        }

        VALUE binaryOp(System.Type Opcode, VALUE Arg1, VALUE Arg2)
        {
            TYPE Type1 = Arg1.T;
            TYPE Type2 = Arg2.T;
            TYPE ResultType = binaryOpType(Opcode, Type1, Type2);
            TYPE ArithType = ResultType;

            // If the result is a pointer, see if we have simple
            // pointer + int op...
            if (ResultType.isPointerTy())
            {
                if (Opcode == typeof(i_add))
                {
                    VALUE PtrAdd = genPointerAdd(Arg1, Arg2);
                    if (PtrAdd != null)
                    {
                        return PtrAdd;
                    }
                }
                else if (Opcode == typeof(i_add_ovf_un))
                {
                    VALUE PtrSub = genPointerSub(Arg1, Arg2);
                    if (PtrSub != null)
                    {
                        return PtrSub;
                    }
                }
                else if (Opcode == typeof(i_sub_ovf_un))
                { 
                    // Arithmetic with overflow must use an appropriately-sized integer to
                    // perform the arithmetic, then convert the result back to the pointer
                    // type.
                    ArithType = new TYPE(TYPE.getIntNTy(LLVM.GetModuleContext(RUNTIME.global_llvm_module), TargetPointerSizeInBits));
                }
            }

            Debug.Assert(ArithType == ResultType || ResultType.isPointerTy());

            bool IsFloat = ResultType.isFloatingPointTy();
            List<BinaryInstTable> Triple = IsFloat ? FloatMap : IntMap;

            bool IsOverflow = Triple.Where(trip => Opcode == trip.Op).Select(trip => trip.IsOverflow).First();
            bool IsUnsigned = Triple.Where(trip => Opcode == trip.Op).Select(trip => trip.IsUnsigned).First();

            if (Type1 != ArithType)
            {
                Arg1 = convert(ArithType, Arg1, !IsUnsigned);
            }

            if (Type2 != ArithType)
            {
                Arg2 = convert(ArithType, Arg2, !IsUnsigned);
            }

            VALUE Result;
            //if (IsFloat && Opcode == typeof(i_rem))
            //{
            //    // FRem must be lowered to a JIT helper call to avoid undefined symbols
            //    // during emit.
            //    //
            //    // TODO: it may be possible to delay this lowering by updating the JIT
            //    // APIs to allow the definition of a target library (via TargeLibraryInfo).
            //    CorInfoHelpFunc Helper = CORINFO_HELP_UNDEF;
            //    if (ResultType.isFloatTy())
            //    {
            //        Helper = CORINFO_HELP_FLTREM;
            //    }
            //    else if (ResultType.isDoubleTy())
            //    {
            //        Helper = CORINFO_HELP_DBLREM;
            //    }
            //    else
            //    {
            //        llvm_unreachable("Bad floating point type!");
            //    }

            //    const bool MayThrow = false;
            //    Result = (Value)callHelperImpl(Helper, MayThrow, ResultType, Arg1, Arg2)
            //    .getInstruction();
            //}
            //else
            //if (IsOverflow)
            //{
            //    // Call the appropriate intrinsic.  Its result is a pair of the arithmetic
            //    // result and a bool indicating whether the operation overflows.
            //    Value Intrinsic = Intrinsic::getDeclaration(
            //        JitContext.CurrentModule, Triple[Opcode].Op.Intrinsic, ArithType);
            //    Value[] Args = new Value[] { Arg1, Arg2 };
            //    const bool MayThrow = false;
            //    Value Pair = makeCall(Intrinsic, MayThrow, Args).getInstruction();

            //    // Extract the bool and raise an overflow exception if set.
            //    Value OvfBool = new Value(LLVM.BuildExtractValue(Builder, Pair.V, 1, "Ovf"));
            //    genConditionalThrow(OvfBool, CORINFO_HELP_OVERFLOW, "ThrowOverflow");

            //    // Extract the result.
            //    Result = new Value(LLVM.BuildExtractValue(Builder, Pair.V, 0, ""));
            //}
            //else
            {
                // Create a simple binary operation.
                BinaryInstTable OpI = Triple.Find(t => t.Op == Opcode);

                if (Opcode == typeof(i_div) ||
                    Opcode == typeof(i_div_un) ||
                    Opcode == typeof(i_rem) ||
                    Opcode == typeof(i_rem_un))
                {
                    // Integer divide and remainder throw a DivideByZeroException
                    // if the divisor is zero
                    if (UseExplicitZeroDivideChecks)
                    {
                        VALUE IsZero = new VALUE(LLVM.BuildIsNull(Builder, Arg2.V, "i" + instruction_id++));
                        //genConditionalThrow(IsZero, CORINFO_HELP_THROWDIVZERO, "ThrowDivideByZero");
                    }
                    else
                    {
                        // This configuration isn't really supported.  To support it we'd
                        // need to annotate the divide we're about to generate as possibly
                        // throwing an exception (that would be raised from a machine trap).
                    }
                }

                Result = new VALUE(LLVM.BuildBinOp(Builder, OpI.Opcode, Arg1.V, Arg2.V, "i"+instruction_id++));
            }

            if (ResultType != ArithType)
            {
                Debug.Assert(ResultType.isPointerTy());
                Debug.Assert(ArithType.isIntegerTy());

                Result = new VALUE(LLVM.BuildIntToPtr(Builder, Result.V, ResultType.IntermediateTypeLLVM, "i" + instruction_id++));
            }
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(Result);

            return Result;
        }

        public bool UseExplicitZeroDivideChecks { get; set; }
    }
    #endregion BinOp definition

    #region Call definition
    public class Call : INST
    {
        MethodReference call_closure_method = null;
        public override MethodReference CallTarget() { return call_closure_method; }

        public Call(CFG.Vertex b, Instruction i) : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            INST new_inst = this;
            object method = this.Operand;
            if (method as Mono.Cecil.MethodReference == null) throw new Exception();
            Mono.Cecil.MethodReference orig_mr = method as Mono.Cecil.MethodReference;
            var mr = orig_mr;
            bool has_this = false;
            if (mr.HasThis) has_this = true;
            if (OpCode.Code == Code.Callvirt) has_this = true;
            bool is_explicit_this = mr.ExplicitThis;
            int xargs = (has_this && !is_explicit_this ? 1 : 0) + mr.Parameters.Count;
            List<TypeReference> args = new List<TypeReference>();
            for (int k = 0; k < xargs; ++k)
            {
                var v = state._stack.Pop();
                args.Insert(0, v);
            }
            var args_array = args.ToArray();
            mr = orig_mr.SwapInBclMethod(this.Block._method_reference.DeclaringType, args_array);
            if (mr == null)
            {
                call_closure_method = orig_mr;
                return; // Can't do anything with this.
            }
            if (mr.ReturnType.FullName != "System.Void")
            {
                state._stack.Push(mr.ReturnType);
            }
            call_closure_method = mr;
            IMPORTER.Singleton().Add(mr);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var mr = call_closure_method;

            // Two general cases here: (1) Calling a method that is in CIL. (2) calling
            // a BCL method that has no CIL body.

            // Find bb entry for method.
            CFG.Vertex entry_corresponding_to_method_called = this.Block._graph.Vertices.Where(node
                =>
            {
                var g = this.Block._graph;
                CFG.Vertex v = node;
                COMPILER c = COMPILER.Singleton;
                if (v.IsEntry && v._method_reference.FullName == mr.FullName)
                    return true;
                else return false;
            }).ToList().FirstOrDefault();

            if (entry_corresponding_to_method_called == null)
            {
                // If there is no entry block discovered, so this function is probably to a BCL for GPU method.
                var name = mr.Name;
                var full_name = mr.FullName;
                // For now, look for qualified name not including parameters.
                Regex regex = new Regex(@"^[^\s]+\s+(?<name>[^\(]+).+$");
                Match m = regex.Match(full_name);
                if (!m.Success)
                    throw new Exception();
                var mangled_name = m.Groups["name"].Value;
                mangled_name = mangled_name.Replace("::", "_");
                mangled_name = mangled_name.Replace(".", "_");

                BuilderRef bu = this.Builder;

                // Find the specific function called in BCL.
                var xx = RUNTIME._bcl_runtime_csharp_internal_to_valueref.Where(t => t.Key.Contains(mangled_name) || mangled_name.Contains(t.Key));
                var first_kv_pair = xx.FirstOrDefault();
                if (first_kv_pair.Key == null)
                {
                    // No direct entry in the BCL--we don't have a direct implementation.
                    // This can happen with arrays, e.g.,
                    // "System.Void System.Int32[0...,0...]::Set(System.Int32,System.Int32,System.Int32)"
                    TypeReference declaring_type = mr.DeclaringType;
                    if (declaring_type != null && declaring_type.IsArray)
                    {
                        // Handle array calls with special code.
                        var the_array_type = declaring_type as Mono.Cecil.ArrayType;
                        TypeReference element_type = declaring_type.GetElementType();
                        Collection<ArrayDimension> dimensions = the_array_type.Dimensions;
                        var count = dimensions.Count;

                        if (mr.Name == "Set")
                        {
                            // Make "set" call
                            unsafe
                            {
                                ValueRef[] args = new ValueRef[1 // this
                                                               + 1 // indices
                                                               + 1 // val
                                ];

                                // Allocate space on stack for one value to be passed to function call.
                                var val_type = element_type.ToTypeRef();
                                var val_buffer = LLVM.BuildAlloca(Builder, val_type, "i" + instruction_id++);
                                LLVM.SetAlignment(val_buffer, 64);
                                LLVM.BuildStore(Builder, state._stack.Pop().V, val_buffer);

                                // Assign value arg for function call to set.
                                args[2] = LLVM.BuildPtrToInt(Builder, val_buffer, LLVM.Int64Type(), "i" + instruction_id++);

                                // Allocate space on stack for "count" indices, 64 bits each.
                                var ind_buffer = LLVM.BuildAlloca(Builder, LLVM.ArrayType(LLVM.Int64Type(), (uint)count), "i" + instruction_id++);
                                LLVM.SetAlignment(ind_buffer, 64);
                                var base_of_indices = LLVM.BuildPointerCast(Builder, ind_buffer, LLVM.PointerType(LLVM.Int64Type(), 0), "i" + instruction_id++);

                                // Place each value in indices array.
                                for (int i = count - 1; i >= 0; i--)
                                {
                                    VALUE index = state._stack.Pop();
                                    if (Campy.Utils.Options.IsOn("jit_trace"))
                                        System.Console.WriteLine(index);
                                    ValueRef[] id = new ValueRef[1] { LLVM.ConstInt(LLVM.Int64Type(), (ulong)i, true) };
                                    var add = LLVM.BuildInBoundsGEP(Builder, base_of_indices, id, "i" + instruction_id++);
                                    var cast = LLVM.BuildIntCast(Builder, index.V, LLVM.Int64Type(), "i" + instruction_id++);
                                    ValueRef store = LLVM.BuildStore(Builder, cast, add);
                                    if (Campy.Utils.Options.IsOn("jit_trace"))
                                        System.Console.WriteLine(new VALUE(store));
                                }

                                // Assign indices arg for function call to set.
                                args[1] = LLVM.BuildPtrToInt(Builder, ind_buffer, LLVM.Int64Type(), "i" + instruction_id++);

                                // Assign "this" array to arg for function call to set.
                                VALUE p = state._stack.Pop();
                                args[0] = LLVM.BuildPtrToInt(Builder, p.V, LLVM.Int64Type(), "i" + instruction_id++);

                                string nme = "_Z31SystemArray_StoreElementIndicesPhPyS0_";
                                var list2 = RUNTIME.PtxFunctions.ToList();
                                var f = list2.Where(t => t._mangled_name == nme).First();
                                ValueRef fv = f._valueref;
                                var call = LLVM.BuildCall(Builder, fv, args, "");
                                if (Campy.Utils.Options.IsOn("jit_trace"))
                                    System.Console.WriteLine(call.ToString());
                            }
                            return;
                        }
                        else if (mr.Name == "Get")
                        {
                            unsafe
                            {
                                ValueRef[] args = new ValueRef[1 // this
                                                               + 1 // indices
                                                               + 1 // val
                                ];

                                // Allocate space on stack for one value to be received from function call.
                                var val_type = element_type.ToTypeRef();
                                var val_buffer = LLVM.BuildAlloca(Builder, val_type, "i" + instruction_id++);
                                LLVM.SetAlignment(val_buffer, 64);

                                // Assign value arg for function call to set.
                                args[2] = LLVM.BuildPtrToInt(Builder, val_buffer, LLVM.Int64Type(), "i" + instruction_id++);

                                // Allocate space on stack for "count" indices, 64 bits each.
                                var ind_buffer = LLVM.BuildAlloca(Builder,
                                    LLVM.ArrayType(
                                    LLVM.Int64Type(),
                                    (uint)count), "i" + instruction_id++);
                                LLVM.SetAlignment(ind_buffer, 64);
                                var base_of_indices = LLVM.BuildPointerCast(Builder, ind_buffer, LLVM.PointerType(LLVM.Int64Type(), 0), "i" + instruction_id++);
                                for (int i = count - 1; i >= 0; i--)
                                {
                                    VALUE index = state._stack.Pop();
                                    if (Campy.Utils.Options.IsOn("jit_trace"))
                                        System.Console.WriteLine(index);
                                    ValueRef[] id = new ValueRef[1]
                                        {LLVM.ConstInt(LLVM.Int64Type(), (ulong) i, true)};
									var add = LLVM.BuildInBoundsGEP(Builder, base_of_indices, id, "i" + instruction_id++);
                                    var cast = LLVM.BuildIntCast(Builder, index.V, LLVM.Int64Type(), "i" + instruction_id++);
                                    ValueRef store = LLVM.BuildStore(Builder, cast, add);
                                    if (Campy.Utils.Options.IsOn("jit_trace"))
                                        System.Console.WriteLine(new VALUE(store));
                                }

                                // Assign indices arg for function call to set.
                                args[1] = LLVM.BuildPtrToInt(Builder, ind_buffer, LLVM.Int64Type(), "i" + instruction_id++);

                                // Assign "this" array to arg for function call to set.
                                VALUE p = state._stack.Pop();
                                args[0] = LLVM.BuildPtrToInt(Builder, p.V, LLVM.Int64Type(), "i" + instruction_id++);

                                string nme = "_Z30SystemArray_LoadElementIndicesPhPyS0_";
                                var list = RUNTIME.BclNativeMethods.ToList();
                                var list2 = RUNTIME.PtxFunctions.ToList();
                                var f = list2.Where(t => t._mangled_name == nme).First();
                                ValueRef fv = f._valueref;
                                var call = LLVM.BuildCall(Builder, fv, args, "");
                                if (Campy.Utils.Options.IsOn("jit_trace"))
                                    System.Console.WriteLine(call.ToString());
                                var load = LLVM.BuildLoad(Builder, val_buffer, "i" + instruction_id++);
                                var result = new VALUE(load);
                                state._stack.Push(result);
                                if (Campy.Utils.Options.IsOn("jit_trace"))
                                    System.Console.WriteLine(result);
                            }
                            return;
                        }
                    }
                    throw new Exception("Unknown, internal, function for which there is no body and no C/C++ code. "
                                        + mangled_name
                                        + " "
                                        + full_name);
                }
                else
                {

                    Mono.Cecil.MethodReturnType rt = mr.MethodReturnType;
                    Mono.Cecil.TypeReference tr = rt.ReturnType;
                    var ret = tr.FullName != "System.Void";
                    var HasScalarReturnValue = ret && !tr.IsStruct() && !tr.IsReferenceType();
                    var HasStructReturnValue = ret && tr.IsStruct() && !tr.IsReferenceType();
                    bool has_this = false;
                    if (mr.HasThis) has_this = true;

                    if (OpCode.Code == Code.Callvirt) has_this = true;
                    bool is_explicit_this = mr.ExplicitThis;
                    int xargs = (has_this && !is_explicit_this ? 1 : 0) + mr.Parameters.Count;

                    var NumberOfArguments = mr.Parameters.Count
                                            + (has_this ? 1 : 0)
                                            + (HasStructReturnValue ? 1 : 0);
                    int locals = 0;
                    var NumberOfLocals = locals;
                    int xret = (HasScalarReturnValue || HasStructReturnValue) ? 1 : 0;

                    ValueRef fv = first_kv_pair.Value;
                    var t_fun = LLVM.TypeOf(fv);
                    var t_fun_con = LLVM.GetTypeContext(t_fun);
                    var context = LLVM.GetModuleContext(RUNTIME.global_llvm_module);
                    {
                        ValueRef[] args = new ValueRef[3];

                        // Set up "this".
                        ValueRef nul = LLVM.ConstPointerNull(LLVM.PointerType(LLVM.VoidType(), 0));
                        VALUE t = new VALUE(nul);

                        // Pop all parameters and stuff into params buffer. Note, "this" and
                        // "return" are separate parameters in GPU BCL runtime C-functions,
                        // unfortunately, reminates of the DNA runtime I decided to use.
                        var entry = this.Block.Entry.LlvmInfo.BasicBlock;
                        var beginning = LLVM.GetFirstInstruction(entry);
                        //LLVM.PositionBuilderBefore(Builder, beginning);
                        var parameter_type = LLVM.ArrayType(LLVM.Int64Type(), (uint)mr.Parameters.Count);
                        var param_buffer = LLVM.BuildAlloca(Builder, parameter_type, "i" + instruction_id++);
                        LLVM.SetAlignment(param_buffer, 64);
                        //LLVM.PositionBuilderAtEnd(Builder, this.Block.BasicBlock);
                        var base_of_parameters = LLVM.BuildPointerCast(Builder, param_buffer,
                            LLVM.PointerType(LLVM.Int64Type(), 0), "i" + instruction_id++);
                        for (int i = mr.Parameters.Count - 1; i >= 0; i--)
                        {
                            VALUE p = state._stack.Pop();
                            ValueRef[] index = new ValueRef[1] { LLVM.ConstInt(LLVM.Int32Type(), (ulong)i, true) };
                            var add = LLVM.BuildInBoundsGEP(Builder, base_of_parameters, index, "i" + instruction_id++);
                            ValueRef v = LLVM.BuildPointerCast(Builder, add, LLVM.PointerType(LLVM.TypeOf(p.V), 0), "i" + instruction_id++);
                            ValueRef store = LLVM.BuildStore(Builder, p.V, v);
                            if (Campy.Utils.Options.IsOn("jit_trace"))
                                System.Console.WriteLine(new VALUE(store));
                        }

                        if (has_this)
                        {
                            t = state._stack.Pop();
                        }

                        // Set up return. For now, always allocate buffer.
                        // Note function return is type of third parameter.
                        var return_type = mr.ReturnType.ToTypeRef();
                        if (mr.ReturnType.FullName == "System.Void")
                            return_type = typeof(IntPtr).ToMonoTypeReference().ToTypeRef();
                        var return_buffer = LLVM.BuildAlloca(Builder, return_type, "i" + instruction_id++);
                        LLVM.SetAlignment(return_buffer, 64);
                        //LLVM.PositionBuilderAtEnd(Builder, this.Block.BasicBlock);

                        // Set up call.
                        var pt = LLVM.BuildPtrToInt(Builder, t.V, LLVM.Int64Type(), "i" + instruction_id++);
                        var pp = LLVM.BuildPtrToInt(Builder, param_buffer, LLVM.Int64Type(), "i" + instruction_id++);
                        var pr = LLVM.BuildPtrToInt(Builder, return_buffer, LLVM.Int64Type(), "i" + instruction_id++);

                        //var pt = LLVM.BuildPointerCast(Builder, t.V,
                        //    LLVM.PointerType(LLVM.VoidType(), 0), "i" + instruction_id++);
                        //var pp = LLVM.BuildPointerCast(Builder, param_buffer,
                        //    LLVM.PointerType(LLVM.VoidType(), 0), "i" + instruction_id++);
                        //var pr = LLVM.BuildPointerCast(Builder, return_buffer,
                        //    LLVM.PointerType(LLVM.VoidType(), 0), "i" + instruction_id++);

                        args[0] = pt;
                        args[1] = pp;
                        args[2] = pr;

                        this.DebuggerInfo();
                        var call = LLVM.BuildCall(Builder, fv, args, "");

                        if (Campy.Utils.Options.IsOn("jit_trace"))
                            System.Console.WriteLine(call.ToString());

                        if (ret)
                        {
                            var src = LLVM.BuildLoad(Builder, return_buffer, "i" + instruction_id++);

                            VALUE dst = new VALUE(src);
                            var stype = LLVM.TypeOf(src);
                            // Calls return a StorageTypeLLVM. Convert that to an IntermediateType.
                            var dtype = new TYPE(mr.ReturnType).IntermediateTypeLLVM;
                            bool is_unsigned = mr.ReturnType.IsUnsigned();
                            // Widen or trunc value.
                            if (stype != dtype)
                            {
                                bool ext = false;

                                /* Extend */
                                if (dtype == LLVM.Int64Type()
                                    && (stype == LLVM.Int32Type() || stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
                                    ext = true;
                                else if (dtype == LLVM.Int32Type()
                                    && (stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
                                    ext = true;
                                else if (dtype == LLVM.Int16Type()
                                    && (stype == LLVM.Int8Type()))
                                    ext = true;

                                if (ext)
                                    dst = new VALUE(
                                       is_unsigned
                                        ? LLVM.BuildZExt(Builder, src, dtype, "i" + instruction_id++)
                                        : LLVM.BuildSExt(Builder, src, dtype, "i" + instruction_id++));
                                else if (dtype == LLVM.DoubleType() && stype == LLVM.FloatType())
                                    dst = new VALUE(LLVM.BuildFPExt(Builder, src, dtype, "i" + instruction_id++));
                                else /* Trunc */ if (stype == LLVM.Int64Type()
                                    && (dtype == LLVM.Int32Type() || dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
                                    dst = new VALUE(LLVM.BuildTrunc(Builder, src, dtype, "i" + instruction_id++));
                                else if (stype == LLVM.Int32Type()
                                    && (dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
                                    dst = new VALUE(LLVM.BuildTrunc(Builder, src, dtype, "i" + instruction_id++));
                                else if (stype == LLVM.Int16Type()
                                    && dtype == LLVM.Int8Type())
                                    dst = new VALUE(LLVM.BuildTrunc(Builder, src, dtype, "i" + instruction_id++));
                                else if (stype == LLVM.DoubleType()
                                    && dtype == LLVM.FloatType())
                                    dst = new VALUE(LLVM.BuildFPTrunc(Builder, src, dtype, "i" + instruction_id++));

                                else if (stype == LLVM.Int64Type()
                                    && (dtype == LLVM.FloatType()))
                                    dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                                else if (stype == LLVM.Int32Type()
                                    && (dtype == LLVM.FloatType()))
                                    dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                                else if (stype == LLVM.Int64Type()
                                    && (dtype == LLVM.DoubleType()))
                                    dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                                else if (stype == LLVM.Int32Type()
                                    && (dtype == LLVM.DoubleType()))
                                    dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                            }
                            state._stack.Push(dst);
                        }
                    }
                }
            }
            else
            {
                // There is an entry block discovered for this call.
                int xret = (entry_corresponding_to_method_called.HasScalarReturnValue || entry_corresponding_to_method_called.HasStructReturnValue) ? 1 : 0;
                int xargs = entry_corresponding_to_method_called.StackNumberOfArguments;
                var name = mr.FullName;
                BuilderRef bu = this.Builder;
                ValueRef fv = entry_corresponding_to_method_called.LlvmInfo.MethodValueRef;
                var t_fun = LLVM.TypeOf(fv);
                var t_fun_con = LLVM.GetTypeContext(t_fun);
                var context = LLVM.GetModuleContext(RUNTIME.global_llvm_module);
                if (t_fun_con != context) throw new Exception("not equal");
				// Set up args, type casting if required.
                ValueRef[] args = new ValueRef[xargs];
                if (mr.ReturnType.FullName == "System.Void")
                {
                    // No return.
                    for (int k = xargs - 1; k >= 0; --k)
                    {
                        VALUE v = state._stack.Pop();
                        ValueRef par = LLVM.GetParam(fv, (uint)k);
                        ValueRef value = v.V;
                        if (LLVM.TypeOf(value) != LLVM.TypeOf(par))
                        {
                            if (LLVM.GetTypeKind(LLVM.TypeOf(par)) == TypeKind.StructTypeKind
                                && LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.PointerTypeKind)
                                value = LLVM.BuildLoad(Builder, value, "i" + instruction_id++);
                            else if (LLVM.GetTypeKind(LLVM.TypeOf(par)) == TypeKind.PointerTypeKind)
                                value = LLVM.BuildPointerCast(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
                            else if (LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.IntegerTypeKind)
                                value = LLVM.BuildIntCast(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
                            else
                                value = LLVM.BuildBitCast(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
						}

						//if (this.Block.Entry.CheckArgsAlloc(k))
						//{
						//	var a = LLVM.BuildAlloca(Builder, LLVM.TypeOf(par), "i" + instruction_id++);
						//	var store = LLVM.BuildStore(Builder, a, value);
						//	value = a;
						//}
						
                        args[k] = value;
                    }
                    this.DebuggerInfo();
                    var call = LLVM.BuildCall(Builder, fv, args, "");
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(call.ToString());
                }
                else if (entry_corresponding_to_method_called.HasStructReturnValue)
                {
                    // Special case for call with struct return. The return value is actually another
                    // parameter on the stack, which must be allocated.
                    // Further, the return for LLVM code is actually void.
                    ValueRef ret_par = LLVM.GetParam(fv, (uint)0);
                    var alloc_type = LLVM.GetElementType(LLVM.TypeOf(ret_par));

                    var entry = this.Block.Entry.LlvmInfo.BasicBlock;
                    var beginning = LLVM.GetFirstInstruction(entry);
                    //LLVM.PositionBuilderBefore(Builder, beginning);

                    var new_obj =
                        LLVM.BuildAlloca(Builder, alloc_type,
                            "i" + instruction_id++); // Allocates struct on stack, but returns a pointer to struct.
                    //LLVM.PositionBuilderAtEnd(Builder, this.Block.BasicBlock);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(new_obj));
                    args[0] = new_obj;
                    for (int k = xargs - 1; k >= 1; --k)
                    {
                        VALUE v = state._stack.Pop();
                        ValueRef par = LLVM.GetParam(fv, (uint)k);
                        ValueRef value = v.V;
                        if (LLVM.TypeOf(value) != LLVM.TypeOf(par))
                        {
                            if (LLVM.GetTypeKind(LLVM.TypeOf(par)) == TypeKind.StructTypeKind
                                && LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.PointerTypeKind)
                            {
                                value = LLVM.BuildLoad(Builder, value, "i" + instruction_id++);
                            }
                            else if (LLVM.GetTypeKind(LLVM.TypeOf(par)) == TypeKind.PointerTypeKind)
                            {
                                value = LLVM.BuildPointerCast(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
                            }
                            else
                            {
                                value = LLVM.BuildBitCast(Builder, value, LLVM.TypeOf(par), "");
                            }
						}

						//if (this.Block.Entry.CheckArgsAlloc(k))
						//{
						//	var a = LLVM.BuildAlloca(Builder, LLVM.TypeOf(par), "i" + instruction_id++);
						//	var store = LLVM.BuildStore(Builder, a, value);
						//	value = a;
						//}

                        args[k] = value;
                    }
                    this.DebuggerInfo();
                    var call = LLVM.BuildCall(Builder, fv, args, "");
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(call.ToString());
                    // Push the return on the stack. Note, it's not the call, but the new obj dereferenced.
                    var dereferenced_return_value = LLVM.BuildLoad(Builder, new_obj, "i" + instruction_id++);
                    state._stack.Push(new VALUE(dereferenced_return_value));
                }
                else if (entry_corresponding_to_method_called.HasScalarReturnValue)
                {
                    for (int k = xargs - 1; k >= 0; --k)
                    {
                        VALUE v = state._stack.Pop();
                        if (Campy.Utils.Options.IsOn("jit_trace"))
                            System.Console.WriteLine(v.ToString());
                        ValueRef par = LLVM.GetParam(fv, (uint)k);
                        ValueRef value = v.V;
                        if (Campy.Utils.Options.IsOn("jit_trace"))
                            System.Console.WriteLine(par.ToString());
                        if (LLVM.TypeOf(value) != LLVM.TypeOf(par))
                        {
                            if (LLVM.GetTypeKind(LLVM.TypeOf(par)) == TypeKind.StructTypeKind
                                && LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.PointerTypeKind)
                                value = LLVM.BuildLoad(Builder, value, "i" + instruction_id++);
                            else if (LLVM.GetTypeKind(LLVM.TypeOf(par)) == TypeKind.PointerTypeKind)
                                value = LLVM.BuildPointerCast(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
                            else if (LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.IntegerTypeKind)
                                value = LLVM.BuildIntCast(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
                            else
                                value = LLVM.BuildBitCast(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
                        }

						//if (this.Block.Entry.CheckArgsAlloc(k))
						//{
						//	var a = LLVM.BuildAlloca(Builder, LLVM.TypeOf(par), "i" + instruction_id++);
						//	var store = LLVM.BuildStore(Builder, a, value);
						//	value = a;
						//}

                        args[k] = value;
                    }
                    this.DebuggerInfo();
                    ValueRef src = LLVM.BuildCall(Builder, fv, args, "");
                    VALUE dst = new VALUE(src);
                    var stype = LLVM.TypeOf(src);
                    // Calls return a StorageTypeLLVM. Convert that to an IntermediateType.
                    var dtype = new TYPE(mr.ReturnType).IntermediateTypeLLVM;
                    bool is_unsigned = mr.ReturnType.IsUnsigned();
                    // Widen or trunc value.
                    if (stype != dtype)
                    {
                        bool ext = false;

                        /* Extend */
                        if (dtype == LLVM.Int64Type()
                            && (stype == LLVM.Int32Type() || stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
                            ext = true;
                        else if (dtype == LLVM.Int32Type()
                            && (stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
                            ext = true;
                        else if (dtype == LLVM.Int16Type()
                            && (stype == LLVM.Int8Type()))
                            ext = true;

                        if (ext)
                            dst = new VALUE(
                               is_unsigned
                                ? LLVM.BuildZExt(Builder, src, dtype, "i" + instruction_id++)
                                : LLVM.BuildSExt(Builder, src, dtype, "i" + instruction_id++));
                        else if (dtype == LLVM.DoubleType() && stype == LLVM.FloatType())
                            dst = new VALUE(LLVM.BuildFPExt(Builder, src, dtype, "i" + instruction_id++));
                        else /* Trunc */ if (stype == LLVM.Int64Type()
                            && (dtype == LLVM.Int32Type() || dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
                            dst = new VALUE(LLVM.BuildTrunc(Builder, src, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int32Type()
                            && (dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
                            dst = new VALUE(LLVM.BuildTrunc(Builder, src, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int16Type()
                            && dtype == LLVM.Int8Type())
                            dst = new VALUE(LLVM.BuildTrunc(Builder, src, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.DoubleType()
                            && dtype == LLVM.FloatType())
                            dst = new VALUE(LLVM.BuildFPTrunc(Builder, src, dtype, "i" + instruction_id++));

                        else if (stype == LLVM.Int64Type()
                            && (dtype == LLVM.FloatType()))
                            dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int32Type()
                            && (dtype == LLVM.FloatType()))
                            dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int64Type()
                            && (dtype == LLVM.DoubleType()))
                            dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int32Type()
                            && (dtype == LLVM.DoubleType()))
                            dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                    }
                    state._stack.Push(dst);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(dst.ToString());
                }
                else
                    throw new Exception("Unknown type for function.");
            }
        }
    }
    #endregion Call definition

    #region LdArg definition
    public class LdArg : INST
    {
        public int _arg;
        TypeReference call_closure_arg_type = null;

        public LdArg(CFG.Vertex b, Mono.Cecil.Cil.Instruction i, int arg = -1) : base(b, i)
        {
            _arg = arg;
            var operand = this.Operand;
            var reference = operand as ParameterDefinition;
            if (reference != null)
                _arg = reference.Sequence;
            var by_ref = this.Instruction.OpCode.Code == Code.Ldarga || this.Instruction.OpCode.Code == Code.Ldarga_S;
            CFG.Vertex entry = this.Block.Entry;
            entry.SetArgsAlloc(_arg, by_ref);
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = state._arguments[_arg];
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(v.ToString());
            var by_ref = this.Instruction.OpCode.Code == Code.Ldarga || this.Instruction.OpCode.Code == Code.Ldarga_S;
            if (by_ref) v = new ByReferenceType(v);
            call_closure_arg_type = v;
            state._stack.Push(v);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var bb = this.Block;
            var mn = bb._method_reference.FullName;
                VALUE v = state._arguments[_arg];
                CFG.Vertex entry = this.Block.Entry;
                bool use_alloca = entry.CheckArgsAlloc(_arg);
                var by_ref = this.Instruction.OpCode.Code == Code.Ldarga || this.Instruction.OpCode.Code == Code.Ldarga_S;
                if (by_ref)
                {
                    if (!use_alloca) throw new Exception("There is a load address of a local, but not compiled as such.");

                    /*
                        Within the runtime, there is a method for a struct. The "this" is a pointer to a struct. In Campy,
                        all structs are passed via pointer. So, this needs to be special cased here because it is already by reference.
                        Instead of a copy, load the value.
                            Method System.String System.NumberFormatter::FormatGeneral(System.NumberFormatter/NumberStore,System.Int32,System.Globalization.NumberFormatInfo,System.Boolean,System.Boolean) corlib.dll C:\Users\Kenne\Documents\Campy\ConsoleApp4\bin\Debug\\corlib.dll
                               HasThis   False
                               Args   5
                               Locals 21
                               Return (reuse) True
                               Edges to: 97 96
                               Instructions:
                                   IL_0000: nop    
                                   IL_0001: ldarga.s ns    
                                   IL_0003: call System.Boolean System.NumberFormatter/NumberStore::get_ZeroOnly()    
                                   IL_0008: stloc.3    
                                   IL_0009: ldloc.3    
                                   IL_000a: brfalse.s IL_0018    
                        */
                    if (this.call_closure_arg_type.IsByReference)
                    {
                        var t = call_closure_arg_type as ByReferenceType;
                        var element_type = t.ElementType;
                        if (element_type.IsStruct())
                        {
                            v = new VALUE(LLVM.BuildLoad(Builder, v.V, "i" + instruction_id++));
                        }
                    }

                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(v);
                    state._stack.Push(v);
                }
            else
            {
                if (use_alloca)
                {
                    v = new VALUE(LLVM.BuildLoad(Builder, v.V, "i" + instruction_id++));
                }

                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(v);
                state._stack.Push(v);
            }
        }
    }
    #endregion LdArg definition

    #region StArg definition
    public class StArg : INST
    {
        public int _arg;
        private TypeReference call_closure_value;
		private TypeReference call_closure_type;
		private TypeReference call_closure_parameter_type;

        public StArg(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i)
        {
            var operand = this.Operand;
            var reference = operand as ParameterDefinition;
            if (reference != null)
                _arg = reference.Sequence;
            var by_ref = true;
            CFG.Vertex entry = this.Block.Entry;
            entry.SetArgsAlloc(_arg, by_ref);
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(v);
            state._arguments[_arg] = v; // Likely should not do.
            call_closure_value = v;

			var operand = this.Operand;
            var parameter_definition = operand as ParameterDefinition;
            var parameter_type = parameter_definition.ParameterType;
			if (parameter_type == null) throw new Exception("Unknown field type");
			var f = parameter_type;
			f = f.SwapInBclType();
			f = f.Deresolve(this.Block._method_reference.DeclaringType, null);
			call_closure_parameter_type = f;
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(v);
            CFG.Vertex entry = this.Block.Entry;

            bool use_alloca = entry.CheckArgsAlloc(_arg);
            if (use_alloca)
            {

                var fixed_value = Casting.CastArg(Builder,
                    v.V, LLVM.TypeOf(v.V), call_closure_parameter_type.ToTypeRef(),
                    true);
                LLVM.BuildStore(Builder, fixed_value, state._arguments[_arg].V);
            }
            else
            {
                state._arguments[_arg] = v;
            }
        }
    }
    #endregion StArg definition

    #region LDCInstI4 definition
    public class LDCInstI4 : INST
    {
        public Int32 _arg;

        public LDCInstI4(CFG.Vertex b, Instruction i) : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var value = typeof(System.Int32).ToMonoTypeReference();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(value);

            state._stack.Push(value);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE value = new VALUE(LLVM.ConstInt(LLVM.Int32Type(), (ulong)_arg, true));
            state._stack.Push(value);
        }
    }
    #endregion LDCInstI4 definition

    #region LDCInstI8 definition
    public class LDCInstI8 : INST
    {
        public Int64 _arg;

        public LDCInstI8(CFG.Vertex b, Instruction i) : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var value = typeof(System.Int64).ToMonoTypeReference();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(value);

            state._stack.Push(value);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE value = new VALUE(LLVM.ConstInt(LLVM.Int64Type(), (ulong)_arg, true));
            state._stack.Push(value);
        }
    }
    #endregion LDCInstI8 definition

    #region LDCInstR4 definition
    public class LDCInstR4 : INST
    {
        public double _arg;

        public LDCInstR4(CFG.Vertex b, Instruction i) : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var value = typeof(System.Single).ToMonoTypeReference();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(value);

            state._stack.Push(value);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE value = new VALUE(LLVM.ConstReal(LLVM.FloatType(), _arg));
            state._stack.Push(value);
        }
    }
    #endregion LDCInstR4 definition

    #region LDCInstR8 definition
    public class LDCInstR8 : INST
    {
        public double _arg;

        public LDCInstR8(CFG.Vertex b, Instruction i) : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var value = typeof(System.Double).ToMonoTypeReference();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(value);

            state._stack.Push(value);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE value = new VALUE(LLVM.ConstReal(LLVM.DoubleType(), _arg));
            state._stack.Push(value);
        }
    }
    #endregion LDCInstR8 definition

    #region LdLoc definition
    public class LdLoc : INST
    {
        protected int _arg;
        protected TypeReference call_closure_local_type = null;
		protected bool by_ref;

        public LdLoc(CFG.Vertex b, Instruction i, int arg = -1) : base(b, i)
        {
            _arg = arg;
            var operand = this.Operand;
            var reference = operand as VariableReference;
            if (reference != null)
                _arg = reference.Index;
            var by_ref = this.Instruction.OpCode.Code == Code.Ldloca || this.Instruction.OpCode.Code == Code.Ldloca_S;
            CFG.Vertex entry = this.Block.Entry;
            entry.SetLocalsAlloc(_arg, by_ref);
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = state._locals[_arg];
            var by_ref = this.Instruction.OpCode.Code == Code.Ldloca || this.Instruction.OpCode.Code == Code.Ldloca_S;
            if (by_ref) v = new ByReferenceType(v);
            call_closure_local_type = v;
            state._stack.Push(v);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var by_ref = this.Instruction.OpCode.Code == Code.Ldloca || this.Instruction.OpCode.Code == Code.Ldloca_S;
            var v = state._locals[_arg];
            var vv = this.Block.Entry._locals[_arg];
            CFG.Vertex entry = this.Block.Entry;
            bool use_alloca = entry.CheckLocalsAlloc(_arg);
            if (by_ref)
            {
                if (!use_alloca) throw new Exception("There is a load address of a local, but not compiled as such.");
                TypeRef dtype = v.T.IntermediateTypeLLVM;
                TypeRef stype = v.T.CilTypeLLVM;
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(v);
                state._stack.Push(v);
            }
            else
            {
                if (use_alloca)
                {
                    v = new VALUE(LLVM.BuildLoad(Builder, v.V, "i" + instruction_id++));
                    TYPE st = new TYPE(vv);
                    TypeRef stype = st.CilTypeLLVM;
                    TypeRef dtype = st.IntermediateTypeLLVM;
                    if (stype != dtype)
                    {
                        bool ext = false;

                        /* Extend */
                        if (dtype == LLVM.Int64Type()
                            && (stype == LLVM.Int32Type() || stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
                            ext = true;
                        else if (dtype == LLVM.Int32Type()
                            && (stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
                            ext = true;
                        else if (dtype == LLVM.Int16Type()
                            && (stype == LLVM.Int8Type()))
                            ext = true;

                        if (ext)
                            v = new VALUE(
                                LLVM.BuildZExt(Builder, v.V, dtype, "i" + instruction_id++));
                        else if (dtype == LLVM.DoubleType() && stype == LLVM.FloatType())
                            v = new VALUE(LLVM.BuildFPExt(Builder, v.V, dtype, "i" + instruction_id++));
                        else /* Trunc */ if (stype == LLVM.Int64Type()
                            && (dtype == LLVM.Int32Type() || dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
                            v = new VALUE(LLVM.BuildTrunc(Builder, v.V, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int32Type()
                            && (dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
                            v = new VALUE(LLVM.BuildTrunc(Builder, v.V, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int16Type()
                            && dtype == LLVM.Int8Type())
                            v = new VALUE(LLVM.BuildTrunc(Builder, v.V, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.DoubleType()
                            && dtype == LLVM.FloatType())
                            v = new VALUE(LLVM.BuildFPTrunc(Builder, v.V, dtype, "i" + instruction_id++));

                        else if (stype == LLVM.Int64Type()
                            && (dtype == LLVM.FloatType()))
                            v = new VALUE(LLVM.BuildSIToFP(Builder, v.V, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int32Type()
                            && (dtype == LLVM.FloatType()))
                            v = new VALUE(LLVM.BuildSIToFP(Builder, v.V, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int64Type()
                            && (dtype == LLVM.DoubleType()))
                            v = new VALUE(LLVM.BuildSIToFP(Builder, v.V, dtype, "i" + instruction_id++));
                        else if (stype == LLVM.Int32Type()
                            && (dtype == LLVM.DoubleType()))
                            v = new VALUE(LLVM.BuildSIToFP(Builder, v.V, dtype, "i" + instruction_id++));

                        else if (LLVM.GetTypeKind(stype) == TypeKind.PointerTypeKind)
                            v = new VALUE(LLVM.BuildPointerCast(Builder, v.V, dtype, "i" + instruction_id++));
                    }
                }
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(v);
                state._stack.Push(v);
            }
        }
    }
    #endregion LdLoc definition

    #region StLoc definition
    public class StLoc : INST
    {
        public int _arg;
        protected TypeReference call_closure_local_type = null;
        protected bool by_ref;

        public StLoc(CFG.Vertex b, Instruction i, int arg = -1) : base(b, i)
        {
            _arg = arg;
            var operand = this.Operand;
            var reference = operand as VariableReference;
            if (reference != null)
                _arg = reference.Index;
            var by_ref = true;
            CFG.Vertex entry = this.Block.Entry;
            entry.SetLocalsAlloc(_arg, by_ref);
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = state._stack.Pop();
            call_closure_local_type = v;
            // do not erase. state._locals[_arg] = v;
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // Note, see page 369 of the ECMA.
            // It notes that locals are converted from the presumably full type to
            // the Intermediate Type. That means the type must be the CilTypeLLVM.
            var v = state._stack.Pop();
            CFG.Vertex entry = this.Block.Entry;
            bool use_alloca = entry.CheckLocalsAlloc(_arg);
            if (use_alloca)
            {
                TypeRef stype = v.T.IntermediateTypeLLVM;
                VALUE src = v;
                VALUE dst = state._locals[_arg];
                TypeRef dtype = LLVM.TypeOf(dst.V);
                dtype = LLVM.GetElementType(dtype);
                src = new VALUE(Casting.CastArg(Builder, src.V, stype, dtype, true));                
                LLVM.BuildStore(Builder, src.V, dst.V);
            }
            else
            {
                state._locals[_arg] = v;
            }
        }
    }
    #endregion StLoc definition

    #region Cmp definition
    public class Cmp : INST
    {
        TypeReference call_closure_lhs = null;
        TypeReference call_closure_rhs = null;

        public Cmp(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i)
        {
        }

        public enum PredicateType
        {
            eq,
            ne,
            gt,
            lt,
            ge,
            le,
        };

        public Swigged.LLVM.IntPredicate[] _int_pred = new Swigged.LLVM.IntPredicate[]
        {
            Swigged.LLVM.IntPredicate.IntEQ,
            Swigged.LLVM.IntPredicate.IntNE,
            Swigged.LLVM.IntPredicate.IntSGT,
            Swigged.LLVM.IntPredicate.IntSLT,
            Swigged.LLVM.IntPredicate.IntSGE,
            Swigged.LLVM.IntPredicate.IntSLE,
        };

        public Swigged.LLVM.IntPredicate[] _uint_pred = new Swigged.LLVM.IntPredicate[]
        {
            Swigged.LLVM.IntPredicate.IntEQ,
            Swigged.LLVM.IntPredicate.IntNE,
            Swigged.LLVM.IntPredicate.IntUGT,
            Swigged.LLVM.IntPredicate.IntULT,
            Swigged.LLVM.IntPredicate.IntUGE,
            Swigged.LLVM.IntPredicate.IntULE,
        };

        public Swigged.LLVM.RealPredicate[] _real_pred = new Swigged.LLVM.RealPredicate[]
        {
            Swigged.LLVM.RealPredicate.RealOEQ,
            Swigged.LLVM.RealPredicate.RealONE,
            Swigged.LLVM.RealPredicate.RealOGT,
            Swigged.LLVM.RealPredicate.RealOLT,
            Swigged.LLVM.RealPredicate.RealOGE,
            Swigged.LLVM.RealPredicate.RealOLE,
        };

        public Swigged.LLVM.RealPredicate[] _ureal_pred = new Swigged.LLVM.RealPredicate[]
        {
            Swigged.LLVM.RealPredicate.RealUEQ,
            Swigged.LLVM.RealPredicate.RealUNE,
            Swigged.LLVM.RealPredicate.RealUGT,
            Swigged.LLVM.RealPredicate.RealULT,
            Swigged.LLVM.RealPredicate.RealUGE,
            Swigged.LLVM.RealPredicate.RealULE,
        };

        public virtual PredicateType Predicate { get; set; }
        public virtual bool IsSigned { get; set; }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v2 = state._stack.Pop();
            var v1 = state._stack.Pop();
            call_closure_lhs = v1;
            call_closure_rhs = v2;
            state._stack.Push(typeof(Int32).ToMonoTypeReference().SwapInBclType());
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            // NB: the result of comparisons is a 32-bit quantity, not a bool
            // It must be 32 bits because that is what the spec says.
            // ceq instruction -- page 346 of ecma

            VALUE v2 = state._stack.Pop();
            VALUE v1 = state._stack.Pop();
            // TODO Undoubtably, this will be much more complicated than my initial stab.
            TYPE t1 = v1.T;
            TYPE t2 = v2.T;
            ValueRef v1_v = v1.V;
            ValueRef v2_v = v2.V;
            ValueRef cmp = default(ValueRef);
            // Deal with various combinations of types.
            if (t1.isIntegerTy() && t2.isIntegerTy())
            {
                var t1_t = t1.IntermediateTypeLLVM;
                var t2_t = t2.IntermediateTypeLLVM;
                var w1 = LLVM.GetIntTypeWidth(t1_t);
                var w2 = LLVM.GetIntTypeWidth(t2_t);
                var s1 = !call_closure_lhs.Name.Contains("UInt");
                var s2 = !call_closure_rhs.Name.Contains("UInt");
                if (w1 != w2 && s1 != s2) throw new Exception("Sign extention not the same?");
                if (w1 > w2)
                {
                    if (s1)
                        v2_v = LLVM.BuildSExt(Builder, v2_v, t1_t, "i" + instruction_id++);
                    else
                        v2_v = LLVM.BuildZExt(Builder, v2_v, t1_t, "i" + instruction_id++);
                }
                else if (w1 < w2)
                {
                    if (s1)
                        v1_v = LLVM.BuildSExt(Builder, v1_v, t2_t, "i" + instruction_id++);
                    else
                        v1_v = LLVM.BuildZExt(Builder, v1_v, t2_t, "i" + instruction_id++);
                }
                IntPredicate op;
                if (IsSigned) op = _int_pred[(int) Predicate];
                else op = _uint_pred[(int) Predicate];
                cmp = LLVM.BuildICmp(Builder, op, v1_v, v2_v, "i" + instruction_id++);
                // Set up for push of 0/1.
                var return_type = new TYPE(typeof(bool));
                var ret_llvm = LLVM.BuildZExt(Builder, cmp, return_type.IntermediateTypeLLVM, "");
                var ret = new VALUE(ret_llvm, return_type);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(ret);
                state._stack.Push(ret);
            }
            else if (t1.isPointerTy() && t2.isPointerTy())
            {
                // Cast pointers to integer, then compare.
                var i1 = LLVM.BuildPtrToInt(Builder, v1.V, LLVM.Int64Type(), "i" + instruction_id++);
                var i2 = LLVM.BuildPtrToInt(Builder, v2.V, LLVM.Int64Type(), "i" + instruction_id++);
                IntPredicate op;
                if (IsSigned) op = _int_pred[(int)Predicate];
                else op = _uint_pred[(int)Predicate];
                cmp = LLVM.BuildICmp(Builder, op, i1, i2, "i" + instruction_id++);
                // Set up for push of 0/1.
                var return_type = new TYPE(typeof(bool));
                var ret_llvm = LLVM.BuildZExt(Builder, cmp, return_type.IntermediateTypeLLVM, "");
                var ret = new VALUE(ret_llvm, return_type);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(ret);
                state._stack.Push(ret);
            }
            else if (t1.isFloatingPointTy() && t2.isFloatingPointTy())
            {
                var t1_t = t1.IntermediateTypeLLVM;
                var t2_t = t2.IntermediateTypeLLVM;
                var w1 = t1_t == LLVM.DoubleType() ? 2 : 1;
                var w2 = t2_t == LLVM.DoubleType() ? 2 : 1;
                if (w1 != w2)
                    throw new Exception("Can't compare float and double--I don't know how to in LLVM!");
                RealPredicate op;
                if (IsSigned) op = _real_pred[(int)Predicate];
                else op = _ureal_pred[(int)Predicate];
                cmp = LLVM.BuildFCmp(Builder, op, v1_v, v2_v, "i" + instruction_id++);
                // Set up for push of 0/1.
                var return_type = new TYPE(typeof(bool));
                var ret_llvm = LLVM.BuildZExt(Builder, cmp, return_type.IntermediateTypeLLVM, "");
                var ret = new VALUE(ret_llvm, return_type);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(ret);
                state._stack.Push(ret);
            }
            else
                throw new Exception("Unhandled binary operation for given types. "
                                    + t1 + " " + t2);
        }
    }
    #endregion Cmp definition

    #region CmpBr definition
    public class CmpBr : INST
    {
        TypeReference call_closure_lhs = null;
        TypeReference call_closure_rhs = null;

        public CmpBr(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i)
        {
        }

        public enum PredicateType
        {
            eq,
            ne,
            gt,
            lt,
            ge,
            le,
        };

        public Swigged.LLVM.IntPredicate[] _int_pred = new Swigged.LLVM.IntPredicate[]
        {
            Swigged.LLVM.IntPredicate.IntEQ,
            Swigged.LLVM.IntPredicate.IntNE,
            Swigged.LLVM.IntPredicate.IntSGT,
            Swigged.LLVM.IntPredicate.IntSLT,
            Swigged.LLVM.IntPredicate.IntSGE,
            Swigged.LLVM.IntPredicate.IntSLE,
        };

        public Swigged.LLVM.IntPredicate[] _uint_pred = new Swigged.LLVM.IntPredicate[]
        {
            Swigged.LLVM.IntPredicate.IntEQ,
            Swigged.LLVM.IntPredicate.IntNE,
            Swigged.LLVM.IntPredicate.IntUGT,
            Swigged.LLVM.IntPredicate.IntULT,
            Swigged.LLVM.IntPredicate.IntUGE,
            Swigged.LLVM.IntPredicate.IntULE,
        };

        public Swigged.LLVM.RealPredicate[] _real_pred = new Swigged.LLVM.RealPredicate[]
        {
            Swigged.LLVM.RealPredicate.RealOEQ,
            Swigged.LLVM.RealPredicate.RealONE,
            Swigged.LLVM.RealPredicate.RealOGT,
            Swigged.LLVM.RealPredicate.RealOLT,
            Swigged.LLVM.RealPredicate.RealOGE,
            Swigged.LLVM.RealPredicate.RealOLE,
        };

        public virtual PredicateType Predicate { get; set; }
        public virtual bool IsSigned { get; set; }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v2 = state._stack.Pop();
            var v1 = state._stack.Pop();
            call_closure_lhs = v1;
            call_closure_rhs = v2;
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE v2 = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(v2);
            VALUE v1 = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(v1);
            // TODO Undoubtably, this will be much more complicated than my initial stab.
            TYPE t1 = v1.T;
            TYPE t2 = v2.T;
            ValueRef v1_v = v1.V;
            ValueRef v2_v = v2.V;
            ValueRef cmp = default(ValueRef);
            // Deal with various combinations of types.
            if (t1.isIntegerTy() && t2.isIntegerTy())
            {
                var t1_t = t1.IntermediateTypeLLVM;
                var t2_t = t2.IntermediateTypeLLVM;
                var w1 = LLVM.GetIntTypeWidth(t1_t);
                var w2 = LLVM.GetIntTypeWidth(t2_t);
                var ss1 = !call_closure_lhs.Name.Contains("UInt");
                var ss2 = !call_closure_rhs.Name.Contains("UInt");
                if (w1 != w2 && ss1 != ss2) throw new Exception("Sign extention not the same?");
                if (w1 > w2)
                {
                    if (ss1)
                        v2_v = LLVM.BuildSExt(Builder, v2_v, t1_t, "i" + instruction_id++);
                    else
                        v2_v = LLVM.BuildZExt(Builder, v2_v, t1_t, "i" + instruction_id++);
                }
                else if (w1 < w2)
                {
                    if (ss1)
                        v1_v = LLVM.BuildSExt(Builder, v1_v, t2_t, "i" + instruction_id++);
                    else
                        v1_v = LLVM.BuildZExt(Builder, v1_v, t2_t, "i" + instruction_id++);
                }
                IntPredicate op;
                if (IsSigned) op = _int_pred[(int)Predicate];
                else op = _uint_pred[(int)Predicate];
                cmp = LLVM.BuildICmp(Builder, op, v1_v, v2_v, "i" + instruction_id++);

                var edge1 = Block._graph.SuccessorEdges(Block).ToList()[0];
                var edge2 = Block._graph.SuccessorEdges(Block).ToList()[1];
                var s1 = edge1.To;
                var s2 = edge2.To;
                // Now, in order to select the correct branch, we need to know what
                // edge represents the "true" branch. During construction, there is
                // no guarentee that the order is consistent.
                var owner = Block._graph.Vertices.Where(
                    n => n.Instructions.Where(ins =>
                    {
                        if (n.Entry._method_reference != Block.Entry._method_reference)
                            return false;
                        if (ins.Instruction.Offset != this.Instruction.Offset)
                            return false;
                        return true;
                    }).Any()).ToList();
                if (owner.Count() != 1)
                    throw new Exception("Cannot find instruction!");
                CFG.Vertex true_node = owner.FirstOrDefault();
                if (s2 == true_node)
                {
                    s1 = s2;
                    s2 = true_node;
                }
                LLVM.BuildCondBr(Builder, cmp, s1.LlvmInfo.BasicBlock, s2.LlvmInfo.BasicBlock);
                return;
            }
            if (t1.isFloatingPointTy() && t2.isFloatingPointTy())
            {
                RealPredicate op;
                if (IsSigned) op = _real_pred[(int)Predicate];
                else op = _real_pred[(int)Predicate];

                cmp = LLVM.BuildFCmp(Builder, op, v1.V, v2.V, "i" + instruction_id++);

                var edge1 = Block._graph.SuccessorEdges(Block).ToList()[0];
                var edge2 = Block._graph.SuccessorEdges(Block).ToList()[1];
                var s1 = edge1.To;
                var s2 = edge2.To;
                // Now, in order to select the correct branch, we need to know what
                // edge represents the "true" branch. During construction, there is
                // no guarentee that the order is consistent.
                var owner = Block._graph.Vertices.Where(
                    n => n.Instructions.Where(ins =>
                    {
                        if (n.Entry._method_reference != Block.Entry._method_reference)
                            return false;
                        if (ins.Instruction.Offset != this.Instruction.Offset)
                            return false;
                        return true;
                    }).Any()).ToList();
                if (owner.Count() != 1)
                    throw new Exception("Cannot find instruction!");
                CFG.Vertex true_node = owner.FirstOrDefault();
                if (s2 == true_node)
                {
                    s1 = s2;
                    s2 = true_node;
                }
                LLVM.BuildCondBr(Builder, cmp, s1.LlvmInfo.BasicBlock, s2.LlvmInfo.BasicBlock);
                return;
            }
            throw new Exception("Unhandled compare and branch.");
        }
    }
    #endregion CmpBr definition

    #region Conv definition
    public class Conv : INST
    {
        protected TYPE _dst;
        protected bool _check_overflow;
        protected bool _from_unsigned;

        VALUE convert_full(VALUE src)
        {
            TypeRef stype = LLVM.TypeOf(src.V);
            TypeRef dtype = _dst.IntermediateTypeLLVM;
            var result = new VALUE(Casting.CastArg(Builder, src.V, stype, dtype, true));
            return result;
        }

        public Conv(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var s = state._stack.Pop();
            state._stack.Push(s);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE s = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(s.ToString());

            VALUE d = convert_full(s);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(d.ToString());

            state._stack.Push(d);
        }
    }
    #endregion Conv definition

    #region ConvOvf definition
    public class ConvOvf : Conv
    {
        public ConvOvf(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            _check_overflow = true;
        }
    }
    #endregion ConvOvf definition

    #region ConvOvfUns definition
    public class ConvOvfUns : Conv
    {
        public ConvOvfUns(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            _check_overflow = true;
            _from_unsigned = true;
        }
    }
    #endregion ConvOvfUns definition

    #region ConvUns definition
    public class ConvUns : Conv
    {
        public ConvUns(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            _from_unsigned = true;
        }
    }
    #endregion ConvUns definition

    #region LdElem definition
    public class LdElem : INST
    {
        protected TYPE _dst;
        protected bool _check_overflow;
        protected bool _from_unsigned;

        public LdElem(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var i = state._stack.Pop();
            var a = state._stack.Pop();
            var e = a.GetElementType();
            var ar = a as ArrayType;
            var e2 = ar.ElementType;
            state._stack.Push(e2);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE i = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(i.ToString());

            VALUE a = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(a.ToString());

            var load = a.V;
            load = LLVM.BuildLoad(Builder, load, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(load));

            // Load array base.
            ValueRef extract_value = LLVM.BuildExtractValue(Builder, load, 0, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(extract_value));

            // Now add in index to pointer.
            ValueRef[] indexes = new ValueRef[1];
            indexes[0] = i.V;
            ValueRef gep = LLVM.BuildInBoundsGEP(Builder, extract_value, indexes, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(gep));

            load = LLVM.BuildLoad(Builder, gep, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(load));

            if (_dst != null &&_dst.IntermediateTypeLLVM != LLVM.TypeOf(load))
            {
                load = LLVM.BuildIntCast(Builder, load, _dst.IntermediateTypeLLVM, "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(load));
            }
            else if (_dst == null)
            {
                var t_v = LLVM.TypeOf(load);
                TypeRef t_to;
                // Type information for instruction obtuse. 
                // Use LLVM type and set stack type.
                if (t_v == LLVM.Int8Type() || t_v == LLVM.Int16Type())
                {
                    load = LLVM.BuildIntCast(Builder, load, LLVM.Int32Type(), "i" + instruction_id++);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(load));
                }
                else
                    t_to = t_v;
                //var op = this.Operand;
                //var tt = op.GetType();
            }

            state._stack.Push(new VALUE(load));
        }
    }
    #endregion LdElem definition

    #region StElem definition
    public class StElem : INST
    {
        protected TYPE _dst;
        protected bool _check_overflow;
        protected bool _from_unsigned;

        public StElem(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(v.ToString());

            var i = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(i.ToString());

            var a = state._stack.Pop();
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(v.ToString());

            VALUE i = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(i.ToString());

            VALUE a = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(a.ToString());

            var load = a.V;
            load = LLVM.BuildLoad(Builder, load, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(load));

            ValueRef extract_value = LLVM.BuildExtractValue(Builder, load, 0, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(extract_value));

            // Now add in index to pointer.
            ValueRef[] indexes = new ValueRef[1];
            indexes[0] = i.V;
            ValueRef gep = LLVM.BuildInBoundsGEP(Builder, extract_value, indexes, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(gep));

            var value = v.V;
            if (_dst != null && _dst.VerificationType.ToTypeRef() != v.T.IntermediateTypeLLVM)
            {
                value = LLVM.BuildIntCast(Builder, value, _dst.VerificationType.ToTypeRef(), "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(value));
            }
            else if (_dst == null)
            {
                var t_v = LLVM.TypeOf(value);
                var t_d = LLVM.TypeOf(gep);
                var t_e = LLVM.GetElementType(t_d);
                if (t_v != t_e && LLVM.GetTypeKind(t_e) != TypeKind.StructTypeKind)
                {
                    value = LLVM.BuildIntCast(Builder, value, t_e, "i" + instruction_id++);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(value));
                }
            }

            // Store.
            var store = LLVM.BuildStore(Builder, value, gep);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(store));
        }
    }
    #endregion StElem definition

    #region LdElemA definition
    public class LdElemA : INST
    {
        public LdElemA(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var i = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(i.ToString());

            var a = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(a.ToString());

            var e = a.GetElementType();

            // Create reference type of element type.
            var v = new Mono.Cecil.ByReferenceType(e);

            state._stack.Push(v);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE i = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(i.ToString());

            VALUE a = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(a.ToString());

            var load = a.V;
            load = LLVM.BuildLoad(Builder, load, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(load));

            // Load array base.
            ValueRef extract_value = LLVM.BuildExtractValue(Builder, load, 0, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(extract_value));

            // Now add in index to pointer.
            ValueRef[] indexes = new ValueRef[1];
            indexes[0] = i.V;
            ValueRef gep = LLVM.BuildInBoundsGEP(Builder, extract_value, indexes, "i" + instruction_id++);
            var result = new VALUE(gep);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(result);

            state._stack.Push(result);
        }
    }
    #endregion LdElemA definition

    #region StFld definition
    public class StFld : INST
    {
        TypeReference call_closure_value = null;
        TypeReference call_closure_object = null;

        public StFld(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // stfld, page 427 of ecma 335
            var v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(v.ToString());
            var o = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(o.ToString());
            var operand = this.Operand;
            if (operand as FieldReference == null) throw new Exception("Error in parsing stfld.");
            var field_reference = operand as FieldReference;
            call_closure_value = v;
            call_closure_object = o;
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // stfld, page 427 of ecma 335
            var operand = this.Operand;
            if (operand as FieldReference == null) throw new Exception("Error in parsing stfld.");
            var field_reference = operand as FieldReference;
            VALUE v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(v);
            VALUE o = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(o);
            TypeRef tr = LLVM.TypeOf(o.V);
            bool isPtr = o.T.isPointerTy();
            bool isArr = o.T.isArrayTy();
            bool isSt = o.T.isStructTy();
            bool is_ptr = false;

            IntPtr tr_bcltype = IntPtr.Zero;
            if (call_closure_object as PointerType != null)
            {
                var pointer = call_closure_object as PointerType;
                var element = pointer.ElementType;
                tr_bcltype = RUNTIME.MonoBclMap_GetBcl(element);
            }
            else if (call_closure_object as ByReferenceType != null)
            {
                var pointer = call_closure_object as ByReferenceType;
                var element = pointer.ElementType;
                tr_bcltype = RUNTIME.MonoBclMap_GetBcl(element);
            }
            else
                tr_bcltype = RUNTIME.MonoBclMap_GetBcl(call_closure_object);

            if (isPtr)
            {
                uint offset = 0;
                var yy = this.Instruction.Operand;
                if (yy == null) throw new Exception("Cannot convert.");

                var declaring_type = call_closure_object;
                var declaring_type_tr = field_reference.DeclaringType;
                var declaring_type_field = declaring_type_tr.Resolve();

                // need to take into account padding fields. Unfortunately,
                // LLVM does not name elements in a struct/class. So, we must
                // compute padding and adjust.
                int size = 0;
                var myfields = declaring_type.MyGetFields();
                int current_offset = 0;
                foreach (var field in myfields)
                {
                    var attr = field.Resolve().Attributes;
                    if ((attr & FieldAttributes.Static) != 0)
                        continue;
                    var bcl_field = RUNTIME.BclFindFieldInTypeAll(tr_bcltype, field.Name);
                    int field_size = RUNTIME.BclGetFieldSize(bcl_field);
                    int field_offset = RUNTIME.BclGetFieldOffset(bcl_field);
                    int padding = field_offset - current_offset;
                    size = size + padding + field_size;
                    if (padding != 0)
                    {
                        // Add in bytes to effect padding.
                        for (int j = 0; j < padding; ++j)
                            offset++;
                    }
                    if (field.Name == field_reference.Name)
                    {
                        is_ptr = field.FieldType.IsArray || field.FieldType.IsPointer;
                        break;
                    }
                    offset++;
                    current_offset = field_offset + field_size;
                }

                var dst = LLVM.BuildStructGEP(Builder, o.V, offset, "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(dst));

                var dd = LLVM.TypeOf(dst);
                var ddd = LLVM.GetElementType(dd);
                var src = v;
                TypeRef stype = LLVM.TypeOf(src.V);
                TypeRef dtype = ddd;

                /* Trunc */
                if (stype == LLVM.Int64Type()
                    && (dtype == LLVM.Int32Type() || dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type() ||
                        dtype == LLVM.Int1Type()))
                    src = new VALUE(LLVM.BuildTrunc(Builder, src.V, dtype, "i" + instruction_id++));
                else if (stype == LLVM.Int32Type()
                         && (dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type() || dtype == LLVM.Int1Type()))
                    src = new VALUE(LLVM.BuildTrunc(Builder, src.V, dtype, "i" + instruction_id++));
                else if (stype == LLVM.Int16Type()
                         && (dtype == LLVM.Int8Type() || dtype == LLVM.Int1Type()))
                    src = new VALUE(LLVM.BuildTrunc(Builder, src.V, dtype, "i" + instruction_id++));

                if (LLVM.TypeOf(src.V) != dtype)
                {
                    if (LLVM.GetTypeKind(LLVM.TypeOf(src.V)) == TypeKind.PointerTypeKind)
                    {
                        src = new VALUE(LLVM.BuildPointerCast(Builder, src.V, dtype, "i" + instruction_id++));
                    }
                    else
                    {
                        src = new VALUE(LLVM.BuildBitCast(Builder, src.V, dtype, "i" + instruction_id++));
                    }
                }

                var store = LLVM.BuildStore(Builder, src.V, dst);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(store));
            }
            else if (isSt)
            {
                uint offset = 0;

                var declaring_type = call_closure_object;
                var declaring_type_tr = field_reference.DeclaringType;
                var declaring_type_field = declaring_type_tr.Resolve();

                // need to take into account padding fields. Unfortunately,
                // LLVM does not name elements in a struct/class. So, we must
                // compute padding and adjust.
				int size = 0;
				var myfields = declaring_type.MyGetFields();
				int current_offset = 0;
				foreach (var field in myfields)
				{
					var attr = field.Resolve().Attributes;
					if ((attr & FieldAttributes.Static) != 0)
						continue;
					var bcl_field = RUNTIME.BclFindFieldInTypeAll(tr_bcltype, field.Name);
					int field_size = RUNTIME.BclGetFieldSize(bcl_field);
					int field_offset = RUNTIME.BclGetFieldOffset(bcl_field);
					int padding = field_offset - current_offset;
					size = size + padding + field_size;
					if (padding != 0)
					{
						// Add in bytes to effect padding.
						for (int j = 0; j < padding; ++j)
							offset++;
					}
					if (field.Name == field_reference.Name)
					{
						is_ptr = field.FieldType.IsArray || field.FieldType.IsPointer;
						break;
					}
					offset++;
					current_offset = field_offset + field_size;
				}

                var value = LLVM.BuildExtractValue(Builder, o.V, offset, "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(value));

                var load_value = new VALUE(value);
                bool isPtrLoad = load_value.T.isPointerTy();
                if (isPtrLoad)
                {
                    var mono_field_type = field_reference.FieldType;
                    TypeRef type = mono_field_type.ToTypeRef();
                    value = LLVM.BuildBitCast(Builder,
                        value, type, "i" + instruction_id++);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(value));
                }

                var store = LLVM.BuildStore(Builder, v.V, value);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(store));
            }
            else
            {
                throw new Exception("Value type ldfld not implemented!");
            }
        }
    }
    #endregion StFld definition

    #region LdInd definition
    public class LdInd : INST
    {
        protected TYPE _dst;
        protected bool _check_overflow;
        protected bool _from_unsigned;
        private TypeReference _after_indirect_type;
        private TypeReference _before_indirect_type;

        public LdInd(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {  // ldind -- load value indirect onto the stack, p 367
            var i = state._stack.Pop();
            _before_indirect_type = i;
            TypeReference v = null;
            if (i as ByReferenceType != null)
            {
                var j = i as ByReferenceType;
                v = j.ElementType;
            }
            else if (i as PointerType != null)
            {
                var j = i as PointerType;
                v = j.ElementType;
            }
            else if (_dst != null)
            {
                // The value is a boxed type and should be marked as such.
                // The original type should be by ref type or pointer.
                v = _dst.CilType;
            }
            else
            {
                // The value is a ref type. It is the value v.
                // I haven't seen this, but I guess it could happen.
                // The original type should be by ref type or pointer.
            }
            _after_indirect_type = v;
            state._stack.Push(v);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
		{  // ldind -- load value indirect onto the stack, p 367
            VALUE v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine("LdInd into function " + v.ToString());

		    var load = v.V;
            TypeRef tr = LLVM.TypeOf(load);
            TypeKind kind = LLVM.GetTypeKind(tr);

		    if (kind == TypeKind.IntegerTypeKind)
		    {
                // This is ok as it's probably just a native int. Make sure
                // it's 64-bits, then type cast.
		        if (tr == LLVM.Int64Type() && _dst != null)
		        {
		            load = LLVM.BuildIntToPtr(Builder,
		                v.V, LLVM.PointerType(_dst.CilTypeLLVM, 0), "i" + instruction_id++);
		        }
		    }
            load = LLVM.BuildLoad(Builder, load, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(load));

            if (_dst != null && _dst.IntermediateTypeLLVM != LLVM.TypeOf(load))
            {
                load = LLVM.BuildIntCast(Builder, load, _dst.IntermediateTypeLLVM, "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(load));
            }
            else if (_dst == null)
            {
                var t_v = LLVM.TypeOf(load);
                TypeRef t_to;
                // Type information for instruction obtuse. 
                // Use LLVM type and set stack type.
                if (t_v == LLVM.Int8Type() || t_v == LLVM.Int16Type())
                {
                    load = LLVM.BuildIntCast(Builder, load, LLVM.Int32Type(), "i" + instruction_id++);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(load));
                }
                else
                    t_to = t_v;
            }

            state._stack.Push(new VALUE(load));
        }
    }
    #endregion LdInd definition

    #region StInd definition
    public class StInd : INST
    {
        protected TYPE _dst;
        protected TypeReference _call_closure_value_type = null;
        protected TypeReference _call_closure_ref_type = null;
        protected bool _check_overflow;
        protected bool _from_unsigned;

        public StInd(CFG.Vertex b, Mono.Cecil.Cil.Instruction i, TypeReference dst)
            : base(b, i)
		{
			_dst = dst != null ? new TYPE(dst) : null;
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(v.ToString());
            _call_closure_value_type = v;
            var o = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(o.ToString());
            _call_closure_ref_type = o;
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            VALUE src = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(src);

            VALUE a = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(a);

            TypeRef stype = LLVM.TypeOf(src.V);
            TypeRef dtype;
            if (_dst == null)
            {
                // Determine target type dynamically.
                var t = this._call_closure_ref_type as ByReferenceType;
                if (t == null) throw new Exception("Cannot convert target type to by reference type.");
                var t2 = t.ElementType;
                dtype = t2.ToTypeRef();
            }
            else
            {
                dtype = _dst.StorageTypeLLVM;
            }

            /* Trunc */
            if (stype == LLVM.Int64Type()
                  && (dtype == LLVM.Int32Type() || dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type() || dtype == LLVM.Int1Type()))
                src = new VALUE(LLVM.BuildTrunc(Builder, src.V, dtype, "i" + instruction_id++));
            else if (stype == LLVM.Int32Type()
                  && (dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type() || dtype == LLVM.Int1Type()))
                src = new VALUE(LLVM.BuildTrunc(Builder, src.V, dtype, "i" + instruction_id++));
            else if (stype == LLVM.Int16Type()
                  && (dtype == LLVM.Int8Type() || dtype == LLVM.Int1Type()))
                src = new VALUE(LLVM.BuildTrunc(Builder, src.V, dtype, "i" + instruction_id++));

            if (LLVM.TypeOf(src.V) != dtype)
            {
                if (LLVM.GetTypeKind(LLVM.TypeOf(src.V)) == TypeKind.PointerTypeKind)
                {
                    src = new VALUE(LLVM.BuildPointerCast(Builder, src.V, dtype, "i" + instruction_id++));
                }
                else
                {
                    src = new VALUE(LLVM.BuildBitCast(Builder, src.V, dtype, "i" + instruction_id++));
                }
            }

            var zz = LLVM.BuildStore(Builder, src.V, a.V);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine("Store = " + new VALUE(zz).ToString());
        }
    }
    #endregion StInd definition

    #region Unbox definition
    public class Unbox : INST
    {
        TypeReference call_closure_typetok = null;

        protected Unbox(CFG.Vertex b, Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // unbox – convert boxed value type to its raw form, page 431
            var type = this.Operand;
            var tr = type as TypeReference;
            var tr2 = tr.SwapInBclType();
            var v = tr2.Deresolve(this.Block._method_reference.DeclaringType, null);
            call_closure_typetok = v;
            TypeReference v2 = state._stack.Pop();
            state._stack.Push(v);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            ValueRef new_obj;

            // Get meta of object.
            var operand = this.Operand;
            var tr = operand as TypeReference;
            tr = tr.SwapInBclType();
            tr = tr.Deresolve(this.Block._method_reference.DeclaringType, null);
            var meta = RUNTIME.MonoBclMap_GetBcl(tr);

            var o = state._stack.Pop();
            ValueRef value = o.V;
            // If it is expected to be a struct or class, leave it along.
            if (call_closure_typetok.IsValueType)
            {
                // Generate code to deref object.
                value = LLVM.BuildLoad(Builder, o.V, "i" + instruction_id++);
            }
            state._stack.Push(new VALUE(value));
        }
    }
    #endregion Unbox definition

    #region i_add definition
    public class i_add : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_add(b, i); }
        private i_add(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_add definition

    #region i_add_ovf definition
    public class i_add_ovf : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_add_ovf(b, i); }
        private i_add_ovf(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_add_ovf definition

    #region i_add_ovf_un definition
    public class i_add_ovf_un : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_add_ovf_un(b, i); }
        private i_add_ovf_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_add_ovf_un definition

    #region i_and definition
    public class i_and : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_and(b, i); }
        private i_and(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_and definition

    #region i_arglist definition
    public class i_arglist : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_arglist(b, i); }
        private i_arglist(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_arglist definition

    #region i_beq definition
    public class i_beq : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_beq(b, i); }
        private i_beq(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.eq; IsSigned = true; }
    }
    #endregion i_beq definition

    #region i_beq_s definition
    public class i_beq_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_beq_s(b, i); }
        private i_beq_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.eq; IsSigned = true; }
    }
    #endregion i_beq_s definition

    #region i_bge definition
    public class i_bge : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bge(b, i); }
        private i_bge(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.ge; IsSigned = true; }
    }
    #endregion i_bge definition

    #region i_bge_un definition
    public class i_bge_un : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bge_un(b, i); }
        private i_bge_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.ge; IsSigned = false; }
    }
    #endregion i_bge_un definition

    #region i_bge_un_s definition
    public class i_bge_un_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bge_un_s(b, i); }
        private i_bge_un_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.ge; IsSigned = false; }
    }
    #endregion i_bge_un_s definition

    #region i_bge_s definition
    public class i_bge_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bge_s(b, i); }
        private i_bge_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.ge; IsSigned = true; }
    }
    #endregion i_bge_s definition

    #region i_bgt definition
    public class i_bgt : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bgt(b, i); }
        private i_bgt(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.gt; IsSigned = true; }
    }
    #endregion i_bgt definition

    #region i_bgt_s definition
    public class i_bgt_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bgt_s(b, i); }
        private i_bgt_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.gt; IsSigned = true; }
    }
    #endregion i_bgt_s definition

    #region i_bgt_un definition
    public class i_bgt_un : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bgt_un(b, i); }
        private i_bgt_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.gt; IsSigned = false; }
    }
    #endregion i_bgt_un definition

    #region i_bgt_un_s definition
    public class i_bgt_un_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bgt_un_s(b, i); }
        private i_bgt_un_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.gt; IsSigned = false; }
    }
    #endregion i_bgt_un_s definition

    #region i_ble definition
    public class i_ble : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ble(b, i); }
        private i_ble(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.le; IsSigned = true; }
    }
    #endregion i_ble definition

    #region i_ble_s definition
    public class i_ble_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ble_s(b, i); }
        private i_ble_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.le; }
    }
    #endregion i_ble_s definition

    #region i_ble_un definition
    public class i_ble_un : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ble_un(b, i); }
        private i_ble_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.le; IsSigned = false; }
    }
    #endregion i_ble_un definition

    #region i_ble_un_s definition
    public class i_ble_un_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ble_un_s(b, i); }
        private i_ble_un_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.le; IsSigned = false; }
    }
    #endregion i_ble_un_s definition

    #region i_blt definition
    public class i_blt : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_blt(b, i); }
        private i_blt(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.lt; IsSigned = true; }
    }
    #endregion i_blt definition

    #region i_blt_s definition
    public class i_blt_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_blt_s(b, i); }
        private i_blt_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.lt; IsSigned = true; }
    }
    #endregion i_blt_s definition

    #region i_blt_un definition
    public class i_blt_un : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_blt_un(b, i); }
        private i_blt_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.lt; IsSigned = false; }
    }
    #endregion i_blt_un definition

    #region i_blt_un_s definition
    public class i_blt_un_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_blt_un_s(b, i); }
        private i_blt_un_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.lt; IsSigned = false; }
    }
    #endregion i_blt_un_s definition

    #region i_bne_un definition
    public class i_bne_un : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bne_un(b, i); }
        private i_bne_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.ne; IsSigned = false; }
    }
    #endregion i_bne_un definition

    #region i_bne_un_s definition
    public class i_bne_un_s : CmpBr
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_bne_un_s(b, i); }
        private i_bne_un_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.ne; IsSigned = false; }
    }
    #endregion i_bne_un_s definition

    #region i_box definition
    public class i_box : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_box(b, i); }

        TypeReference call_closure_typetok = null;

        private i_box(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // box – convert a boxable value to its boxed form, page 394
            var typetok = this.Operand;
            var tr = typetok as TypeReference;
            var tr2 = tr.SwapInBclType();
            var v = tr2.Deresolve(this.Block._method_reference.DeclaringType, null);
            call_closure_typetok = v;
            TypeReference v2 = state._stack.Pop();
            state._stack.Push(v);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            ValueRef new_obj;

            // Get meta of object.
            var operand = this.Operand;
            var tr = operand as TypeReference;
            tr = tr.SwapInBclType();
            tr = tr.Deresolve(this.Block._method_reference.DeclaringType, null);
            var meta = RUNTIME.MonoBclMap_GetBcl(tr);

            // Generate code to allocate object and stuff.
            // This boxes the value.
            var xx1 = RUNTIME.BclNativeMethods.ToList();
            var xx2 = RUNTIME.PtxFunctions.ToList();
            var xx = xx2
                .Where(t => { return t._mangled_name == "_Z23Heap_AllocTypeVoidStarsPv"; });
            var xxx = xx.ToList();
            RUNTIME.PtxFunction first_kv_pair = xx.FirstOrDefault();
            if (first_kv_pair == null)
                throw new Exception("Yikes.");

            ValueRef fv2 = first_kv_pair._valueref;
            ValueRef[] args = new ValueRef[1];

            //args[0] = LLVM.BuildIntToPtr(Builder,
            //    LLVM.ConstInt(LLVM.Int64Type(), (ulong)meta.ToInt64(), false),
            //    LLVM.PointerType(LLVM.VoidType(), 0),
            //    "i" + instruction_id++);
            args[0] = LLVM.ConstInt(LLVM.Int64Type(), (ulong)meta.ToInt64(), false);
            var call = LLVM.BuildCall(Builder, fv2, args, "i" + instruction_id++);
            var type_casted = LLVM.BuildIntToPtr(Builder, call,
                typeof(System.Object).ToMonoTypeReference().ToTypeRef(),
                "i" + instruction_id++);
            new_obj = type_casted;
            // Stuff value in buffer of object.
            var s = state._stack.Pop();
            ValueRef v = LLVM.BuildPointerCast(Builder, new_obj, LLVM.PointerType(LLVM.TypeOf(s.V), 0),
                "i" + instruction_id++);
            ValueRef store = LLVM.BuildStore(Builder, s.V, v);

            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(new_obj));

            state._stack.Push(new VALUE(new_obj));
        }
    }
    #endregion i_box definition

    #region i_br definition
    public class i_br : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_br(b, i); }

        private i_br(CFG.Vertex b, Mono.Cecil.Cil.Instruction i): base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var edge = Block._graph.SuccessorEdges(Block).ToList()[0];
            var s = edge.To;
            var br = LLVM.BuildBr(Builder, s.LlvmInfo.BasicBlock);
        }
    }
    #endregion i_br definition

    #region i_br_s definition
    public class i_br_s : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_br_s(b, i); }

        private i_br_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var edge = Block._graph.SuccessorEdges(Block).ToList()[0];
            var s = edge.To;
            var br = LLVM.BuildBr(Builder, s.LlvmInfo.BasicBlock);
        }
    }
    #endregion i_br_s definition

    #region i_break definition
    public class i_break : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_break(b, i); }
        private i_break(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_break definition

    #region i_brfalse definition
    public class i_brfalse : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_brfalse(b, i); }

        private i_brfalse(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // brfalse, page 340 of ecma 335
            var v = state._stack.Pop();
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // brfalse, page 340 of ecma 335
            object operand = this.Operand;
            Instruction instruction = operand as Instruction;
            var v = state._stack.Pop();
            var value = v.V;
            ValueRef condition;
            var type_of_value = LLVM.TypeOf(v.V);
            if (LLVM.GetTypeKind(type_of_value) == TypeKind.PointerTypeKind)
            {
                var cast = LLVM.BuildPtrToInt(Builder, v.V, LLVM.Int64Type(), "i" + instruction_id++);
                var v2 = LLVM.ConstInt(LLVM.Int64Type(), 0, false);
                condition = LLVM.BuildICmp(Builder, IntPredicate.IntEQ, cast, v2, "i" + instruction_id++);
            }
            else if (LLVM.GetTypeKind(type_of_value) == TypeKind.IntegerTypeKind)
            {
                if (type_of_value == LLVM.Int8Type() || type_of_value == LLVM.Int16Type())
                {
                    value = LLVM.BuildIntCast(Builder, value, LLVM.Int32Type(), "i" + instruction_id++);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(value));
                    var v2 = LLVM.ConstInt(LLVM.Int32Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntEQ, value, v2, "i" + instruction_id++);
                }
                else if (type_of_value == LLVM.Int32Type())
                {
                    var v2 = LLVM.ConstInt(LLVM.Int32Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntEQ, value, v2, "i" + instruction_id++);
                }
                else if (type_of_value == LLVM.Int64Type())
                {
                    var v2 = LLVM.ConstInt(LLVM.Int64Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntEQ, value, v2, "i" + instruction_id++);
                }
                else throw new Exception("Unhandled type in brfalse.s");
            }
            else throw new Exception("Unhandled type in brfalse.s");
            // In order to select the correct branch, we need to know what
            // edge represents the "true" branch. During construction, there is
            // no guarentee that the order is consistent.
            var owner = Block._graph.Vertices.Where(
                n => n.Instructions.Where(ins =>
                {
                    if (n.Entry._method_reference != Block.Entry._method_reference)
                        return false;
                    if (ins.Instruction.Offset != instruction.Offset)
                        return false;
                    return true;
                }).Any()).ToList();
            if (owner.Count != 1)
                throw new Exception("Cannot find instruction!");
            var edge1 = Block._graph.SuccessorEdges(Block).ToList()[0];
            var s1 = edge1.To;
            var edge2 = Block._graph.SuccessorEdges(Block).ToList()[1];
            var s2 = edge2.To;
            CFG.Vertex then_node = owner.FirstOrDefault();
            CFG.Vertex else_node = s1 == then_node ? s2 : s1;
            LLVM.BuildCondBr(Builder, condition, then_node.LlvmInfo.BasicBlock, else_node.LlvmInfo.BasicBlock);
        }
    }
    #endregion i_brfalse definition

    #region i_brfalse_s definition
    public class i_brfalse_s : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_brfalse_s(b, i); }

        private i_brfalse_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
        }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // brfalse.s, page 340 of ecma 335
            var v = state._stack.Pop();
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // brfalse.s, page 340 of ecma 335
            object operand = this.Operand;
            Instruction instruction = operand as Instruction;
            var v = state._stack.Pop();
            var value = v.V;
            ValueRef condition;
            var type_of_value = LLVM.TypeOf(v.V);
            if (LLVM.GetTypeKind(type_of_value) == TypeKind.PointerTypeKind)
            {
                var cast = LLVM.BuildPtrToInt(Builder, v.V, LLVM.Int64Type(), "i" + instruction_id++);
                var v2 = LLVM.ConstInt(LLVM.Int64Type(), 0, false);
                condition = LLVM.BuildICmp(Builder, IntPredicate.IntEQ, cast, v2, "i" + instruction_id++);
            }
            else if (LLVM.GetTypeKind(type_of_value) == TypeKind.IntegerTypeKind)
            {
                if (type_of_value == LLVM.Int8Type() || type_of_value == LLVM.Int16Type())
                {
                    value = LLVM.BuildIntCast(Builder, value, LLVM.Int32Type(), "i" + instruction_id++);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(value));
                    var v2 = LLVM.ConstInt(LLVM.Int32Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntEQ, value, v2, "i" + instruction_id++);
                }
                else if (type_of_value == LLVM.Int32Type())
                {
                    var v2 = LLVM.ConstInt(LLVM.Int32Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntEQ, value, v2, "i" + instruction_id++);
                }
                else if (type_of_value == LLVM.Int64Type())
                {
                    var v2 = LLVM.ConstInt(LLVM.Int64Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntEQ, value, v2, "i" + instruction_id++);
                }
                else throw new Exception("Unhandled type in brfalse.s");
            }
            else throw new Exception("Unhandled type in brfalse.s");
            // In order to select the correct branch, we need to know what
            // edge represents the "true" branch. During construction, there is
            // no guarentee that the order is consistent.
            var owner = Block._graph.Vertices.Where(
                n => n.Instructions.Where(ins =>
                {
                    if (n.Entry._method_reference != Block.Entry._method_reference)
                        return false;
                    if (ins.Instruction.Offset != instruction.Offset)
                        return false;
                    return true;
                }).Any()).ToList();
            if (owner.Count != 1)
                throw new Exception("Cannot find instruction!");
            var edge1 = Block._graph.SuccessorEdges(Block).ToList()[0];
            var s1 = edge1.To;
            var edge2 = Block._graph.SuccessorEdges(Block).ToList()[1];
            var s2 = edge2.To;
            CFG.Vertex then_node = owner.FirstOrDefault();
            CFG.Vertex else_node = s1 == then_node ? s2 : s1;
            LLVM.BuildCondBr(Builder, condition, then_node.LlvmInfo.BasicBlock, else_node.LlvmInfo.BasicBlock);
        }
    }
    #endregion i_brfalse_s definition

    #region i_brtrue definition
    public class i_brtrue : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_brtrue(b, i); }

        private i_brtrue(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = state._stack.Pop();
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // brtrue, page 341 of ecma 335
            object operand = this.Operand;
            Instruction instruction = operand as Instruction;
            var v = state._stack.Pop();
            var value = v.V;
            ValueRef condition;
            var type_of_value = LLVM.TypeOf(v.V);
            if (LLVM.GetTypeKind(type_of_value) == TypeKind.PointerTypeKind)
            {
                var cast = LLVM.BuildPtrToInt(Builder, v.V, LLVM.Int64Type(), "i" + instruction_id++);
                // Verify an object, as according to spec. We'll do that using BCL.
                var v2 = LLVM.ConstInt(LLVM.Int64Type(), 0, false);
                condition = LLVM.BuildICmp(Builder, IntPredicate.IntNE, cast, v2, "i" + instruction_id++);
            }
            else if (LLVM.GetTypeKind(type_of_value) == TypeKind.IntegerTypeKind)
            {
                if (type_of_value == LLVM.Int8Type() || type_of_value == LLVM.Int16Type())
                {
                    value = LLVM.BuildIntCast(Builder, value, LLVM.Int32Type(), "i" + instruction_id++);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(value));
                    var v2 = LLVM.ConstInt(LLVM.Int32Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntNE, value, v2, "i" + instruction_id++);
                }
                else if (type_of_value == LLVM.Int32Type())
                {
                    var v2 = LLVM.ConstInt(LLVM.Int32Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntNE, value, v2, "i" + instruction_id++);
                }
                else if (type_of_value == LLVM.Int64Type())
                {
                    var v2 = LLVM.ConstInt(LLVM.Int64Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntNE, value, v2, "i" + instruction_id++);
                }
                else throw new Exception("Unhandled type in brtrue");
            }
            else throw new Exception("Unhandled type in brtrue");
            // In order to select the correct branch, we need to know what
            // edge represents the "true" branch. During construction, there is
            // no guarentee that the order is consistent.
            var owner = Block._graph.Vertices.Where(
                n => n.Instructions.Where(ins =>
                {
                    if (n.Entry._method_reference != Block.Entry._method_reference)
                        return false;
                    if (ins.Instruction.Offset != instruction.Offset)
                        return false;
                    return true;
                }).Any()).ToList();
            if (owner.Count != 1)
                throw new Exception("Cannot find instruction!");
            var edge1 = Block._graph.SuccessorEdges(Block).ToList()[0];
            var s1 = edge1.To;
            var edge2 = Block._graph.SuccessorEdges(Block).ToList()[1];
            var s2 = edge2.To;
            CFG.Vertex then_node = owner.FirstOrDefault();
            CFG.Vertex else_node = s1 == then_node ? s2 : s1;
            LLVM.BuildCondBr(Builder, condition, then_node.LlvmInfo.BasicBlock, else_node.LlvmInfo.BasicBlock);
        }
    }
    #endregion i_brtrue definition

    #region i_brtrue_s definition
    public class i_brtrue_s : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_brtrue_s(b, i); }

        private i_brtrue_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = state._stack.Pop();
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // brtrue, page 341 of ecma 335
            object operand = this.Operand;
            Instruction instruction = operand as Instruction;
            var v = state._stack.Pop();
            var value = v.V;
            ValueRef condition;
            var type_of_value = LLVM.TypeOf(v.V);
            if (LLVM.GetTypeKind(type_of_value) == TypeKind.PointerTypeKind)
            {
                var cast = LLVM.BuildPtrToInt(Builder, v.V, LLVM.Int64Type(), "i" + instruction_id++);
                var v2 = LLVM.ConstInt(LLVM.Int64Type(), 0, false);
                condition = LLVM.BuildICmp(Builder, IntPredicate.IntNE, cast, v2, "i" + instruction_id++);
            }
            else if (LLVM.GetTypeKind(type_of_value) == TypeKind.IntegerTypeKind)
            {
                if (type_of_value == LLVM.Int8Type() || type_of_value == LLVM.Int16Type())
                {
                    value = LLVM.BuildIntCast(Builder, value, LLVM.Int32Type(), "i" + instruction_id++);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(value));
                    var v2 = LLVM.ConstInt(LLVM.Int32Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntNE, value, v2, "i" + instruction_id++);
                }
                else if (type_of_value == LLVM.Int32Type())
                {
                    var v2 = LLVM.ConstInt(LLVM.Int32Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntNE, value, v2, "i" + instruction_id++);
                }
                else if (type_of_value == LLVM.Int64Type())
                {
                    var v2 = LLVM.ConstInt(LLVM.Int64Type(), 0, false);
                    condition = LLVM.BuildICmp(Builder, IntPredicate.IntNE, value, v2, "i" + instruction_id++);
                }
                else throw new Exception("Unhandled type in brtrue");
            }
            else throw new Exception("Unhandled type in brtrue");
            // In order to select the correct branch, we need to know what
            // edge represents the "true" branch. During construction, there is
            // no guarentee that the order is consistent.
            var owner = Block._graph.Vertices.Where(
                n => n.Instructions.Where(ins =>
                {
                    if (n.Entry._method_reference != Block.Entry._method_reference)
                        return false;
                    if (ins.Instruction.Offset != instruction.Offset)
                        return false;
                    return true;
                }).Any()).ToList();
            if (owner.Count != 1)
                throw new Exception("Cannot find instruction!");
            var edge1 = Block._graph.SuccessorEdges(Block).ToList()[0];
            var s1 = edge1.To;
            var edge2 = Block._graph.SuccessorEdges(Block).ToList()[1];
            var s2 = edge2.To;
            CFG.Vertex then_node = owner.FirstOrDefault();
            CFG.Vertex else_node = s1 == then_node ? s2 : s1;
            LLVM.BuildCondBr(Builder, condition, then_node.LlvmInfo.BasicBlock, else_node.LlvmInfo.BasicBlock);
        }
    }
    #endregion i_brtrue_s definition

    #region i_call definition
    public class i_call : Call
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_call(b, i); }
        private i_call(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_call definition

    #region i_calli definition
    public class i_calli : Call
    {
        public override MethodReference CallTarget() { throw new Exception("Calli not handled."); }
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_calli(b, i); }
        private i_calli(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_calli definition

    #region i_callvirt definition
    public class i_callvirt : INST
    {
        MethodReference call_closure_method = null;
        public override MethodReference CallTarget() { return call_closure_method; }

        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_callvirt(b, i); }

        private i_callvirt(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            INST new_inst = this;
            object method = this.Operand;
            if (method as Mono.Cecil.MethodReference == null) throw new Exception();
            Mono.Cecil.MethodReference orig_mr = method as Mono.Cecil.MethodReference;
            var mr = orig_mr;
            bool has_this = false;
            if (mr.HasThis) has_this = true;
            if (OpCode.Code == Code.Callvirt) has_this = true;
            bool is_explicit_this = mr.ExplicitThis;
            int xargs = (has_this && !is_explicit_this ? 1 : 0) + mr.Parameters.Count;
            List<TypeReference> args = new List<TypeReference>();
            for (int k = 0; k < xargs; ++k)
            {
                var v = state._stack.Pop();
                v = v.SwapInBclType();
                args.Insert(0, v);
            }
            var args_array = args.ToArray();
            var first = args[0];

            // JOY! "contrained. type" can appear before this instruction,
            // so several things to do here. First, Sometimes we get "type&" in
            // args for "this". Strip the value and get "type".
            /*
    Method System.Int32 System.Collections.Generic.Dictionary`2<System.String,System.Globalization.CultureInfo>::GetSlot(System.String) corlib.dll C:\Users\kenne\Documents\Campy2\ConsoleApp4\bin\Debug\\corlib.dll
    Method System.Int32 System.Collections.Generic.Dictionary`2::GetSlot(TKey) corlib.dll C:\Users\kenne\Documents\Campy2\ConsoleApp4\bin\Debug\\corlib.dll
    HasThis   True
    Args   2
    Locals 2
    Return (reuse) True
    Edges to: 451
    Instructions:
        IL_0000: nop    
        IL_0001: ldarga.s key    
        IL_0003: constrained. TKey    
        IL_0009: callvirt System.Int32 System.Object::GetHashCode()    
             */
            if (first.IsByReference)
            {
                first = first.GetElementType();
                // With Constrained instruction, the function we call is for the type
                // in the constraint. NOT SURE WHAT THE FUCK TO DO!
                args[0] = first;
            }

            mr = orig_mr.SwapInBclMethod(this.Block._method_reference.DeclaringType, args_array);
            if (mr == null)
            {
                call_closure_method = orig_mr;
                return; // Can't do anything with this.
            }
            if (mr.ReturnType.FullName != "System.Void")
            {
                state._stack.Push(mr.ReturnType);
            }
            call_closure_method = mr;
            IMPORTER.Singleton().Add(mr);

            // Here's where great fun happens. For every virtual method, go up base class tree
            // to get other implementations. Further, go through every type scanned and check
            // for virtual functons of the same name. This analysis isn't perfect however.
            Stack<TypeReference> chain = new Stack<TypeReference>();
            var p = first;
            while (p != null)
            {
                chain.Push(p);
                var bt = p.Resolve().BaseType;
                p = bt?.Deresolve(p, null);
            }
            while (chain.Any())
            {
                var q = chain.Pop();
                foreach (var m in q.Resolve().Methods)
                {
                    if (m.Name == mr.Name)
                    {
                        var mf = m.Deresolve(q, null);
                        // Also look at args, mf!
                        if (mf.Parameters.Count != mr.Parameters.Count)
                            continue;
                        bool match = true;
                        for (int i = 0; i < mf.Parameters.Count; ++i)
                        {
                            var p1 = mf.Parameters[i].ParameterType;
                            var p2 = mr.Parameters[i].ParameterType;
                            if (p1.Name != p2.Name)
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match) continue;
                        IMPORTER.Singleton().Add(mf);
                    }
                }
            }
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // callvirt – call a method associated, at runtime, with an object, page 396.
            var mr = this.call_closure_method;
            var md = mr.Resolve();
            bool is_virtual = md.IsVirtual;
            bool has_this = true;
            bool is_explicit_this = mr.ExplicitThis;
            has_this = has_this && !is_explicit_this;
            int xargs = (has_this ? 1 : 0) + mr.Parameters.Count;
            // callvirt can be called for non-virtual functions!!!!!!!! BS!
            // Switch appropriately.
           // if (!is_virtual) throw new Exception("Fucked.");
            {
                VALUE this_parameter = state._stack.PeekTop(xargs - 1);
                ValueRef[] args1 = new ValueRef[2];
                var this_ptr = LLVM.BuildPtrToInt(Builder, this_parameter.V, LLVM.Int64Type(), "i" + instruction_id++);
                args1[0] = this_ptr;
                var token = 0x06000000 | mr.MetadataToken.RID;
                var v2 = LLVM.ConstInt(LLVM.Int32Type(), token, false);
                args1[1] = v2;
                var f = RUNTIME.PtxFunctions.Where(t => t._mangled_name == "_Z21MetaData_GetMethodJitPvi").First();
                var addr_method = LLVM.BuildCall(Builder, f._valueref, args1, "");
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(addr_method));
                bool has_return = mr.ReturnType.FullName != "System.Void";
                TypeRef[] lparams = new TypeRef[xargs];
                ValueRef[] args = new ValueRef[xargs];
                var pars = mr.Parameters;
                for (int k = mr.Parameters.Count - 1; k >= 0; --k)
                {
                    VALUE v = state._stack.Pop();
                    var par_type = pars[k].ParameterType.InstantiateGeneric(mr);
                    TypeRef par = par_type.ToTypeRef();
                    ValueRef value = v.V;
                    if (LLVM.TypeOf(value) != par)
                    {
                        if (LLVM.GetTypeKind(par) == TypeKind.StructTypeKind
                            && LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.PointerTypeKind)
                            value = LLVM.BuildLoad(Builder, value, "i" + instruction_id++);
                        else if (LLVM.GetTypeKind(par) == TypeKind.PointerTypeKind)
                            value = LLVM.BuildPointerCast(Builder, value, par, "i" + instruction_id++);
                        else if (LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.IntegerTypeKind)
                            value = LLVM.BuildIntCast(Builder, value, par, "i" + instruction_id++);
                        else
                            value = LLVM.BuildBitCast(Builder, value, par, "i" + instruction_id++);
                    }
                    lparams[k + xargs - mr.Parameters.Count] = par;
                    args[k + xargs - mr.Parameters.Count] = value;
                }
                if (has_this)
                {
                    VALUE v = state._stack.Pop();
                    TypeRef par = mr.DeclaringType.ToTypeRef();
                    ValueRef value = v.V;
                    if (LLVM.TypeOf(value) != par)
                    {
                        if (LLVM.GetTypeKind(par) == TypeKind.StructTypeKind
                            && LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.PointerTypeKind)
                            value = LLVM.BuildLoad(Builder, value, "i" + instruction_id++);
                        else if (LLVM.GetTypeKind(par) == TypeKind.PointerTypeKind)
                            value = LLVM.BuildPointerCast(Builder, value, par, "i" + instruction_id++);
                        else if (LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.IntegerTypeKind)
                            value = LLVM.BuildIntCast(Builder, value, par, "i" + instruction_id++);
                        else
                            value = LLVM.BuildBitCast(Builder, value, par, "");
                    }
                    lparams[0] = par;
                    args[0] = value;
                }
                TypeRef return_type = has_return ?
                    mr.ReturnType.InstantiateGeneric(mr).ToTypeRef() : LLVM.Int64Type();

                // There are two ways a function can be called: with direct parameters,
                // or with arrayed parameters. One way is "direct" where parameters are
                // passed as is. This occurs for Campy JIT code. The other way is "indirect"
                // where the parameters are passed via arrays. This occurs for BCL internal
                // functions.

                CFG.Vertex the_entry = this.Block._graph.Vertices.Where(v =>
                    (v.IsEntry && v._method_reference.FullName == mr.FullName)).ToList().FirstOrDefault();

                if (the_entry != null)
                {
                    var function_type = LLVM.FunctionType(return_type, lparams, false);
                    var ptr_function_type = LLVM.PointerType(function_type, 0);
                    var ptr_method = LLVM.BuildIntToPtr(Builder, addr_method, ptr_function_type, "i" + instruction_id++);
                    this.DebuggerInfo();
                    var call = LLVM.BuildCall(Builder, ptr_method, args, "");
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(call.ToString());
                    if (has_return)
                    {
                        state._stack.Push(new VALUE(call));
                    }
                }
                else
                {
                    TypeRef[] internal_lparams = new TypeRef[3];
                    ValueRef[] internal_args = new ValueRef[3];
                    TypeRef internal_return_type = LLVM.VoidType();
                    internal_lparams[0] = internal_lparams[1] = internal_lparams[2] = LLVM.Int64Type();
                    var function_type = LLVM.FunctionType(internal_return_type, internal_lparams, false);
                    var ptr_function_type = LLVM.PointerType(function_type, 0);
                    var ptr_method = LLVM.BuildIntToPtr(Builder, addr_method, ptr_function_type, "i" + instruction_id++);
                    var parameter_type = LLVM.ArrayType(LLVM.Int64Type(), (uint)args.Count() - 1);
                    var arg_buffer = LLVM.BuildAlloca(Builder, parameter_type, "i" + instruction_id++);
                    LLVM.SetAlignment(arg_buffer, 64);
                    var base_of_args = LLVM.BuildPointerCast(Builder, arg_buffer,
                        LLVM.PointerType(LLVM.Int64Type(), 0), "i" + instruction_id++);
                    for (int i = 1; i < args.Count(); ++i)
                    {
                        var im1 = i - 1;
                        ValueRef[] index = new ValueRef[1] { LLVM.ConstInt(LLVM.Int32Type(), (ulong)im1, true) };
                        var add = LLVM.BuildInBoundsGEP(Builder, base_of_args, index, "i" + instruction_id++);
                        ValueRef v = LLVM.BuildPointerCast(Builder, add, LLVM.PointerType(LLVM.TypeOf(args[i]), 0), "i" + instruction_id++);
                        ValueRef store = LLVM.BuildStore(Builder, args[i], v);
                        if (Campy.Utils.Options.IsOn("jit_trace"))
                            System.Console.WriteLine(new VALUE(store));
                    }
                    ValueRef return_buffer = LLVM.BuildAlloca(Builder, return_type, "i" + instruction_id++);
                    LLVM.SetAlignment(return_buffer, 64);
                    var pt = LLVM.BuildPtrToInt(Builder, args[0], LLVM.Int64Type(), "i" + instruction_id++);
                    var pp = LLVM.BuildPtrToInt(Builder, arg_buffer, LLVM.Int64Type(), "i" + instruction_id++);
                    var pr = LLVM.BuildPtrToInt(Builder, return_buffer, LLVM.Int64Type(), "i" + instruction_id++);
                    internal_args[0] = pt;
                    internal_args[1] = pp;
                    internal_args[2] = pr;
                    this.DebuggerInfo();
                    var call = LLVM.BuildCall(Builder, ptr_method, internal_args, "");
                    if (has_return)
                    {
                        var load = LLVM.BuildLoad(Builder, return_buffer, "i" + instruction_id++);
                        state._stack.Push(new VALUE(load));
                    }
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(call.ToString());
                }
            }
        }
    }
    #endregion i_callvirt definition

    #region i_castclass definition
    public class i_castclass : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_castclass(b, i); }

        TypeReference call_closure_typetok = null;

        private i_castclass(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var typetok = Operand;
            var tr = typetok as TypeReference;
            var tr2 = tr.SwapInBclType();
            var v = tr2.Deresolve(this.Block._method_reference.DeclaringType, null);
            call_closure_typetok = v;
        }

        public override void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
        }
    }
    #endregion i_castclass definition

    #region i_ceq definition
    public class i_ceq : Cmp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ceq(b, i); }
        private i_ceq(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.eq; IsSigned = true; }
    }
    #endregion i_ceq definition

    #region i_cgt definition
    public class i_cgt : Cmp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_cgt(b, i); }
        private i_cgt(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.gt; IsSigned = true; }
    }
    #endregion i_cgt definition

    #region i_cgt_un definition
    public class i_cgt_un : Cmp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_cgt_un(b, i); }
        private i_cgt_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.gt; IsSigned = false; }
    }
    #endregion i_cgt_un definition

    #region i_ckfinite definition
    public class i_ckfinite : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ckfinite(b, i); }
        private i_ckfinite(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ckfinite definition

    #region i_clt definition
    public class i_clt : Cmp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_clt(b, i); }
        private i_clt(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.lt; IsSigned = true; }
    }
    #endregion i_clt definition

    #region i_clt_un definition
    public class i_clt_un : Cmp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_clt_un(b, i); }
        private i_clt_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { Predicate = PredicateType.lt; IsSigned = false; }
    }
    #endregion i_clt_un definition

    #region i_constrained definition
    public class i_constrained : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_constrained(b, i); }
        private i_constrained(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state) { }
        public override void Convert(STATE<VALUE, StackQueue<VALUE>> state) { }
    }
    #endregion i_constrained definition

    #region i_conv_i1 definition
    public class i_conv_i1 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_i1(b, i); }
        private i_conv_i1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(sbyte)); }
    }
    #endregion i_conv_i1 definition

    #region i_conv_i2 definition
    public class i_conv_i2 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_i2(b, i); }
        private i_conv_i2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(short)); }
    }
    #endregion i_conv_i2 definition

    #region i_conv_i4 definition
    public class i_conv_i4 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_i4(b, i); }
        private i_conv_i4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(int)); }
    }
    #endregion i_conv_i4 definition

    #region i_conv_i8 definition
    public class i_conv_i8 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_i8(b, i); }
        private i_conv_i8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_conv_i8 definition

    #region i_conv_i definition
    public class i_conv_i : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_i(b, i); }
        private i_conv_i(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_conv_i definition

    #region i_conv_ovf_i1 definition
    public class i_conv_ovf_i1 : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i1(b, i); }
        private i_conv_ovf_i1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(sbyte)); }
    }
    #endregion i_conv_ovf_i1 definition

    #region i_conv_ovf_i1_un definition
    public class i_conv_ovf_i1_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i1_un(b, i); }
        private i_conv_ovf_i1_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(sbyte)); }
    }
    #endregion i_conv_ovf_i1_un definition

    #region i_conv_ovf_i2 definition
    public class i_conv_ovf_i2 : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i2(b, i); }
        private i_conv_ovf_i2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(short)); }
    }
    #endregion i_conv_ovf_i2 definition

    #region i_conv_ovf_i2_un definition
    public class i_conv_ovf_i2_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i2_un(b, i); }
        private i_conv_ovf_i2_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(short)); }
    }
    #endregion i_conv_ovf_i2_un definition

    #region i_conv_ovf_i4 definition
    public class i_conv_ovf_i4 : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i4(b, i); }
        private i_conv_ovf_i4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(int)); }
    }
    #endregion i_conv_ovf_i4 definition

    #region i_conv_ovf_i4_un definition
    public class i_conv_ovf_i4_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i4_un(b, i); }
        private i_conv_ovf_i4_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(int)); }
    }
    #endregion i_conv_ovf_i4_un definition

    #region i_conv_ovf_i8 definition
    public class i_conv_ovf_i8 : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i8(b, i); }
        private i_conv_ovf_i8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_conv_ovf_i8 definition

    #region i_conv_ovf_i8_un definition
    public class i_conv_ovf_i8_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i8_un(b, i); }
        private i_conv_ovf_i8_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_conv_ovf_i8_un definition

    #region i_conv_ovf_i definition
    public class i_conv_ovf_i : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i(b, i); }
        private i_conv_ovf_i(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_conv_ovf_i definition

    #region i_conv_ovf_i_un definition
    public class i_conv_ovf_i_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_i_un(b, i); }
        private i_conv_ovf_i_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_conv_ovf_i_un definition

    #region i_conv_ovf_u1 definition
    public class i_conv_ovf_u1 : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u1(b, i); }
        private i_conv_ovf_u1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(byte)); }
    }
    #endregion i_conv_ovf_u1 definition

    #region i_conv_ovf_u1_un definition
    public class i_conv_ovf_u1_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u1_un(b, i); }
        private i_conv_ovf_u1_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(byte)); }
    }
    #endregion i_conv_ovf_u1_un definition

    #region i_conv_ovf_u2 definition
    public class i_conv_ovf_u2 : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u2(b, i); }
        private i_conv_ovf_u2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ushort)); }
    }
    #endregion i_conv_ovf_u2 definition

    #region i_conv_ovf_u2_un definition
    public class i_conv_ovf_u2_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u2_un(b, i); }
        private i_conv_ovf_u2_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ushort)); }
    }
    #endregion i_conv_ovf_u2_un definition

    #region i_conv_ovf_u4 definition
    public class i_conv_ovf_u4 : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u4(b, i); }
        private i_conv_ovf_u4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(uint)); }
    }
    #endregion i_conv_ovf_u4 definition

    #region i_conv_ovf_u4_un definition
    public class i_conv_ovf_u4_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u4_un(b, i); }
        private i_conv_ovf_u4_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(uint)); }
    }
    #endregion i_conv_ovf_u4_un definition

    #region i_conv_ovf_u8 definition
    public class i_conv_ovf_u8 : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u8(b, i); }
        private i_conv_ovf_u8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ulong)); }
    }
    #endregion i_conv_ovf_u8 definition

    #region i_conv_ovf_u8_un definition
    public class i_conv_ovf_u8_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u8_un(b, i); }
        private i_conv_ovf_u8_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ulong)); }
    }
    #endregion i_conv_ovf_u8_un definition

    #region i_conv_ovf_u definition
    public class i_conv_ovf_u : ConvOvf
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u(b, i); }
        private i_conv_ovf_u(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ulong)); }
    }
    #endregion i_conv_ovf_u definition

    #region i_conv_ovf_u_un definition
    public class i_conv_ovf_u_un : ConvOvfUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_ovf_u_un(b, i); }
        private i_conv_ovf_u_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ulong)); }
    }
    #endregion i_conv_ovf_u_un definition

    #region i_conv_r4 definition
    public class i_conv_r4 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_r4(b, i); }
        private i_conv_r4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(float)); }
    }
    #endregion i_conv_r4 definition

    #region i_conv_r8 definition
    public class i_conv_r8 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_r8(b, i); }
        private i_conv_r8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(double)); }
    }
    #endregion i_conv_r8 definition

    #region i_conv_r_un definition
    public class i_conv_r_un : ConvUns
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_r_un(b, i); }
        private i_conv_r_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(float)); }
    }
    #endregion i_conv_r_un definition

    #region i_conv_u1 definition
    public class i_conv_u1 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_u1(b, i); }
        private i_conv_u1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(byte)); }
    }
    #endregion i_conv_u1 definition

    #region i_conv_u2 definition
    public class i_conv_u2 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_u2(b, i); }
        private i_conv_u2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ushort)); }
    }
    #endregion i_conv_u2 definition

    #region i_conv_u4 definition
    public class i_conv_u4 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_u4(b, i); }
        private i_conv_u4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(uint)); }
    }
    #endregion i_conv_u4 definition

    #region i_conv_u8 definition
    public class i_conv_u8 : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_u8(b, i); }
        private i_conv_u8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ulong)); }
    }
    #endregion i_conv_u8 definition

    #region i_conv_u definition
    public class i_conv_u : Conv
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_conv_u(b, i); }
        private i_conv_u(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ulong)); }
    }
    #endregion i_conv_u definition

    #region i_cpblk definition
    public class i_cpblk : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_cpblk(b, i); }
        private i_cpblk(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_cpblk definition

    #region i_cpobj definition
    public class i_cpobj : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_cpobj(b, i); }
        private i_cpobj(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_cpobj definition

    #region i_div definition
    public class i_div : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_div(b, i); }
        private i_div(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_div definition

    #region i_div_un definition
    public class i_div_un : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_div_un(b, i); }
        private i_div_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_div_un definition

    #region i_dup definition
    public class i_dup : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_dup(b, i); }
        private i_dup(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var rhs = state._stack.Pop();
            state._stack.Push(rhs);
            state._stack.Push(rhs);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var rhs = state._stack.Pop();
            state._stack.Push(rhs);
            state._stack.Push(rhs);
        }

    }
    #endregion i_dup definition

    #region i_endfilter definition
    public class i_endfilter : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_endfilter(b, i); }
        private i_endfilter(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_endfilter definition

    #region i_endfinally definition
    public class i_endfinally : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_endfinally(b, i); }
        private i_endfinally(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // leave.* page 372 of ecma 335
            var edges = Block._graph.SuccessorEdges(Block).ToList();
            if (edges.Count > 1)
                throw new Exception("There shouldn't be more than one edge from a leave instruction.");
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // leave.* page 372 of ecma 335
            var edge = Block._graph.SuccessorEdges(Block).ToList()[0];
            var s = edge.To;
            // Build a branch to appease LLVM. CUDA does not seem to support exception handling.
            var br = LLVM.BuildBr(Builder, s.LlvmInfo.BasicBlock);
        }
    }
    #endregion i_endfinally definition

    #region i_initblk definition
    public class i_initblk : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_initblk(b, i); }
        private i_initblk(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_initblk definition

    #region i_initobj definition
    public class i_initobj : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_initobj(b, i); }
        private i_initobj(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // initobj – initialize the value at an address page 400
            var dst = state._stack.Pop();
        }

        public override void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // initobj – initialize the value at an address page 400
            var dst = state._stack.Pop();
            var typetok = this.Operand as TypeReference;
            if (typetok == null) throw new Exception("Unknown operand for instruction: " + this.Instruction);
            if (typetok.IsStruct())
            {

            }
            else if (typetok.IsValueType)
            {

            }
            else
            {
                var pt = LLVM.TypeOf(dst.V);
                var t = LLVM.GetElementType(pt);
                ValueRef nul = LLVM.ConstPointerNull(t);
                var v = new VALUE(nul);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(v);
                var zz = LLVM.BuildStore(Builder, v.V, dst.V);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine("Store = " + new VALUE(zz).ToString());
            }
        }
    }
    #endregion i_initobj definition

    #region i_isinst definition
    public class i_isinst : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_isinst(b, i); }
        private i_isinst(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        TypeReference call_closure_typetok = null;

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // isinst – test if an object is an instance of a class or interface, page 401
            var typetok = Operand;
            var tr = typetok as TypeReference;
            var tr2 = tr.SwapInBclType();
            var v = tr2.Deresolve(this.Block._method_reference.DeclaringType, null);
            call_closure_typetok = v;
            state._stack.Pop();
            state._stack.Push(v);
        }

        public override void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // isinst – test if an object is an instance of a class or interface, page 401
            var typetok = Operand;
            var tr = typetok as TypeReference;
            var tr2 = tr.SwapInBclType();
            var v = tr2.Deresolve(this.Block._method_reference.DeclaringType, null);
            // No change for now.
        }
    }
    #endregion i_isinst definition

    #region i_jmp definition
    public class i_jmp : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_jmp(b, i); }
        private i_jmp(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_jmp definition

    #region i_ldarg definition
    public class i_ldarg : LdArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldarg(b, i); }
        private i_ldarg(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldarg definition

    #region i_ldarg_0 definition
    public class i_ldarg_0 : LdArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldarg_0(b, i); }
        private i_ldarg_0(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 0) { }
    }
    #endregion i_ldarg_0 definition

    #region i_ldarg_1 definition
    public class i_ldarg_1 : LdArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldarg_1(b, i); }
        private i_ldarg_1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 1) { }
    }
    #endregion i_ldarg_1 definition

    #region i_ldarg_2 definition
    public class i_ldarg_2 : LdArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldarg_2(b, i); }
        private i_ldarg_2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 2) { }
    }
    #endregion i_ldarg_2 definition

    #region i_ldarg_3 definition
    public class i_ldarg_3 : LdArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldarg_3(b, i); }
        private i_ldarg_3(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 3) { }
    }
    #endregion i_ldarg_3 definition

    #region i_ldarg_3 definition
    public class i_ldarg_s : LdArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldarg_s(b, i); }
        private i_ldarg_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldarg_3 definition

    #region i_ldarga definition
    public class i_ldarga : LdArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldarga(b, i); }
        private i_ldarga(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldarga definition

    #region i_ldarga_s definition
    public class i_ldarga_s : LdArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldarga_s(b, i); }
        private i_ldarga_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldarga_s definition

    #region i_ldc_i4 definition
    public class i_ldc_i4 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4(b, i); }
        private i_ldc_i4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            int arg = default(int);
            object o = i.Operand;
            if (o != null)
            {
                // Fuck C# casting in the way of just getting
                // a plain ol' int.
                for (;;)
                {
                    bool success = false;
                    try
                    {
                        byte? o3 = (byte?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        sbyte? o3 = (sbyte?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        short? o3 = (short?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        ushort? o3 = (ushort?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        int? o3 = (int?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    throw new Exception("Cannot convert ldc_i4. Unknown type of operand. F... Mono.");
                }
            }
            _arg = arg;
        }
    }
    #endregion i_ldc_i4 definition

    #region i_ldc_i4_0 definition
    public class i_ldc_i4_0 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_0(b, i); }
        private i_ldc_i4_0(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = 0; _arg = arg; }
    }
    #endregion i_ldc_i4_0 definition

    #region i_ldc_i4_1 definition
    public class i_ldc_i4_1 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_1(b, i); }
        private i_ldc_i4_1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = 1; _arg = arg; }
    }
    #endregion i_ldc_i4_1 definition

    #region i_ldc_i4_2 definition
    public class i_ldc_i4_2 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_2(b, i); }
        private i_ldc_i4_2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = 2; _arg = arg; }
    }
    #endregion i_ldc_i4_2 definition

    #region i_ldc_i4_3 definition
    public class i_ldc_i4_3 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_3(b, i); }
        private i_ldc_i4_3(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = 3; _arg = arg; }
    }
    #endregion i_ldc_i4_3 definition

    #region i_ldc_i4_4 definition
    public class i_ldc_i4_4 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_4(b, i); }
        private i_ldc_i4_4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = 4; _arg = arg; }
    }
    #endregion i_ldc_i4_4 definition

    #region i_ldc_i4_5 definition
    public class i_ldc_i4_5 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_5(b, i); }
        private i_ldc_i4_5(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = 5; _arg = arg; }
    }
    #endregion i_ldc_i4_5 definition

    #region i_ldc_i4_6 definition
    public class i_ldc_i4_6 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_6(b, i); }
        private i_ldc_i4_6(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = 6; _arg = arg; }
    }
    #endregion i_ldc_i4_6 definition

    #region i_ldc_i4_7 definition
    public class i_ldc_i4_7 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_7(b, i); }
        private i_ldc_i4_7(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = 7; _arg = arg; }
    }
    #endregion i_ldc_i4_7 definition

    #region i_ldc_i4_8 definition
    public class i_ldc_i4_8 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_8(b, i); }
        private i_ldc_i4_8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = 8; _arg = arg; }
    }
    #endregion i_ldc_i4_8 definition

    #region i_ldc_i4_m1 definition
    public class i_ldc_i4_m1 : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_m1(b, i); }
        private i_ldc_i4_m1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { int arg = -1; _arg = arg; }
    }
    #endregion i_ldc_i4_m1 definition

    #region i_ldc_i4_s definition
    public class i_ldc_i4_s : LDCInstI4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i4_s(b, i); }
        private i_ldc_i4_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            int arg = default(int);
            object o = i.Operand;
            if (o != null)
            {
                // Fuck C# casting in the way of just getting
                // a plain ol' int.
                for (;;)
                {
                    bool success = false;
                    try
                    {
                        byte? o3 = (byte?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        sbyte? o3 = (sbyte?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        short? o3 = (short?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        ushort? o3 = (ushort?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        int? o3 = (int?)o;
                        arg = (int)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    throw new Exception("Cannot convert ldc_i4. Unknown type of operand. F... Mono.");
                }
            }
            _arg = arg;
        }
    }
    #endregion i_ldc_i4_s definition

    #region i_ldc_i8 definition
    public class i_ldc_i8 : LDCInstI8
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_i8(b, i); }
        private i_ldc_i8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            Int64 arg = default(Int64);
            object o = i.Operand;
            if (o != null)
            {
                // Fuck C# casting in the way of just getting
                // a plain ol' int.
                for (;;)
                {
                    bool success = false;
                    try
                    {
                        byte? o3 = (byte?)o;
                        arg = (Int64)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        sbyte? o3 = (sbyte?)o;
                        arg = (Int64)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        short? o3 = (short?)o;
                        arg = (Int64)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        ushort? o3 = (ushort?)o;
                        arg = (Int64)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        int? o3 = (int?)o;
                        arg = (Int64)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        Int64? o3 = (Int64?)o;
                        arg = (Int64)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    throw new Exception("Cannot convert ldc_i4. Unknown type of operand. F... Mono.");
                }
            }
            _arg = arg;
        }
    }
    #endregion i_ldc_i8 definition

    #region i_ldc_r4 definition
    public class i_ldc_r4 : LDCInstR4
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_r4(b, i); }
        private i_ldc_r4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            Single arg = default(Single);
            object o = i.Operand;
            if (o != null)
            {
                // Fuck C# casting in the way of just getting
                // a plain ol' int.
                for (;;)
                {
                    bool success = false;
                    try
                    {
                        byte? o3 = (byte?)o;
                        arg = (Single)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        sbyte? o3 = (sbyte?)o;
                        arg = (Single)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        short? o3 = (short?)o;
                        arg = (Single)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        ushort? o3 = (ushort?)o;
                        arg = (Single)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        int? o3 = (int?)o;
                        arg = (Single)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        Single? o3 = (Single?)o;
                        arg = (Single)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    throw new Exception("Cannot convert ldc_i4. Unknown type of operand. F... Mono.");
                }
            }
            _arg = arg;
        }
    }
    #endregion i_ldc_r4 definition

    #region i_ldc_r8 definition
    public class i_ldc_r8 : LDCInstR8
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldc_r8(b, i); }
        private i_ldc_r8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            Double arg = default(Double);
            object o = i.Operand;
            if (o != null)
            {
                // Fuck C# casting in the way of just getting
                // a plain ol' int.
                for (;;)
                {
                    bool success = false;
                    try
                    {
                        byte? o3 = (byte?)o;
                        arg = (Double)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        sbyte? o3 = (sbyte?)o;
                        arg = (Double)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        short? o3 = (short?)o;
                        arg = (Double)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        ushort? o3 = (ushort?)o;
                        arg = (Double)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        int? o3 = (int?)o;
                        arg = (Double)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        Single? o3 = (Single?)o;
                        arg = (Double)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    try
                    {
                        Double? o3 = (Double?)o;
                        arg = (Double)o3;
                        success = true;
                    }
                    catch { }
                    if (success) break;
                    throw new Exception("Cannot convert ldc_i4. Unknown type of operand. F... Mono.");
                }
            }
            _arg = arg;
        }
    }
    #endregion i_ldc_r8 definition

    #region i_ldelem_any definition
    public class i_ldelem_any : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_any(b, i); }
        private i_ldelem_any(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldelem_any definition

    #region i_ldelem_i1 definition
    public class i_ldelem_i1 : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_i1(b, i); }
        private i_ldelem_i1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(sbyte)); }
    }
    #endregion i_ldelem_i1 definition

    #region i_ldelem_i2 definition
    public class i_ldelem_i2 : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_i2(b, i); }
        private i_ldelem_i2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(short)); }
    }
    #endregion i_ldelem_i2 definition

    #region i_ldelem_i4 definition
    public class i_ldelem_i4 : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_i4(b, i); }
        private i_ldelem_i4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(int)); }
    }
    #endregion i_ldelem_i4 definition

    #region i_ldelem_i8 definition
    public class i_ldelem_i8 : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_i8(b, i); }
        private i_ldelem_i8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_ldelem_i8 definition

    #region i_ldelem_i definition
    public class i_ldelem_i : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_i(b, i); }
        private i_ldelem_i(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_ldelem_i definition

    #region i_ldelem_r4 definition
    public class i_ldelem_r4 : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_r4(b, i); }
        private i_ldelem_r4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(float)); }
    }
    #endregion i_ldelem_r4 definition

    #region i_ldelem_r8 definition
    public class i_ldelem_r8 : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_r8(b, i); }
        private i_ldelem_r8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(double)); }
    }
    #endregion i_ldelem_r8 definition

    #region i_ldelem_ref definition
    public class i_ldelem_ref : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_ref(b, i); }
        private i_ldelem_ref(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldelem_ref definition

    #region i_ldelem_u1 definition
    public class i_ldelem_u1 : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_u1(b, i); }
        private i_ldelem_u1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(byte)); }
    }
    #endregion i_ldelem_u1 definition

    #region i_ldelem_u2 definition
    public class i_ldelem_u2 : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_u2(b, i); }
        private i_ldelem_u2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ushort)); }
    }
    #endregion i_ldelem_u2 definition

    #region i_ldelem_u4 definition
    public class i_ldelem_u4 : LdElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelem_u4(b, i); }
        private i_ldelem_u4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(uint)); }
    }
    #endregion i_ldelem_u4 definition

    #region i_ldelema definition
    public class i_ldelema : LdElemA
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldelema(b, i); }
        private i_ldelema(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldelema definition

    #region i_ldfld definition
    public class i_ldfld : INST
    {
        TypeReference call_closure_field_type;
        TypeReference call_closure_object = null;

        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldfld(b, i); }

        private i_ldfld(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // ldfld, page 406 of ecma 335
            var o = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(o.ToString());
            var operand = this.Instruction.Operand;
            var field_reference = operand as Mono.Cecil.FieldReference;
            if (field_reference == null) throw new Exception("Cannot convert ldfld.");
            var field = field_reference.FieldType.InstantiateGeneric(this.Block._method_reference);
            call_closure_field_type = field;
            call_closure_object = o;
            state._stack.Push(field);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // ldfld, page 405 of ecma 335
            VALUE v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(v);
            TypeRef tr = LLVM.TypeOf(v.V);
            bool isPtr = v.T.isPointerTy();
            bool is_ptr = false;
            IntPtr tr_bcltype = IntPtr.Zero;
            if (call_closure_object as PointerType != null)
            {
                var pointer = call_closure_object as PointerType;
                var element = pointer.ElementType;
                tr_bcltype = RUNTIME.MonoBclMap_GetBcl(element);
            }
            else if (call_closure_object as ByReferenceType != null)
            {
                var pointer = call_closure_object as ByReferenceType;
                var element = pointer.ElementType;
                tr_bcltype = RUNTIME.MonoBclMap_GetBcl(element);
            }
            else
                tr_bcltype = RUNTIME.MonoBclMap_GetBcl(call_closure_object);


            ValueRef load;
            if (isPtr)
            {
                uint offset = 0;
                object yy = this.Instruction.Operand;
                FieldReference field_reference = yy as Mono.Cecil.FieldReference;
                if (yy == null) throw new Exception("Cannot convert.");

                // The instruction may be generic, even if the method
                // is an instance. Convert field to generic instance type reference
                // if it is a generic, in the context of this basic block.

                TypeReference declaring_type_tr = field_reference.DeclaringType;
                TypeDefinition declaring_type = declaring_type_tr.Resolve();

                if (!declaring_type.IsGenericInstance && declaring_type.HasGenericParameters)
                {
                    // This is a red flag. We need to come up with a generic instance for type.
                    declaring_type_tr = this.Block._method_reference.DeclaringType;
                }

                // need to take into account padding fields. Unfortunately,
                // LLVM does not name elements in a struct/class. So, we must
                // compute padding and adjust.
				int size = 0;
				var myfields = declaring_type.MyGetFields();
				int current_offset = 0;
				foreach (var field in myfields)
				{
					var attr = field.Resolve().Attributes;
					if ((attr & FieldAttributes.Static) != 0)
						continue;
					var bcl_field = RUNTIME.BclFindFieldInTypeAll(tr_bcltype, field.Name);
					int field_size = RUNTIME.BclGetFieldSize(bcl_field);
					int field_offset = RUNTIME.BclGetFieldOffset(bcl_field);
					int padding = field_offset - current_offset;
					size = size + padding + field_size;
					if (padding != 0)
					{
						// Add in bytes to effect padding.
						for (int j = 0; j < padding; ++j)
							offset++;
					}
					if (field.Name == field_reference.Name)
					{
						is_ptr = field.FieldType.IsArray || field.FieldType.IsPointer;
						break;
					}
					offset++;
					current_offset = field_offset + field_size;
				}

                var tt = LLVM.TypeOf(v.V);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(LLVM.PrintTypeToString(tt));

                var addr = LLVM.BuildStructGEP(Builder, v.V, offset, "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(addr));

                load = LLVM.BuildLoad(Builder, addr, "i" + instruction_id++);
            }
            else
            {
                uint offset = 0;
                var yy = this.Instruction.Operand;
                var field_reference = yy as Mono.Cecil.FieldReference;
                if (yy == null) throw new Exception("Cannot convert.");
                var declaring_type_tr = field_reference.DeclaringType;
                var declaring_type = declaring_type_tr.Resolve();

                // need to take into account padding fields. Unfortunately,
                // LLVM does not name elements in a struct/class. So, we must
                // compute padding and adjust.
				int size = 0;
				var myfields = declaring_type.MyGetFields();
				int current_offset = 0;
				foreach (var field in myfields)
				{
					var attr = field.Resolve().Attributes;
					if ((attr & FieldAttributes.Static) != 0)
						continue;
					var bcl_field = RUNTIME.BclFindFieldInTypeAll(tr_bcltype, field.Name);
					int field_size = RUNTIME.BclGetFieldSize(bcl_field);
					int field_offset = RUNTIME.BclGetFieldOffset(bcl_field);
					int padding = field_offset - current_offset;
					size = size + padding + field_size;
					if (padding != 0)
					{
						// Add in bytes to effect padding.
						for (int j = 0; j < padding; ++j)
							offset++;
					}
					if (field.Name == field_reference.Name)
					{
						is_ptr = field.FieldType.IsArray || field.FieldType.IsPointer;
						break;
					}
					offset++;
					current_offset = field_offset + field_size;
				}

                var tt = LLVM.TypeOf(v.V);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(LLVM.PrintTypeToString(tt));

                load = LLVM.BuildExtractValue(Builder, v.V, offset, "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(load));

            }

            ValueRef src = load;
            VALUE dst = new VALUE(src);
            var stype = LLVM.TypeOf(src);
            // Load is a StorageTypeLLVM. Convert that to an IntermediateType.
            var dtype = new TYPE(call_closure_field_type).IntermediateTypeLLVM;
            bool is_unsigned = call_closure_field_type.IsUnsigned();
            // Widen or trunc value.
            if (stype != dtype)
            {
                bool ext = false;

                /* Extend */
                if (dtype == LLVM.Int64Type()
                    && (stype == LLVM.Int32Type() || stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
                    ext = true;
                else if (dtype == LLVM.Int32Type()
                    && (stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
                    ext = true;
                else if (dtype == LLVM.Int16Type()
                    && (stype == LLVM.Int8Type()))
                    ext = true;

                if (ext)
                    dst = new VALUE(
                       is_unsigned
                        ? LLVM.BuildZExt(Builder, src, dtype, "i" + instruction_id++)
                        : LLVM.BuildSExt(Builder, src, dtype, "i" + instruction_id++));
                else if (dtype == LLVM.DoubleType() && stype == LLVM.FloatType())
                    dst = new VALUE(LLVM.BuildFPExt(Builder, src, dtype, "i" + instruction_id++));
                else /* Trunc */ if (stype == LLVM.Int64Type()
                    && (dtype == LLVM.Int32Type() || dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
                    dst = new VALUE(LLVM.BuildTrunc(Builder, src, dtype, "i" + instruction_id++));
                else if (stype == LLVM.Int32Type()
                    && (dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
                    dst = new VALUE(LLVM.BuildTrunc(Builder, src, dtype, "i" + instruction_id++));
                else if (stype == LLVM.Int16Type()
                    && dtype == LLVM.Int8Type())
                    dst = new VALUE(LLVM.BuildTrunc(Builder, src, dtype, "i" + instruction_id++));
                else if (stype == LLVM.DoubleType()
                    && dtype == LLVM.FloatType())
                    dst = new VALUE(LLVM.BuildFPTrunc(Builder, src, dtype, "i" + instruction_id++));

                else if (stype == LLVM.Int64Type()
                    && (dtype == LLVM.FloatType()))
                    dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                else if (stype == LLVM.Int32Type()
                    && (dtype == LLVM.FloatType()))
                    dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                else if (stype == LLVM.Int64Type()
                    && (dtype == LLVM.DoubleType()))
                    dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));
                else if (stype == LLVM.Int32Type()
                    && (dtype == LLVM.DoubleType()))
                    dst = new VALUE(LLVM.BuildSIToFP(Builder, src, dtype, "i" + instruction_id++));

				load = dst.V;
			}

            // If load result is a pointer, then cast it to proper type.
            // This is because I had to avoid recursive data types in classes
            // as LLVM cannot handle these at all. So, all pointer types
            // were defined as void* in the LLVM field.

			var load_value = new VALUE(load);
            bool isPtrLoad = load_value.T.isPointerTy();
            if (isPtrLoad)
            {
                var mono_field_type = call_closure_field_type;
                TypeRef type = mono_field_type.ToTypeRef();
                load = LLVM.BuildBitCast(Builder,
                    load, type, "i" + instruction_id++);
				load_value = new VALUE(load);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(load_value);
            }

            state._stack.Push(load_value);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(load_value);
        }
    }
    #endregion i_ldfld definition

    #region i_ldflda definition
    public class i_ldflda : INST
    {
        TypeReference call_closure_field_type;

        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldflda(b, i); }

        private i_ldflda(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // ldflda, page 407 of ecma 335
            var v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(v.ToString());
            var operand = this.Instruction.Operand;
            var field = operand as Mono.Cecil.FieldReference;
            if (field == null) throw new Exception("Cannot convert ldfld.");
            var type = field.FieldType.Deresolve(this.Block._method_reference.DeclaringType, null);
            var value = new ByReferenceType(type);
            call_closure_field_type = value;
            state._stack.Push(value);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // ldflda, page 407 of ecma 335
            VALUE v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(v);
            TypeRef tr = LLVM.TypeOf(v.V);
            bool isPtr = v.T.isPointerTy();
            bool isArr = v.T.isArrayTy();
            bool isSt = v.T.isStructTy();
            bool is_ptr = false;
            if (isPtr)
            {
                uint offset = 0;
                object yy = this.Instruction.Operand;
                FieldReference field_reference = yy as Mono.Cecil.FieldReference;

                // The instruction may be generic, even if the method
                // is an instance. Convert field to generic instance type reference
                // if it is a generic, in the context of this basic block.

                TypeReference declaring_type_tr = field_reference.DeclaringType;
                TypeDefinition declaring_type = declaring_type_tr.Resolve();

                if (!declaring_type.IsGenericInstance && declaring_type.HasGenericParameters)
                {
                    // This is a red flag. We need to come up with a generic instance for type.
                    declaring_type_tr = this.Block._method_reference.DeclaringType;
                }
                var tr_bcltype = RUNTIME.MonoBclMap_GetBcl(declaring_type_tr);

                // need to take into account padding fields. Unfortunately,
                // LLVM does not name elements in a struct/class. So, we must
                // compute padding and adjust.
                int size = 0;
				var myfields = declaring_type.MyGetFields();
				int current_offset = 0;
				foreach (var field in myfields)
				{
					var attr = field.Resolve().Attributes;
					if ((attr & FieldAttributes.Static) != 0)
						continue;
					var bcl_field = RUNTIME.BclFindFieldInTypeAll(tr_bcltype, field.Name);
					int field_size = RUNTIME.BclGetFieldSize(bcl_field);
					int field_offset = RUNTIME.BclGetFieldOffset(bcl_field);
					int padding = field_offset - current_offset;
					size = size + padding + field_size;
					if (padding != 0)
					{
						// Add in bytes to effect padding.
						for (int j = 0; j < padding; ++j)
							offset++;
					}
					if (field.Name == field_reference.Name)
					{
						is_ptr = field.FieldType.IsArray || field.FieldType.IsPointer;
						break;
					}
					offset++;
					current_offset = field_offset + field_size;
				}

                var tt = LLVM.TypeOf(v.V);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(LLVM.PrintTypeToString(tt));

                var addr = LLVM.BuildStructGEP(Builder, v.V, offset, "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(addr));

                //var load = LLVM.BuildLoad(Builder, addr, "i" + instruction_id++);
                //if (Campy.Utils.Options.IsOn("jit_trace"))
                //    System.Console.WriteLine(new VALUE(load));


                //var you = Converter.FromGenericParameterToTypeReference(field.FieldType,
                //    declaring_type_tr as GenericInstanceType);
                //// Add extra load for pointer types like objects and arrays.
                //var array_or_classyou  = (you.IsArray || !you.IsValueType);
                //if (array_or_classyou)
                //{
                //    load = LLVM.BuildLoad(Builder, load, "");
                //    if (Campy.Utils.Options.IsOn("jit_trace"))
                //        System.Console.WriteLine(new Value(load));
                //}

                bool xInt = LLVM.GetTypeKind(tt) == TypeKind.IntegerTypeKind;
                bool xP = LLVM.GetTypeKind(tt) == TypeKind.PointerTypeKind;
                bool xA = LLVM.GetTypeKind(tt) == TypeKind.ArrayTypeKind;

                // If load result is a pointer, then cast it to proper type.
                // This is because I had to avoid recursive data types in classes
                // as LLVM cannot handle these at all. So, all pointer types
                // were defined as void* in the LLVM field.

                //var load_value = new VALUE(load);
                //bool isPtrLoad = load_value.T.isPointerTy();
                //if (isPtrLoad)
                //{
                //    var mono_field_type = field.FieldType;
                //    TypeRef type = Converter.ToTypeRef(
                //        mono_field_type,
                //        Block.OpsFromOriginal);
                //    load = LLVM.BuildBitCast(Builder,
                //        load, type, "");
                //    if (Campy.Utils.Options.IsOn("jit_trace"))
                //        System.Console.WriteLine(new Value(load));
                //}

                state._stack.Push(new VALUE(addr));
            }
            else
            {
                uint offset = 0;
                var yy = this.Instruction.Operand;
                var field_reference = yy as Mono.Cecil.FieldReference;
                if (yy == null) throw new Exception("Cannot convert.");
                var declaring_type_tr = field_reference.DeclaringType;
                var declaring_type = declaring_type_tr.Resolve();
                var tr_bcltype = RUNTIME.MonoBclMap_GetBcl(declaring_type_tr);

                // need to take into account padding fields. Unfortunately,
                // LLVM does not name elements in a struct/class. So, we must
                // compute padding and adjust.
                int size = 0;
				var myfields = declaring_type.MyGetFields();
				int current_offset = 0;
				foreach (var field in myfields)
				{
					var attr = field.Resolve().Attributes;
					if ((attr & FieldAttributes.Static) != 0)
						continue;
					var bcl_field = RUNTIME.BclFindFieldInTypeAll(tr_bcltype, field.Name);
					int field_size = RUNTIME.BclGetFieldSize(bcl_field);
					int field_offset = RUNTIME.BclGetFieldOffset(bcl_field);
					int padding = field_offset - current_offset;
					size = size + padding + field_size;
					if (padding != 0)
					{
						// Add in bytes to effect padding.
						for (int j = 0; j < padding; ++j)
							offset++;
					}
					if (field.Name == field_reference.Name)
					{
						is_ptr = field.FieldType.IsArray || field.FieldType.IsPointer;
						break;
					}
					offset++;
					current_offset = field_offset + field_size;
				}

				var tt = LLVM.TypeOf(v.V);
				if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(LLVM.PrintTypeToString(tt));

                var addr = LLVM.BuildStructGEP(Builder, v.V, offset, "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(addr));

                //var load = LLVM.BuildExtractValue(Builder, v.V, offset, "i" + instruction_id++);
                //if (Campy.Utils.Options.IsOn("jit_trace"))
                //    System.Console.WriteLine(new VALUE(load));

                bool xInt = LLVM.GetTypeKind(tt) == TypeKind.IntegerTypeKind;
                bool xP = LLVM.GetTypeKind(tt) == TypeKind.PointerTypeKind;
                bool xA = LLVM.GetTypeKind(tt) == TypeKind.ArrayTypeKind;

                // If load result is a pointer, then cast it to proper type.
                // This is because I had to avoid recursive data types in classes
                // as LLVM cannot handle these at all. So, all pointer types
                // were defined as void* in the LLVM field.

                //var load_value = new VALUE(load);
                //bool isPtrLoad = load_value.T.isPointerTy();
                //if (isPtrLoad)
                //{
                //    var mono_field_type = field.FieldType;
                //    TypeRef type = mono_field_type.ToTypeRef();
                //    load = LLVM.BuildBitCast(Builder,
                //        load, type, "i" + instruction_id++);
                //    if (Campy.Utils.Options.IsOn("jit_trace"))
                //        System.Console.WriteLine(new VALUE(load));
                //}

                state._stack.Push(new VALUE(addr));
            }
        }
    }
    #endregion i_ldflda definition

    #region i_ldftn definition
    public class i_ldftn : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldftn(b, i); }

        private i_ldftn(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            state._stack.Push(typeof(System.UInt32).ToMonoTypeReference());
        }
    }
    #endregion i_ldftn definition

    #region i_ldind_i1 definition
    public class i_ldind_i1 : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_i1(b, i); }
        private i_ldind_i1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(sbyte)); }
    }
    #endregion i_ldind_i1 definition

    #region i_ldind_i2 definition
    public class i_ldind_i2 : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_i2(b, i); }
        private i_ldind_i2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(short)); }
    }
    #endregion i_ldind_i2 definition

    #region i_ldind_i4 definition
    public class i_ldind_i4 : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_i4(b, i); }
        private i_ldind_i4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(int)); }
    }
    #endregion i_ldind_i4 definition

    #region i_ldind_i8 definition
    public class i_ldind_i8 : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_i8(b, i); }
        private i_ldind_i8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_ldind_i8 definition

    #region i_ldind_i definition
    public class i_ldind_i : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_i(b, i); }
        private i_ldind_i(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_ldind_i definition

    #region i_ldind_r4 definition
    public class i_ldind_r4 : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_r4(b, i); }
        private i_ldind_r4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(float)); }
    }
    #endregion i_ldind_r4 definition

    #region i_ldind_r8 definition
    public class i_ldind_r8 : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_r8(b, i); }
        private i_ldind_r8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(double)); }
    }
    #endregion i_ldind_r8 definition

    #region i_ldind_ref definition
    public class i_ldind_ref : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_ref(b, i); }
        private i_ldind_ref(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = null; }
    }
    #endregion i_ldind_ref definition

    #region i_ldind_u1 definition
    public class i_ldind_u1 : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_u1(b, i); }
        private i_ldind_u1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(byte)); }
    }
    #endregion i_ldind_u1 definition

    #region i_ldind_u2 definition
    public class i_ldind_u2 : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_u2(b, i); }
        private i_ldind_u2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(ushort)); }
    }
    #endregion i_ldind_u2 definition

    #region i_ldind_u4 definition
    public class i_ldind_u4 : LdInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldind_u4(b, i); }
        private i_ldind_u4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(uint)); }
    }
    #endregion i_ldind_u4 definition

    #region i_ldlen definition
    public class i_ldlen : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldlen(b, i); }
        private i_ldlen(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var a = state._stack.Pop();
            state._stack.Push(typeof(System.UInt32).ToMonoTypeReference());
        }

        // For array implementation, see https://www.codeproject.com/Articles/3467/Arrays-UNDOCUMENTED

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            //VALUE v = state._stack.Pop();
            //if (Campy.Utils.Options.IsOn("jit_trace"))
            //    System.Console.WriteLine(v);

            //var load = v.V;
            //load = LLVM.BuildLoad(Builder, load, "i" + instruction_id++);
            //if (Campy.Utils.Options.IsOn("jit_trace"))
            //    System.Console.WriteLine(new VALUE(load));

            // The length of an array is the product of all dimensions, but this instruction
            // is only used for 1d arrays.

            //// Load len.
            //load = LLVM.BuildExtractValue(Builder, load, 2, "i" + instruction_id++);
            //if (Campy.Utils.Options.IsOn("jit_trace"))
            //    System.Console.WriteLine(new VALUE(load));

            //load = LLVM.BuildTrunc(Builder, load, LLVM.Int32Type(), "i" + instruction_id++);
            //if (Campy.Utils.Options.IsOn("jit_trace"))
            //    System.Console.WriteLine(new VALUE(load));

            {
                // Call PTX method.

                var ret = true;
                var HasScalarReturnValue = true;
                var HasStructReturnValue = false;
                var HasThis = true;
                var NumberOfArguments = 0
                                      + (HasThis ? 1 : 0)
                                      + (HasStructReturnValue ? 1 : 0);
                int locals = 0;
                var NumberOfLocals = locals;
                int xret = (HasScalarReturnValue || HasStructReturnValue) ? 1 : 0;
                int xargs = NumberOfArguments;

                BuilderRef bu = this.Builder;

                string demangled_name = "_Z31System_Array_Internal_GetLengthPhS_S_";
                string full_name = "System.Int32 System.Array::Internal_GetLength()";
                // Find the specific function called.
                var xx = RUNTIME._bcl_runtime_csharp_internal_to_valueref.Where(
                    t =>
                        t.Key.Contains(demangled_name)
                         || demangled_name.Contains(t.Key));
                var first_kv_pair = xx.FirstOrDefault();
                ValueRef fv = first_kv_pair.Value;
                var t_fun = LLVM.TypeOf(fv);
                var t_fun_con = LLVM.GetTypeContext(t_fun);
                var context = LLVM.GetModuleContext(RUNTIME.global_llvm_module);

                RUNTIME.BclNativeMethod mat = null;
                foreach (RUNTIME.BclNativeMethod ci in RUNTIME.BclNativeMethods)
                {
                    if (ci._full_name == full_name)
                    {
                        mat = ci;
                        break;
                    }
                }

                {
                    ValueRef[] args = new ValueRef[3];

                    // Set up "this".
                    ValueRef nul = LLVM.ConstPointerNull(LLVM.PointerType(LLVM.VoidType(), 0));
                    VALUE t = new VALUE(nul);

                    // Pop all parameters and stuff into params buffer. Note, "this" and
                    // "return" are separate parameters in GPU BCL runtime C-functions,
                    // unfortunately, reminates of the DNA runtime I decided to use.
                    var entry = this.Block.Entry.LlvmInfo.BasicBlock;
                    var beginning = LLVM.GetFirstInstruction(entry);
                    //LLVM.PositionBuilderBefore(Builder, beginning);
                    var parameter_type = LLVM.ArrayType(LLVM.Int64Type(), (uint)0);
                    var param_buffer = LLVM.BuildAlloca(Builder, parameter_type, "i" + instruction_id++);
                    LLVM.SetAlignment(param_buffer, 64);
                    //LLVM.PositionBuilderAtEnd(Builder, this.Block.BasicBlock);
                    var base_of_parameters = LLVM.BuildPointerCast(Builder, param_buffer,
                        LLVM.PointerType(LLVM.Int64Type(), 0), "i" + instruction_id++);

                    if (HasThis)
                    {
                        t = state._stack.Pop();
                        var ll = t.V;
                        //ll = LLVM.BuildLoad(Builder, ll, "i" + instruction_id++);
                        if (Campy.Utils.Options.IsOn("jit_trace"))
                            System.Console.WriteLine(new VALUE(ll));
                        t = new VALUE(ll);
                    }

                    // Set up return. For now, always allocate buffer.
                    // Note function return is type of third parameter.
                    var return_type = mat._returnType.ToTypeRef();
                    var return_buffer = LLVM.BuildAlloca(Builder, return_type, "i" + instruction_id++);
                    LLVM.SetAlignment(return_buffer, 64);
                    //LLVM.PositionBuilderAtEnd(Builder, this.Block.BasicBlock);

                    // Set up call.
                    var pt = LLVM.BuildPtrToInt(Builder, t.V, LLVM.Int64Type(), "i" + instruction_id++);
                    var pp = LLVM.BuildPtrToInt(Builder, param_buffer, LLVM.Int64Type(), "i" + instruction_id++);
                    var pr = LLVM.BuildPtrToInt(Builder, return_buffer, LLVM.Int64Type(), "i" + instruction_id++);

                    args[0] = pt;
                    args[1] = pp;
                    args[2] = pr;

                    var call = LLVM.BuildCall(Builder, fv, args, "");

                    if (ret)
                    {
                        var load = LLVM.BuildLoad(Builder, return_buffer, "i" + instruction_id++);
                        //var load = LLVM.ConstInt(LLVM.Int32Type(), 11, false);
                        state._stack.Push(new VALUE(load));
                    }

                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(call.ToString());
                }
            }

            //state._stack.Push(new VALUE(load));
        }
    }
    #endregion i_ldlen definition

    #region i_ldloc definition
    public class i_ldloc : LdLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldloc(b, i); }
        private i_ldloc(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldloc definition

    #region i_ldloc_0 definition
    public class i_ldloc_0 : LdLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldloc_0(b, i); }
        private i_ldloc_0(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 0) { }
    }
    #endregion i_ldloc_0 definition

    #region i_ldloc_1 definition
    public class i_ldloc_1 : LdLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldloc_1(b, i); }
        private i_ldloc_1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 1) { }
    }
    #endregion i_ldloc_1 definition

    #region i_ldloc_2 definition
    public class i_ldloc_2 : LdLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldloc_2(b, i); }
        private i_ldloc_2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 2) { }
    }
    #endregion i_ldloc_2 definition

    #region i_ldloc_3 definition
    public class i_ldloc_3 : LdLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldloc_3(b, i); }
        private i_ldloc_3(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 3) { }
    }
    #endregion i_ldloc_3 definition

    #region i_ldloc_s definition
    public class i_ldloc_s : LdLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldloc_s(b, i); }
        private i_ldloc_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldloc_s definition

    #region i_ldloca definition
    public class i_ldloca : LdLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldloca(b, i); }
        private i_ldloca(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldloca definition

    #region i_ldloca_s definition
    public class i_ldloca_s : LdLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldloca_s(b, i); }
        private i_ldloca_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldloca_s definition

    #region i_ldnull definition
    public class i_ldnull : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldnull(b, i); }
        private i_ldnull(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            state._stack.Push(typeof(System.Object).ToMonoTypeReference());
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            ValueRef nul = LLVM.ConstPointerNull(LLVM.PointerType(LLVM.VoidType(), 0));
            var v = new VALUE(nul);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(v);
            state._stack.Push(v);
        }
    }
    #endregion i_ldnull definition

    #region i_ldobj definition
    public class i_ldobj : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldobj(b, i); }
        private i_ldobj(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // ldobj, copy a value from an address to the stack, ecma 335, page 409
            var v = state._stack.Pop();
            object operand = this.Operand;
            var o = operand as TypeReference;
            o = o.SwapInBclType();
            var p = o.Deresolve(this.Block._method_reference.DeclaringType, null);
            state._stack.Push(p);
        }
    }
    #endregion i_ldobj definition

    #region i_ldsfld definition
    public class i_ldsfld : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldsfld(b, i); }
        private i_ldsfld(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // ldsfld (load static field), ecma 335 page 410
            var operand = this.Operand;
            var operand_field_reference = operand as FieldReference;
            if (operand_field_reference == null)
                throw new Exception("Unknown field type");
            var ft = operand_field_reference.FieldType;
            var ft2 = ft.Deresolve(this.Block._method_reference.DeclaringType, null);
            state._stack.Push(ft2);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // ldsfld (load static field), ecma 335 page 410
            var operand = this.Operand;
            var mono_field_reference = operand as FieldReference;
            if (mono_field_reference == null)
                throw new Exception("Unknown field type");
            var type = mono_field_reference.DeclaringType;
            type = type.SwapInBclType();
            type = type.Deresolve(this.Block._method_reference.DeclaringType, null);
            var mono_field_type = mono_field_reference.FieldType;
            mono_field_type = mono_field_type.SwapInBclType();
            var llvm_field_type = mono_field_type.ToTypeRef();
            // Call meta to get static field. This can be done now because
            // the address of the static field does not change.
            var bcl_type = RUNTIME.MonoBclMap_GetBcl(type);
            if (bcl_type == IntPtr.Zero) throw new Exception();
            IntPtr[] fields = null;
            IntPtr* buf;
            int len;
            RUNTIME.BclGetFields(bcl_type, &buf, &len);
            fields = new IntPtr[len];
            for (int i = 0; i < len; ++i) fields[i] = buf[i];
            var mono_fields = type.ResolveFields().ToArray();
            var find = fields.Where(f =>
            {
                var ptrName = RUNTIME.BclGetFieldName(f);
                string name = Marshal.PtrToStringAnsi(ptrName);
                return name == mono_field_reference.Name;
            });
            IntPtr first = find.FirstOrDefault();
            if (first == IntPtr.Zero) throw new Exception("Cannot find field--ldsfld");
            var ptr = RUNTIME.BclGetStaticField(first);
            bool isArr = mono_field_type.IsArray;
            bool isSt = mono_field_type.IsStruct();
            bool isRef = mono_field_type.IsReferenceType();
            if (Campy.Utils.Options.IsOn("jit_trace"))
            System.Console.WriteLine(LLVM.PrintTypeToString(llvm_field_type));
            var ptr_llvm_field_type = LLVM.PointerType(llvm_field_type, 0);
            var address = LLVM.ConstInt(LLVM.Int64Type(), (ulong)ptr, false);
            var f1 = LLVM.BuildIntToPtr(Builder, address, ptr_llvm_field_type, "i" + instruction_id++);
            var load = LLVM.BuildLoad(Builder, f1, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(load));
            state._stack.Push(new VALUE(load));
        }
    }
    #endregion i_ldsfld definition

    #region i_ldsflda definition
    public class i_ldsflda : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldsflda(b, i); }
        private i_ldsflda(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldsflda definition

    #region i_ldstr definition
    public class i_ldstr : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldstr(b, i); }
        private i_ldstr(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = typeof(string).ToMonoTypeReference();
            
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(v.ToString());

            state._stack.Push(v);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            // Call SystemString_FromCharPtrASCII and push new string object on the stack.
            // _Z29SystemString_FromCharPtrASCIIPc

            unsafe {
                ValueRef[] args = new ValueRef[1];

                // Get char string froom instruction.
                var operand = Operand;
                string str = (string)operand;

                var llvm_cstr_t = LLVM.BuildGlobalString(Builder, str, "i" + instruction_id++);
                var llvm_cstr = LLVM.BuildPtrToInt(Builder, llvm_cstr_t, LLVM.Int64Type(), "i" + instruction_id++);
                args[0] = llvm_cstr;
                string name = "_Z29SystemString_FromCharPtrASCIIPc";
                var list = RUNTIME.BclNativeMethods.ToList();
                var list2 = RUNTIME.PtxFunctions.ToList();
                var f = list2.Where(t => t._mangled_name == name).First();
                ValueRef fv = f._valueref;
                var call = LLVM.BuildCall(Builder, fv, args, "");

                // Find type of System.String in BCL.
                Mono.Cecil.TypeReference tr = RUNTIME.FindBCLType(typeof(System.String));
                var llvm_type = tr.ToTypeRef();

                // Convert to pointer to pointer of string.
                var cast = LLVM.BuildIntToPtr(Builder, call, llvm_type, "i" + instruction_id++);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(cast));

                state._stack.Push(new VALUE(cast));
            }
        }
    }
    #endregion i_ldstr definition

    #region i_ldtoken definition
    public class i_ldtoken : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldtoken(b, i); }
        private i_ldtoken(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // ldtoken (load token handle), ecma 335 page 413
            var rth = typeof(System.RuntimeTypeHandle).ToMonoTypeReference().SwapInBclType();
            var v = rth.Deresolve(this.Block._method_reference.DeclaringType, null);
            state._stack.Push(v);
            // Parse System.RuntimeTypeHandle.ctor(IntPtr). We'll make
            // a call to that in code generation.
            var list = rth.Resolve().Methods.Where(m => m.FullName.Contains("ctor")).ToList();
            if (list.Count() != 1) throw new Exception("There should be only one constructor for System.RuntimeTypeHandle.");
            var mr = list.First();
            IMPORTER.Singleton().Add(mr);
        }

        public override void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // ldtoken (load token handle), ecma 335 page 413
            // Load System.RuntimeTypeHandle for arg.
            var tr = this.Operand as TypeReference;
            var fd = this.Operand as FieldDefinition;
            IntPtr handle_value = IntPtr.Zero;
            if (fd != null)
            {
                // For field types, this refers to an array initializer.
                // Get the field reference and set 
                tr = fd.FieldType;
                // Get enclosing parent, find field, get blob.
                var field_name = fd.Name;
                var declaring_type = fd.DeclaringType;
                var all_fields = declaring_type.MyGetFields();
                var possible = all_fields.Where(f => f.Name == field_name);
                if (!possible.Any()) throw new Exception("Cannot find field for cctor init");
                var fi = possible.First();
                var bcl_pa = RUNTIME.MonoBclMap_GetBcl(declaring_type);
                handle_value = RUNTIME.BclFindFieldInType(bcl_pa, fi.Name);
            }
            else if (tr != null)
            {
                var arg2 = tr.SwapInBclType();
                var v = arg2.Deresolve(this.Block._method_reference.DeclaringType, null);
                handle_value = RUNTIME.MonoBclMap_GetBcl(v);
			}
			
			var runtimetypehandle = typeof(System.RuntimeTypeHandle).ToMonoTypeReference().SwapInBclType();
			var type = runtimetypehandle.Deresolve(this.Block._method_reference.DeclaringType, null);
            var meta = RUNTIME.MonoBclMap_GetBcl(type);
			var llvm_type = type.ToTypeRef();
			llvm_type = LLVM.PointerType(llvm_type, 0);
			ValueRef token;

			// Note, pushing on stack an object of type RuntimeTypeHandle.
			{
                // Generate code to allocate System.RuntimeTypeHandle object.
                var xx1 = RUNTIME.BclNativeMethods.ToList();
				var xx2 = RUNTIME.PtxFunctions.ToList();
				var xx = xx2
					.Where(t => { return t._mangled_name == "_Z23Heap_AllocTypeVoidStarsPv"; });
				var xxx = xx.ToList();
				RUNTIME.PtxFunction first_kv_pair = xx.FirstOrDefault();
				if (first_kv_pair == null)
					throw new Exception("Yikes.");
				ValueRef fv2 = first_kv_pair._valueref;
				ValueRef[] args = new ValueRef[1];
				args[0] = LLVM.ConstInt(LLVM.Int64Type(), (ulong)meta.ToInt64(), false);
				var call = LLVM.BuildCall(Builder, fv2, args, "i" + instruction_id++);
				var cast = LLVM.BuildIntToPtr(Builder, call, llvm_type, "i" + instruction_id++);
				token = cast;
				if (Campy.Utils.Options.IsOn("jit_trace"))
					System.Console.WriteLine(new VALUE(token));
				state._stack.Push(new VALUE(token));
			}

			// Push on stack the handle to the type or field.
			state._stack.Push(new VALUE(LLVM.ConstInt(LLVM.Int64Type(), (ulong)handle_value.ToInt64(), false)));

			// Call the constructor to initialize the RuntimeTypeHandle object.
			{
				var t = typeof(System.RuntimeTypeHandle).ToMonoTypeReference().SwapInBclType();
				var list = runtimetypehandle.Resolve().Methods.Where(m => m.FullName.Contains(".ctor")).ToList();
				if (list.Count() != 1)
					throw new Exception("There should be only one constructor for System.RuntimeTypeHandle.");
				var mr = list.First();
				// Find bb entry.
				CFG.Vertex entry_corresponding_to_method_called = this.Block._graph.Vertices.Where(node
					=>
				{
					if (node.IsEntry && node._method_reference.FullName == mr.FullName)
						return true;
					return false;
				}).ToList().FirstOrDefault();
				if (entry_corresponding_to_method_called == null)
					throw new Exception("Cannot find constructor for System.RuntimeTypeHandle.");

				int xret = (entry_corresponding_to_method_called.HasScalarReturnValue ||
							entry_corresponding_to_method_called.HasStructReturnValue)
					? 1
					: 0;
				int xargs = entry_corresponding_to_method_called.StackNumberOfArguments;
				var name = mr.FullName;
				BuilderRef bu = this.Builder;
				ValueRef fv = entry_corresponding_to_method_called.LlvmInfo.MethodValueRef;
				var t_fun = LLVM.TypeOf(fv);
				var t_fun_con = LLVM.GetTypeContext(t_fun);
				var context = LLVM.GetModuleContext(RUNTIME.global_llvm_module);
				if (t_fun_con != context) throw new Exception("not equal");
				ValueRef[] args = new ValueRef[xargs];
				// No return.
				for (int k = xargs - 1; k >= 0; --k)
				{
					VALUE a = state._stack.Pop();
				    if (Campy.Utils.Options.IsOn("jit_trace"))
				        System.Console.WriteLine(a);
					ValueRef par = LLVM.GetParam(fv, (uint) k);
					ValueRef value = a.V;
					if (LLVM.TypeOf(value) != LLVM.TypeOf(par))
					{
						if (LLVM.GetTypeKind(LLVM.TypeOf(par)) == TypeKind.StructTypeKind
							&& LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.PointerTypeKind)
							value = LLVM.BuildLoad(Builder, value, "i" + instruction_id++);
						else if (LLVM.GetTypeKind(LLVM.TypeOf(par)) == TypeKind.PointerTypeKind
						         && LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.PointerTypeKind)
						    value = LLVM.BuildPointerCast(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
						else if (LLVM.GetTypeKind(LLVM.TypeOf(par)) == TypeKind.PointerTypeKind
						         && LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.IntegerTypeKind)
						    value = LLVM.BuildIntToPtr(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
						else
                            value = LLVM.BuildBitCast(Builder, value, LLVM.TypeOf(par), "i" + instruction_id++);
					}

					args[k] = value;
				}

				var call = LLVM.BuildCall(Builder, fv, args, "");
				if (Campy.Utils.Options.IsOn("jit_trace"))
					System.Console.WriteLine(call.ToString());
				state._stack.Push(new VALUE(token));
			}
        }
    }
    #endregion i_ldtoken definition

    #region i_ldvirtftn definition
    public class i_ldvirtftn : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ldvirtftn(b, i); }
        private i_ldvirtftn(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_ldvirtftn definition

    #region i_leave definition
    public class i_leave : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_leave(b, i); }
        private i_leave(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // leave.* page 372 of ecma 335
            var edges = Block._graph.SuccessorEdges(Block).ToList();
            if (edges.Count > 1)
                throw new Exception("There shouldn't be more than one edge from a leave instruction.");
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // leave.* page 372 of ecma 335
            var edge = Block._graph.SuccessorEdges(Block).ToList()[0];
            var s = edge.To;
            var br = LLVM.BuildBr(Builder, s.LlvmInfo.BasicBlock);
        }
    }
    #endregion i_leave definition

    #region i_leave_s definition
    public class i_leave_s : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_leave_s(b, i); }
        private i_leave_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // leave.* page 372 of ecma 335
            var edges = Block._graph.SuccessorEdges(Block).ToList();
            if (edges.Count > 1)
                throw new Exception("There shouldn't be more than one edge from a leave instruction.");
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // leave.* page 372 of ecma 335
            var edge = Block._graph.SuccessorEdges(Block).ToList()[0];
            var s = edge.To;
            var br = LLVM.BuildBr(Builder, s.LlvmInfo.BasicBlock);
        }
    }
    #endregion i_leave_s definition

    #region i_localloc definition
    public class i_localloc : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_localloc(b, i); }
        private i_localloc(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_localloc definition

    #region i_mkrefany definition
    public class i_mkrefany : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_mkrefany(b, i); }
        private i_mkrefany(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_mkrefany definition

    #region i_mul definition
    public class i_mul : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_mul(b, i); }
        private i_mul(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_mul definition

    #region i_mul_ovf definition
    public class i_mul_ovf : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_mul_ovf(b, i); }
        private i_mul_ovf(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_mul_ovf definition

    #region i_mul_ovf_un definition
    public class i_mul_ovf_un : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_mul_ovf_un(b, i); }
        private i_mul_ovf_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_mul_ovf_un definition

    #region i_neg definition
    public class i_neg : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_neg(b, i); }
        private i_neg(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var v = state._stack.Pop();

            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(v.ToString());

            state._stack.Push(v);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var rhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(rhs);

            var @typeof = LLVM.TypeOf(rhs.V);
            var kindof = LLVM.GetTypeKind(@typeof);
            ValueRef neg;
            if (kindof == TypeKind.DoubleTypeKind || kindof == TypeKind.FloatTypeKind)
                neg = LLVM.BuildFNeg(Builder, rhs.V, "i" + instruction_id++);
            else
                neg = LLVM.BuildNeg(Builder, rhs.V, "i" + instruction_id++);

            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(neg));

            state._stack.Push(new VALUE(neg));
        }
    }
    #endregion i_neg definition

    #region i_newarr definition
    public class i_newarr : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_newarr(b, i); }
        private i_newarr(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // newarr, page 416 of ecma 335
            var v = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(v.ToString());
            object operand = this.Operand;
            TypeReference type = operand as TypeReference;
            type = type.SwapInBclType();
            var actual_element_type = type.Deresolve(this.Block._method_reference.DeclaringType, null);
            TypeReference new_array_type = new ArrayType(actual_element_type, 1 /* 1D array */);
            state._stack.Push(new_array_type);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // newarr, page 416 of ecma 335
            object operand = this.Operand;
            TypeReference element_type = operand as TypeReference;
            element_type = element_type.SwapInBclType();
            element_type = element_type.Deresolve(this.Block._method_reference.DeclaringType, null);

            TypeReference new_array_type = new ArrayType(element_type, 1 /* 1D array */);
            var meta = RUNTIME.MonoBclMap_GetBcl(new_array_type);
            var array_type_to_create = new_array_type.ToTypeRef();
            var xx2 = RUNTIME.PtxFunctions.ToList();
            var xx = xx2.Where(t => { return t._mangled_name == "_Z21SystemArray_NewVectorP12tMD_TypeDef_jPj"; });
            RUNTIME.PtxFunction first_kv_pair = xx.FirstOrDefault();
            if (first_kv_pair == null) throw new Exception("Yikes.");
            ValueRef fv2 = first_kv_pair._valueref;
            ValueRef[] args = new ValueRef[3];
            var length_buffer = LLVM.BuildAlloca(Builder, LLVM.ArrayType(LLVM.Int32Type(), (uint)1), "i" + instruction_id++);
            LLVM.SetAlignment(length_buffer, 64);
            var base_of_lengths = LLVM.BuildPointerCast(Builder, length_buffer, LLVM.PointerType(LLVM.Int32Type(), 0), "i" + instruction_id++);
            int rank = 1;
            for (int i = 0; i < rank; ++i)
            {
                VALUE len = state._stack.Pop();
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(len);
                ValueRef[] id = new ValueRef[1] { LLVM.ConstInt(LLVM.Int32Type(), (ulong)i, true) };
                var add = LLVM.BuildInBoundsGEP(Builder, base_of_lengths, id, "i" + instruction_id++);
                var lcast = LLVM.BuildIntCast(Builder, len.V, LLVM.Int32Type(), "i" + instruction_id++);
                ValueRef store = LLVM.BuildStore(Builder, lcast, add);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(store));
            }
            args[2] = LLVM.BuildPtrToInt(Builder, length_buffer, LLVM.Int64Type(), "i" + instruction_id++);
            args[1] = LLVM.ConstInt(LLVM.Int32Type(), (ulong)rank, false);
            args[0] = LLVM.ConstInt(LLVM.Int64Type(), (ulong)meta, false);
            var call = LLVM.BuildCall(Builder, fv2, args, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(call));
            var new_obj = LLVM.BuildIntToPtr(Builder, call, array_type_to_create, "i" + instruction_id++);
            var stack_result = new VALUE(new_obj);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(stack_result);
            state._stack.Push(stack_result);
        }
    }
    #endregion i_newarr definition

    #region i_newobj definition
    public class i_newobj : INST
    {
        MethodReference call_closure_method = null;
        public override MethodReference CallTarget() { return call_closure_method; }

        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_newobj(b, i); }
        private i_newobj(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            INST new_inst = this;
            object method = this.Operand;
            if (method as Mono.Cecil.MethodReference == null) throw new Exception();
            Mono.Cecil.MethodReference orig_mr = method as Mono.Cecil.MethodReference;
            var mr = orig_mr;
            int xargs = /* always pass "this", but it does not count because newobj
                creates the object. So, it is just the number of standard parameters
                of the contructor. */
                mr.Parameters.Count;
            List<TypeReference> args = new List<TypeReference>();
            for (int k = 0; k < xargs; ++k)
            {
                var v = state._stack.Pop();
                args.Insert(0, v);
            }
            var args_array = args.ToArray();
            mr = orig_mr.SwapInBclMethod(this.Block._method_reference.DeclaringType, args_array);
            call_closure_method = mr;
            if (mr == null)
            {
                call_closure_method = orig_mr;
                return; // Can't do anything with this.
            }
            if (mr.DeclaringType == null)
                throw new Exception("can't handle.");
            if (mr.DeclaringType.HasGenericParameters)
                throw new Exception("can't handle.");
            if (mr.ReturnType.FullName != "System.Void")
            {
                throw new Exception(
                    "Constructor has a return type, but they should never have a type. Something is wrong.");
            }
            state._stack.Push(mr.DeclaringType);
            IMPORTER.Singleton().Add(mr);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            // The JIT of a call instructure requires a little explanation. The operand
            // for the instruction is a MethodReference, which is a C# method of some type.
            // Note, however, there are two cases here. One case is that the method has
            // CLI code that implements the method. The other are those that are DLL references.
            // These have no code that Mono.Cecil can pick up as it is usally C-code, which
            // is compiled for a specific target. The function signature of the native function
            // is not necessarily the same as that declared in the C#/NET assembly. This method,
            // Convert(), needs to handle native function calls carefully. These functions will
            // create native structures that C# references.

            // Get some basic information about the instruction, method, and type of object to create.
            var inst = this;
            object operand = this.Operand;
            MethodReference method = operand as MethodReference;
            CFG graph = (CFG)this.Block._graph;
            TypeReference type = method.DeclaringType;
            if (type == null)
                throw new Exception("Cannot get type of object/value for newobj instruction.");
            bool is_type_value_type = type.IsValueType;
            var name = method.FullName;
            CFG.Vertex the_entry = this.Block._graph.Vertices.Where(node
                =>
            {
                var g = inst.Block._graph;
                CFG.Vertex v = node;
                COMPILER c = COMPILER.Singleton;
                if (v.IsEntry && v._method_reference.FullName == name)
                    return true;
                else return false;
            }).ToList().FirstOrDefault();
            var llvm_type = type.ToTypeRef();
            var td = type.Resolve();

            // There four basic cases for newobj:
            // 1) type is a value type
            //   The object must be allocated on the stack, and the contrustor called with a pointer to that.
            //   a) the_entry is null, which means the constructor is a C function.
            //   b) the_entry is NOT null, which means the constructor is further CIL code.
            // 2) type is a reference_type.
            //   The object will be allocated on the heap, but done according to a convention of DNA.
            //   b) the_entry is null, which means the constructor is a C function, and it performs the allocation.
            //   c) the_entry is NOT null, which means we must allocate the object, then call the constructor, which is further CIL code.
            if (is_type_value_type && the_entry == null)
            {

            }
            else if (is_type_value_type && the_entry != null)
            {
                int nargs = the_entry.StackNumberOfArguments;
                int ret = the_entry.HasScalarReturnValue ? 1 : 0;

                // First, create a struct.
                var entry = this.Block.Entry.LlvmInfo.BasicBlock;
                var beginning = LLVM.GetFirstInstruction(entry);
                LLVM.PositionBuilderBefore(Builder, beginning);
                var new_obj = LLVM.BuildAlloca(Builder, llvm_type, "i" + instruction_id++); // Allocates struct on stack, but returns a pointer to struct.
                LLVM.PositionBuilderAtEnd(Builder, this.Block.LlvmInfo.BasicBlock);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(new_obj));

                BuilderRef bu = this.Builder;
                ValueRef fv = the_entry.LlvmInfo.MethodValueRef;
                var t_fun = LLVM.TypeOf(fv);
                var t_fun_con = LLVM.GetTypeContext(t_fun);
                var context = LLVM.GetModuleContext(RUNTIME.global_llvm_module);
                if (t_fun_con != context) throw new Exception("not equal");

                // Set up args, type casting if required.
                ValueRef[] args = new ValueRef[nargs];
                for (int k = nargs - 1; k >= 1; --k)
                {
                    VALUE v = state._stack.Pop();
                    ValueRef par = LLVM.GetParam(fv, (uint)k);
                    ValueRef value = v.V;
                    value = Casting.CastArg(Builder, value, LLVM.TypeOf(value), LLVM.TypeOf(par), true);
                    args[k] = value;
                }
                args[0] = new_obj;

                this.DebuggerInfo();
                var call = LLVM.BuildCall(Builder, fv, args, "");
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(call));

                // All structs in state._stack are actually pointers to structures,
                // as with reference types.

                state._stack.Push(new VALUE(new_obj));
            }
            else if (!is_type_value_type && the_entry == null)
            {
                // As noted in JIT_execute.c code of BCL:
                // "All internal constructors MUST allocate their own 'this' objects"
                // So, we don't call any allocator here, just the internal function in the BCL,
                // as that function will do the allocation over on the GPU.
                //
                // Also note: these calls are to internal constructors, which have a signature
                // of three args of type void* (in C). When the constructor code, in CUDA, is compiled,
                // the arguments are Int64. So, all parameters must be cast to Int64 in LLVM.
                //
                // Variable "method" is the signature as appears from C#, not C++, nor PTX.
                //
                Mono.Cecil.MethodReturnType cs_method_return_type_aux = method.MethodReturnType;
                Mono.Cecil.TypeReference cs_method_return_type = cs_method_return_type_aux.ReturnType;
                var cs_has_ret = cs_method_return_type.FullName != "System.Void";
                var cs_HasScalarReturnValue = cs_has_ret && !cs_method_return_type.IsStruct();
                var cs_HasStructReturnValue = cs_has_ret && cs_method_return_type.IsStruct();
                var cs_HasThis = method.HasThis;
                var cs_NumberOfArguments = method.Parameters.Count
                                        + (cs_HasThis ? 1 : 0)
                                        + (cs_HasStructReturnValue ? 1 : 0);
                int locals = 0;
                var NumberOfLocals = locals;
                int cs_xret = (cs_HasScalarReturnValue || cs_HasStructReturnValue) ? 1 : 0;
                int cs_xargs = cs_NumberOfArguments;

                // Search for native function in loaded libraries.
                name = method.Name;
                var full_name = method.FullName;
                Regex regex = new Regex(@"^[^\s]+\s+(?<name>[^\(]+).+$");
                Match m = regex.Match(full_name);
                if (!m.Success) throw new Exception();
                var demangled_name = m.Groups["name"].Value;
                demangled_name = demangled_name.Replace("::", "_");
                demangled_name = demangled_name.Replace(".", "_");
                demangled_name = demangled_name.Replace("__", "_");
                BuilderRef bu = this.Builder;
                var as_name = method.Module.Assembly.Name;
                var xx = RUNTIME.BclNativeMethods
                    .Where(t =>
                    {
                        return t._full_name == full_name;
                    });
                var xxx = xx.ToList();
                RUNTIME.BclNativeMethod first_kv_pair = xx.FirstOrDefault();
                if (first_kv_pair == null)
                    throw new Exception("Yikes.");

                RUNTIME.PtxFunction fffv = RUNTIME.PtxFunctions.Where(
                    t => first_kv_pair._native_name.Contains(t._mangled_name)).FirstOrDefault();
                ValueRef fv = fffv._valueref;
                var t_fun = LLVM.TypeOf(fv);
                var t_fun_con = LLVM.GetTypeContext(t_fun);
                var context = LLVM.GetModuleContext(RUNTIME.global_llvm_module);

                {
                    ValueRef[] args = new ValueRef[3];
                    ValueRef nul = LLVM.ConstInt(LLVM.Int64Type(), 0, false);
                    VALUE t = new VALUE(nul);
                    var parameter_type = LLVM.ArrayType(LLVM.Int64Type(), (uint)method.Parameters.Count);
                    var param_buffer = LLVM.BuildAlloca(Builder, parameter_type, "i"+instruction_id++);
                    LLVM.SetAlignment(param_buffer, 64);
                    var base_of_parameters = LLVM.BuildPointerCast(Builder, param_buffer,
                        LLVM.PointerType(LLVM.Int64Type(), 0), "i" + instruction_id++);

                    for (int i = method.Parameters.Count - 1; i >= 0; i--)
                    {
                        VALUE p = state._stack.Pop();
                        if (Campy.Utils.Options.IsOn("jit_trace"))
                            System.Console.WriteLine(p);
                        ValueRef[] index = new ValueRef[1] { LLVM.ConstInt(LLVM.Int32Type(), (ulong)i, true) };
                        var add = LLVM.BuildInBoundsGEP(Builder, base_of_parameters, index, "i" + instruction_id++);
                        if (Campy.Utils.Options.IsOn("jit_trace"))
                            System.Console.WriteLine(new VALUE(add));
                        ValueRef v = LLVM.BuildPointerCast(Builder, add, LLVM.PointerType(LLVM.TypeOf(p.V), 0), "i" + instruction_id++);
                        if (Campy.Utils.Options.IsOn("jit_trace"))
                            System.Console.WriteLine(new VALUE(v));
                        ValueRef store = LLVM.BuildStore(Builder, p.V, v);
                        if (Campy.Utils.Options.IsOn("jit_trace"))
                            System.Console.WriteLine(new VALUE(store));
                    }

                    // Set up return. For now, always allocate buffer.
                    // Note function return is type of third parameter.
                    var native_return_type2 = first_kv_pair._returnType.ToTypeRef();

                    var native_return_type = LLVM.ArrayType(
                       LLVM.Int64Type(),
                       (uint)1);
                    var native_return_buffer = LLVM.BuildAlloca(Builder,
                        native_return_type, "i" + instruction_id++);
                    LLVM.SetAlignment(native_return_buffer, 64);
                    //LLVM.PositionBuilderAtEnd(Builder, this.Block.BasicBlock);

                    // Set up call.
                    var pt = LLVM.BuildPtrToInt(Builder, t.V, LLVM.Int64Type(), "i" + instruction_id++);
                    var pp = LLVM.BuildPtrToInt(Builder, param_buffer, LLVM.Int64Type(), "i" + instruction_id++);
                    var pr = LLVM.BuildPtrToInt(Builder, native_return_buffer, LLVM.Int64Type(), "i" + instruction_id++);

                    args[0] = pt;
                    args[1] = pp;
                    args[2] = pr;

                    this.DebuggerInfo();
                    var call = LLVM.BuildCall(Builder, fv, args, name);
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(call));

                    // There is always a return from a newobj instruction.
                    var ptr_cast = LLVM.BuildBitCast(Builder,
                        native_return_buffer,
                        LLVM.PointerType(llvm_type, 0), "i" + instruction_id++);

                    var load = LLVM.BuildLoad(Builder, ptr_cast, "i" + instruction_id++);

                    // Cast the damn object into the right type.
                    state._stack.Push(new VALUE(load));

                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(load));
                }
            }
            else if (!is_type_value_type && the_entry != null)
            {
                ValueRef new_obj;
                {
                    var meta = RUNTIME.MonoBclMap_GetBcl(type);

                    // Generate code to allocate object and stuff.
                    // This boxes the value.
                    var xx1 = RUNTIME.BclNativeMethods.ToList();
                    var xx2 = RUNTIME.PtxFunctions.ToList();
                    var xx = xx2
                        .Where(t => { return t._mangled_name == "_Z23Heap_AllocTypeVoidStarsPv"; });
                    var xxx = xx.ToList();
                    RUNTIME.PtxFunction first_kv_pair = xx.FirstOrDefault();
                    if (first_kv_pair == null)
                        throw new Exception("Yikes.");

                    ValueRef fv2 = first_kv_pair._valueref;
                    ValueRef[] args = new ValueRef[1];

                    args[0] = LLVM.ConstInt(LLVM.Int64Type(), (ulong) meta.ToInt64(), false);
                    this.DebuggerInfo();
                    var call = LLVM.BuildCall(Builder, fv2, args, "i" + instruction_id++);
                    var cast = LLVM.BuildIntToPtr(Builder, call, llvm_type, "i" + instruction_id++);
                    new_obj = cast;

                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(new_obj));
                }

                {
                    int nargs = the_entry.StackNumberOfArguments;
                    int ret = the_entry.HasScalarReturnValue ? 1 : 0;

                    BuilderRef bu = this.Builder;
                    ValueRef fv = the_entry.LlvmInfo.MethodValueRef;
                    var t_fun = LLVM.TypeOf(fv);
                    var t_fun_con = LLVM.GetTypeContext(t_fun);
                    var context = LLVM.GetModuleContext(RUNTIME.global_llvm_module);
                    if (t_fun_con != context) throw new Exception("not equal");

                    // Set up args, type casting if required.
                    ValueRef[] args = new ValueRef[nargs];
                    for (int k = nargs - 1; k >= 1; --k)
                    {
                        VALUE v = state._stack.Pop();
                        ValueRef par = LLVM.GetParam(fv, (uint)k);
                        ValueRef value = v.V;
                        value = Casting.CastArg(Builder, value, LLVM.TypeOf(value), LLVM.TypeOf(par), true);
                        args[k] = value;
                    }
                    args[0] = new_obj;

                    var call = LLVM.BuildCall(Builder, fv, args, "");
                    if (Campy.Utils.Options.IsOn("jit_trace"))
                        System.Console.WriteLine(new VALUE(call));

                    state._stack.Push(new VALUE(new_obj));
                }
            }
        }
    }
    #endregion i_newobj definition

    #region i_no definition
    public class i_no : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_no(b, i); }
        private i_no(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_no definition

    #region i_nop definition
    public class i_nop : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_nop(b, i); }
        private i_nop(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state) { }
        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state) { }
    }
    #endregion i_nop definition

    #region i_not definition
    public class i_not : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_not(b, i); }
        private i_not(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_not definition

    #region i_or definition
    public class i_or : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_or(b, i); }
        private i_or(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_or definition

    #region i_pop definition
    public class i_pop : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_pop(b, i); }
        private i_pop(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            state._stack.Pop();
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            state._stack.Pop();
        }
    }
    #endregion i_pop definition

    #region i_readonly definition
    public class i_readonly : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_readonly(b, i); }
        private i_readonly(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_readonly definition

    #region i_refanytype definition
    public class i_refanytype : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_refanytype(b, i); }
        private i_refanytype(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_refanytype definition

    #region i_refanyval definition
    public class i_refanyval : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_refanyval(b, i); }
        private i_refanyval(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_refanyval definition

    #region i_rem definition
    public class i_rem : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_rem(b, i); }
        private i_rem(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_rem definition

    #region i_rem_un definition
    public class i_rem_un : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_rem_un(b, i); }
        private i_rem_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_rem_un definition

    #region i_ret definition
    public class i_ret : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_ret(b, i); }
        private i_ret(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            if (!(this.Block.HasStructReturnValue || this.Block.HasScalarReturnValue))
            {
            }
            else if (this.Block.HasScalarReturnValue)
            {
                // See this on struct return--https://groups.google.com/forum/#!topic/llvm-dev/RSnV-Vr17nI
                // The following fails for structs, so do not do this for struct returns.
                var v = state._stack.Pop();
                state._stack.Push(v);
            }
            else if (this.Block.HasStructReturnValue)
            {
            }
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            if (!(this.Block.HasStructReturnValue || this.Block.HasScalarReturnValue))
            {
                var i = LLVM.BuildRetVoid(Builder);
            }
            else if (this.Block.HasScalarReturnValue)
            {
                // See this on struct return--https://groups.google.com/forum/#!topic/llvm-dev/RSnV-Vr17nI
                // The following fails for structs, so do not do this for struct returns.
                var v = state._stack.Pop();
                var value = v.V;
                var ra = this.Block._method_reference.ReturnType;
                var rb = new TYPE(ra);
                var r = rb.CilTypeLLVM;
                if (LLVM.TypeOf(value) != r)
                {
                    if (LLVM.GetTypeKind(r) == TypeKind.StructTypeKind
                        && LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.PointerTypeKind)
                        value = LLVM.BuildLoad(Builder, value, "i" + instruction_id++);
                    else if (LLVM.GetTypeKind(r) == TypeKind.PointerTypeKind)
                        value = LLVM.BuildPointerCast(Builder, value, r, "i" + instruction_id++);
                    else if (LLVM.GetTypeKind(LLVM.TypeOf(value)) == TypeKind.IntegerTypeKind)
                        value = LLVM.BuildIntCast(Builder, value, r, "i" + instruction_id++);
                    else
                        value = LLVM.BuildBitCast(Builder, value, r, "i" + instruction_id++);
                }
                var i = LLVM.BuildRet(Builder, value);
                state._stack.Push(new VALUE(i));
            }
            else if (this.Block.HasStructReturnValue)
            {
                var v = state._stack.Pop();
                var p = state._struct_ret[0];
                var store = LLVM.BuildStore(Builder, v.V, p.V);
                if (Campy.Utils.Options.IsOn("jit_trace"))
                    System.Console.WriteLine(new VALUE(store));
                var i = LLVM.BuildRetVoid(Builder);
            }
        }
    }
    #endregion i_ret definition

    #region i_rethrow definition
    public class i_rethrow : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_rethrow(b, i); }
        private i_rethrow(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_rethrow definition

    #region i_shl definition
    public class i_shl : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_shl(b, i); }
        private i_shl(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var rhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(rhs);

            var lhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(lhs);

            var result = lhs;
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(result);

            state._stack.Push(result);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var rhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(rhs);

            var lhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(lhs);

            var result = LLVM.BuildShl(Builder, lhs.V, rhs.V, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(result));

            state._stack.Push(new VALUE(result));
        }
    }
    #endregion i_shl definition

    #region i_shr definition
    public class i_shr : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_shr(b, i); }
        private i_shr(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var rhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(rhs);

            var lhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(lhs);

            var result = lhs;
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(result);

            state._stack.Push(result);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var rhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(rhs);

            var lhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(lhs);

            // Very annoyingly, LLVM requires rhs to be of the same type as lhs.
            var src = rhs.V;
            var stype = LLVM.TypeOf(src);
            var dtype = LLVM.TypeOf(lhs.V);
            var new_rhs = Casting.CastArg(Builder, src, stype, dtype, true);
            var result = LLVM.BuildAShr(Builder, lhs.V, new_rhs, "i" + instruction_id++);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(new VALUE(result));

            state._stack.Push(new VALUE(result));
        }
    }
    #endregion i_shr definition

    #region i_shr_un definition
    public class i_shr_un : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_shr_un(b, i); }
        private i_shr_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_shr_un definition

    #region i_sizeof definition
    public class i_sizeof : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_sizeof(b, i); }
        private i_sizeof(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var value = typeof(System.IntPtr).ToMonoTypeReference();
            object operand = this.Operand;
            System.Type t = operand.GetType();
            if (t.FullName == "Mono.Cecil.PointerType")
                state._stack.Push(value);
            else
                throw new Exception("Unimplemented sizeof");
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            object operand = this.Operand;
            System.Type t = operand.GetType();
            if (t.FullName == "Mono.Cecil.PointerType")
                state._stack.Push(new VALUE(LLVM.ConstInt(LLVM.Int32Type(), 8, false)));
            else
                throw new Exception("Unimplemented sizeof");
        }
    }
    #endregion i_sizeof definition

    #region i_starg definition
    public class i_starg : StArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_starg(b, i); }
        private i_starg(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            Mono.Cecil.ParameterDefinition pr = i.Operand as Mono.Cecil.ParameterDefinition;
            int arg = pr.Sequence;
            _arg = arg;
        }
    }
    #endregion i_starg definition

    #region i_starg_s definition
    public class i_starg_s : StArg
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_starg_s(b, i); }

        private i_starg_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i)
            : base(b, i)
        {
            Mono.Cecil.ParameterDefinition pr = i.Operand as Mono.Cecil.ParameterDefinition;
            int arg = pr.Sequence;
            _arg = arg;
        }
    }
    #endregion i_starg_s definition

    #region i_stelem_any definition
    public class i_stelem_any : StElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stelem_any(b, i); }
        private i_stelem_any(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_stelem_any definition

    #region i_stelem_i1 definition
    public class i_stelem_i1 : StElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stelem_i1(b, i); }
        private i_stelem_i1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(sbyte)); }
    }
    #endregion i_stelem_i1 definition

    #region i_stelem_i2 definition
    public class i_stelem_i2 : StElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stelem_i2(b, i); }
        private i_stelem_i2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(short)); }
    }
    #endregion i_stelem_i2 definition

    #region i_stelem_i4 definition
    public class i_stelem_i4 : StElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stelem_i4(b, i); }
        private i_stelem_i4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(int)); }
    }
    #endregion i_stelem_i4 definition

    #region i_stelem_i8 definition
    public class i_stelem_i8 : StElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stelem_i8(b, i); }
        private i_stelem_i8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_stelem_i8 definition

    #region i_stelem_i definition
    public class i_stelem_i : StElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stelem_i(b, i); }
        private i_stelem_i(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(long)); }
    }
    #endregion i_stelem_i definition

    #region i_stelem_r4 definition
    public class i_stelem_r4 : StElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stelem_r4(b, i); }
        private i_stelem_r4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(float)); }
    }
    #endregion i_stelem_r4 definition

    #region i_stelem_r8 definition
    public class i_stelem_r8 : StElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stelem_r8(b, i); }
        private i_stelem_r8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { _dst = new TYPE(typeof(double)); }
    }
    #endregion i_stelem_r8 definition

    #region i_stelem_ref definition
    public class i_stelem_ref : StElem
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stelem_ref(b, i); }
        private i_stelem_ref(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_stelem_ref definition

    #region i_stfld definition
    public class i_stfld : StFld
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stfld(b, i); }
        private i_stfld(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_stfld definition

    #region i_stind_i1 definition
    public class i_stind_i1 : StInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stind_i1(b, i); }
        private i_stind_i1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, typeof(sbyte).ToMonoTypeReference()) { }
    }
    #endregion i_stind_i1 definition

    #region i_stind_i2 definition
    public class i_stind_i2 : StInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stind_i2(b, i); }
        private i_stind_i2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, typeof(short).ToMonoTypeReference()) { }
    }
    #endregion i_stind_i2 definition

    #region i_stind_i4 definition
    public class i_stind_i4 : StInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stind_i4(b, i); }
        private i_stind_i4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, typeof(int).ToMonoTypeReference()) { }
    }
    #endregion i_stind_i4 definition

    #region i_stind_i8 definition
    public class i_stind_i8 : StInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stind_i8(b, i); }
        private i_stind_i8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, typeof(long).ToMonoTypeReference()) { }
    }
    #endregion i_stind_i8 definition

    #region i_stind_i definition
    public class i_stind_i : StInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stind_i(b, i); }
        private i_stind_i(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, typeof(long).ToMonoTypeReference()) { }
        // native and c# long the same.
    }
    #endregion i_stind_i definition

    #region i_stind_r4 definition
    public class i_stind_r4 : StInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stind_r4(b, i); }
        private i_stind_r4(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, typeof(float).ToMonoTypeReference()) { }
    }
    #endregion i_stind_r4 definition

    #region i_stind_r8 definition
    public class i_stind_r8 : StInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stind_r8(b, i); }
        private i_stind_r8(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, typeof(double).ToMonoTypeReference()) { }
    }
    #endregion i_stind_r8 definition

    #region i_stind_ref definition
    public class i_stind_ref : StInd
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stind_ref(b, i); }
        private i_stind_ref(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, null) { } /* dynamic target type */
    }
    #endregion i_stind_ref definition

    #region i_stloc definition
    public class i_stloc : StLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stloc(b, i); }
        public i_stloc(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_stloc definition

    #region i_stloc_0 definition
    public class i_stloc_0 : StLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stloc_0(b, i); }
        private i_stloc_0(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 0) { }
    }
    #endregion i_stloc_0 definition

    #region i_stloc_1 definition
    public class i_stloc_1 : StLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stloc_1(b, i); }
        private i_stloc_1(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 1) { }
    }
    #endregion i_stloc_1 definition

    #region i_stloc_2 definition
    public class i_stloc_2 : StLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stloc_2(b, i); }
        private i_stloc_2(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 2) { }
    }
    #endregion i_stloc_2 definition

    #region i_stloc_3 definition
    public class i_stloc_3 : StLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stloc_3(b, i); }
        private i_stloc_3(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i, 3) { }
    }
    #endregion i_stloc_3 definition

    #region i_stloc_s definition
    public class i_stloc_s : StLoc
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stloc_s(b, i); }
        private i_stloc_s(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_stloc_s definition

    #region i_stobj definition
    public class i_stobj : INST
    {
        private TypeReference fully_typed_operand;

        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stobj(b, i); }
        private i_stobj(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   //  stobj – store a value at an address , page 428
            var s = state._stack.Pop();
            var d = state._stack.Pop();
            object operand = this.Operand;
            var o = operand as TypeReference;
            o = o.SwapInBclType();
            fully_typed_operand = o.Deresolve(this.Block._method_reference.DeclaringType, null);
        }

        public override void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   //  stobj – store a value at an address, page 428
            var src = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(src);
            var dst = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(dst);
            TypeRef stype = LLVM.TypeOf(src.V);
            TypeRef dtype = LLVM.GetElementType(LLVM.TypeOf(dst.V));
            src = new VALUE(Casting.CastArg(Builder, src.V, stype, dtype, true));
            var result = LLVM.BuildStore(Builder, src.V, dst.V);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine("Store = " + new VALUE(result).ToString());
        }
    }
    #endregion i_stobj definition

    #region i_stsfld definition
    public class i_stsfld : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_stsfld(b, i); }
        private i_stsfld(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        TypeReference call_closure_type = null;
        TypeReference call_closure_field_type = null;

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {   // stsfld (store static field), ecma 335 page 429
            var v = state._stack.Pop();
            var operand = this.Operand;
            var mono_field_reference = operand as FieldReference;
            if (mono_field_reference == null) throw new Exception("Unknown field type");
            var d = mono_field_reference.DeclaringType;
            d = d.SwapInBclType();
            var o = d.Deresolve(this.Block._method_reference.DeclaringType, null);
            call_closure_type = o;
            var f = mono_field_reference.FieldType;
            f = f.SwapInBclType();
            f = f.Deresolve(this.Block._method_reference.DeclaringType, null);
            call_closure_field_type = f;
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {   // stsfld (store static field), ecma 335 page 429
            var v = state._stack.Pop();
            var operand = this.Operand;
            var mono_field_reference = operand as FieldReference;
            var type_f1 = call_closure_field_type.ToTypeRef();
            // Call meta to get static field. This can be done now because
            // the address of the static field does not change.
            var bcl_type = RUNTIME.MonoBclMap_GetBcl(call_closure_type);
            if (bcl_type == IntPtr.Zero) throw new Exception();
            IntPtr[] fields = null;
            IntPtr* buf;
            int len;
            RUNTIME.BclGetFields(bcl_type, &buf, &len);
            fields = new IntPtr[len];
            for (int i = 0; i < len; ++i) fields[i] = buf[i];
            var mono_fields = call_closure_type.ResolveFields().ToArray();
            var find = fields.Where(f =>
            {
                var ptrName = RUNTIME.BclGetFieldName(f);
                string name = Marshal.PtrToStringAnsi(ptrName);
                return name == mono_field_reference.Name;
            });
            IntPtr first = find.FirstOrDefault();
            if (first == IntPtr.Zero) throw new Exception("Cannot find field--stsfld");
            var ptr = RUNTIME.BclGetStaticField(first);
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(LLVM.PrintTypeToString(type_f1));
            var address = LLVM.ConstInt(LLVM.Int64Type(), (ulong)ptr, false);
            var f1 = LLVM.BuildIntToPtr(Builder, address, type_f1, "i" + instruction_id++);
            var type_f2 = LLVM.PointerType(type_f1, 0);
            var f2 = LLVM.BuildPointerCast(Builder, f1, type_f2, "i" + instruction_id++);
            var src = v.V;
            src = Casting.CastArg(Builder, v.V, LLVM.TypeOf(v.V), type_f1, true);
            LLVM.BuildStore(Builder, src, f2);
        }
    }
    #endregion i_stsfld definition

    #region i_sub definition
    public class i_sub : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_sub(b, i); }
        private i_sub(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_sub definition

    #region i_sub_ovf definition
    public class i_sub_ovf : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_sub_ovf(b, i); }
        private i_sub_ovf(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_sub_ovf definition

    #region i_sub_ovf_un definition
    public class i_sub_ovf_un : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_sub_ovf_un(b, i); }
        private i_sub_ovf_un(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_sub_ovf_un definition

    #region i_switch definition
    public class i_switch : INST
    {
        private TypeReference call_closure_value;
        private CFG.Vertex exit_block;
        private CFG.Vertex default_block;

        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_switch(b, i); }
        private i_switch(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        { // switch – table switch based on value page 382
          /* The switch statement. Cases shown below.

            i =>
            {
                switch (i)
                {
                    case 0: a[0] = 3;
                        break;
                    case 1: a[1] = 4;
                        break;
                    case 2: a[2] = 5;
                        break;
                    default:
                        a[0] = 0;
                        break;
                }
                if (q < 10)
                    q = 0;
                else
                    q = 1;
                //var j = i.ToString();
                //System.Console.WriteLine();
                //System.Console.WriteLine(i.ToString());
            }


              .method assembly hidebysig instance void 
                '<Main>b__0'(
                  int32 i
                ) cil managed 
              {
                .maxstack 3
                .locals init (
                  [0] int32 V_0,
                  [1] bool V_1
                )

                // [192 13 - 192 14]
                IL_0000: nop          

                // [193 17 - 193 27]
                IL_0001: ldarg.1      // i
                IL_0002: stloc.0      // V_0

                IL_0003: ldloc.0      // V_0
                IL_0004: switch       (IL_0017, IL_0022, IL_002d)
                IL_0015: br.s         IL_0038

                // [195 29 - 195 38]
                IL_0017: ldarg.0      // this
                IL_0018: ldfld        int32[] ConsoleApp4.Program/'<>c__DisplayClass1_0'::a
                IL_001d: ldc.i4.0     
                IL_001e: ldc.i4.3     
                IL_001f: stelem.i4    

                // [196 25 - 196 31]
                IL_0020: br.s         IL_0043

                // [197 29 - 197 38]
                IL_0022: ldarg.0      // this
                IL_0023: ldfld        int32[] ConsoleApp4.Program/'<>c__DisplayClass1_0'::a
                IL_0028: ldc.i4.1     
                IL_0029: ldc.i4.4     
                IL_002a: stelem.i4    

                // [198 25 - 198 31]
                IL_002b: br.s         IL_0043

                // [199 29 - 199 38]
                IL_002d: ldarg.0      // this
                IL_002e: ldfld        int32[] ConsoleApp4.Program/'<>c__DisplayClass1_0'::a
                IL_0033: ldc.i4.2     
                IL_0034: ldc.i4.5     
                IL_0035: stelem.i4    

                // [200 25 - 200 31]
                IL_0036: br.s         IL_0043

                // [202 25 - 202 34]
                IL_0038: ldarg.0      // this
                IL_0039: ldfld        int32[] ConsoleApp4.Program/'<>c__DisplayClass1_0'::a
                IL_003e: ldc.i4.0     
                IL_003f: ldc.i4.0     
                IL_0040: stelem.i4    

                // [203 25 - 203 31]
                IL_0041: br.s         IL_0043

                // [205 17 - 205 28]
                IL_0043: ldarg.0      // this
                IL_0044: ldfld        int32 ConsoleApp4.Program/'<>c__DisplayClass1_0'::q
                IL_0049: ldc.i4.s     10 // 0x0a
                IL_004b: clt          
                IL_004d: stloc.1      // V_1

                IL_004e: ldloc.1      // V_1
                IL_004f: brfalse.s    IL_005a

                // [206 21 - 206 27]
                IL_0051: ldarg.0      // this
                IL_0052: ldc.i4.0     
                IL_0053: stfld        int32 ConsoleApp4.Program/'<>c__DisplayClass1_0'::q
                IL_0058: br.s         IL_0061

                // [208 21 - 208 27]
                IL_005a: ldarg.0      // this
                IL_005b: ldc.i4.1     
                IL_005c: stfld        int32 ConsoleApp4.Program/'<>c__DisplayClass1_0'::q

                // [212 13 - 212 14]
                IL_0061: ret          

              } // end of method '<>c__DisplayClass1_0'::'<Main>b__0'

=======================================================================
i =>
            {
                switch (i)
                {
                    case 0: a[0] = 3;
                        break;
                    case 1: a[1] = 4;
                        break;
                    case 2: a[2] = 5;
                        break;
                    //default:
                    //    a[0] = 0;
                    //    break;
                }
                if (q < 10)
                    q = 0;
                else
                    q = 1;
            }

                .method assembly hidebysig instance void 
                  '<Main>b__0'(
                    int32 i
                  ) cil managed 
                {
                  .maxstack 3
                  .locals init (
                    [0] int32 V_0,
                    [1] bool V_1
                  )

                  // [192 13 - 192 14]
                  IL_0000: nop          

                  // [193 17 - 193 27]
                  IL_0001: ldarg.1      // i
                  IL_0002: stloc.0      // V_0

                  IL_0003: ldloc.0      // V_0
                  IL_0004: switch       (IL_0017, IL_0022, IL_002d)
                  IL_0015: br.s         IL_0038

                  // [195 29 - 195 38]
                  IL_0017: ldarg.0      // this
                  IL_0018: ldfld        int32[] ConsoleApp4.Program/'<>c__DisplayClass1_0'::a
                  IL_001d: ldc.i4.0     
                  IL_001e: ldc.i4.3     
                  IL_001f: stelem.i4    

                  // [196 25 - 196 31]
                  IL_0020: br.s         IL_0038

                  // [197 29 - 197 38]
                  IL_0022: ldarg.0      // this
                  IL_0023: ldfld        int32[] ConsoleApp4.Program/'<>c__DisplayClass1_0'::a
                  IL_0028: ldc.i4.1     
                  IL_0029: ldc.i4.4     
                  IL_002a: stelem.i4    

                  // [198 25 - 198 31]
                  IL_002b: br.s         IL_0038

                  // [199 29 - 199 38]
                  IL_002d: ldarg.0      // this
                  IL_002e: ldfld        int32[] ConsoleApp4.Program/'<>c__DisplayClass1_0'::a
                  IL_0033: ldc.i4.2     
                  IL_0034: ldc.i4.5     
                  IL_0035: stelem.i4    

                  // [200 25 - 200 31]
                  IL_0036: br.s         IL_0038

                  // [205 17 - 205 28]
                  IL_0038: ldarg.0      // this
                  IL_0039: ldfld        int32 ConsoleApp4.Program/'<>c__DisplayClass1_0'::q
                  IL_003e: ldc.i4.s     10 // 0x0a
                  IL_0040: clt          
                  IL_0042: stloc.1      // V_1

                  IL_0043: ldloc.1      // V_1
                  IL_0044: brfalse.s    IL_004f

                  // [206 21 - 206 27]
                  IL_0046: ldarg.0      // this
                  IL_0047: ldc.i4.0     
                  IL_0048: stfld        int32 ConsoleApp4.Program/'<>c__DisplayClass1_0'::q
                  IL_004d: br.s         IL_0056

                  // [208 21 - 208 27]
                  IL_004f: ldarg.0      // this
                  IL_0050: ldc.i4.1     
                  IL_0051: stfld        int32 ConsoleApp4.Program/'<>c__DisplayClass1_0'::q

                  // [209 13 - 209 14]
                  IL_0056: ret          

                } // end of method '<>c__DisplayClass1_0'::'<Main>b__0'
            */
            default_block = this.Block._graph.Successors(this.Block).Last();
            call_closure_value = state._stack.Pop();
            object operand = this.Operand;
            var t = operand.GetType();
            Instruction[] instructions = operand as Instruction[];
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        { // switch – table switch based on value page 382
            var value = state._stack.Pop();
            // Generate code for switch.
            object operand = this.Operand;
            var t = operand.GetType();
            var s = LLVM.BuildIntCast(Builder, value.V, LLVM.Int32Type(), "i" + instruction_id++);
            Instruction[] instructions = operand as Instruction[];
            var sw = LLVM.BuildSwitch(Builder, s, default_block.LlvmInfo.BasicBlock, (uint)instructions.Length);
            for (int j = 0; j < instructions.Length; ++j)
            {
                var inst = instructions[j];
                var goto_block = this.Block._graph.Vertices.Where(
                    e =>
                    {
                        var i = e.Instructions[0];
                        if (e._method_reference != Block.Entry._method_reference)
                            return false;
                        if (i.Instruction.Offset != inst.Offset)
                            return false;
                        return true;
                    }).First();
                LLVM.AddCase(sw, LLVM.ConstInt(LLVM.Int32Type(), (ulong)j, false), goto_block.LlvmInfo.BasicBlock);
            }
        }
    }
    #endregion i_switch definition

    #region i_tail definition
    public class i_tail : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_tail(b, i); }
        private i_tail(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_tail definition

    #region i_throw definition
    public class i_throw : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_throw(b, i); }
        private i_throw(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }

        public override void CallClosure(STATE<TypeReference, SafeStackQueue<TypeReference>> state)
        {
            var rhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("detailed_import_computation_trace"))
                System.Console.WriteLine(rhs);
        }

        public override unsafe void Convert(STATE<VALUE, StackQueue<VALUE>> state)
        {
            var rhs = state._stack.Pop();
            if (Campy.Utils.Options.IsOn("jit_trace"))
                System.Console.WriteLine(rhs);
        }
    }
    #endregion i_throw definition

    #region i_unaligned definition
    public class i_unaligned : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_unaligned(b, i); }
        private i_unaligned(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_unaligned definition

    #region i_unbox definition
    public class i_unbox : Unbox
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_unbox(b, i); }
        private i_unbox(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_unbox definition

    #region i_unbox_any definition
    public class i_unbox_any : Unbox
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_unbox_any(b, i); }
        private i_unbox_any(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_unbox_any definition

    #region i_volatile definition
    public class i_volatile : INST
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_volatile(b, i); }
        private i_volatile(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_volatile definition

    #region i_xor definition
    public class i_xor : BinOp
    {
        public static INST factory(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) { return new i_xor(b, i); }
        private i_xor(CFG.Vertex b, Mono.Cecil.Cil.Instruction i) : base(b, i) { }
    }
    #endregion i_xor definition

    #region Casting definition
    public class Casting
	{
		public static ValueRef CastArg(BuilderRef Builder, ValueRef src, TypeRef stype, TypeRef dtype, bool is_unsigned)
		{
		    ValueRef dst = src;
			if (stype != dtype)
			{
				bool ext = false;

				/* Extend */
				if (dtype == LLVM.Int64Type() && (stype == LLVM.Int32Type() || stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
					ext = true;
				else if (dtype == LLVM.Int32Type() && (stype == LLVM.Int16Type() || stype == LLVM.Int8Type()))
					ext = true;
				else if (dtype == LLVM.Int16Type() && (stype == LLVM.Int8Type()))
					ext = true;

				if (ext)
					dst = is_unsigned
						  ? LLVM.BuildZExt(Builder, src, dtype, "i" + INST.instruction_id++)
						  : LLVM.BuildSExt(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (LLVM.GetTypeKind(stype) == TypeKind.PointerTypeKind)
					dst = LLVM.BuildPointerCast(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (dtype == LLVM.DoubleType() && stype == LLVM.FloatType())
					dst = LLVM.BuildFPExt(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (stype == LLVM.Int64Type() && (dtype == LLVM.Int32Type() || dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
					dst = LLVM.BuildTrunc(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (stype == LLVM.Int32Type() && (dtype == LLVM.Int16Type() || dtype == LLVM.Int8Type()))
					dst = LLVM.BuildTrunc(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (stype == LLVM.Int16Type() && dtype == LLVM.Int8Type())
					dst = LLVM.BuildTrunc(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (stype == LLVM.DoubleType() && dtype == LLVM.FloatType())
					dst = LLVM.BuildFPTrunc(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (stype == LLVM.Int64Type() && (dtype == LLVM.FloatType()))
					dst = LLVM.BuildSIToFP(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (stype == LLVM.Int32Type() && (dtype == LLVM.FloatType()))
					dst = LLVM.BuildSIToFP(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (stype == LLVM.Int64Type() && (dtype == LLVM.DoubleType()))
					dst = LLVM.BuildSIToFP(Builder, src, dtype, "i" + INST.instruction_id++);
				else if (stype == LLVM.Int32Type() && (dtype == LLVM.DoubleType()))
					dst = LLVM.BuildSIToFP(Builder, src, dtype, "i" + INST.instruction_id++);
				else
					dst = LLVM.BuildBitCast(Builder, src, dtype, "i" + INST.instruction_id++);
			}

		    return dst;
		}
	}
    #endregion Casting definition
}

