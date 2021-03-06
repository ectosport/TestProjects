﻿using System;
using Fibonacci;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FibonacciUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private ISolveFibonacci solver;

        [TestInitialize]
        public void Initialize()
        {
            solver = new FibonacciRecursive();    
        }

        [TestMethod]
        public void CalculateFib0()
        {
            Assert.AreEqual(solver.Calculate(0), 0);
        }

        [TestMethod]
        public void CalculateFib1()
        {
            Assert.AreEqual(solver.Calculate(1), 1);
        }

        [TestMethod]
        public void CalculateFibNeg1()
        {
            Assert.AreEqual(solver.Calculate(-1), 1);
        }

        [TestMethod]
        public void CalculateFibNeg2()
        {
            Assert.AreEqual(solver.Calculate(-2), -1);
        }

        [TestMethod]
        public void CalculateFibToNeg8()
        {
            int[] answers = new int[8] { 1, -1, 2, -3, 5, -8, 13, -21 };
            int i = -1;
            foreach (var answer in answers)
            {
                Assert.AreEqual(solver.Calculate(i--), answer);
            }
        }

        [TestMethod]
        public void CalculateFibTo10()
        {
            int[] answers = new int[10] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34 };
            int i = 0;
            foreach (var answer in answers)
            {
                Assert.AreEqual(solver.Calculate(i++), answer);   
            }
        }
    }
}
