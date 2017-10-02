﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using core.util;
using JetBrains.Annotations;

namespace core.cfg
{
    public class IfNode : Node
    {
        [NotNull] private readonly INode _condition;

        [NotNull] private readonly INode _body;

        private readonly bool _invertedCondition;

        public override string Id => "if_" + _condition.Id;

        public override IEnumerable<int> InputRegisters => _condition.InputRegisters.Concat(_body.InputRegisters).Distinct();

        public override IEnumerable<int> OutputRegisters => _condition.OutputRegisters.Concat(_body.OutputRegisters).Distinct();

        public IfNode([NotNull] INode condition)
            : base(condition.Graph)
        {
            Debug.Assert(IsCandidate(condition));

            Debug.Assert(condition.Outs.Count() == 2);
            
            var trueEdge = condition.Outs.First(e => e is TrueEdge);
            var falseEdge = condition.Outs.First(e => e is FalseEdge);

            var trueNode = trueEdge.To;
            var falseNode = falseEdge.To;
            _invertedCondition = trueNode.Ins.Count() != 1 || trueNode.Outs.Count() != 1 || !(trueNode.Outs.First() is AlwaysEdge);

            INode body, common;
            if (!_invertedCondition)
            {
                body = trueNode;
                common = body.Outs.First().To;
                Debug.Assert(common.Equals(falseNode));
            }
            else
            {
                body = falseNode;
                common = body.Outs.First().To;
                Debug.Assert(common.Equals(trueNode));
            }

            Debug.Assert(body.Outs.Count() == 1);
            Debug.Assert(body.Outs.First() is AlwaysEdge);

            _condition = condition;
            _body = body;

            Graph.ReplaceNode(condition, this);
            Graph.RemoveNode(body);
            var outs = Outs.ToList();
            foreach(var e in outs)
                Graph.RemoveEdge(e);
            
            Graph.AddEdge(new AlwaysEdge(this, common));
        }

        public override IEnumerable<Instruction> Instructions
        {
            get
            {
                foreach (var insn in _condition.Instructions) yield return insn;
                foreach (var insn in _body.Instructions) yield return insn;
            }
        }

        public override bool ContainsAddress(uint address) =>
            _condition.ContainsAddress(address) || _body.ContainsAddress(address);

        public override void Dump(IndentedTextWriter writer, IDataFlowState dataFlowState)
        {
            writer.WriteLine(_invertedCondition ? "if_not{" : "if{");
            ++writer.Indent;
            _condition.Dump(writer, dataFlowState);
            --writer.Indent;
            writer.WriteLine("} {");
            ++writer.Indent;
            _body.Dump(writer, dataFlowState);
            --writer.Indent;
            writer.WriteLine("}");
        }

        public static bool IsCandidate([NotNull] INode condition)
        {
            if (condition is EntryNode || condition is ExitNode)
                return false;
            
            if (condition.Outs.Count() != 2)
                return false;

            var trueNode = condition.Outs.FirstOrDefault(e => e is TrueEdge)?.To;
            if (trueNode == null)
                return false;

            var falseNode = condition.Outs.FirstOrDefault(e => e is FalseEdge)?.To;
            if (falseNode == null)
                return false;

            // if(condition) trueNode;
            if (trueNode.Ins.Count() == 1 && trueNode.Outs.Count() == 1 && trueNode.Outs.First() is AlwaysEdge)
            {
                if (trueNode.Outs.First().To.Equals(falseNode))
                {
                    return true;
                }
            }

            // ReSharper disable once InvertIf
            // if(!condition) falseNode;
            if (falseNode.Ins.Count() == 1 && falseNode.Outs.Count() == 1 && falseNode.Outs.First() is AlwaysEdge)
            {
                // ReSharper disable once InvertIf
                if (falseNode.Outs.First().To.Equals(trueNode))
                {
                    return true;
                }
            }

            return false;
        }
    }
}