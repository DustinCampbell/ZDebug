using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using ZDebug.Core.Instructions;
using ZDebug.Terp.Profiling;
using ZDebug.Terp.Services;
using ZDebug.UI.Services;
using ZDebug.UI.ViewModel;

namespace ZDebug.Terp.ViewModel
{
    [Export]
    internal sealed class ProfilerViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private readonly ProfilerService profilerService;

        private List<ICall> callTreeRoot;
        private IEnumerable<IRoutine> routines;
        private IEnumerable instructions;
        private IEnumerable opcodes;

        [ImportingConstructor]
        private ProfilerViewModel(
            ProfilerService profilerService,
            StoryService storyService)
            : base("ProfilerView")
        {
            this.profilerService = profilerService;
            this.storyService = storyService;

            this.profilerService.Starting += ProfilerService_Starting;
            this.profilerService.Stopped += ProfilerService_Stopped;
        }

        private void ProfilerService_Starting(object sender, ProfilerStartingEventArgs e)
        {
            ClearProfilerData();
        }

        private void ProfilerService_Stopped(object sender, ProfilerStoppedEventArgs e)
        {
            PopulateProfilerData();
        }

        private void ClearProfilerData()
        {
            Dispatch(() =>
            {
                callTreeRoot = null;
                routines = null;
                instructions = null;
                opcodes = null;

                AllPropertiesChanged();
            });
        }

        private void PopulateProfilerData()
        {
            Dispatch(() =>
            {
                callTreeRoot = new List<ICall>() { profilerService.Profiler.RootCall };
                routines = profilerService.Profiler.Routines;

                var reader = new InstructionReader(0, storyService.Story.Memory);

                var instructions = profilerService.Profiler.InstructionTimings.Select(timing =>
                {
                    reader.Address = timing.Item1;
                    var i = reader.NextInstruction();
                    return new
                    {
                        Instruction = i,
                        Address = i.Address,
                        OpcodeName = i.Opcode.Name,
                        OperandCount = i.OperandCount,
                        TimesExecuted = timing.Item2.Item1,
                        TotalTime = timing.Item2.Item2
                    };
                });

                this.instructions = instructions.OrderByDescending(x => x.TotalTime).ToList();

                var opcodes = from i in instructions
                              group i by i.Instruction.Opcode.Name into g
                              select new
                              {
                                  Name = g.Key,
                                  TotalTime = g.Aggregate(TimeSpan.Zero, (r, t) => r + t.TotalTime),
                                  Count = g.Sum(x => x.TimesExecuted)
                              };

                this.opcodes = opcodes.OrderByDescending(x => x.TotalTime).ToList();

                AllPropertiesChanged();
            });
        }

        public List<ICall> CallTreeRoot
        {
            get
            {
                return callTreeRoot;
            }
        }

        public IEnumerable<IRoutine> Routines
        {
            get
            {
                return routines;
            }
        }

        public IEnumerable Instructions
        {
            get
            {
                return instructions;
            }
        }

        public IEnumerable Opcodes
        {
            get
            {
                return opcodes;
            }
        }
    }
}
