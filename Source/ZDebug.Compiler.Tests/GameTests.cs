using NUnit.Framework;
using ZDebug.Compiler.Tests.Mocks;
using ZDebug.Compiler.Tests.Utilities;
using ZDebug.Core;

namespace ZDebug.Compiler.Tests
{
    [TestFixture]
    public class GameTests
    {
        private string RunGame(string storyName, string scriptName = null)
        {
            var story = Story.FromStream(Resources.LoadStream(storyName));
            story.RegisterInterpreter(new MockInterpreter());

            var scriptCommands = scriptName != null
                ? Resources.LoadLines(scriptName)
                : null;

            var mockScreen = new MockScreen(
                scriptCommands,
                doneAction: () =>
                {
                    throw new ZMachineInterruptedException();
                });

            var machine = new CompiledZMachine(story);
            machine.RegisterScreen(mockScreen);
            machine.SetRandomSeed(42);

            try
            {
                machine.Run();
            }
            catch (ZMachineQuitException)
            {
                // done
            }
            catch (ZMachineInterruptedException)
            {
                // done
            }

            return mockScreen.Output;
        }

        [Test]
        public void RunCZech()
        {
            var output = RunGame(Resources.CZech);
            var expected = Resources.LoadText(Resources.CZechTranscript);

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        public void LoadRota()
        {
            var output = RunGame(Resources.Rota, Resources.RotaScript);
            var expected = Resources.LoadText(Resources.RotaTranscript);

            Assert.That(output, Is.EqualTo(expected));
        }

        [Test]
        public void RunZork()
        {
            var output = RunGame(Resources.Zork1, Resources.Zork1Script);
            var expected = Resources.LoadText(Resources.Zork1Transcript);

            Assert.That(output, Is.EqualTo(expected));
        }
    }
}
