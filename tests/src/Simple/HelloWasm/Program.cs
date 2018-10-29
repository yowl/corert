// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Globalization;
#if PLATFORM_WINDOWS
using CpObj;
#endif

public static class Program
{
//    private static int staticInt;
//    [ThreadStatic]
//    private static int threadStaticInt;

//    [DllImport("*")]
//    private static unsafe extern int setenv(byte* str, byte* unused, int overwrite);

    private static unsafe int Main(string[] args)
    {
        // mono uses ninja.WriteLine ("emcc_flags = -Os -g -s DISABLE_EXCEPTION_CATCHING=0 -s ASSERTIONS=1 -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s \"BINARYEN_TRAP_MODE=\'clamp\'\" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s NO_EXIT_RUNTIME=1 -s ERROR_ON_UNDEFINED_SYMBOLS=1 -s \"EXTRA_EXPORTED_RUNTIME_METHODS=[\'ccall\', \'cwrap\', \'setValue\', \'getValue\', \'UTF8ToString\']\" -s \"EXPORTED_FUNCTIONS=[\'___cxa_is_pointer_type\', \'___cxa_can_catch\']\"");
//        CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
//        CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator = ".";
        PrintLine("Starting");
        double z = 1.057;
        var b = (byte)Math.Min(z * 255, 255);

        PrintLine(b.ToString());
//        var x = 0;
        byte[] res = RayTraceBenchmark.BenchmarkMain.Start();
        byte[] rgba = RayTraceBenchmark.BenchmarkMain.ConvertRGBToBGRA(res);
        //        SetEnv("ARRAYPTR","17");
        //        for (var i = 0; i < rgba.Length; i++)
        //        {
        //            PrintLine(rgba[i].ToString());
        //            x++;
        //        }


        int heapPtr = 100;
        fixed (byte* arrayPtr = rgba)
        {
            heapPtr = (int)arrayPtr;
        }
        PrintLine("Done");
        return heapPtr;
    }


    private static int StaticDelegateTarget()
    {         
        return 7;
    }

    public static unsafe void PrintString(string s)
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

    public static unsafe void SetEnv(string name, string value)
    {
        int length = name.Length;
        var nameStr = new byte[length + 1];
        fixed (char* curChar = name)
        {
            for (int i = 0; i < length; i++)
            {
                nameStr[i] = (byte)(*(curChar + i));
            }
            nameStr[length] = 0;
        }
        length = value.Length;
        var valueStr = new byte[length + 1];
        fixed (char* curChar = value)
        {
            for (int i = 0; i < length; i++)
            {
                valueStr[i] = (byte)(*(curChar + i));
            }
            valueStr[length] = 0;
        }
//        fixed (byte* bytePtr = valueStr)
//        fixed (byte* namePtr = nameStr)
//        {
//            setenv(namePtr, bytePtr, 1);
//        }
    }

    public static void PrintLine(string s)
    {
        PrintString(s);
        PrintString("\n");
    }

    private static int Add(int a, int b)
    {
        return a + b;
    }

    private static uint Not(uint a)
    {
        return ~a;
    }

    private static int Neg(int a)
    {
        return -a;
    }

    private static int ShiftLeft(int a, int b)
    {
        return a << b;
    }

    private static int ShiftRight(int a, int b)
    {
        return a >> b;
    }

    private static uint UnsignedShift(uint a, int b)
    {
        return a >> b;
    }
    
    private static int SwitchOp(int a, int b, int mode)
    {
        switch(mode)
        {
          case 0:
            return a + b;
          case 1:
            return a * b;
          case 2:
            return a / b;
          case 3:
            return a - b;
          default:
            return 0;
        }
    }

    private static IntPtr NewobjValueType()
    {
        return new IntPtr(3);
    }

    private unsafe static void StackallocTest()
    {
        int* intSpan = stackalloc int[2];
        intSpan[0] = 3;
        intSpan[1] = 7;

        if (intSpan[0] == 3 && intSpan[1] == 7)
        {
            PrintLine("Stackalloc test: Ok.");
        }
    }

    private static void IntToStringTest()
    {
        PrintLine("Int to String Test: Ok if next line says 42.");
        string intString = 42.ToString();
        PrintLine(intString);
    }

    private unsafe static void ldindTest()
    {
        var ldindTarget = new TwoByteStr { first = byte.MaxValue, second = byte.MinValue };
        var ldindField = &ldindTarget.first;
        if((*ldindField) == byte.MaxValue)
        {
            ldindTarget.second = byte.MaxValue;
            *ldindField = byte.MinValue;
            //ensure there isnt any overwrite of nearby fields
            if(ldindTarget.first == byte.MinValue && ldindTarget.second == byte.MaxValue)
            {
                PrintLine("ldind test: Ok.");
            }
            else if(ldindTarget.first != byte.MinValue)
            {
                PrintLine("ldind test: Failed didnt update target.");
            }
            else
            {
                PrintLine("ldind test: Failed overwrote data");
            }
        }
        else
        {
            uint ldindFieldValue = *ldindField;
            PrintLine("ldind test: Failed." + ldindFieldValue.ToString());
        }
    }

