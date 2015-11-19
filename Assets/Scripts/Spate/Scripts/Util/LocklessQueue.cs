using System;
using System.Collections.Generic;
using System.Threading;

namespace Spate
{
    /// <summary>
    /// 无锁队列
    /// </summary>
    public class LocklessQueue<T>
    {
        private class Node
        {
            internal T Item;
            internal Node Next;

            public Node(T item, Node next)
            {
                Item = item;
                Next = next;
            }
        }

        private Node mHead;
        private Node mTail;

        public LocklessQueue()
        {
            mHead = new Node(default(T), null);
            mTail = mHead;
        }

        public bool IsEmpty
        {
            get { return mHead.Next == null; }
        }

        public void Clear()
        {
            mHead.Next = null;
            mHead = null;
            if (mTail != null)
                mTail.Next = null;
            mTail = null;
        }

        public void Enqueue(T item)
        {
            Node newNode = new Node(item, null);
            while (true)
            {
                Node curTail = mTail;
                Node residue = curTail.Next;
                // 判断mTail是否被其他线程修改过
                if (curTail == mTail)
                {
                    // 如果被修改过,mTail应该指向新的节点
                    if (residue == null)
                    {
                        // 被修改过,需要重新去tail节点
                        if (Interlocked.CompareExchange<Node>(ref curTail.Next, newNode, residue) == residue)
                        {
                            // 尝试修改Tail
                            Interlocked.CompareExchange<Node>(ref mTail, newNode, curTail);
                            return;
                        }
                    }
                    else
                    {
                        // 帮助其他线程完成操作
                        Interlocked.CompareExchange<Node>(ref mTail, residue, curTail);
                    }
                }
            }
        }

        public bool TryDequeue(out T item)
        {
            Node curHead;
            Node curTail;
            Node next;

            do
            {
                curHead = mHead;
                curTail = mTail;
                next = mHead.Next;
                if (curHead == mHead)
                {
                    if (next == null)
                    {
                        item = default(T);
                        return false;
                    }
                    if (curHead == curTail)
                    {
                        Interlocked.CompareExchange<Node>(ref mTail, next, curTail);
                    }
                    else
                    {
                        item = next.Item;
                        if (Interlocked.CompareExchange<Node>(ref mHead, next, curHead) == curHead)
                        {
                            break;
                        }
                    }
                }
            } while (true);
            return true;
        }
    }
}
