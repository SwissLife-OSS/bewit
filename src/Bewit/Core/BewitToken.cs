using System;

namespace Bewit.Core
{
    public sealed class BewitToken<T>: IEquatable<BewitToken<T>>
    {
        private readonly string _value;

        public BewitToken(string value)
        {
            _value = value;
        }

        public bool Equals(BewitToken<T> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(
                _value, other._value, StringComparison.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is BewitToken<T> token)
            {
                return Equals(token);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _value != null
                ? StringComparer.InvariantCulture.GetHashCode(_value)
                : 0;
        }

        public static bool operator ==(BewitToken<T> left, BewitToken<T> right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(BewitToken<T> left, BewitToken<T> right)
        {
            return !(left == right);
        }

        public static explicit operator string(BewitToken<T> result)
        {
            return result._value;
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
