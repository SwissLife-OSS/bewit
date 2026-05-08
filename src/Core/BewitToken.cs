namespace Bewit;

/// <summary>
/// Opaque, URL-safe wrapper around a serialized and Base64-encoded <see cref="Bewit{T}"/>.
/// This is the value transmitted over the wire (in headers, query parameters, or URLs).
/// </summary>
/// <typeparam name="T">The payload type.</typeparam>
public sealed class BewitToken<T> : IEquatable<BewitToken<T>>
{
    private readonly string _value;

    public BewitToken(string value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool Equals(BewitToken<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(_value, other._value, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) =>
        obj is BewitToken<T> token && Equals(token);

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(_value);

    public static bool operator ==(BewitToken<T>? left, BewitToken<T>? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(BewitToken<T>? left, BewitToken<T>? right) =>
        !(left == right);

    public static explicit operator string(BewitToken<T> result) =>
        result._value;

    public override string ToString() => _value;
}
