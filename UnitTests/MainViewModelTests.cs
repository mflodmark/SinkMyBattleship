using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SinkMyBattleshipWPF.Models;
using SinkMyBattleshipWPF.ViewModels;
using SinkMyBattleshipWPF.Extensions;
using SinkMyBattleshipWPF.Utils;
using System.Collections.Generic;

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
        public void CommandSyntaxCheckToLowerIsTrue()
        {
            //arrange
            var input = "fire a2";
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
        public void CommandSyntaxCheckIsFalse2()
        {
            //arrange
            var input = "verer";
            var main = new MainViewModel(new Player(null, "", 0, null));

            //act
            var test = main.CommandSyntaxCheck(input);

            //assert
            Assert.IsFalse(test);
        }

        [TestMethod]
        public void FireAtIsTrue()
        {
            //arrange
            var input = "fira a1";
            var player = new Player(null, "", 0, null);

            //act
            var test = player.FireAt(input);

            //assert
            Assert.AreEqual("A1", test);
        }

        [TestMethod]
        public void CheckFiredAtIsTrue()
        {
            //arrange
            var input = "fira a1";
            var player = new Player(null, "", 0, null);

            //act
            player.FireAt(input);
            var test = player.CheckFiredAt(input);

            //assert
            Assert.AreEqual(true, test);
        }

        [TestMethod]
        public void CheckFiredAtIsFalse()
        {
            //arrange
            var input = "fira a1";
            var player = new Player(null, "", 0, null);

            //act
            var test = player.CheckFiredAt(input);

            //assert
            Assert.AreEqual(false, test);
        }

        [TestMethod]
        public void GetFiredAtIsFalse()
        {
            //arrange
            var input = "fira b1";
            var player = new Player(null, "", 0, new List<Boat>());
            player.Boats.Add(new Boat("Carrier", new Dictionary<string, bool>() { { "A1", false }, { "A2", false }, { "A3", false }, { "A4", false }, { "A5", false } }));

            //act
            var test = player.GetFiredAt(input);

            //assert
            Assert.AreEqual(false, test);
        }

        [TestMethod]
        public void GetFiredAtIsTrue()
        {
            //arrange
            var input = "fira a1";
            var player = new Player(null, "", 0, new List<Boat>());
            player.Boats.Add(new Boat("Carrier", new Dictionary<string, bool>() { { "A1", false }, { "A2", false }, { "A3", false }, { "A4", false }, { "A5", false } }));

            //act
            var test = player.GetFiredAt(input);

            //assert
            Assert.AreEqual(true, test);
        }

        [TestMethod]
        public void GetFiredAtMessageIsTrue()
        {
            //arrange
            var input = "fira a1";
            var player = new Player(null, "", 0, new List<Boat>());
            player.Boats.Add(new Boat("Carrier", new Dictionary<string, bool>() { { "A1", false }, { "A2", false }, { "A3", false }, { "A4", false }, { "A5", false } }));

            //act
            var test = player.GetFiredAtMessage(input);

            //assert
            Assert.AreEqual(AnswerCodes.YouHitMyCarrier.GetDescription(), test);
        }

        [TestMethod]
        public void GetFiredAtMessageYouWon()
        {
            //arrange
            var input = "fira a1";
            var player = new Player(null, "", 0, new List<Boat>());
            player.Boats.Add(new Boat("Carrier", new Dictionary<string, bool>() { { "A1", false } }));

            //act
            player.GetFiredAt(input);
            var test = player.GetFiredAtMessage(input);

            //assert
            Assert.AreEqual(AnswerCodes.YouWin.GetDescription(), test);
        }

        [TestMethod]
        public void GetFiredAtMessageYouSunk()
        {
            //arrange
            var input = "fira a1";
            var player = new Player(null, "", 0, new List<Boat>());
            player.Boats.Add(new Boat("Carrier", new Dictionary<string, bool>() { { "A1", false } }));
            player.Boats.Add(new Boat("Destroyer", new Dictionary<string, bool>() { { "B1", false } }));

            //act
            player.GetFiredAt(input);
            var test = player.GetFiredAtMessage(input);

            //assert
            Assert.AreEqual(AnswerCodes.YouSunkMyCarrier.GetDescription(), test);
        }

        [TestMethod]
        public void GetFiredAtMessageMiss()
        {
            //arrange
            var input = "fira C1";
            var player = new Player(null, "", 0, new List<Boat>());
            player.Boats.Add(new Boat("Carrier", new Dictionary<string, bool>() { { "A1", false } }));
            player.Boats.Add(new Boat("Destroyer", new Dictionary<string, bool>() { { "B1", false } }));

            //act
            player.GetFiredAt(input);
            var test = player.GetFiredAtMessage(input);

            //assert
            Assert.AreEqual(AnswerCodes.Miss.GetDescription(), test);
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