    private static void InterfaceDispatchTest()
    {
        ItfStruct itfStruct = new ItfStruct();
        if (ItfCaller(itfStruct) == 4)
        {
            PrintLine("Struct interface test: Ok.");
        }

        ClassWithSealedVTable classWithSealedVTable = new ClassWithSealedVTable();
        PrintString("Interface dispatch with sealed vtable test: ");
        if (CallItf(classWithSealedVTable) == 37)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }
    }

    // Calls the ITestItf interface via a generic to ensure the concrete type is known and
    // an interface call is generated instead of a virtual or direct call
    private static int ItfCaller<T>(T obj) where T : ITestItf
    {
        return obj.GetValue();
    }

    private static int CallItf(ISomeItf asItf)
    {
        return asItf.GetValue();
    }

    private static void StaticCtorTest()
    {
        BeforeFieldInitTest.Nop();
        if (StaticsInited.BeforeFieldInitInited)
        {
            PrintLine("BeforeFieldInitType inited too early");
        }
        else
        {
            int x = BeforeFieldInitTest.TestField;
            if (StaticsInited.BeforeFieldInitInited)
            {
                PrintLine("BeforeFieldInit test: Ok.");
            }
            else
            {
                PrintLine("BeforeFieldInit cctor not run");
            }
        }

        NonBeforeFieldInitTest.Nop();
        if (StaticsInited.NonBeforeFieldInitInited)
        {
            PrintLine("NonBeforeFieldInit test: Ok.");
        }
        else
        { 
            PrintLine("NonBeforeFieldInitType cctor not run");
        }
    }

    private static void TestConstrainedClassCalls()
    {
        string s = "utf-8";

        PrintString("Direct ToString test: ");
        string stringDirectToString = s.ToString();
        if (s.Equals(stringDirectToString))
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintString("Failed. Returned string:\"");
            PrintString(stringDirectToString);
            PrintLine("\"");
        }
       
        // Generic calls on methods not defined on object
        uint dataFromBase = GenericGetData<MyBase>(new MyBase(11));
        PrintString("Generic call to base class test: ");
        if (dataFromBase == 11)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }

        uint dataFromUnsealed = GenericGetData<UnsealedDerived>(new UnsealedDerived(13));
        PrintString("Generic call to unsealed derived class test: ");
        if (dataFromUnsealed == 26)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }

        uint dataFromSealed = GenericGetData<SealedDerived>(new SealedDerived(15));
        PrintString("Generic call to sealed derived class test: ");
        if (dataFromSealed == 45)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }

        uint dataFromUnsealedAsBase = GenericGetData<MyBase>(new UnsealedDerived(17));
        PrintString("Generic call to unsealed derived class as base test: ");
        if (dataFromUnsealedAsBase == 34)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }

        uint dataFromSealedAsBase = GenericGetData<MyBase>(new SealedDerived(19));
        PrintString("Generic call to sealed derived class as base test: ");
        if (dataFromSealedAsBase == 57)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }

        // Generic calls to methods defined on object
        uint hashCodeOfSealedViaGeneric = (uint)GenericGetHashCode<MySealedClass>(new MySealedClass(37));
        PrintString("Generic GetHashCode for sealed class test: ");
        if (hashCodeOfSealedViaGeneric == 74)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }

        uint hashCodeOfUnsealedViaGeneric = (uint)GenericGetHashCode<MyUnsealedClass>(new MyUnsealedClass(41));
        PrintString("Generic GetHashCode for unsealed class test: ");
        if (hashCodeOfUnsealedViaGeneric == 82)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }
    }

    static uint GenericGetData<T>(T obj) where T : MyBase
    {
        return obj.GetData();
    }

    static int GenericGetHashCode<T>(T obj)
    {
        return obj.GetHashCode();
    }

    private static void TestArrayItfDispatch()
    {
        ICollection<int> arrayItfDispatchTest = new int[37];
        PrintString("Array interface dispatch test: ");
        if (arrayItfDispatchTest.Count == 37)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.  asm.js (WASM=1) known to fail due to alignment problem, although this problem sometimes means we don't even get this far and fails with an invalid function pointer.");
        }
    }

    private static void TestValueTypeElementIndexing()
    {
        var chars = new[] { 'i', 'p', 's', 'u', 'm' };
        PrintString("Value type element indexing: ");
        if (chars[0] == 'i' && chars[1] == 'p' && chars[2] == 's' && chars[3] == 'u' && chars[4] == 'm')
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }
    }

    private static void floatDoubleTest()
    {
        int intToCast = 1;
        double castedDouble = (double)intToCast;
        if (castedDouble == 1d)
        {
            PrintLine("(double) cast test: Ok.");
        }
        else
        {
            var toInt = (int)castedDouble;
            //            PrintLine("expected 1m, but was " + castedDouble.ToString());  // double.ToString is not compiling at the time of writing, but this would be better output
            PrintLine($"(double) cast test : Failed. Back to int on next line");
            PrintLine(toInt.ToString());
        }

        if (1f < 2d && 1d < 2f && 1f == 1d)
        {
            PrintLine("different width float comparisons: Ok.");
        }

        // floats are 7 digits precision, so check some double more precise to make sure there is no loss occurring through some inadvertent cast to float
        if (10.23456789d != 10.234567891d)
        {
            PrintLine("double precision comparison: Ok.");
        }

        if (12.34567f == 12.34567f && 12.34567f != 12.34568f)
        {
            PrintLine("float comparison: Ok.");
        }

        PrintString("Test comparison of float constant: ");
        var maxFloat = Single.MaxValue;
        if (maxFloat == Single.MaxValue)
        {
            PrintLine("Ok.");
        }
        else
        {
            PrintLine("Failed.");
        }
    }


    [DllImport("*")]
    private static unsafe extern int printf(byte* str, byte* unused);
}

