// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// Thread tracks managed thread IDs, recycling them when threads die to keep the set of
// live IDs compact.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using Internal.Runtime.Augments;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerServices;

namespace System.Threading
{
    public struct TwoByteStr
    {
        public byte first;
        public byte second;
    }

    internal class ManagedThreadId
    {
        [DllImport("*")]
        private static unsafe extern int printf(byte* str, byte* unused);
        private static unsafe void PrintString(string s)
        {
            int length = s.Length;
            fixed (char* curChar = s)
            {
                for (int i = 0; i < length; i++)
                {
                    TwoByteStr curCharStr = new TwoByteStr();
                    curCharStr.first = (byte)(*(curChar + i));
                    printf((byte*)&curCharStr, null);
                }
            }
        }

        internal static void PrintLine(string s)
        {
            PrintString(s);
            PrintString("\n");
        }

        //
        // Binary tree used to keep track of active thread ids. Each node of the tree keeps track of 32 consecutive ids.
        // Implemented as immutable collection to avoid locks. Each modification creates a new top level node.
        // 
        private class ImmutableIdDispenser
        {
            private readonly ImmutableIdDispenser _left; // Child nodes
            private readonly ImmutableIdDispenser _right;

            private readonly int _used; // Number of ids tracked by this node and all its childs
            internal readonly int _size; // Maximum number of ids that can be tracked by this node and all its childs

            private readonly uint _bitmap; // Bitmap of ids tracked by this node

            private const int BitsPerNode = 32;

            private ImmutableIdDispenser(ImmutableIdDispenser left, ImmutableIdDispenser right, int used, int size, uint bitmap)
            {
                _left = left;
                _right = right;
                _used = used;
                PrintLine("size set to");
                _size = size;
                PrintLine(_size.ToString());
                _bitmap = bitmap;

                CheckInvariants();
//                if (_size == 0)
//                {
//                    PrintLine("_size at end of ctor is 0");
//                }
//                else
//                {
//                    PrintLine("_size at end of ctor is not  0");
//                }
            }

            [Conditional("DEBUG")]
            private void CheckInvariants()
            {
                int actualUsed = 0;

                uint countBits = _bitmap;
                while (countBits != 0)
                {
                    actualUsed += (int)(countBits & 1);
                    countBits >>= 1;
                }

                if (_left != null)
                {
                    Debug.Assert(_left._size == ChildSize);
                    actualUsed += _left._used;
                }
                if (_right != null)
                {
                    Debug.Assert(_right._size == ChildSize);
                    actualUsed += _right._used;
                }

                Debug.Assert(actualUsed == _used);
                Debug.Assert(_used <= _size);
            }

            private int ChildSize
            {
                get
                {
                    Debug.Assert((_size / 2) >= (BitsPerNode / 2));
                    return (_size / 2) - (BitsPerNode / 2);
                }
            }

            public static ImmutableIdDispenser Empty
            {
                get
                {
                    // The empty dispenser has the id=0 allocated, so it is not really empty.
                    // It saves us from dealing with the corner case of true empty dispenser,
                    // and it ensures that IdNone will not be ever given out.
                    return new ImmutableIdDispenser(null, null, 1, BitsPerNode, 1);
                }
            }

            public ImmutableIdDispenser AllocateId(out int id)
            {
//                PrintLine("_size is:");
//
//                if (_size == 0)
//                {
//                    PrintLine("_size is 0");
//                }
//                else
//                {
//                    PrintLine("_size not 0:");
//                    PrintLine(_size.ToString());
//                }

                if (_used == _size)
                {
                    id = _size;
//                    PrintLine("id is _size:");
//                    if (id == 0)
//                    {
//                        PrintLine("id is 0");
//                    }
//                    else
//                    {
//                        PrintLine("id not 0:");
//                        PrintLine(id.ToString());
//                    }
                    var x = new ImmutableIdDispenser(this, null, _size + 1, checked(2 * _size + BitsPerNode), 1);
                    PrintLine("id after ctor ImmutableIdDispenser");
//                    PrintLine(id.ToString());
                    return x;
                }

                var bitmap = _bitmap;
                var left = _left;
                var right = _right;

                // Any free bits in current node?
                if (bitmap != uint.MaxValue)
                {
                    int bit = 0;
                    while ((bitmap & (uint)(1 << bit)) != 0)
                        bit++;
                    bitmap |= (uint)(1 << bit);
                    id = ChildSize + bit;
                    PrintLine("id is ChildSize + bit");

                }
                else
                {
                    Debug.Assert(ChildSize > 0);
                    if (left == null)
                    {
                        left = new ImmutableIdDispenser(null, null, 1, ChildSize, 1);
                        id = left.ChildSize;
                        PrintLine("id is left.ChildSize");

                    }
                    else
                    if (right == null)
                    {
                        right = new ImmutableIdDispenser(null, null, 1, ChildSize, 1);
                        id = ChildSize + BitsPerNode + right.ChildSize;
                        PrintLine("id is ChildSize + BitsPerNode + right.ChildSize");
                    }
                    else
                    {
                        if (left._used < right._used)
                        {
                            Debug.Assert(left._used < left._size);
                            left = left.AllocateId(out id);
                            PrintLine("id is left.AllocateId");
                        }
                        else
                        {
                            Debug.Assert(right._used < right._size);
                            PrintLine("id is right.AllocateId");

                            right = right.AllocateId(out id);
                            id += (ChildSize + BitsPerNode);
                        }
                    }
                }
//                PrintLine("id is ");
//                PrintLine(id.ToString());

                return new ImmutableIdDispenser(left, right, _used + 1, _size, bitmap);
            }

