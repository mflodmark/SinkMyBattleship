using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SinkMyBattleshipWPF.Models;
using SinkMyBattleshipWPF.ViewModels;
using SinkMyBattleshipWPF.Extensions;


namespace UnitTests
{
    [TestClass]
    public class MainViewModelTests
    {
        [TestMethod]
        public void FireSyntaxCheckIsTrue()
        {
            //arrange
            var input = "A3";
            var main = new MainViewModel(new Player(null, "", 0, null));

            //act
            var test = main.FireSyntaxCheck(input);

            //assert
            Assert.IsTrue(test);
        }

        [TestMethod]
        public void FireSyntaxCheckIsFalse()
        {
            //arrange
            var input = "M9";
            var main = new MainViewModel(new Player(null, "", 0, null));

            //act
            var test = main.FireSyntaxCheck(input);

            //assert
            Assert.IsFalse(test);
        }

        [TestMethod]
        public void CommandSyntaxCheckIsTrue()
        {
            //arrange
            var input = "FIRE A3";
            var main = new MainViewModel(new Player(null, "", 0, null));

            //act
            var test = main.CommandSyntaxCheck(input);

            //assert
            Assert.IsTrue(test);
        }

        [TestMethod]
        public void CommandSyntaxCheckIsFalse()
        {
            //arrange
            var input = "FIRE M9";
            var main = new MainViewModel(new Player(null, "", 0, null));

            //act
            var test = main.CommandSyntaxCheck(input);

            //assert
            Assert.IsFalse(test);
        }

        [TestMethod]
        public void EnumDescriptionIsTrue()
        {
            //arrange
            var enumClass = AnswerCodes.Sequence_Error;

            //act
            var enumString = enumClass.GetDescription();

            //assert
            Assert.AreEqual("501 Sequence Error", enumString);
        }
    }
}
