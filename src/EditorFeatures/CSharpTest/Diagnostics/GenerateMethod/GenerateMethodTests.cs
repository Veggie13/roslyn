﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.CodeFixes.GenerateMethod;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics.GenerateMethod
{
    public class GenerateMethodTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        internal override Tuple<DiagnosticAnalyzer, CodeFixProvider> CreateDiagnosticProviderAndFixer(Workspace workspace)
        {
            return new Tuple<DiagnosticAnalyzer, CodeFixProvider>(null, new GenerateMethodCodeFixProvider());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimpleInvocationIntoSameType()
        {
            Test(
@"class Class { void Method() { [|Foo|](); } }",
@"using System; class Class { void Method() { Foo(); } private void Foo() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimpleInvocationOffOfThis()
        {
            Test(
@"class Class { void Method() { this.[|Foo|](); } }",
@"using System; class Class { void Method() { this.Foo(); } private void Foo() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimpleInvocationOffOfType()
        {
            Test(
@"class Class { void Method() { Class.[|Foo|](); } }",
@"using System; class Class { void Method() { Class.Foo(); } private static void Foo() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimpleInvocationValueExpressionArg()
        {
            Test(
@"class Class { void Method() { [|Foo|](0); } }",
@"using System; class Class { void Method() { Foo(0); } private void Foo(int v) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimpleInvocationMultipleValueExpressionArg()
        {
            Test(
@"class Class { void Method() { [|Foo|](0, 0); } }",
@"using System; class Class { void Method() { Foo(0, 0); } private void Foo(int v1, int v2) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimpleInvocationValueArg()
        {
            Test(
@"class Class { void Method(int i) { [|Foo|](i); } }",
@"using System; class Class { void Method(int i) { Foo(i); } private void Foo(int i) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimpleInvocationNamedValueArg()
        {
            Test(
@"class Class { void Method(int i) { [|Foo|](bar: i); } }",
@"using System; class Class { void Method(int i) { Foo(bar: i); } private void Foo(int bar) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateAfterMethod()
        {
            Test(
@"class Class { void Method() { [|Foo|](); } void NextMethod() { } }",
@"using System; class Class { void Method() { Foo(); } private void Foo() { throw new NotImplementedException(); } void NextMethod() { } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInterfaceNaming()
        {
            Test(
@"class Class { void Method(int i) { [|Foo|](NextMethod()); } IFoo NextMethod() { } }",
@"using System; class Class { void Method(int i) { Foo(NextMethod()); } private void Foo(IFoo foo) { throw new NotImplementedException(); } IFoo NextMethod() { } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestFuncArg0()
        {
            Test(
@"class Class { void Method(int i) { [|Foo|](NextMethod); } string NextMethod() { } }",
@"using System; class Class { void Method(int i) { Foo(NextMethod); } private void Foo(Func<string> nextMethod) { throw new NotImplementedException(); } string NextMethod() { } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestFuncArg1()
        {
            Test(
@"class Class { void Method(int i) { [|Foo|](NextMethod); } string NextMethod(int i) { } }",
@"using System; class Class { void Method(int i) { Foo(NextMethod); } private void Foo(Func<int,string> nextMethod) { throw new NotImplementedException(); } string NextMethod(int i) { } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestActionArg()
        {
            Test(
@"class Class { void Method(int i) { [|Foo|](NextMethod); } void NextMethod() { } }",
@"using System; class Class { void Method(int i) { Foo(NextMethod); } private void Foo(Action nextMethod) { throw new NotImplementedException(); } void NextMethod() { } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestActionArg1()
        {
            Test(
@"class Class { void Method(int i) { [|Foo|](NextMethod); } void NextMethod(int i) { } }",
@"using System; class Class { void Method(int i) { Foo(NextMethod); } private void Foo(Action<int> nextMethod) { throw new NotImplementedException(); } void NextMethod(int i) { } }");
        }

        // Note: we only test type inference once.  This is just to verify that it's being used
        // properly by Generate Method.  The full wealth of type inference tests can be found
        // elsewhere and don't need to be repeated here.
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestTypeInference()
        {
            Test(
@"class Class { void Method() { if ([|Foo|]()) { } } }",
@"using System; class Class { void Method() { if (Foo()) { } } private bool Foo() { throw new NotImplementedException(); } }");
        }

        [WorkItem(784793)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestOutRefArguments()
        {
            Test(
@"class Class { void Method() { [|Foo|](out a, ref b); } }",
@"using System; class Class { void Method() { Foo(out a, ref b); } private void Foo(out object a, ref object b) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMemberAccessArgumentName()
        {
            Test(
@"class Class { void Method() { [|Foo|](this.Bar); } }",
@"using System; class Class { void Method() { Foo(this.Bar); } private void Foo(object bar) { throw new NotImplementedException(); } }");
        }

        [WorkItem(784793)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestParenthesizedArgumentName()
        {
            Test(
@"class Class { void Method() { [|Foo|]((Bar)); } }",
@"using System; class Class { void Method() { Foo((Bar)); } private void Foo(object bar) { throw new NotImplementedException(); } }");
        }

        [WorkItem(784793)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestCastedArgumentName()
        {
            Test(
@"class Class { void Method() { [|Foo|]((Bar)this.Baz); } }",
@"using System; class Class { void Method() { Foo((Bar)this.Baz); } private void Foo(Bar baz) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestNullableArgument()
        {
            Test(
@"class C { void Method() { [|Foo|]((int?)1); } }",
@"using System; class C { void Method() { Foo((int?)1); } private void Foo(int? v) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestNullArgument()
        {
            Test(
@"class C { void Method() { [|Foo|](null); } }",
@"using System; class C { void Method() { Foo(null); } private void Foo(object p) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestTypeofArgument()
        {
            Test(
@"class C { void Method() { [|Foo|](typeof(int)); } }",
@"using System; class C { void Method() { Foo(typeof(int)); } private void Foo(Type type) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDefaultArgument()
        {
            Test(
@"class C { void Method() { [|Foo|](default(int)); } }",
@"using System; class C { void Method() { Foo(default(int)); } private void Foo(int v) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestAsArgument()
        {
            Test(
@"class C { void Method() { [|Foo|](1 as int?); } }",
@"using System; class C { void Method() { Foo(1 as int?); } private void Foo(int? v) { throw new NotImplementedException(); } }");
        }

        [Fact(Skip = "530177"), Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestPointArgument()
        {
            Test(
@"class C { void Method() { int* p; [|Foo|](p); } }",
@"using System; class C { void Method() { int* p; Foo(p); } private unsafe void Foo(int* p) { throw new NotImplementedException(); } }");
        }

        [Fact(Skip = "530177"), Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestArgumentWithPointerName()
        {
            Test(
@"class C { void Method() { int* p; [|Foo|](p); } }",
@"using System; class C { void Method() { int* p; Foo(p); } private unsafe void Foo(int* p) { throw new NotImplementedException(); } }");
        }

        [Fact(Skip = "530177"), Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestArgumentWithPointTo()
        {
            Test(
@"class C { void Method() { int* p; [|Foo|](*p); } }",
@"using System; class C { void Method() { int* p; Foo(*p); } private void Foo(int p) { throw new NotImplementedException(); } }");
        }

        [Fact(Skip = "530177"), Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestArgumentWithAddress()
        {
            Test(
@"class C { unsafe void Method() { int a = 10; [|Foo|](&a); } }",
@"using System; class C { unsafe void Method() { int a = 10; Foo(&a); } private unsafe void Foo(int* p) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateWithPointerReturn()
        {
            Test(
@"class C { void Method() { int* p = [|Foo|](); } }",
@"using System; class C { void Method() { int* p = Foo(); } private unsafe int* Foo() { throw new NotImplementedException(); } }");
        }

        [WorkItem(784793)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDuplicateNames()
        {
            Test(
@"class Class { void Method() { [|Foo|]((Bar)this.Baz, this.Baz); } }",
@"using System; class Class { void Method() { Foo((Bar)this.Baz, this.Baz); } private void Foo(Bar baz1, object baz2) { throw new NotImplementedException(); } }");
        }

        [WorkItem(784793)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDuplicateNamesWithNamedArgument()
        {
            Test(
@"class Class { void Method() { [|Foo|]((Bar)this.Baz, this.Baz, baz: this.Baz); } }",
@"using System; class Class { void Method() { Foo((Bar)this.Baz, this.Baz, baz: this.Baz); } private void Foo(Bar baz1, object baz2, object baz) { throw new NotImplementedException(); } }");
        }

        // Note: we do not test the range of places where a delegate type can be inferred.  This is
        // just to verify that it's being used properly by Generate Method.  The full wealth of
        // delegate inference tests can be found elsewhere and don't need to be repeated here.
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimpleDelegate()
        {
            Test(
@"using System; class Class { void Method() { Func<int,string,bool> f = [|Foo|]; } }",
@"using System; class Class { void Method() { Func<int,string,bool> f = Foo; } private bool Foo(int arg1, string arg2) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDelegateWithRefParameter()
        {
            Test(
@"class Class { void Method() { Foo f = [|Bar|]; } } delegate void Foo(ref int i);",
@"using System; class Class { void Method() { Foo f = Bar; } private void Bar(ref int i) { throw new NotImplementedException(); } } delegate void Foo(ref int i);");
        }

        // TODO(cyrusn): Add delegate tests that cover delegates with interesting signatures (i.e.
        // out/ref).
        //
        // add negative tests to verify that Generate Method doesn't show up in unexpected places.

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenericArgs1()
        {
            Test(
@"using System; class Class { void Method() { [|Foo<int>|](); } }",
@"using System; class Class { void Method() { Foo<int>(); } private void Foo<T>() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenericArgs2()
        {
            Test(
@"using System; class Class { void Method() { [|Foo<int,string>|](); } }",
@"using System; class Class { void Method() { Foo<int,string>(); } private void Foo<T1,T2>() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenericArgsFromMethod()
        {
            Test(
@"using System; class Class { void Method<X,Y>(X x, Y y) { [|Foo|](x); } }",
@"using System; class Class { void Method<X,Y>(X x, Y y) { Foo(x); } private void Foo<X>(X x) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMultipleGenericArgsFromMethod()
        {
            Test(
@"using System; class Class { void Method<X,Y>(X x, Y y) { [|Foo|](x, y); } }",
@"using System; class Class { void Method<X,Y>(X x, Y y) { Foo(x, y); } private void Foo<X, Y>(X x, Y y) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMultipleGenericArgsFromMethod2()
        {
            Test(
@"using System; class Class { void Method<X,Y>(Func<X> x, Y[] y) { [|Foo|](y, x); } }",
@"using System; class Class { void Method<X,Y>(Func<X> x, Y[] y) { Foo(y, x); } private void Foo<Y, X>(Y[] y, Func<X> x) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenericArgThatIsTypeParameter()
        {
            Test(
@"class Program { void Main < T > ( T t ) { [|Foo < T >|] ( t ) ; } } ",
@"using System; class Program { void Main < T > ( T t ) { Foo < T > ( t ) ; } private void Foo < T > ( T t ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMultipleGenericArgsThatAreTypeParameters()
        {
            Test(
@"class Program { void Main < T , U > ( T t , U u ) { [|Foo < T , U >|] ( t , u ) ; } } ",
@"using System; class Program { void Main < T , U > ( T t , U u ) { Foo < T , U > ( t , u ) ; } private void Foo < T , U > ( T t , U u ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoOuterThroughInstance()
        {
            Test(
@"class Outer { class Class { void Method(Outer o) { o.[|Foo|](); } } }",
@"using System; class Outer { class Class { void Method(Outer o) { o.Foo(); } } private void Foo() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoOuterThroughClass()
        {
            Test(
@"class Outer { class Class { void Method(Outer o) { Outer.[|Foo|](); } } }",
@"using System; class Outer { class Class { void Method(Outer o) { Outer.Foo(); } } private static void Foo() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoSiblingThroughInstance()
        {
            Test(
@"class Class { void Method(Sibling s) { s.[|Foo|](); } } class Sibling { }",
@"using System; class Class { void Method(Sibling s) { s.Foo(); } } class Sibling { internal void Foo() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoSiblingThroughClass()
        {
            Test(
@"class Class { void Method(Sibling s) { Sibling.[|Foo|](); } } class Sibling { }",
@"using System; class Class { void Method(Sibling s) { Sibling.Foo(); } } class Sibling { internal static void Foo() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoInterfaceThroughInstance()
        {
            Test(
@"class Class { void Method(ISibling s) { s.[|Foo|](); } } interface ISibling { }",
@"class Class { void Method(ISibling s) { s.Foo(); } } interface ISibling { void Foo(); }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoInterfaceThroughInstanceWithDelegate()
        {
            Test(
@"using System; class Class { void Method(ISibling s) { Func<int,string> f = s.[|Foo|]; } } interface ISibling { }",
@"using System; class Class { void Method(ISibling s) { Func<int,string> f = s.Foo; } } interface ISibling { string Foo(int arg); }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateAbstractIntoSameType()
        {
            Test(
@"abstract class Class { void Method() { [|Foo|](); } }",
@"abstract class Class { void Method() { Foo(); } internal abstract void Foo(); }",
index: 1);
        }

        [WorkItem(537906)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMethodReturningDynamic()
        {
            Test(
@"class Class { void Method() { dynamic d = [|Foo|](); } }",
@"using System; class Class { void Method() { dynamic d = Foo(); } private dynamic Foo() { throw new NotImplementedException(); } }");
        }

        [WorkItem(537906)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMethodTakingDynamicArg()
        {
            Test(
@"class Class { void Method(dynamic d) { [|Foo|](d); } }",
@"using System; class Class { void Method(dynamic d) { Foo(d); } private void Foo(dynamic d) { throw new NotImplementedException(); } }");
        }

        [WorkItem(3203, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestNegativeWithNamedOptionalArg1()
        {
            TestMissing(
@"namespace SyntaxError { class C1 { public void Method(int num, string str) { } } class C2 { static void Method2() { (new C1()).[|Method|](num: 5, ""hi""); } } }");
        }

        [WorkItem(537972)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestWithNamedOptionalArg2()
        {
            Test(
@"namespace SyntaxError { class C1 { void Method(int num, string str) { } } class C2 { static void Method2() { (new C1()).[|Method|](num: 5, ""hi""); } } }",
@"using System;

namespace SyntaxError { class C1 { void Method(int num, string str) { } internal void Method(int num, string v) { throw new NotImplementedException(); } } class C2 { static void Method2() { (new C1()).Method(num: 5, ""hi""); } } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestArgOrderInNamedArgs()
        {
            Test(
@"class Foo { static void Test() { (new Foo()). [|Method|](3, 4, n1 : 5, n3 : 6, n2 : 7, n0 : 8); } }",
@"using System; class Foo { static void Test() { (new Foo()). Method(3, 4, n1 : 5, n3 : 6, n2 : 7, n0 : 8); } private void Method(int v1, int v2, int n1, int n3, int n2, int n0) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestForMissingOptionalArg()
        {
            TestMissing(
@"class Foo { static void Test ( ) { ( new Foo ( ) ) . [|Method|] ( s : ""hello"" , b : true ) ; } private void Method ( double n = 3.14 , string s , bool b ) { } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestNamingOfArgWithClashes()
        {
            Test(
@"class Foo { static int i = 32; static void Test() { (new Foo()).[|Method|](s: ""hello"", i: 52); } }",
@"using System; class Foo { static int i = 32; static void Test() { (new Foo()).Method(s: ""hello"", i: 52); } private void Method(string s, int i) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestFixCountGeneratingIntoInterface()
        {
            TestActionCount(
@"interface I2 { } class C2 : I2 { public void Meth(){ I2 i = (I2)this; i.[|M|](); } }",
count: 1);
        }

        [WorkItem(527278)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationOffOfBase()
        {
            Test(
@"class C3A { } class C3 : C3A { public void C4() { base.[|M|](); } }",
@"using System; class C3A { internal void M() { throw new NotImplementedException(); } } class C3 : C3A { public void C4() { base.M(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationWithinCtor()
        {
            Test(
@"class C1 { C1() { [|M|](); } }",
@"using System; class C1 { C1() { M(); } private void M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationWithinBaseCtor()
        {
            Test(
@"class C1 { C1() { [|M|](); } }",
@"using System; class C1 { C1() { M(); } private void M() { throw new NotImplementedException(); } }");
        }

        [WorkItem(3095, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestForMultipleSmartTagsInvokingWithinCtor()
        {
            TestMissing(
@"using System; class C1 { C1() { [|M|](); } private void M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationWithinDestructor()
        {
            Test(
@"class C1 { ~C1() { [|M|](); } }",
@"using System; class C1 { ~C1() { M(); } private void M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationWithinConditional()
        {
            Test(
@"class C4 { void A() { string s; if ((s = [|M|]()) == null) { } } }",
@"using System; class C4 { void A() { string s; if ((s = M()) == null) { } }  private string M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoStaticClass()
        {
            Test(
@"class Bar { void Test() { Foo.[|M|](); } } static class Foo { } ",
@"using System; class Bar { void Test() { Foo.M(); } } static class Foo { internal static void M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoAbstractClass()
        {
            Test(
@"class Bar { void Test() { Foo.[|M|](); } } abstract class Foo { } ",
@"using System; class Bar { void Test() { Foo.M(); } } abstract class Foo { internal static void M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoAbstractClassThoughInstance1()
        {
            Test(
@"class C { void Test(Foo f) { f.[|M|](); } } abstract class Foo { }",
@"using System; class C { void Test(Foo f) { f.M(); } } abstract class Foo { internal void M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoAbstractClassThoughInstance2()
        {
            Test(
@"class C { void Test(Foo f) { f.[|M|](); } } abstract class Foo { }",
@"class C { void Test(Foo f) { f.M(); } } abstract class Foo { internal abstract void M(); }",
index: 1);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoPartialClass1()
        {
            Test(
@"class Bar { void Test() { Foo.[|M|](); } } partial class Foo { } partial class Foo { }",
@"using System; class Bar { void Test() { Foo.M(); } } partial class Foo { internal static void M() { throw new NotImplementedException(); } } partial class Foo { }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoPartialClass2()
        {
            Test(
@"partial class Foo { void Test() { Foo.[|M|](); } } partial class Foo { }",
@"using System; partial class Foo { void Test() { Foo.M(); } private static void M() { throw new NotImplementedException(); } } partial class Foo { } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoStruct()
        {
            Test(
@"class Foo { void Test() { (new S()).[|M|](); } } struct S { }",
@"using System; class Foo { void Test() { (new S()).M(); } } struct S { internal void M() { throw new NotImplementedException(); } }");
        }

        [WorkItem(527291)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationOffOfIndexer()
        {
            Test(
@"class Bar { Foo f = new Foo(); void Test() { this[1].[|M|](); } Foo this[int i] { get { return f; } set { f = value; } } } class Foo { }",
@"using System; class Bar { Foo f = new Foo(); void Test() { this[1].M(); } Foo this[int i] { get { return f; } set { f = value; } } } 
class Foo { internal void M() { throw new NotImplementedException(); } }");
        }

        [WorkItem(527292)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationWithinForEach()
        {
            Test(
@"class C8 { C8A[] items = { new C8A(), new C8A() }; 
public IEnumerable GetItems() { for (int i = items.Length - 1; i >= 0; --i) { yield return items[i]; } } 
void Test() { foreach (C8A c8a in this.GetItems()) { c8a.[|M|](); } } } class C8A { }",
@"using System; class C8 { C8A[] items = { new C8A(), new C8A() }; 
public IEnumerable GetItems() { for (int i = items.Length - 1; i >= 0; --i) { yield return items[i]; } } 
void Test() { foreach (C8A c8a in this.GetItems()) { c8a.M(); } } } 
class C8A { internal void M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationOffOfAnotherMethodCall()
        {
            Test(
@"class C9 { C9A m_item = new C9A(); C9A GetItem() { return m_item; } void Test() { GetItem().[|M|](); } } struct C9A { }",
@"using System; class C9 { C9A m_item = new C9A(); C9A GetItem() { return m_item; } void Test() { GetItem().M(); } } struct C9A {  internal void M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationIntoNestedNamespaces()
        {
            Test(
@"namespace NS11X { namespace NS11Y { class C11 { void Test() { NS11A.NS11B.C11A.[|M|](); } } } } 
namespace NS11A { namespace NS11B { class C11A { } } }",
@"using System; namespace NS11X { namespace NS11Y { class C11 { void Test() { NS11A.NS11B.C11A.M(); } } } } namespace NS11A { namespace NS11B { class C11A { internal static void M() { throw new NotImplementedException(); } } } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationIntoAliasedNamespaces()
        {
            Test(
@"namespace NS11X { 
    using NS = NS11A.NS11B;
    class C11 {
        void Test() { NS.C11A.[|M|](); }
    }

    namespace NS11A {
        namespace NS11B {
            class C11A { }
        }
    }
}",
@"namespace NS11X { using System; using NS = NS11A.NS11B; class C11 { void Test() { NS.C11A.M(); } } 
namespace NS11A {  namespace NS11B { class C11A { internal static void M() { throw new NotImplementedException(); } } } } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInvocationOnGlobalNamespace()
        {
            Test(
@"namespace NS13X { namespace NS13A { namespace NS13B { struct S13B { } } } class C13 { void Test() { global::NS13A.NS13B.S13A.[|M|](); } } } 
namespace NS13A { namespace NS13B { struct S13A { } } }",
@"using System; namespace NS13X { namespace NS13A { namespace NS13B { struct S13B { } } } class C13 { void Test() { global::NS13A.NS13B.S13A.M(); } } } 
namespace NS13A { namespace NS13B { struct S13A { internal static void M() { throw new NotImplementedException(); } } } }");
        }

        [WorkItem(538353)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoAppropriatePart()
        {
            Test(
@"public partial class C { } public partial class C { void Method() { [|Test|](); } }",
@"using System; public partial class C { } public partial class C { void Method() { Test(); } private void Test() { throw new NotImplementedException(); } }");
        }

        [WorkItem(538541)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateWithVoidArgument()
        {
            Test(
@"class C { void VoidMethod() { } void Method() { [|Test|](VoidMethod()); } }",
@"using System; class C { void VoidMethod() { } void Method() { Test(VoidMethod()); } private void Test(object v) { throw new NotImplementedException(); } }");
        }

        [WorkItem(538993)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateInLambda()
        {
            Test(
@"using System; class Program { static void Main(string[] args) { Func<int, int> f = x => [|Foo|](x); } }",
@"using System; class Program { static void Main(string[] args) { Func<int, int> f = x => Foo(x); } private static int Foo(int x) { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateInAnonymousMethod()
        {
            Test(
@"class C { void M() { System.Action<int> v = delegate(int x) { x = [|Foo|](x); }; } }",
@"using System; class C { void M() { System.Action<int> v = delegate(int x) { x = Foo(x); }; } private int Foo(int x) { throw new NotImplementedException(); } }");
        }

        [WorkItem(539024)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOffOfExplicitInterface1()
        {
            Test(
@"interface I { } class A : I { [|void I.Foo() { }|] }",
@"interface I { void Foo(); } class A : I { void I.Foo() { } }");
        }

        [WorkItem(539024)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOffOfExplicitInterface2()
        {
            Test(
@"interface I { } class A : I { [|int I.Foo() { }|] }",
@"interface I { int Foo(); } class A : I { int I.Foo() { } }");
        }

        [WorkItem(539024)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOffOfExplicitInterface3()
        {
            Test(
@"interface I { } class A : I { [|void I.Foo(int i) { }|] }",
@"interface I { void Foo(int i); } class A : I { void I.Foo(int i) { } }");
        }

        [WorkItem(539024)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOffOfExplicitInterface4()
        {
            Test(
@"interface I { } class A : I { void I.[|Foo|]<T>() { } }",
@"interface I { void Foo<T>(); } class A : I { void I.Foo<T>() { } }",
index: 0);
        }

        [WorkItem(539024)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOffOfExplicitInterface5()
        {
            Test(
@"interface I { } class A : I { void I.[|Foo|]<in T>() { } }",
@"interface I { void Foo<T>(); } class A : I { void I.Foo<in T>() { } }",
index: 0);
        }

        [WorkItem(539024)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOffOfExplicitInterface6()
        {
            TestMissing(
@"interface I { void Foo(); } class A : I { void I.[|Foo|]() { } }");
        }

        [WorkItem(539024)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOffOfExplicitInterface7()
        {
            TestMissing(
@"interface I { } class A { void I.[|Foo|]() { } }");
        }

        [WorkItem(539024)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOffOfExplicitInterface8()
        {
            Test(
@"interface I<T> { } class A : I<int> { void I<int>.[|Foo|]() { } }",
@"interface I<T> { void Foo(); } class A : I<int> { void I<int>.Foo() { } }");
        }

        [WorkItem(539024)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOffOfExplicitInterface9()
        {
            // TODO(cyrusn): It might be nice if we generated "Foo(T i)" here in the future.
            Test(
@"interface I<T> { } class A : I<int> { void I<int>.[|Foo|](int i) { } }",
@"interface I<T> { void Foo(int i); } class A : I<int> { void I<int>.Foo(int i) { } }");
        }

        [WorkItem(5016, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodWithArgumentFromBaseConstructorsArgument()
        {
            Test(
@"class A { public A(string s) { } } class B : A { B(string s) : base([|M|](s)) { } }",
@"using System; class A { public A(string s) { } } class B : A { B(string s) : base(M(s)) { } private static string M(string s) { throw new NotImplementedException(); } }");
        }

        [WorkItem(5016, "DevDiv_Projects/Roslyn")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodWithArgumentFromGenericConstructorsArgument()
        {
            Test(
@"class A<T> { public A(T t) { } } class B : A<int> { B() : base([|M|]()) { } }",
@"using System; class A<T> { public A(T t) { } } class B : A<int> { B() : base(M()) { } private static int M() { throw new NotImplementedException(); } }");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodWithVar()
        {
            Test(
@"class C { void M() { var v = 10; v = [|Foo|](v);} }",
@"using System; class C { void M() { var v = 10; v = Foo(v);} private int Foo(int v) { throw new NotImplementedException(); } }");
        }

        [WorkItem(539489)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestEscapedName()
        {
            Test(
@"class Class { void Method() { [|@Foo|](); } }",
@"using System; class Class { void Method() { @Foo(); } private void Foo() { throw new NotImplementedException(); } }");
        }

        [WorkItem(539489)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestEscapedKeyword()
        {
            Test(
@"class Class { void Method() { [|@int|](); } }",
@"using System; class Class { void Method() { @int(); } private void @int() { throw new NotImplementedException(); } }");
        }

        [WorkItem(539527)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestUnmentionableTypeParameter1()
        {
            Test(
@"class Class<A> { void Method(A a) { B.[|C|](a); } } class B { }",
@"using System; class Class<A> { void Method(A a) { B.C(a); } } class B { internal static void C<A>(A a) { throw new NotImplementedException(); } }");
        }

        [WorkItem(539527)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestUnmentionableTypeParameter2()
        {
            Test(
@"class Class<A> { void Method(A a) { [|C|](a); } }",
@"using System; class Class<A> { void Method(A a) { C(a); } private void C(A a) { throw new NotImplementedException(); } }");
        }

        [WorkItem(539527)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestUnmentionableTypeParameter3()
        {
            Test(
@"class Class<A> { class Internal { void Method(A a) { [|C|](a); } } }",
@"using System; class Class<A> { class Internal { void Method(A a) { C(a); } private void C(A a) { throw new NotImplementedException(); } } }");
        }

        [WorkItem(539527)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestUnmentionableTypeParameter4()
        {
            Test(
@"class Class<A> { class Internal { void Method(Class<A> c, A a) { c.[|M|](a); } } }",
@"using System; class Class<A> { class Internal { void Method(Class<A> c, A a) { c.M(a); } } private void M(A a) { throw new NotImplementedException(); } }");
        }

        [WorkItem(539527)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestUnmentionableTypeParameter5()
        {
            Test(
@"class Class<A> { class Internal { void Method(Class<int> c, A a) { c.[|M|](a); } } }",
@"using System; class Class<A> { class Internal { void Method(Class<int> c, A a) { c.M(a); } } private void M(A a) { throw new NotImplementedException(); } }");
        }

        [WorkItem(539596)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestUnmentionableTypeParameter6()
        {
            Test(
@"class Test { void F < U , V > ( U u1 , V v1 ) { [|Foo < int , string >|] ( u1 , v1 ) ; } } ",
@"using System; class Test { void F < U , V > ( U u1 , V v1 ) { Foo < int , string > ( u1 , v1 ) ; } private void Foo < T1 , T2 > ( object u1 , object v1 ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(539593)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestUnmentionableTypeParameter7()
        {
            Test(
@"class H < T > { void A ( T t1 ) { t1 = [|Foo < T >|] ( t1 ) ; } } ",
@"using System; class H < T > { void A ( T t1 ) { t1 = Foo < T > ( t1 ) ; } private T Foo < T1 > ( T t1 ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(539593)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestUnmentionableTypeParameter8()
        {
            Test(
@"class H < T1 , T2 > { void A ( T1 t1 ) { t1 = [|Foo < int , string >|] ( t1 ) ; } } ",
@"using System; class H < T1 , T2 > { void A ( T1 t1 ) { t1 = Foo < int , string > ( t1 ) ; } private T1 Foo < T3 , T4 > ( T1 t1 ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(539597)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestOddErrorType()
        {
            Test(
@"public class C { void M ( ) { @public c = [|F|] ( ) ; } } ",
@"using System; public class C { void M ( ) { @public c = F ( ) ; } private @public F ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(539594)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenericOverloads()
        {
            Test(
@"class C { public C ( ) { CA . [|M < char , bool >|] ( ) ; } } class CA { public static void M < V > ( ) { } public static void M < V , W , X > ( ) { } } ",
@"using System; class C { public C ( ) { CA . M < char , bool > ( ) ; } } class CA { public static void M < V > ( ) { } public static void M < V , W , X > ( ) { } internal static void M < T1 , T2 > ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(537929)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInScript1()
        {
            Test(
@"using System ; static void Main ( string [ ] args ) { [|Foo|] ( ) ; } ",
@"using System ; static void Main ( string [ ] args ) { Foo ( ) ; } void Foo ( ) { throw new NotImplementedException ( ) ; } ",
parseOptions: GetScriptOptions());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInTopLevelImplicitClass1()
        {
            Test(
@"using System ; static void Main ( string [ ] args ) { [|Foo|] ( ) ; } ",
@"using System ; static void Main ( string [ ] args ) { Foo ( ) ; } void Foo ( ) { throw new NotImplementedException ( ) ; } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInNamespaceImplicitClass1()
        {
            Test(
@"namespace N { using System ; static void Main ( string [ ] args ) { [|Foo|] ( ) ; } } ",
@"namespace N { using System ; static void Main ( string [ ] args ) { Foo ( ) ; } void Foo ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact]
        [Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInNamespaceImplicitClass_FieldInitializer()
        {
            Test(
@"namespace N { using System ; int f = [|Foo|] ( ) ; } ",
@"namespace N { using System ; int f = Foo ( ) ; int Foo ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(539571)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimplification1()
        {
            Test(
@"using System ; class Program { static void Main ( string [ ] args ) { [|Bar|] ( ) ; } private static void Foo ( ) { throw new System . NotImplementedException ( ) ; } } ",
@"using System ; class Program { static void Main ( string [ ] args ) { Bar ( ) ; } private static void Bar ( ) { throw new NotImplementedException ( ) ; } private static void Foo ( ) { throw new System . NotImplementedException ( ) ; } } ");
        }

        [WorkItem(539571)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestSimplification2()
        {
            Test(
@"using System ; class Program { static void Main ( string [ ] args ) { System . Action a = [|Bar|] ( DateTime . Now ) ; } } ",
@"using System ; class Program { static void Main ( string [ ] args ) { System . Action a = Bar ( DateTime . Now ) ; } private static Action Bar ( DateTime now ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(539618)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestClashesWithMethod1()
        {
            TestMissing(
@"using System ; class Program { void Main () { [|Foo|](x: 1, true) ; } private void Foo(int x, bool b); } ");
        }

        [WorkItem(539618)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestClashesWithMethod2()
        {
            TestMissing(
@"class Program : IFoo { [|bool IFoo.Foo() { }|] } } interface IFoo { void Foo(); }");
        }

        [WorkItem(539637)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestReservedParametername1()
        {
            Test(
@"class C { public void Method ( ) { long Long = 10 ; [|M|] ( Long ) ; } } ",
@"using System; class C { public void Method ( ) { long Long = 10 ; M ( Long ) ; } private void M ( long @long ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(539751)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestShadows1()
        {
            TestMissing(
@"using System ; class Program { static void Main ( string [ ] args ) { int Name ; Name = [|Name|] ( ) ; } } ");
        }

        [WorkItem(539769)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestShadows2()
        {
            TestMissing(
@"using System ; class Program { delegate void Func ( int i , int j ) ; static void Main ( string [ ] args ) { Func myExp = ( x , y ) => Console . WriteLine ( x == y ) ; myExp ( 10 , 20 ) ; [|myExp|] ( 10 , 20 , 10 ) ; } } ");
        }

        [WorkItem(539781)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInTopLevelMethod()
        {
            Test(
@"void M ( ) { [|Foo|] ( ) ; } ",
@"using System; void M ( ) { Foo ( ) ; } void Foo ( ) { throw new NotImplementedException ( ) ; } ");
        }

        [WorkItem(539823)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestLambdaReturnType()
        {
            Test(
@"using System ; class C < T , R > { private static Func < T , R > g = null ; private static Func < T , R > f = ( T ) => { return [|Foo < T , R >|] ( g ) ; } ; } ",
@"using System ; class C < T , R > { private static Func < T , R > g = null ; private static Func < T , R > f = ( T ) => { return Foo < T , R > ( g ) ; } ; private static R Foo < T1 , T2 > ( Func < T , R > g ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateWithThrow()
        {
            Test(
@"using System ; using System . Collections . Generic ; using System . Linq ; class C { void M ( ) { throw [|F|] ( ) ; } } ",
@"using System ; using System . Collections . Generic ; using System . Linq ; class C { void M ( ) { throw F ( ) ; } private Exception F ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInDelegateConstructor()
        {
            Test(
@"using System ; delegate void D ( int x ) ; class C { void M ( ) { D d = new D ( [|Test|] ) ; } } ",
@"using System ; delegate void D ( int x ) ; class C { void M ( ) { D d = new D ( Test ) ; } private void Test ( int x ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(539871)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDelegateScenario()
        {
            TestMissing(
@"class C < T > { public delegate void Foo < R > ( R r ) ; static void M ( ) { Foo < T > r = [|Goo < T >|] ; } } ");
        }

        [WorkItem(539928)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInheritedTypeParameters1()
        {
            Test(
@"class C < T , R > { void M ( ) { I < T , R > i1 ; I < T , R > i2 = i1 . [|Foo|] ( ) ; } } interface I < T , R > { } ",
@"class C < T , R > { void M ( ) { I < T , R > i1 ; I < T , R > i2 = i1 . Foo ( ) ; } } interface I < T , R > { I < T , R > Foo ( ) ; } ");
        }

        [WorkItem(539928)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInheritedTypeParameters2()
        {
            Test(
@"class C < T > { void M ( ) { I < T > i1 ; I < T > i2 = i1 . [|Foo|] ( ) ; } } interface I < T > { } ",
@"class C < T > { void M ( ) { I < T > i1 ; I < T > i2 = i1 . Foo ( ) ; } } interface I < T > { I < T > Foo ( ) ; } ");
        }

        [WorkItem(539928)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInheritedTypeParameters3()
        {
            Test(
@"class C < T > { void M ( ) { I < T > i1 ; I < T > i2 = i1 . [|Foo|] ( ) ; } } interface I < X > { } ",
@"class C < T > { void M ( ) { I < T > i1 ; I < T > i2 = i1 . Foo ( ) ; } } interface I < X > { I < object > Foo ( ) ; } ");
        }

        [WorkItem(538995)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestBug4777()
        {
            Test(
@"class C { void M ( ) { F([|123.4|]); } void F(int x) {} }",
@"using System; class C { void M ( ) { F(123.4); } private void F(double v) {throw new NotImplementedException ( ) ; } void F(int x) {}  }");
        }

        [WorkItem(539856)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateOnInvalidInvocation()
        {
            TestMissing(
@"class C { public delegate int Func ( ref int i ) ; public int Goo { get ; set ; } public Func Foo ( ) { return [|Foo|] ( ref Goo ) ; } } ");
        }

        [WorkItem(539752)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMissingOnMultipleLambdaInferences()
        {
            TestMissing(
@"using System ; using System . Collections . Generic ; class Program { static void Main ( string [ ] args ) { C < int > c = new C < int > ( ) ; c . [|Sum|] ( ( arg ) => { return 2 ; } ) ; } } class C < T > : List < T > { public int Sum ( Func < T , int > selector ) { return 2 ; } public int Sum ( Func < T , double > selector ) { return 3 ; } } ");
        }

        [WorkItem(540505)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestParameterTypeAmbiguity()
        {
            Test(
@"namespace N { class N { static void Main ( string [ ] args ) { C c ; [|Foo|] ( c ) ; } } class C { } } ",
@"using System; namespace N { class N { static void Main ( string [ ] args ) { C c ; Foo ( c ) ; } private static void Foo ( C c ) { throw new NotImplementedException ( ) ; } } class C { } } ");
        }

        [WorkItem(541176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestTernaryWithBodySidesBroken1()
        {
            Test(
@"public class C { void Method ( ) { int a = 5 , b = 10 ; int x = a > b ? [|M|] ( a ) : M ( b ) ; } } ",
@"using System; public class C { void Method ( ) { int a = 5 , b = 10 ; int x = a > b ? M ( a ) : M ( b ) ; } private int M ( int a ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(541176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestTernaryWithBodySidesBroken2()
        {
            Test(
@"public class C { void Method ( ) { int a = 5 , b = 10 ; int x = a > b ? M ( a ) : [|M|] ( b ) ; } } ",
@"using System; public class C { void Method ( ) { int a = 5 , b = 10 ; int x = a > b ? M ( a ) : M ( b ) ; } private int M ( int b ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestNotOnLeftOfAssign()
        {
            TestMissing(
@"using System ; class C { public static void Main ( ) { string s = ""Hello"" ; [|f|] = s . ExtensionMethod ; } } public static class MyExtension { public static int ExtensionMethod ( this String s ) { return s . Length ; } } ");
        }

        [WorkItem(541405)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMissingOnImplementedInterfaceMethod()
        {
            TestMissing(
@"class Program < T > : ITest { [|void ITest . Method ( T t ) { }|] } interface ITest { void Method ( object t ) ; } ");
        }

        [WorkItem(541660)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDelegateNamedVar()
        {
            Test(
@"using System ; class Program { public static void Main ( ) { var v = [|M|] ; } delegate void var ( int x ) ; } ",
@"using System ; class Program { public static void Main ( ) { var v = M ; } private static void M ( int x ) { throw new NotImplementedException ( ) ; } delegate void var ( int x ) ; } ");
        }

        [WorkItem(540991)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestErrorVersusNamedTypeInSignature()
        {
            TestMissing(
@"using System ; class Outer { class Inner { } void M ( ) { A . [|Test|] ( new Inner ( ) ) ; } } class A { internal static void Test ( global :: Outer . Inner inner ) { throw new NotImplementedException ( ) ; } } ",
Options.Regular);
        }

        [WorkItem(542529)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestTypeParameterConstraints1()
        {
            Test(
@"using System ; class A < T > where T : class { } class Program { static void Foo < T > ( A < T > x ) where T : class { [|Bar|] ( x ) ; } } ",
@"using System ; class A < T > where T : class { } class Program { static void Foo < T > ( A < T > x ) where T : class { Bar ( x ) ; } private static void Bar < T > ( A < T > x ) where T : class { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(542622)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestLambdaTypeParameters()
        {
            Test(
@"using System ; using System . Collections . Generic ; class Program { static void Foo < T > ( List < T > x ) { [|Bar|] ( ( ) => x ) ; } } ",
@"using System ; using System . Collections . Generic ; class Program { static void Foo < T > ( List < T > x ) { Bar ( ( ) => x ) ; } private static void Bar < T > ( Func < List < T > > p ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(542626)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMethodConstraints1()
        {
            Test(
@"using System ; class A < T > where T : class { } class Program { static void Foo < T > ( A < T > x ) where T : class { [|Bar < T >|] ( x ) ; } } ",
@"using System ; class A < T > where T : class { } class Program { static void Foo < T > ( A < T > x ) where T : class { Bar < T > ( x ) ; } private static void Bar < T > ( A < T > x ) where T : class { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(542627)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestCaptureMethodTypeParametersReferencedInOuterType1()
        {
            Test(
@"using System . Collections . Generic ; class Program { static void Foo < T > ( List < T > . Enumerator x ) { [|Bar|] ( x ) ; } } ",
@"using System ; using System . Collections . Generic ; class Program { static void Foo < T > ( List < T > . Enumerator x ) { Bar ( x ) ; } private static void Bar < T > ( List < T > . Enumerator x ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(542658)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestCaptureTypeParametersInConstraints()
        {
            Test(
@"using System ; using System . Collections . Generic ; class Program { static void Foo < T , S > ( List < T > x ) where T : S { [|Bar|] ( x ) ; } } ",
@"using System ; using System . Collections . Generic ; class Program { static void Foo < T , S > ( List < T > x ) where T : S { Bar ( x ) ; } private static void Bar < T , S > ( List < T > x ) where T : S { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(542659)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestConstraintOrder1()
        {
            Test(
@"using System ; class A < T , S > where T : ICloneable , S { } class B < S > { public virtual void Foo < T > ( A < T , S > x ) where T : ICloneable , S { } } class C : B < Exception > { public override void Foo < T > ( A < T , Exception > x ) { [|Bar|] ( x ) ; } } ",
@"using System ; class A < T , S > where T : ICloneable , S { } class B < S > { public virtual void Foo < T > ( A < T , S > x ) where T : ICloneable , S { } } class C : B < Exception > { public override void Foo < T > ( A < T , Exception > x ) { Bar ( x ) ; } private void Bar < T > ( A < T , Exception > x ) where T : Exception , ICloneable { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(542678)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestConstraintOrder2()
        {
            Test(
@"using System ; class A < T , S , U > where T : U , S { } class B < S , U > { public virtual void Foo < T > ( A < T , S , U > x ) where T : U , S { } } class C < U > : B < Exception , U > { public override void Foo < T > ( A < T , Exception , U > x ) { [|Bar|] ( x ) ; } } ",
@"using System ; class A < T , S , U > where T : U , S { } class B < S , U > { public virtual void Foo < T > ( A < T , S , U > x ) where T : U , S { } } class C < U > : B < Exception , U > { public override void Foo < T > ( A < T , Exception , U > x ) { Bar ( x ) ; } private void Bar < T > ( A < T , Exception , U > x ) where T : Exception , U { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(542674)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateStaticMethodInField()
        {
            Test(
@"using System ; class C { int x = [|Foo|] ( ) ; } ",
@"using System ; class C { int x = Foo ( ) ; private static int Foo ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(542680)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateIntoConstrainedTypeParameter()
        {
            Test(
@"interface I { } class Program { static void Foo < T > ( T x ) where T : I { x . [|Bar|] ( ) ; } } ",
@"interface I { void Bar ( ) ; } class Program { static void Foo < T > ( T x ) where T : I { x . Bar ( ) ; } } ");
        }

        [WorkItem(542750)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestCaptureOuterTypeParameter()
        {
            Test(
@"using System ; using System . Collections . Generic ; class C < T > { void Bar ( ) { D d = new D ( ) ; List < T > y ; d . [|Foo|] ( y ) ; } } class D { } ",
@"using System ; using System . Collections . Generic ; class C < T > { void Bar ( ) { D d = new D ( ) ; List < T > y ; d . Foo ( y ) ; } } class D { internal void Foo < T > ( List < T > y ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(542744)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestMostDerivedTypeParameter()
        {
            Test(
@"using System ; class A < T , U > where T : U { } class B < U > { public virtual void Foo < T > ( A < T , U > x ) where T : Exception , U { } } class C < U > : B < ArgumentException > { public override void Foo < T > ( A < T , ArgumentException > x ) { [|Bar|] ( x ) ; } } ",
@"using System ; class A < T , U > where T : U { } class B < U > { public virtual void Foo < T > ( A < T , U > x ) where T : Exception , U { } } class C < U > : B < ArgumentException > { public override void Foo < T > ( A < T , ArgumentException > x ) { Bar ( x ) ; } private void Bar < T > ( A < T , ArgumentException > x ) where T : ArgumentException { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(543152)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestAnonymousTypeArgument()
        {
            Test(
@"class C { void M ( ) { [|M|] ( new { x = 1 } ) ; } } ",
@"using System ; class C { void M ( ) { M ( new { x = 1 } ) ; } private void M ( object p ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestListOfAnonymousTypesArgument()
        {
            Test(
@"using System ; using System . Collections . Generic ; class C { void M ( ) { var v = new { } ; var u = Foo ( v ) ; [|M|] ( u ) ; } private List < T > Foo < T > ( T v ) { return new List < T > ( ) ; } } ",
@"using System ; using System . Collections . Generic ; class C { void M ( ) { var v = new { } ; var u = Foo ( v ) ; M ( u ) ; } private void M ( List < object > u ) { throw new NotImplementedException ( ) ; } private List < T > Foo < T > ( T v ) { return new List < T > ( ) ; } } ");
        }

        [WorkItem(543336)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateImplicitlyTypedArrays()
        {
            Test(
@"class C { void M() { var a = new[] { [|foo|](2), 2, 3 }; } }",
@"using System ; class C { void M() { var a = new[] { foo(2), 2, 3 }; } private int foo(int v) { throw new NotImplementedException(); } } ");
        }

        [WorkItem(543510)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenericArgWithMissingTypeParameter()
        {
            TestMissing(
@"class Program { public static int foo(ref int i) { return checked([|goo|]<>(ref i) * i); } public static int goo<T>(ref int i) { return i; } } ");
        }

        [WorkItem(544334)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDuplicateWithErrorType()
        {
            TestMissing(
@"using System ; class class1 { public void Test ( ) { [|Foo|] ( x ) ; } private void Foo ( object x ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestNoGenerationIntoEntirelyHiddenType()
        {
            TestMissing(
@"
class C
{
    void Foo()
    {
        D.[|Bar|]();
    }
}

#line hidden
class D
{
}
#line default
");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDoNotGenerateIntoHiddenRegion1()
        {
            Test(
@"#line default
class C
{
    void Foo()
    {
        [|Bar|]();
#line hidden
    }
#line default
}",
@"#line default
class C
{
    private void Bar()
    {
        throw new System.NotImplementedException();
    }

    void Foo()
    {
        Bar();
#line hidden
    }
#line default
}", compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDoNotGenerateIntoHiddenRegion2()
        {
            Test(
@"#line default
class C
{
    void Foo()
    {
        [|Bar|]();
#line hidden
    }

    void Baz()
    {
#line default
    }
}",
@"#line default
class C
{
    void Foo()
    {
        Bar();
#line hidden
    }

    void Baz()
    {
#line default
    }

    private void Bar()
    {
        throw new System.NotImplementedException();
    }
}", compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDoNotGenerateIntoHiddenRegion3()
        {
            Test(
@"#line default
class C
{
    void Foo()
    {
        [|Bar|]();
#line hidden
    }

    void Baz()
    {
#line default
    }

    void Quux()
    {
    }
}",
@"#line default
class C
{
    void Foo()
    {
        Bar();
#line hidden
    }

    void Baz()
    {
#line default
    }

    private void Bar()
    {
        throw new System.NotImplementedException();
    }

    void Quux()
    {
    }
}", compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDoNotAddImportsIntoHiddenRegion()
        {
            Test(
@"
#line hidden
class C
#line default
{
    void Foo()
    {
        [|Bar|]();
#line hidden
    }
#line default
}",
@"
#line hidden
class C
#line default
{
    private void Bar()
    {
        throw new System.NotImplementedException();
    }

    void Foo()
    {
        Bar();
#line hidden
    }
#line default
}", compareTokens: false);
        }

        [WorkItem(545397)]
        [WorkItem(784793)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestVarParameterTypeName()
        {
            Test(
@"using System ; class Program { void Main ( ) { var x ; [|foo|] ( out x ) ; } } ",
@"using System ; class Program { void Main ( ) { var x ; foo ( out x ) ; } private void foo ( out object x ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(545269)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateInVenus1()
        {
            TestMissing(
@"
class C
{
#line 1 ""foo""
    void Foo()
    {
        this.[|Bar|]();
    }
#line default
#line hidden
}
");
        }

        [WorkItem(538521)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestInIterator1()
        {
            Test(
@"using System . Collections . Generic ; class Program { IEnumerable < int > Foo ( ) { yield return [|Bar|] ( ) ; } } ",
@"using System ; using System . Collections . Generic ; class Program { IEnumerable < int > Foo ( ) { yield return Bar ( ) ; } private int Bar ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(784793)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodMissingForAnyArgumentInInvocationHavingErrorTypeAndNotBelongingToEnclosingNamedType()
        {
            Test(
@"
class Program
{
    static void Main(string[] args)
    {
        [|Main(args.Foo())|];
    }
}
",
@"
using System;

class Program
{
    static void Main(string[] args)
    {
        Main(args.Foo());
    }

    private static void Main(object p)
    {
        throw new NotImplementedException();
    }
}
");
        }

        [WorkItem(907612)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodWithLambda()
        {
            Test(
@"
using System;

class Program
{
    void Baz(string[] args)
    {
        Baz([|() => { return true; }|]);
    }
}",
@"
using System;

class Program
{
    void Baz(string[] args)
    {
        Baz(() => { return true; });
    }

    private void Baz(Func<bool> p)
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(889349)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodForDifferentParameterName()
        {
            Test(
@"
class C
{
    void M()
    {
        M([|x: 42|]);
    }

    void M(int y) { }
}",
@"
using System;

class C
{
    void M()
    {
        M(x: 42);
    }

    private void M(int x)
    {
        throw new NotImplementedException();
    }

    void M(int y) { }
}", compareTokens: false);
        }

        [WorkItem(889349)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodForDifferentParameterNameCaseSensitive()
        {
            Test(
@"
class C
{
    void M()
    {
        M([|Y: 42|]);
    }

    void M(int y) { }
}",
@"
using System;

class C
{
    void M()
    {
        M(Y: 42);
    }

    private void M(int Y)
    {
        throw new NotImplementedException();
    }

    void M(int y) { }
}", compareTokens: false);
        }

        [WorkItem(769760)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodForSameNamedButGenericUsage()
        {
            Test(
@"using System;

class Program
{
    static void Main(string[] args)
    {
        Foo();
        [|Foo<int>|]();
    }

    private static void Foo()
    {
        throw new NotImplementedException();
    }
}",
@"using System;

class Program
{
    static void Main(string[] args)
    {
        Foo();
        Foo<int>();
    }

    private static void Foo<T>()
    {
        throw new NotImplementedException();
    }

    private static void Foo()
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(910589)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodForNewErrorCodeCS7036()
        {
            Test(
@"using System;
class C
{
    void M(int x)
    {
        [|M|]();
    }
}",
    @"using System;
class C
{
    void M(int x)
    {
        M();
    }

    private void M()
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(934729)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodUnknownReturnTypeInLambda()
        {
            Test(
@"using System.Collections.Generic; 
class C
{
    void TestMethod(IEnumerable<C> c)
    {
       new C().[|TestMethod((a,b) => c.Add)|]
    }
}",
    @"using System;
using System.Collections.Generic;
class C
{
    void TestMethod(IEnumerable<C> c)
    {
       new C().TestMethod((a,b) => c.Add)
    }

    private void TestMethod(Func<object, object, object> p)
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(530177)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInUnsafeMethod()
        {
            Test(
@"using System;
class C {
    unsafe void Method(he) {
        int a = 10; [|TestMethod(&a)|];
    }
}",
    @"using System;
class C {
    unsafe void Method(he) {
        int a = 10; TestMethod(&a);
    }

    private unsafe void TestMethod(int* v)
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(530177)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInUnsafeMethodWithPointerArray()
        {
            Test(
@"class C
{
    unsafe static void M1(int *[] o)
    {
        [|M2(o)|];
    }
}",
    @"using System;

class C
{
    unsafe static void M1(int *[] o)
    {
        M2(o);
    }

    private static unsafe void M2(int*[] o)
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(530177)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInUnsafeBlock()
        {
            Test(
@"using System;
class Program
{
    static void Main()
    {
        unsafe
        {
            fixed (char* value = ""sam"")
            {
                [|TestMethod(value)|];
            }
        }
    }
}",
@"using System;
class Program
{
    static void Main()
    {
        unsafe
        {
            fixed (char* value = ""sam"")
            {
                TestMethod(value);
            }
        }
    }

    private static unsafe void TestMethod(char* value)
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(530177)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInUnsafeMethodNoPointersInParameterList()
        {
            Test(
@"using System;
class C {
    unsafe void Method(he) {
        int a = 10; [|TestMethod(a)|];
    }
}",
    @"using System;
class C {
    unsafe void Method(he) {
        int a = 10; TestMethod(a);
    }

    private void TestMethod(int a)
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(530177)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInUnsafeBlockNoPointers()
        {
            Test(
@"using System;
class Program
{
    static void Main()
    {
        unsafe
        {
            fixed (char value = ""sam"")
            {
                [|TestMethod(value)|];
            }
        }
    }
}",
@"using System;
class Program
{
    static void Main()
    {
        unsafe
        {
            fixed (char value = ""sam"")
            {
                TestMethod(value);
            }
        }
    }

    private static void TestMethod(char value)
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(530177)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodUnsafeReturnType()
        {
            Test(
@"using System;
class Program
{
    static void Main()
    {
        int* a = [|Test()|];
    }
}",
@"using System;
class Program
{
    static void Main()
    {
        int* a = Test();
    }

    private static unsafe int* Test()
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(530177)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodUnsafeClass()
        {
            Test(
@"using System;
unsafe class Program
{
    static void Main()
    {
        int* a = [|Test()|];
    }
}",
@"using System;
unsafe class Program
{
    static void Main()
    {
        int* a = Test();
    }

    private static int* Test()
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [WorkItem(530177)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodUnsafeNestedClass()
        {
            Test(
@"using System;
unsafe class Program
{
    class MyClass
    {
        static void Main()
        {
            int* a = [|Test()|];
        }
    }
}",
@"using System;
unsafe class Program
{
    class MyClass
    {
        static void Main()
        {
            int* a = [|Test()|];
        }

        private static int* Test()
        {
            throw new NotImplementedException();
        }
    }
}", compareTokens: false);
        }

        [WorkItem(530177)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodUnsafeNestedClass2()
        {
            Test(
@"using System;
class Program
{
    unsafe class MyClass
    {
        static void Main(string[] args)
        {
            int* a = [|Program.Test()|];
        }
    }
}",
@"using System;
class Program
{
    unsafe class MyClass
    {
        static void Main(string[] args)
        {
            int* a = Program.Test();
        }
    }

    private static unsafe int* Test()
    {
        throw new NotImplementedException();
    }
}", compareTokens: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestDoNotOfferMethodWithoutParenthesis()
        {
            TestMissing(
@"class Class { void Method() { [|Foo|]; } }");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf()
        {
            Test(
@"class C
{
    void M()
    {
        var x = nameof([|Z|]);
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(Z);
    }

    private object Z()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf2()
        {
            Test(
@"class C
{
    void M()
    {
        var x = nameof([|Z.X|]);
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(Z.X);
    }

    private object nameof(object x)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf3()
        {
            Test(
@"class C
{
    void M()
    {
        var x = nameof([|Z.X.Y|]);
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(Z.X.Y);
    }

    private object nameof(object y)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf4()
        {
            Test(
@"using System;

class C
{
    void M()
    {
        var x = nameof([|Z.X.Y|]);
    }
}


namespace Z
{
    class X
    {

    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(Z.X.Y);
    }
}


namespace Z
{
    class X
    {
        internal static object Y()
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf5()
        {
            TestMissing(
@"class C
{
    void M()
    {
        var x = nameof([|1 + 2|]);
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf6()
        {
            TestMissing(
@"class C
{
    void M()
    {
        var y = 1 + 2;
        var x = [|nameof(y)|];
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf7()
        {
            Test(
@"class C
{
    void M()
    {
        var y = 1 + 2;
        var z = "";
        var x = [|nameof(y, z)|];
    }
}",
@"using System;

class C
{
    void M()
    {
        var y = 1 + 2;
        var z = "";
        var x = nameof(y, z);
    }

    private object nameof(int y, string z)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf8()
        {
            Test(
@"using System;

class C
{
    void M()
    {
        var x = [|nameof|](1 + 2, """");
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(1 + 2, """");
    }

    private object nameof(int v1, string v2)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf9()
        {
            Test(
@"class C
{
    void M()
    {
        var x = [|nameof|](y, z);
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(y, z);
    }

    private object nameof(object y, object z)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf10()
        {
            Test(
@"class C
{
    void M()
    {
        var x = nameof([|y|], z);
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(y, z);
    }

    private object y()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf11()
        {
            Test(
@"class C
{
    void M()
    {
        var x = nameof(y, [|z|]);
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(y, z);
    }

    private object z()
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf12()
        {
            TestMissing(
@"using System;

class C
{
    void M()
    {
        var x = [|nameof|](y, z);
    }

    private object nameof(object y, object z)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf13()
        {
            Test(
@"using System;

class C
{
    void M()
    {
        var x = nameof([|y|], z);
    }

    private object nameof(object y, object z)
    {
        throw new NotImplementedException();
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(y, z);
    }

    private object y()
    {
        throw new NotImplementedException();
    }

    private object nameof(object y, object z)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf14()
        {
            Test(
@"using System;

class C
{
    void M()
    {
        var x = nameof(y, [|z|]);
    }

    private object nameof(object y, object z)
    {
        throw new NotImplementedException();
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(y, z);
    }

    private object z()
    {
        throw new NotImplementedException();
    }

    private object nameof(object y, object z)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf15()
        {
            Test(
@"using System;

class C
{
    void M()
    {
        var x = [|nameof()|];
    }

    private object nameof(object y)
    {
        throw new NotImplementedException();
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof();
    }

    private object nameof()
    {
        throw new NotImplementedException();
    }

    private object nameof(object y)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1032176)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInsideNameOf16()
        {
            Test(
@"using System;

class C
{
    void M()
    {
        var x = [|nameof(1 + 2, 5)|];
    }

    private object nameof(object y)
    {
        throw new NotImplementedException();
    }
}",
@"using System;

class C
{
    void M()
    {
        var x = nameof(1 + 2, 5);
    }

    private object nameof(int v1, int v2)
    {
        throw new NotImplementedException();
    }

    private object nameof(object y)
    {
        throw new NotImplementedException();
    }
}");
        }

        [WorkItem(1075289)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodForInaccessibleMethod()
        {
            Test(
@"namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
        }
        private void Test() { }
    }
    class Program2 : Program
    {
        public Program2()
        {
            [|Test()|];
        }
    }
}",
@"using System;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
        }
        private void Test() { }
    }
    class Program2 : Program
    {
        public Program2()
        {
            Test();
        }

        private void Test()
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [WorkItem(1064748)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInConditionalAccessMissing()
        {
            TestMissing(
@"class C { void Main ( C a ) { C x = new C ? [|. B|] ( ) ; } } ");
        }

        [WorkItem(1064748)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInConditionalAccess()
        {
            Test(
@"public class C { void Main ( C a ) { C x = a ? [|. B|] ( ) ; } } ",
@"using System ; public class C { void Main ( C a ) { C x = a ? . B ( ) ; } private C B ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(1064748)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInConditionalAccess2()
        {
            Test(
@"public class C { void Main ( C a ) { int x = a ? [|. B|] ( ) ; } } ",
@"using System ; public class C { void Main ( C a ) { int x = a ? . B ( ) ; } private int B ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(1064748)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInConditionalAccess3()
        {
            Test(
@"public class C { void Main ( C a ) { int ? x = a ? [|. B|] ( ) ; } } ",
@"using System ; public class C { void Main ( C a ) { int ? x = a ? . B ( ) ; } private int B ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(1064748)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInConditionalAccess4()
        {
            Test(
@"public class C { void Main ( C a ) { MyStruct ? x = a ? [|. B|] ( ) ; } } ",
@"using System ; public class C { void Main ( C a ) { MyStruct ? x = a ? . B ( ) ; } private MyStruct B ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(1064748)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestTestGenerateMethodInConditionalAccess5()
        {
            Test(
@"class C { public E B { get ; private set ; } void Main ( C a ) { C x = a ? . B . [|C|] ( ) ; } public class E { } } ",
@"using System ; class C { public E B { get ; private set ; } void Main ( C a ) { C x = a ? . B . C ( ) ; } public class E { internal C C ( ) { throw new NotImplementedException ( ) ; } } } ");
        }

        [WorkItem(1064748)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInConditionalAccess6()
        {
            Test(
@"class C { public E B { get ; private set ; } void Main ( C a ) { int x = a ? . B . [|C|] ( ) ; } public class E { } } ",
@"using System ; class C { public E B { get ; private set ; } void Main ( C a ) { int x = a ? . B . C ( ) ; } public class E { internal int C ( ) { throw new NotImplementedException ( ) ; } } } ");
        }

        [WorkItem(1064748)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInConditionalAccess7()
        {
            Test(
@"class C { public E B { get ; private set ; } void Main ( C a ) { int ? x = a ? . B . [|C|] ( ) ; } public class E { } } ",
@"using System ; class C { public E B { get ; private set ; } void Main ( C a ) { int ? x = a ? . B . C ( ) ; } public class E { internal int C ( ) { throw new NotImplementedException ( ) ; } } } ");
        }

        [WorkItem(1064748)]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInConditionalAccess8()
        {
            Test(
@"class C { public E B { get ; private set ; } void Main ( C a ) { var x = a ? . B . [|C|] ( ) ; } public class E { } } ",
@"using System ; class C { public E B { get ; private set ; } void Main ( C a ) { var x = a ? . B . C ( ) ; } public class E { internal object C ( ) { throw new NotImplementedException ( ) ; } } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInPropertyInitializer()
        {
            Test(
@"class Program { public int MyProperty { get ; } = [|y|] ( ) ; } ",
@"using System ; class Program { public int MyProperty { get ; } = y ( ) ; private static int y ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInExpressionBodiedMember()
        {
            Test(
@"class Program { public int Y => [|y|] ( ) ; } ",
@"using System ; class Program { public int Y => y ( ) ; private int y ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInExpressionBodiedMember2()
        {
            Test(
@"class C { public static C GetValue ( C p ) => [|x|] ( ) ; } ",
@"using System ; class C { public static C GetValue ( C p ) => x ( ) ; private static C x ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInExpressionBodiedMember3()
        {
            Test(
@"class C { public static C operator -- ( C p ) => [|x|] ( ) ; } ",
@"using System ; class C { public static C operator -- ( C p ) => x ( ) ; private static C x ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInDictionaryInitilizer()
        {
            Test(
@"using System . Collections . Generic ; class Program { static void Main ( string [ ] args ) { var x = new Dictionary < string , int > { [ [|key|] ( ) ] = 0 } ; } } ",
@"using System ; using System . Collections . Generic ; class Program { static void Main ( string [ ] args ) { var x = new Dictionary < string , int > { [ key ( ) ] = 0 } ; } private static string key ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInDictionaryInitilizer2()
        {
            Test(
@"using System . Collections . Generic ; class Program { static void Main ( string [ ] args ) { var x = new Dictionary < string , int > { [ ""Zero"" ] = 0 , [ [|One|] ( ) ] = 1 , [ ""Two"" ] = 2 } ; } } ",
@"using System ; using System . Collections . Generic ; class Program { static void Main ( string [ ] args ) { var x = new Dictionary < string , int > { [ ""Zero"" ] = 0 , [ One ( ) ] = 1 , [ ""Two"" ] = 2 } ; } private static string One ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodInDictionaryInitilizer3()
        {
            Test(
@"using System . Collections . Generic ; class Program { static void Main ( string [ ] args ) { var x = new Dictionary < string , int > { [ ""Zero"" ] = [|i|] ( ) } ; } } ",
@"using System ; using System . Collections . Generic ; class Program { static void Main ( string [ ] args ) { var x = new Dictionary < string , int > { [ ""Zero"" ] = i ( ) } ; } private static int i ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(643, "https://github.com/dotnet/roslyn/issues/643")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodWithConfigureAwaitFalse()
        {
            Test(
@"using System ; using System . Collections . Generic ; using System . Linq ; using System . Threading . Tasks ; class Program { static void Main ( string [ ] args ) { bool x = await [|Foo|] ( ) . ConfigureAwait ( false ) ; } } ",
@"using System ; using System . Collections . Generic ; using System . Linq ; using System . Threading . Tasks ; class Program { static void Main ( string [ ] args ) { bool x = await Foo ( ) . ConfigureAwait ( false ) ; } private static Task < bool > Foo ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(643, "https://github.com/dotnet/roslyn/issues/643")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodWithMethodChaining()
        {
            Test(
@"using System ; using System . Collections . Generic ; using System . Linq ; using System . Threading . Tasks ; class Program { static void Main ( string [ ] args ) { bool x = await [|Foo|] ( ) . ConfigureAwait ( false ) ; } } ",
@"using System ; using System . Collections . Generic ; using System . Linq ; using System . Threading . Tasks ; class Program { static void Main ( string [ ] args ) { bool x = await Foo ( ) . ConfigureAwait ( false ) ; } private static Task < bool > Foo ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        [WorkItem(643, "https://github.com/dotnet/roslyn/issues/643")]
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
        public void TestGenerateMethodWithMethodChaining2()
        {
            Test(
@"using System ; using System . Threading . Tasks ; class C { static async void T ( ) { bool x = await [|M|] ( ) . ContinueWith ( a => { return true ; } ) . ContinueWith ( a => { return false ; } ) ; } } ",
@"using System ; using System . Threading . Tasks ; class C { static async void T ( ) { bool x = await M ( ) . ContinueWith ( a => { return true ; } ) . ContinueWith ( a => { return false ; } ) ; } private static Task < bool > M ( ) { throw new NotImplementedException ( ) ; } } ");
        }

        public class GenerateConversionTest : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
        {
            internal override Tuple<DiagnosticAnalyzer, CodeFixProvider> CreateDiagnosticProviderAndFixer(Workspace workspace)
            {
                return new Tuple<DiagnosticAnalyzer, CodeFixProvider>(null, new GenerateConversionCodeFixProvider());
            }

            [WorkItem(774321)]
            [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
            public void TestGenerateImplicitConversionGenericClass()
            {
                Test(
    @"class Program { void Test ( int [ ] a ) { C < int > x1 = [|1|] ; } } class C < T > { } ",
    @"using System ; class Program { void Test ( int [ ] a ) { C < int > x1 = 1 ; } } class C < T > { public static implicit operator C < T > ( int v ) { throw new NotImplementedException ( ) ; } } ");
            }

            [WorkItem(774321)]
            [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
            public void TestGenerateImplicitConversionClass()
            {
                Test(
    @"class Program { void Test ( int [ ] a ) { C x1 = [|1|] ; } } class C { } ",
    @"using System ; class Program { void Test ( int [ ] a ) { C x1 = 1 ; } } class C { public static implicit operator C ( int v ) { throw new NotImplementedException ( ) ; } } ");
            }

            [WorkItem(774321)]
            [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
            public void TestGenerateImplicitConversionAwaitExpression()
            {
                Test(
    @"using System ; using System . Threading . Tasks ; class Program { async void Test ( ) { var a = Task . FromResult ( 1 ) ; Program x1 = [|await a|] ; } } ",
    @"using System ; using System . Threading . Tasks ; class Program { async void Test ( ) { var a = Task . FromResult ( 1 ) ; Program x1 = await a ; } public static implicit operator Program ( int v ) { throw new NotImplementedException ( ) ; } } ");
            }

            [WorkItem(774321)]
            [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
            public void TestGenerateImplicitConversionTargetTypeNotInSource()
            {
                Test(
    @"class Digit { public Digit ( double d ) { val = d ; } public double val ; } class Program { static void Main ( string [ ] args ) { Digit dig = new Digit ( 7 ) ; double num = [|dig|] ; } } ",
    @"using System ; class Digit { public Digit ( double d ) { val = d ; } public double val ; public static implicit operator double ( Digit v ) { throw new NotImplementedException ( ) ; } } class Program { static void Main ( string [ ] args ) { Digit dig = new Digit ( 7 ) ; double num = dig ; } } ");
            }

            [WorkItem(774321)]
            [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
            public void TestGenerateExplicitConversionGenericClass()
            {
                Test(
    @"class Program { void Test ( int [ ] a ) { C < int > x1 = [|( C < int > ) 1|] ; } } class C < T > { } ",
    @"using System ; class Program { void Test ( int [ ] a ) { C < int > x1 = ( C < int > ) 1 ; } } class C < T > { public static explicit operator C < T > ( int v ) { throw new NotImplementedException ( ) ; } } ");
            }

            [WorkItem(774321)]
            [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
            public void TestGenerateExplicitConversionClass()
            {
                Test(
    @"class Program { void Test ( int [ ] a ) { C x1 = [|( C ) 1|] ; } } class C { } ",
    @"using System ; class Program { void Test ( int [ ] a ) { C x1 = ( C ) 1 ; } } class C { public static explicit operator C ( int v ) { throw new NotImplementedException ( ) ; } } ");
            }

            [WorkItem(774321)]
            [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
            public void TestGenerateExplicitConversionAwaitExpression()
            {
                Test(
    @"using System ; using System . Threading . Tasks ; class Program { async void Test ( ) { var a = Task . FromResult ( 1 ) ; Program x1 = [|( Program ) await a|] ; } } ",
    @"using System ; using System . Threading . Tasks ; class Program { async void Test ( ) { var a = Task . FromResult ( 1 ) ; Program x1 = ( Program ) await a ; } public static explicit operator Program ( int v ) { throw new NotImplementedException ( ) ; } } ");
            }

            [WorkItem(774321)]
            [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
            public void TestGenerateExplicitConversionTargetTypeNotInSource()
            {
                Test(
    @"class Digit { public Digit ( double d ) { val = d ; } public double val ; } class Program { static void Main ( string [ ] args ) { Digit dig = new Digit ( 7 ) ; double num = [|( double ) dig|] ; } } ",
    @"using System ; class Digit { public Digit ( double d ) { val = d ; } public double val ; public static explicit operator double ( Digit v ) { throw new NotImplementedException ( ) ; } } class Program { static void Main ( string [ ] args ) { Digit dig = new Digit ( 7 ) ; double num = ( double ) dig ; } } ");
            }

            [WorkItem(774321)]
            [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateMethod)]
            public void TestEquivalenceKey()
            {
                TestEquivalenceKey(
    @"class C { void M() { this.[|M1|](System.Exception.M2()); } } ",
    string.Format(FeaturesResources.GenerateMethodIn, "C", "M1"));
            }
        }
    }
}
