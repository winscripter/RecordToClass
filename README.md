# RecordToClass
Simple Roslyn utility to convert C# record types to classes.

# Purpose
I had one of the projects where I wanted to convert record types to class types, but didn't want to do that manually and Visual Studio's Quick Actions do not offer any way to convert records to classes. So, I spent about 2 hours working on a small utility to automate this process, and there, `RecordToClass` was born.

This simple utility uses [`Roslyn`](https://github.com/dotnet/roslyn) to analyze records and produce the output.

# Usage
When you launch the application, it will ask you to enter one line of a record type and will print the result right into the console.
However, this may not seem reliable for everyone, as we typically want to convert multiple records into multiple classes at once.

In that case, RecordToClass looks if there's a file named `input.txt`. If there isn't, it asks you to type one record. Otherwise, it reads every
record type from `input.txt` and produces the output.

# Example
Create a file named `inputs.txt` with this content:
```cs
public record A(int I, string S, object O);
public record B(int[] I, string S, float F);
public record C(float F, string S, double D);
```
Now, simply run `RecordToClass.exe` from a terminal, and see the program output this:
```
Parsing code...
Preparing compilation...
Preparing semantic analysis...
Transforming, please wait...
#pragma warning disable IDE0003
public class A
{
    public int I { get; init; }
    public string S { get; init; }
    public object O { get; init; }


    public A(int I, string S, object O)
    {
        this.I = I;
        this.S = S;
        this.O = O;
    }

    protected virtual Type EqualityContract {
        get {
            return typeof(A);
        }
    }

    public override string ToString() {
        var builder = new StringBuilder();
        builder.Append("A");
        builder.Append(" {");
        if (this.PrintMembers(builder)) {
            builder.Append(' ');
        }
        builder.Append('}');
        return builder.ToString();
    }

    protected virtual bool PrintMembers(StringBuilder builder) {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        builder.Append("I = ");
        builder.Append(I);
        builder.Append(", S = ");
        builder.Append(S);
        builder.Append(", O = ");
        builder.Append(O);
        return true;
    }

    public virtual bool Equals(A other) {
        return (object)this == other || (other is not null && this.EqualityContract == other.EqualityContract &&
            EqualityComparer<int>.Default.Equals(this.I, other.I) &&
            EqualityComparer<string>.Default.Equals(this.S, other.S) &&
            EqualityComparer<object>.Default.Equals(this.O, other.O));
    }

    public override bool Equals(object? other) {
        return Equals((other as A)!);
    }

    public override int GetHashCode() {
        var hashCode = new HashCode();
        hashCode.Add(this.I);
        hashCode.Add(this.S);
        hashCode.Add(this.O);
        return hashCode.ToHashCode();
    }

    protected A(A original) {
        this.I = original.I;
        this.S = original.S;
        this.O = original.O;
    }

    public void Deconstruct(out int I, out string S, out object O) {
        I = this.I;
        S = this.S;
        O = this.O;
    }

    public static bool operator ==(A left, A right) {
        return (object)left == right && left is not null && left.Equals(right);
    }

    public static bool operator !=(A left, A right) {
        return !(left == right);
    }
}
#pragma warning restore IDE0003
#pragma warning disable IDE0003
public class B
{
    public Int32[] I { get; init; }
    public string S { get; init; }
    public float F { get; init; }

    public B(Int32[] I, string S, float F)
    {
        this.I = I;
        this.S = S;
        this.F = F;
    }

    protected virtual Type EqualityContract {
        get {
            return typeof(B);
        }
    }

    public override string ToString() {
        var builder = new StringBuilder();
        builder.Append("B");
        builder.Append(" {");
        if (this.PrintMembers(builder)) {
            builder.Append(' ');
        }
        builder.Append('}');
        return builder.ToString();
    }

    protected virtual bool PrintMembers(StringBuilder builder) {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        builder.Append("I = ");
        builder.Append(I);
        builder.Append(", S = ");
        builder.Append(S);
        builder.Append(", F = ");
        builder.Append(F);
        return true;
    }

    public virtual bool Equals(B other) {
        return (object)this == other || (other is not null && this.EqualityContract == other.EqualityContract &&
            EqualityComparer<Int32[]>.Default.Equals(this.I, other.I) &&
            EqualityComparer<string>.Default.Equals(this.S, other.S) &&
            EqualityComparer<float>.Default.Equals(this.F, other.F));
    }

    public override bool Equals(object? other) {
        return Equals((other as B)!);
    }

    public override int GetHashCode() {
        var hashCode = new HashCode();
        hashCode.Add(this.I);
        hashCode.Add(this.S);
        hashCode.Add(this.F);
        return hashCode.ToHashCode();
    }

    protected B(B original) {
        this.I = original.I;
        this.S = original.S;
        this.F = original.F;
    }

    public void Deconstruct(out Int32[] I, out string S, out float F) {
        I = this.I;
        S = this.S;
        F = this.F;
    }

    public static bool operator ==(B left, B right) {
        return (object)left == right && left is not null && left.Equals(right);
    }

    public static bool operator !=(B left, B right) {
        return !(left == right);
    }
}
#pragma warning restore IDE0003
#pragma warning disable IDE0003
public class C
{
    public float F { get; init; }
    public string S { get; init; }
    public double D { get; init; }

    public C(float F, string S, double D)
    {
        this.F = F;
        this.S = S;
        this.D = D;
    }

    protected virtual Type EqualityContract {
        get {
            return typeof(C);
        }
    }

    public override string ToString() {
        var builder = new StringBuilder();
        builder.Append("C");
        builder.Append(" {");
        if (this.PrintMembers(builder)) {
            builder.Append(' ');
        }
        builder.Append('}');
        return builder.ToString();
    }

    protected virtual bool PrintMembers(StringBuilder builder) {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        builder.Append("F = ");
        builder.Append(F);
        builder.Append(", S = ");
        builder.Append(S);
        builder.Append(", D = ");
        builder.Append(D);
        return true;
    }

    public virtual bool Equals(C other) {
        return (object)this == other || (other is not null && this.EqualityContract == other.EqualityContract &&
            EqualityComparer<float>.Default.Equals(this.F, other.F) &&
            EqualityComparer<string>.Default.Equals(this.S, other.S) &&
            EqualityComparer<double>.Default.Equals(this.D, other.D));
    }

    public override bool Equals(object? other) {
        return Equals((other as C)!);
    }

    public override int GetHashCode() {
        var hashCode = new HashCode();
        hashCode.Add(this.F);
        hashCode.Add(this.S);
        hashCode.Add(this.D);
        return hashCode.ToHashCode();
    }

    protected C(C original) {
        this.F = original.F;
        this.S = original.S;
        this.D = original.D;
    }

    public void Deconstruct(out float F, out string S, out double D) {
        F = this.F;
        S = this.S;
        D = this.D;
    }

    public static bool operator ==(C left, C right) {
        return (object)left == right && left is not null && left.Equals(right);
    }

    public static bool operator !=(C left, C right) {
        return !(left == right);
    }
}
#pragma warning restore IDE0003
```

# Output
The output follows the .NET guidelines by suppressing the IDE0003 warning and making sure the result does not
need any refactoring. However, the result does use the K&amp;R curly bracket style, because I like it. But if you configured
Visual Studio for the Allman curly bracket style (which is the default), and paste the generated code, Visual Studio will automatically
convert the K&amp;R curly brace style to Allman for you.