            public ImmutableIdDispenser RecycleId(int id)
            {
                Debug.Assert(id < _size);

                if (_used == 1)
                    return null;

                var bitmap = _bitmap;
                var left = _left;
                var right = _right;

                int childSize = ChildSize;
                if (id < childSize)
                {
                    left = left.RecycleId(id);
                }
                else
                {
                    id -= childSize;
                    if (id < BitsPerNode)
                    {
                        Debug.Assert((bitmap & (uint)(1 << id)) != 0);
                        bitmap &= ~(uint)(1 << id);
                    }
                    else
                    {
                        right = right.RecycleId(id - BitsPerNode);
                    }
                }
                return new ImmutableIdDispenser(left, right, _used - 1, _size, bitmap);
            }
        }

        public const int IdNone = 0;

        // The main thread takes the first available id, which is 1. This id will not be recycled until the process exit.
        // We use this id to detect the main thread and report it as a foreground one.
        public const int IdMainThread = 1;

        // We store ManagedThreadId both here and in the Thread.CurrentThread object. We store it here,
        // because we may need the id very early in the process lifetime (e.g., in ClassConstructorRunner),
        // when a Thread object cannot be created yet. We also store it in the Thread.CurrentThread object,
        // because that object may have longer lifetime than the OS thread.
        [ThreadStatic]
        private static ManagedThreadId t_currentThreadId;
        [ThreadStatic]
        private static int t_currentManagedThreadId;

        // We have to avoid the static constructors on the ManagedThreadId class, otherwise we can run into stack overflow as first time Current property get called, 
        // the runtime will ensure running the static constructor and this process will call the Current property again (when taking any lock) 
        //      System::Environment.get_CurrentManagedThreadId
        //      System::Threading::Lock.Acquire
        //      System::Runtime::CompilerServices::ClassConstructorRunner::Cctor.GetCctor
        //      System::Runtime::CompilerServices::ClassConstructorRunner.EnsureClassConstructorRun
        //      System::Threading::ManagedThreadId.get_Current
        //      System::Environment.get_CurrentManagedThreadId

        private static ImmutableIdDispenser s_idDispenser;

        private int _managedThreadId;

        public int Id => _managedThreadId;

        public static int AllocateId()
        {
            var e = ImmutableIdDispenser.Empty;
//            PrintLine("empty size");
            var si = e._size;
//            if (si == 0)
//            {
//                PrintLine("empty size is 0");
//            }
//            else
//            {
//                PrintLine("empty size is not 0");
//                PrintLine(si.ToString());
//            }

            if (s_idDispenser == null)
            {
                PrintLine("s_idDispenser is null, calling CompareExchange with 3rd param == null    ");
                var o = Unsafe.As<ImmutableIdDispenser, object>(ref e);
                var e2 = Unsafe.As<object, ImmutableIdDispenser>(ref o);
                if (e2._size == 0)
                {
                    PrintLine("e2 _size is 0");
                }
                PrintLine("s_idDispenser is null, calling CompareExchange with 3rd param == null    ");

                Interlocked.CompareExchange(ref s_idDispenser, e, null);
            }

            si = s_idDispenser._size;
            if (si == 0)
            {
                PrintLine("s_idDispenser size is 0");
            }
            else
            {
                PrintLine("s_idDispenser size is not 0");
//                PrintLine(si.ToString());
            }
            int id;

            var priorIdDispenser = Volatile.Read(ref s_idDispenser);
            for (;;)
            {
                var updatedIdDispenser = priorIdDispenser.AllocateId(out id);
                PrintLine("id after AllocateId");
//                PrintLine(id.ToString());
                var interlockedResult = Interlocked.CompareExchange(ref s_idDispenser, updatedIdDispenser, priorIdDispenser);
                if (object.ReferenceEquals(priorIdDispenser, interlockedResult))
                    break;
                priorIdDispenser = interlockedResult; // we already have a volatile read that we can reuse for the next loop
            }

            Debug.Assert(id != IdNone);

            return id;
        }

        public static void RecycleId(int id)
        {
            if (id == IdNone)
            {
                return;
            }

            var priorIdDispenser = Volatile.Read(ref s_idDispenser);
            for (;;)
            {
                var updatedIdDispenser = s_idDispenser.RecycleId(id);
                var interlockedResult = Interlocked.CompareExchange(ref s_idDispenser, updatedIdDispenser, priorIdDispenser);
                if (object.ReferenceEquals(priorIdDispenser, interlockedResult))
                    break;
                priorIdDispenser = interlockedResult; // we already have a volatile read that we can reuse for the next loop
            }
        }

        public static int Current
        {
            get
            {
                int currentManagedThreadId = t_currentManagedThreadId;
                if (currentManagedThreadId == IdNone)
                    return MakeForCurrentThread();
                else
                    return currentManagedThreadId;
            }
        }

        public static ManagedThreadId GetCurrentThreadId()
        {
            if (t_currentManagedThreadId == IdNone)
                MakeForCurrentThread();

            return t_currentThreadId;
        }

        private static int MakeForCurrentThread()
        {
            return SetForCurrentThread(new ManagedThreadId());
        }

        public static int SetForCurrentThread(ManagedThreadId threadId)
        {
            t_currentThreadId = threadId;
            t_currentManagedThreadId = threadId.Id;
            return threadId.Id;
        }

        public ManagedThreadId()
        {
            _managedThreadId = AllocateId();
        }

        ~ManagedThreadId()
        {
            RecycleId(_managedThreadId);
        }
    }
}
