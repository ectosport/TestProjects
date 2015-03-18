using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LinkedList
{
    public class LinkedList<T> : IList<T> where T : IEquatable<T>
    {
        private class Node<T> where T:IEquatable<T>
        {
            private Node<T> nextNode;
            public Node<T> Next
            {
                get { return nextNode; }
                set { nextNode = value; }
            }

            private T data;
            public T Data
            {
                get { return data; }
                set { data = value; }
            }

            public Node(T data)
            {
                this.data = data;
                this.nextNode = null;
            }
        }

        private Node<T> head;
        private Node<T> lastNode;
        private int count;
        private int version;
        
        public int Count
        {
            get { return count; }
        }

        public LinkedList()
        {
            head = null;
            lastNode = null;
            count = 0;
            version = 0;
        }

        private T Retrieve(int index)
        {
            if (index >= count)
            {
                throw new IndexOutOfRangeException();
            }

            Node<T> iterateNode = this.head;
            for (int i = 0; i < index; i++)
            {
                iterateNode = iterateNode.Next;
            }

            return iterateNode.Data;
        }

        public void RemoveAt(int index)
        {
            if (index >= count || index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            Node<T> iterateNode = this.head;
            Node<T> previousNode = null;
            if (index == 0)
            {
                // special case for head
                this.head = this.head.Next;
            }
            else
            {
                for (int i = 0; i < index; i++)
                {
                    previousNode = iterateNode;
                    iterateNode = iterateNode.Next;
                }

                previousNode.Next = iterateNode.Next;

                // check if the last node was removed, if so update last node
                if (lastNode == iterateNode)
                {
                    lastNode = previousNode;
                }
            }

            --count;
            ++version;
        }

        public void PrintNodesToConsole()
        {
            Node<T> iterateNode = this.head;
            int i = 0;

            while (iterateNode != null)
            {
                Console.WriteLine("Node {0}: {1}", i++, iterateNode.Data.ToString());
                iterateNode = iterateNode.Next;
            }
        }

        public int IndexOf(T item)
        {
            Node<T> iterateNode = this.head;
            int i = 0;

            while (iterateNode != null)
            {
                if (item.Equals(iterateNode.Data)) return i;

                iterateNode = iterateNode.Next;
                ++i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            Node<T> newNode = new Node<T>(item);

            if (index > count)
            {
                // Beyond count, throw exception.
                // It's OK to be == count because that means add it to the end.
                throw new IndexOutOfRangeException();
            }
            else if (head == null || index == 0)
            {
                // special case for 
                newNode.Next = head;
                head = newNode;
            }
            else if (index == -1)
            {
                lastNode.Next = newNode;
            }
            else
            {
                Node<T> iterateNode = this.head;
                for (int i = 1; i < index; i++)
                {
                    iterateNode = iterateNode.Next;
                }

                newNode.Next = iterateNode.Next;
                iterateNode.Next = newNode;
            }

            if (newNode.Next == null)
            {
                lastNode = newNode;
            }

            ++count;
            ++version;
        }

        public T this[int index]
        {
            get { return Retrieve(index); }
            set
            {
                if (index >= count || index < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                Node<T> iterateNode = this.head;
                for (int i = 0; i < index; i++)
                {
                    iterateNode = iterateNode.Next;
                }

                iterateNode.Data = value;
            }
        }

        public void Add(T item)
        {
            this.Insert(-1, item);
        }

        public void Clear()
        {
            this.head = null;
            this.lastNode = null;
            this.count = 0;
            ++this.version;
        }

        public bool Contains(T item)
        {
            return this.IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if ((array.Count() - arrayIndex) < this.count)
            {
                throw new IndexOutOfRangeException();
            }

            Node<T> iterateNode = this.head;
            int i = 0;

            while (iterateNode != null)
            {
                array[i++] = iterateNode.Data;
                iterateNode = iterateNode.Next;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index < 0) return false;

            this.RemoveAt(index);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #region Nested classes

        public struct Enumerator : IEnumerator<T>
        {
            /// <summary>
            /// The collection to iterate through.
            /// </summary>
            private LinkedList<T> list;

            /// <summary>
            /// The modification value of the collection when the enumerator was created.
            /// </summary>
            private readonly long version;
            /// <summary>
            /// The current value of the enumeration.
            /// </summary>
            private Node<T> currentNode;
            /// <summary>
            /// The constructor.
            /// </summary>
            /// <param name="buffer">The collection to iterate through.</param>
            internal Enumerator(LinkedList<T> buffer)
            {
                this.list = buffer;
                this.currentNode = buffer.head;
                this.version = buffer.version;
            }
            /// <summary>
            /// Gets the current value of the iteration. Throws an exception if enumerator has whether not started or finished.
            /// </summary>
            public T Current
            {
                get
                {
                    if (currentNode == null) return default(T);
                    
                    return this.currentNode.Data;
                }
            }
            /// <summary>
            /// The method corresponding to the IDisposable interface.
            /// </summary>
            public void Dispose()
            {
                
            }
            /// <summary>
            /// Advances the enumerator to the next element in the collection. Returns true if the enumerator has succefully advanced; otherwise false.
            /// </summary>
            public bool MoveNext()
            {
                if (this.version != this.list.version)
                {
                    throw new InvalidOperationException("Enumeration canceled. Collection was modified.");
                }

                this.currentNode = this.currentNode.Next;
                if (this.currentNode == null) return false;

                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
        }
        #endregion
    }
}