public struct TwoByteStr
{
    public byte first;
    public byte second;
}

public struct BoxStubTest
{
    public string Value;
    public override string ToString()
    {
        return Value;
    }

    public string GetValue()
    {
        Program.PrintLine("BoxStubTest.GetValue called");
        Program.PrintLine(Value);
        return Value;
    }
}

public class TestClass
{
    public string TestString { get; set; }
    public int TestInt { get; set; }

    public TestClass(int number)
    {
        if(number != 1337)
            throw new Exception();
    }

    public void TestMethod(string str)
    {
        TestString = str;
        if (TestString == str)
            Program.PrintLine("Instance method call test: Ok.");
    }
    public virtual void TestVirtualMethod(string str)
    {
        Program.PrintLine("Virtual Slot Test: Ok If second");
    }
	
	public virtual void TestVirtualMethod2(string str)
    {
        Program.PrintLine("Virtual Slot Test 2: Ok");
    }

    public int InstanceDelegateTarget()
    {
        return TestInt;
    }

    public virtual void VirtualDelegateTarget()
    {
        Program.PrintLine("Virtual delegate incorrectly dispatched to base.");
    }
}

public class TestDerivedClass : TestClass
{
    public TestDerivedClass(int number) : base(number)
    {

    }
    public override void TestVirtualMethod(string str)
    {
        Program.PrintLine("Virtual Slot Test: Ok");
        base.TestVirtualMethod(str);
    }
    
    public override string ToString()
    {
        throw new Exception();
    }

    public override void VirtualDelegateTarget()
    {
        Program.PrintLine("Virtual Delegate Test: Ok");
    }
}

public class StaticsInited
{
    public static bool BeforeFieldInitInited;
    public static bool NonBeforeFieldInitInited;
}

public class BeforeFieldInitTest
{
    public static int TestField = BeforeFieldInit();

    public static void Nop() { }

    static int BeforeFieldInit()
    {
        StaticsInited.BeforeFieldInitInited = true;
        return 3;
    }
}

public class NonBeforeFieldInitTest
{
    public static int TestField;

    public static void Nop() { }

    static NonBeforeFieldInitTest()
    {
        TestField = 4;
        StaticsInited.NonBeforeFieldInitInited = true;
    }
}

public interface ICastingTest1
{
    int GetValue();
}

public interface ICastingTest2
{
    int GetValue();
}

public abstract class CastingTestClass
{
    public abstract int GetValue();
}

public class DerivedCastingTestClass1 : CastingTestClass, ICastingTest1
{
    public override int GetValue() => 1;
}

public class DerivedCastingTestClass2 : CastingTestClass, ICastingTest2
{
    public override int GetValue() => 2;
}

public interface ITestItf
{
    int GetValue();
}

public struct ItfStruct : ITestItf
{
    public int GetValue()
    {
        return 4;
    }
}

public sealed class MySealedClass
{
    uint _data;

    public MySealedClass()
    {
        _data = 104;
    }

    public MySealedClass(uint data)
    {
        _data = data;
    }

    public uint GetData()
    {
        return _data;
    }

    public override int GetHashCode()
    {
        return (int)_data * 2;
    }

    public override string ToString()
    {
        Program.PrintLine("MySealedClass.ToString called. Data:");
        Program.PrintLine(_data.ToString());
        return _data.ToString();
    }
}

public class MyUnsealedClass
{
    uint _data;

    public MyUnsealedClass()
    {
        _data = 24;
    }

    public MyUnsealedClass(uint data)
    {
        _data = data;
    }

    public uint GetData()
    {
        return _data;
    }

    public override int GetHashCode()
    {
        return (int)_data * 2;
    }

    public override string ToString()
    {
        return _data.ToString();
    }
}

public class MyBase
{
    protected uint _data;
    public MyBase(uint data)
    {
        _data = data;
    }

    public virtual uint GetData()
    {
        return _data;
    }
}

public class UnsealedDerived : MyBase
{
    public UnsealedDerived(uint data) : base(data) { }
    public override uint GetData()
    {
        return _data * 2;
    }
}

public sealed class SealedDerived : MyBase
{
    public SealedDerived(uint data) : base(data) { }
    public override uint GetData()
    {
        return _data * 3;
    }
}

class ClassWithSealedVTable : ISomeItf
{
    public int GetValue()
    {
        return 37;
    }
}

interface ISomeItf
{
    int GetValue();
}
