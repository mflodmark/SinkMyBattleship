using Microsoft.VisualStudio.TestTools.UnitTesting;
using SinkMyBattleshipWPF.Models;
using SinkMyBattleshipWPF.Views;

namespace UnitTests
{
    [TestClass]
    public class MainViewTests
    {
        [TestMethod]
        public void GetCoordinateFromIsTrue()
        {
            //arrange
            var pos = new Position("A1", 1);

            //act
            var actual = pos.GetCoordinateFrom(1, 5);

            //assert
            Assert.AreEqual("A5", actual);
        }
    }
}
