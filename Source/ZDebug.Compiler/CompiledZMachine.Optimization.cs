using System;
using ZDebug.Core.Collections;

namespace ZDebug.Compiler
{
    public sealed partial class CompiledZMachine
    {
        private readonly IntegerMap<ZCodeDelegate0> addressToCode0Map = new IntegerMap<ZCodeDelegate0>();
        private readonly IntegerMap<ZCodeDelegate1> addressToCode1Map = new IntegerMap<ZCodeDelegate1>();
        private readonly IntegerMap<ZCodeDelegate2> addressToCode2Map = new IntegerMap<ZCodeDelegate2>();
        private readonly IntegerMap<ZCodeDelegate3> addressToCode3Map = new IntegerMap<ZCodeDelegate3>();
        private readonly IntegerMap<ZCodeDelegate4> addressToCode4Map = new IntegerMap<ZCodeDelegate4>();
        private readonly IntegerMap<ZCodeDelegate5> addressToCode5Map = new IntegerMap<ZCodeDelegate5>();
        private readonly IntegerMap<ZCodeDelegate6> addressToCode6Map = new IntegerMap<ZCodeDelegate6>();
        private readonly IntegerMap<ZCodeDelegate7> addressToCode7Map = new IntegerMap<ZCodeDelegate7>();
        private readonly IntegerMap<ZCodeDelegate8> addressToCode8Map = new IntegerMap<ZCodeDelegate8>();
        private readonly IntegerMap<ZCodeDelegate9> addressToCode9Map = new IntegerMap<ZCodeDelegate9>();
        private readonly IntegerMap<ZCodeDelegate10> addressToCode10Map = new IntegerMap<ZCodeDelegate10>();
        private readonly IntegerMap<ZCodeDelegate11> addressToCode11Map = new IntegerMap<ZCodeDelegate11>();
        private readonly IntegerMap<ZCodeDelegate12> addressToCode12Map = new IntegerMap<ZCodeDelegate12>();
        private readonly IntegerMap<ZCodeDelegate13> addressToCode13Map = new IntegerMap<ZCodeDelegate13>();
        private readonly IntegerMap<ZCodeDelegate14> addressToCode14Map = new IntegerMap<ZCodeDelegate14>();
        private readonly IntegerMap<ZCodeDelegate15> addressToCode15Map = new IntegerMap<ZCodeDelegate15>();

        internal ZCodeDelegate0 GetCode0(int address)
        {
            ZCodeDelegate0 code;
            if (!addressToCode0Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile0(GetRoutineByAddress(address), this);
                addressToCode0Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate1 GetCode1(int address)
        {
            ZCodeDelegate1 code;
            if (!addressToCode1Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile1(GetRoutineByAddress(address), this);
                addressToCode1Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate2 GetCode2(int address)
        {
            ZCodeDelegate2 code;
            if (!addressToCode2Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile2(GetRoutineByAddress(address), this);
                addressToCode2Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate3 GetCode3(int address)
        {
            ZCodeDelegate3 code;
            if (!addressToCode3Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile3(GetRoutineByAddress(address), this);
                addressToCode3Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate4 GetCode4(int address)
        {
            ZCodeDelegate4 code;
            if (!addressToCode4Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile4(GetRoutineByAddress(address), this);
                addressToCode4Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate5 GetCode5(int address)
        {
            ZCodeDelegate5 code;
            if (!addressToCode5Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile5(GetRoutineByAddress(address), this);
                addressToCode5Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate6 GetCode6(int address)
        {
            ZCodeDelegate6 code;
            if (!addressToCode6Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile6(GetRoutineByAddress(address), this);
                addressToCode6Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate7 GetCode7(int address)
        {
            ZCodeDelegate7 code;
            if (!addressToCode7Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile7(GetRoutineByAddress(address), this);
                addressToCode7Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate8 GetCode8(int address)
        {
            ZCodeDelegate8 code;
            if (!addressToCode8Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile8(GetRoutineByAddress(address), this);
                addressToCode8Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate9 GetCode9(int address)
        {
            ZCodeDelegate9 code;
            if (!addressToCode9Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile9(GetRoutineByAddress(address), this);
                addressToCode9Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate10 GetCode10(int address)
        {
            ZCodeDelegate10 code;
            if (!addressToCode10Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile10(GetRoutineByAddress(address), this);
                addressToCode10Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate11 GetCode11(int address)
        {
            ZCodeDelegate11 code;
            if (!addressToCode11Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile11(GetRoutineByAddress(address), this);
                addressToCode11Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate12 GetCode12(int address)
        {
            ZCodeDelegate12 code;
            if (!addressToCode12Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile12(GetRoutineByAddress(address), this);
                addressToCode12Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate13 GetCode13(int address)
        {
            ZCodeDelegate13 code;
            if (!addressToCode13Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile13(GetRoutineByAddress(address), this);
                addressToCode13Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate14 GetCode14(int address)
        {
            ZCodeDelegate14 code;
            if (!addressToCode14Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile14(GetRoutineByAddress(address), this);
                addressToCode14Map.Add(address, code);
            }

            return code;
        }

        internal ZCodeDelegate15 GetCode15(int address)
        {
            ZCodeDelegate15 code;
            if (!addressToCode15Map.TryGetValue(address, out code))
            {
                code = ZCompiler.Compile15(GetRoutineByAddress(address), this);
                addressToCode15Map.Add(address, code);
            }

            return code;
        }

        internal Delegate GetCode(int address)
        {
            var localCount = this.Memory[address];

            switch (localCount)
            {
                case 0: return GetCode0(address);
                case 1: return GetCode1(address);
                case 2: return GetCode2(address);
                case 3: return GetCode3(address);
                case 4: return GetCode4(address);
                case 5: return GetCode5(address);
                case 6: return GetCode6(address);
                case 7: return GetCode7(address);
                case 8: return GetCode8(address);
                case 9: return GetCode9(address);
                case 10: return GetCode10(address);
                case 11: return GetCode11(address);
                case 12: return GetCode12(address);
                case 13: return GetCode13(address);
                case 14: return GetCode14(address);
                case 15: return GetCode15(address);

                default:
                    throw new ZMachineException(
                        string.Format("Unexpected local count {0} at address {1:x4}", localCount, address));
            }
        }
    }
}
