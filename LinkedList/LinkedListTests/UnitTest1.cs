using System;
using System.Linq;
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

            Assert.AreEqual(list[0], 5);
            Assert.AreEqual(list[1], 4);
            Assert.AreEqual(list[2], 3);

            Assert.AreEqual(list.Count, 3);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveInMiddle()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.RemoveAt(1);

            Assert.AreEqual(list[0], 5);
            Assert.AreEqual(list[1], 3);

            Assert.AreEqual(list.Count, 2);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveAtStart()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.RemoveAt(0);

            Assert.AreEqual(list[0], 4);
            Assert.AreEqual(list[1], 3);

            Assert.AreEqual(list.Count, 2);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveAtEnd()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.RemoveAt(2);

            Assert.AreEqual(list[0], 5);
            Assert.AreEqual(list[1], 4);

            Assert.AreEqual(list.Count, 2);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveAtStartAndAddAgain()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.RemoveAt(0);

            Assert.AreEqual(list[0], 4);
            Assert.AreEqual(list[1], 3);
            Assert.AreEqual(list.Count, 2);

            list.Add(2);
            Assert.AreEqual(list[0], 4);
            Assert.AreEqual(list[1], 3);
            Assert.AreEqual(list[2], 2);
            Assert.AreEqual(list.Count, 3);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddOneAndRemoveAtStartAndAddAgain()
        {
            list.Add(5);
            list.RemoveAt(0);

            Assert.AreEqual(list.Count, 0);

            list.Add(2);
            list.Add(1);
            Assert.AreEqual(list[0], 2);
            Assert.AreEqual(list[1], 1);
            Assert.AreEqual(list.Count, 2);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAndRemoveAtEndAndAddAgain()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);

            list.RemoveAt(2);

            Assert.AreEqual(list[0], 5);
            Assert.AreEqual(list[1], 4);
            Assert.AreEqual(list.Count, 2);

            list.Add(2);

            Assert.AreEqual(list[0], 5);
            Assert.AreEqual(list[1], 4);
            Assert.AreEqual(list[2], 2);
            Assert.AreEqual(list.Count, 3);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddAtHeadExplicitly()
        {
            list.Insert(0, 5);
            
            Assert.AreEqual(list[0], 5);
            Assert.AreEqual(list.Count, 1);
        }

        [Test]
        [Category("DataIntegrity")]
        public void InsertIntoMiddleStartAndEnd()
        {
            list.Add(5);
            list.Add(4);
            list.Add(3);
            list.Insert(2, 2);
            list.Insert(0, 1);
            list.Insert(1, 0);
            list.Add(-1);
            list.Insert(7, -2);

            Assert.AreEqual(list[0], 1);
            Assert.AreEqual(list[1], 0);
            Assert.AreEqual(list[2], 5);
            Assert.AreEqual(list[3], 4);
            Assert.AreEqual(list[4], 2);
            Assert.AreEqual(list[5], 3);
            Assert.AreEqual(list[6], -1);
            Assert.AreEqual(list[7], -2);
            Assert.AreEqual(list.Count, 8);
        }

        [Test]
        [Category("DataIntegrity")]
        public void AddExplicitlyAtStartMultiple()
        {
            list.Insert(0, 5);
            list.Insert(0, 4);
            list.Insert(0, 3);
            list.Insert(0, 2);
            list.Insert(0, 1);
            list.Insert(0, 0);

            Assert.AreEqual(list[0], 0);
            Assert.AreEqual(list[1], 1);
            Assert.AreEqual(list[2], 2);
            Assert.AreEqual(list[3], 3);
            Assert.AreEqual(list[4], 4);
            Assert.AreEqual(list[5], 5);
            Assert.AreEqual(list.Count, 6);
        }

        [Test]
        [Category("ExceptionTests")]
        [ExpectedException("System.IndexOutOfRangeException")]
        public void RetrieveIndexOutOfBoundsWhenEmpty()
        {
            var test = list[0];
        }

        [Test]
        [Category("ExceptionTests")]
        [ExpectedException("System.IndexOutOfRangeException")]
        public void RetrieveIndexOutOfBoundsWithContents()
        {
            list.Add(1);
            var test = list[1];
        }

        [Test]
        [Category("ExceptionTests")]
        [ExpectedException("System.IndexOutOfRangeException")]
        public void AddBeyondCount()
        {
            list.Insert(1, 0);
        }

        [Test]
        [Category("FindingTests")]
        public void FindIndexOfExistingItem()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            Assert.AreEqual(list.IndexOf(3), 2);
        }

        [Test]
        [Category("FindingTests")]
        public void FindIndexOfMissingItem()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            Assert.AreEqual(list.IndexOf(5), -1);
        }

        [Test]
        [Category("CopyToArrayTests")]
        public void CopyToArrayBasic()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            int[] array = new int[list.Count];
            list.CopyTo(array, 0);

            int answer = 1;
            foreach (var item in array)
            {
                Assert.AreEqual(item, answer++);    
            }
        }

        [Test]
        [Category("CopyToArrayTests")]
        [ExpectedException("System.IndexOutOfRangeException")]
        public void CopyToArrayTooSmall()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            int[] array = new int[list.Count];
            list.CopyTo(array, 1);
        }

        [Test]
        [Category("CopyToArrayTests")]
        [ExpectedException("System.IndexOutOfRangeException")]
        public void CopyToArrayTooSmallIndex0()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            int[] array = new int[list.Count - 1];
            list.CopyTo(array, 0);
        }

        [Test]
        [Category("RemoveTests")]
        public void RemoveItemAtEnd()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            Assert.AreEqual(list.Remove(4), true); 

            Assert.AreEqual(list[0], 1);
            Assert.AreEqual(list[1], 2);
            Assert.AreEqual(list[2], 3);
        }

        [Test]
        [Category("RemoveTests")]
        public void RemoveItemAtStart()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            Assert.AreEqual(list.Remove(1), true);

            Assert.AreEqual(list[0], 2);
            Assert.AreEqual(list[1], 3);
            Assert.AreEqual(list[2], 4);
        }

        [Test]
        [Category("RemoveTests")]
        public void RemoveMissingItem()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            Assert.AreEqual(list.Remove(5), false);

            Assert.AreEqual(list[0], 1);
            Assert.AreEqual(list[1], 2);
            Assert.AreEqual(list[2], 3);
            Assert.AreEqual(list[3], 4);
        }

        [Test]
        [Category("EnumeratorTests")]
        public void EnumerateOverList()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            int answer = 1;
            foreach (var item in list)
            {
                Assert.AreEqual(list[answer - 1], answer);
                ++answer;
            }
        }

        [Test]
        [Category("EnumeratorTests")]
        [ExpectedException("System.InvalidOperationException")]
        public void EnumerateOverListAndModifyWithAdd()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            int answer = 1;
            foreach (var item in list)
            {
                Assert.AreEqual(list[answer - 1], answer);
                ++answer;

                // modify list while enumerating
                list.Add(5);
            }
        }

        [Test]
        [Category("EnumeratorTests")]
        [ExpectedException("System.InvalidOperationException")]
        public void EnumerateOverListAndModifyWithRemove()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            int answer = 1;
            foreach (var item in list)
            {
                Assert.AreEqual(list[answer - 1], answer);
                ++answer;

                // modify list while enumerating
                list.RemoveAt(0);
            }
        }

        [Test]
        [Category("EnumeratorTests")]
        public void NestedEnumerateOverList()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            int answer = 1;
            foreach (var item in list)
            {
                Assert.AreEqual(list[answer - 1], answer);
                ++answer;

                int innerAnswer = 1;
                foreach (var innerItem in list)
                {
                    Assert.AreEqual(list[innerAnswer - 1], innerAnswer);
                    ++innerAnswer;
                }
            }
        }

        [Test]
        [Category("ArrayIndexTests")]
        public void ArrayIndexSetterTest()
        {
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            for (int i = 0; i < 4; ++i)
            {
                list[i] = list[i]*2;
            }

            for (int i = 0; i < 4; ++i)
            {
                Assert.AreEqual(list[i], (i+1)*2);
            }
        }

        [Test]
        [Category("ArrayIndexTests")]
        [ExpectedException("System.IndexOutOfRangeException")]
        public void ArrayIndexOutOfBoundsSetterTest()
        {
            list[5] = 10;
        }
    }
}
