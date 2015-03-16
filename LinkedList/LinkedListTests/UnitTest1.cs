using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinkedList;

namespace LinkedListTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void AddItems()
        {
            LinkedList<int> list = new LinkedList<int>();

            list.Add(5);
            list.Add(4);
            list.Add(3);
            
            Assert.AreEqual(list.Retrieve(0), 5);
            Assert.AreEqual(list.Retrieve(1), 4);
            Assert.AreEqual(list.Retrieve(2), 3);

            Assert.AreEqual(list.Count, 3);
        }

        [TestMethod]
        public void AddAndRemoveInMiddle()
        {
            LinkedList<int> list = new LinkedList<int>();

            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.Remove(1);

            Assert.AreEqual(list.Retrieve(0), 5);
            Assert.AreEqual(list.Retrieve(1), 3);
            
            Assert.AreEqual(list.Count, 2);
        }

        [TestMethod]
        public void AddAndRemoveAtStart()
        {
            LinkedList<int> list = new LinkedList<int>();

            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.Remove(0);

            Assert.AreEqual(list.Retrieve(0), 4);
            Assert.AreEqual(list.Retrieve(1), 3);

            Assert.AreEqual(list.Count, 2);
        }

        [TestMethod]
        public void AddAndRemoveAtEnd()
        {
            LinkedList<int> list = new LinkedList<int>();

            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.Remove(2);

            Assert.AreEqual(list.Retrieve(0), 5);
            Assert.AreEqual(list.Retrieve(1), 4);

            Assert.AreEqual(list.Count, 2);
        }

        [TestMethod]
        public void AddAndRemoveAtStartAndAddAgain()
        {
            LinkedList<int> list = new LinkedList<int>();

            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.Remove(0);

            Assert.AreEqual(list.Retrieve(0), 4);
            Assert.AreEqual(list.Retrieve(1), 3);
            Assert.AreEqual(list.Count, 2);

            list.Add(2);
            Assert.AreEqual(list.Retrieve(0), 4);
            Assert.AreEqual(list.Retrieve(1), 3);
            Assert.AreEqual(list.Retrieve(2), 2);
            Assert.AreEqual(list.Count, 3);
        }

        [TestMethod]
        public void AddOneAndRemoveAtStartAndAddAgain()
        {
            LinkedList<int> list = new LinkedList<int>();

            list.Add(5);
            list.Remove(0);

            Assert.AreEqual(list.Count, 0);

            list.Add(2);
            list.Add(1);
            Assert.AreEqual(list.Retrieve(0), 2);
            Assert.AreEqual(list.Retrieve(1), 1);
            Assert.AreEqual(list.Count, 2);
        }

        [TestMethod]
        public void AddAndRemoveAtEndAndAddAgain()
        {
            LinkedList<int> list = new LinkedList<int>();

            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.Remove(2);

            Assert.AreEqual(list.Retrieve(0), 5);
            Assert.AreEqual(list.Retrieve(1), 4);
            Assert.AreEqual(list.Count, 2);

            list.Add(2);

            Assert.AreEqual(list.Retrieve(0), 5);
            Assert.AreEqual(list.Retrieve(1), 4);
            Assert.AreEqual(list.Retrieve(2), 2);
            Assert.AreEqual(list.Count, 3);
        }
    }
}
