using System;

namespace LinkedList
{
    public class LinkedList<T>
    {
        private class Node<T>
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

        public int Count
        {
            get { return count; }
        }

        public LinkedList()
        {
            head = null;
            lastNode = null;
            count = 0;
        }

        public void Add(T item)
        {
            Node<T> newNode = new Node<T>(item);

            if (head == null)
            {
                head = newNode;
            }
            else
            {
                lastNode.Next = newNode;
            }

            lastNode = newNode;
            ++count;
        }

        public T Retrieve(int index)
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

        public T Remove(int index)
        {
            if (index >= count)
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
            return iterateNode.Data;
        }

        public void PrintNodesToConsole()
        {
            Node<T> iterateNode = head;
            int i = 0;

            while (iterateNode != null)
            {
                Console.WriteLine("Node {0}: {1}", i++, iterateNode.Data.ToString());
                iterateNode = iterateNode.Next;
            }
        }
    }
}