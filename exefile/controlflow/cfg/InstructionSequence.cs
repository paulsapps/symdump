﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using core;
using core.util;
using JetBrains.Annotations;

namespace exefile.controlflow.cfg
{
    public class InstructionSequence : Node
    {
        public InstructionSequence([NotNull] IGraph graph)
            : base(graph)
        {
        }

        public override SortedDictionary<uint, Instruction> Instructions { get; } =
            new SortedDictionary<uint, Instruction>();

        public override bool ContainsAddress(uint address)
        {
            if (Instructions.Count == 0)
                return false;

            return address >= Instructions.Keys.First() && address <= Instructions.Keys.Last();
        }

        public override void Dump(IndentedTextWriter writer)
        {
            foreach (var edge in Outs)
            {
                writer.WriteLine($"// {edge}");
            }

            ++writer.Indent;
            foreach (var insn in Instructions)
            {
                writer.WriteLine($"0x{insn.Key:X}  {insn.Value.AsReadable()}");
            }
            --writer.Indent;
        }

        public InstructionSequence Chop(uint from)
        {
            var result = new InstructionSequence(Graph);
            foreach (var split in Instructions.Where(i => i.Key >= from))
            {
                result.Instructions.Add(split.Key, split.Value);
            }
            foreach (var rm in result.Instructions.Keys)
            {
                Instructions.Remove(rm);
            }

            return result;
        }
    }
}