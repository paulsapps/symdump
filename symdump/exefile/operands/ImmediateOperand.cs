﻿using symdump.exefile.dataflow;
using symdump.exefile.expression;

namespace symdump.exefile.operands
{
    public class ImmediateOperand : IOperand
    {
        public readonly long value;

        public ImmediateOperand(long value)
        {
            this.value = value;
        }

        public bool Equals(IOperand other)
        {
            var o = other as ImmediateOperand;
            return value == o?.value;
        }

        public IExpressionNode toExpressionNode(DataFlowState dataFlowState)
        {
            return new ValueNode(value);
        }

        public override string ToString()
        {
            return value >= 0 ? $"0x{value:X}" : $"-0x{-value:X}";
        }
    }
}