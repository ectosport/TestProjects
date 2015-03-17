using System;
using LinkedList;
using NUnit.Framework;

namespace LinkedListTests
{
    [TestFixture]
    public class UnitTest1
    {
        private LinkedList<int> list;

        [SetUp]
        public void TestSetup()
        {
            list = new LinkedList<int>();
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndValidateItems()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);

            Assert.AreEqual(list.Retrieve(0), 5);
            Assert.AreEqual(list.Retrieve(1), 4);
            Assert.AreEqual(list.Retrieve(2), 3);

            Assert.AreEqual(list.Count, 3);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveInMiddle()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.Remove(1);

            Assert.AreEqual(list.Retrieve(0), 5);
            Assert.AreEqual(list.Retrieve(1), 3);

            Assert.AreEqual(list.Count, 2);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveAtStart()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.Remove(0);

            Assert.AreEqual(list.Retrieve(0), 4);
            Assert.AreEqual(list.Retrieve(1), 3);

            Assert.AreEqual(list.Count, 2);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveAtEnd()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.Remove(2);

            Assert.AreEqual(list.Retrieve(0), 5);
            Assert.AreEqual(list.Retrieve(1), 4);

            Assert.AreEqual(list.Count, 2);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveAtStartAndAddAgain()
        {
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

        [Test]
        [Category("DataIntegrity")]
        public void AddOneAndRemoveAtStartAndAddAgain()
        {
            list.Add(5);
            list.Remove(0);

            Assert.AreEqual(list.Count, 0);

            list.Add(2);
            list.Add(1);
            Assert.AreEqual(list.Retrieve(0), 2);
            Assert.AreEqual(list.Retrieve(1), 1);
            Assert.AreEqual(list.Count, 2);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveAtEndAndAddAgain()
        {
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

        [Test]
        [Category("DataIntegrity")]
        public void AddAtHeadExplicitly()
        {
            list.Add(5, 0);
            
            Assert.AreEqual(list.Retrieve(0), 5);
            Assert.AreEqual(list.Count, 1);
        }

        [Test]
        [Category("DataIntegrity")]
        public void InsertIntoMiddleStartAndEnd()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);
            list.Add(2, 2);
            list.Add(1, 0);
            list.Add(0, 1);
            list.Add(-1);
            list.Add(-2, 7);

            Assert.AreEqual(list.Retrieve(0), 1);
            Assert.AreEqual(list.Retrieve(1), 0);
            Assert.AreEqual(list.Retrieve(2), 5);
            Assert.AreEqual(list.Retrieve(3), 4);
            Assert.AreEqual(list.Retrieve(4), 2);
            Assert.AreEqual(list.Retrieve(5), 3);
            Assert.AreEqual(list.Retrieve(6), -1);
            Assert.AreEqual(list.Retrieve(7), -2);
            Assert.AreEqual(list.Count, 8);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddExplicitlyAtStartMultiple()
        {
            list.Add(5, 0);
            list.Add(4, 0);
            list.Add(3, 0);
            list.Add(2, 0);
            list.Add(1, 0);
            list.Add(0, 0);

            Assert.AreEqual(list.Retrieve(0), 0);
            Assert.AreEqual(list.Retrieve(1), 1);
            Assert.AreEqual(list.Retrieve(2), 2);
            Assert.AreEqual(list.Retrieve(3), 3);
            Assert.AreEqual(list.Retrieve(4), 4);
            Assert.AreEqual(list.Retrieve(5), 5);
            Assert.AreEqual(list.Count, 6);
        }

        [Test]
        [Category("ExceptionTests")]
        [ExpectedException("System.IndexOutOfRangeException")]
        public void RetrieveIndexOutOfBoundsWhenEmpty()
        {
            list.Retrieve(0);
        }

        [Test]
        [Category("ExceptionTests")]
        [ExpectedException("System.IndexOutOfRangeException")]
        public void RetrieveIndexOutOfBoundsWithContents()
        {
            LinkedList<int> list = new LinkedList<int>();

            list.Add(1);
            list.Retrieve(1);
        }

        [Test]
        [Category("ExceptionTests")]
        [ExpectedException("System.IndexOutOfRangeException")]
        public void AddBeyondCount()
        {
            list.Add(0, 1);
        }
    }
}
