using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BowlingGame;

namespace BowlingGameTests
{
    [TestClass]
    public class BowlingGameTests
    {
        

        [TestMethod]
        public void TestAllGutters()
        {
            BowlingGame.BowlingGame game = new BowlingGame.BowlingGame();

            RollBowlingBall(game, 0, 20);

            Assert.AreEqual(game.Score, 0);
        }

        [TestMethod]
        public void TestAllSinglePins()
        {
            BowlingGame.BowlingGame game = new BowlingGame.BowlingGame();

            RollBowlingBall(game, 1, 20);

            Assert.AreEqual(game.Score, 20);
        }

        [TestMethod]
        public void TestSingleSpare()
        {
            BowlingGame.BowlingGame game = new BowlingGame.BowlingGame();

            game.Roll(6);
            game.Roll(4);
            game.Roll(7);
            RollBowlingBall(game, 0, 17);

            Assert.AreEqual(game.Score, 24);
        }

        [TestMethod]
        public void TestSingleStrike()
        {
            BowlingGame.BowlingGame game = new BowlingGame.BowlingGame();

            game.Roll(10);
            game.Roll(4);
            game.Roll(5);
            RollBowlingBall(game, 0, 16);

            Assert.AreEqual(game.Score, 28);
        }

        [TestMethod]
        public void TestTwoStrikes()
        {
            BowlingGame.BowlingGame game = new BowlingGame.BowlingGame();

            game.Roll(10);
            game.Roll(10);
            game.Roll(5);
            game.Roll(4);
            RollBowlingBall(game, 0, 14);

            Assert.AreEqual(game.Score, 53);
        }

        [TestMethod]
        public void TestPerfectGame()
        {
            BowlingGame.BowlingGame game = new BowlingGame.BowlingGame();

            RollBowlingBall(game, 10, 12);

            Assert.AreEqual(game.Score, 300);
        }

        [TestMethod]
        public void TestHittingMoreThan10Pins()
        {
            BowlingGame.BowlingGame game = new BowlingGame.BowlingGame();

            try
            {
                game.Roll(11);
            }
            catch (ArgumentException e)
            {
                StringAssert.Contains(e.Message, BowlingGame.BowlingGame.ExceptionMoreThan10PinsSingleRollMessage);
                return;
            }

            Assert.Fail("Expected exception for hitting more than 10 pins in a single roll.");
        }

        [TestMethod]
        public void TestHittingMoreThan10PinsTwoRolls()
        {
            BowlingGame.BowlingGame game = new BowlingGame.BowlingGame();

            try
            {
                game.Roll(5);
                game.Roll(6);
            }
            catch (ArgumentException e)
            {
                StringAssert.Contains(e.Message, BowlingGame.BowlingGame.ExceptionMoreThan10PinsTwoRollsMessage);
                return;
            }

            Assert.Fail("Expected exception for hitting more than 10 pins in two rolls.");
        }

        private static void RollBowlingBall(BowlingGame.BowlingGame game, int pinsHit, int numberOfRolls)
        {
            for (int i = 0; i < numberOfRolls; ++i)
            {
                game.Roll(pinsHit);
            }
        }
    }
}

