namespace Locomotiv.Utils
{
    public static class StringExtension
    {
        public static bool Empty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool NotEmpty(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
