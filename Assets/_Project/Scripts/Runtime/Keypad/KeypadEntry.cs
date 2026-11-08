using System;

namespace NightShift
{
    public class KeypadEntry
    {
        private readonly string code;

        public KeypadEntry(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("Code must not be empty", nameof(code));
            }

            foreach (char character in code)
            {
                if (character < '0' || character > '9')
                {
                    throw new ArgumentException($"Code must contain only digits: '{code}'", nameof(code));
                }
            }

            this.code = code;
        }

        public string Digits { get; private set; } = "";
        public bool IsUnlocked { get; private set; }

        public event Action Unlocked;
        public event Action Rejected;

        public void PressDigit(int digit)
        {
            if (digit < 0 || digit > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(digit), digit, "Digit must be between 0 and 9");
            }

            if (IsUnlocked || Digits.Length >= code.Length)
            {
                return;
            }

            Digits += (char)('0' + digit);
        }

        public void Erase()
        {
            if (IsUnlocked || Digits.Length == 0)
            {
                return;
            }

            Digits = Digits.Substring(0, Digits.Length - 1);
        }

        public void Confirm()
        {
            if (IsUnlocked)
            {
                return;
            }

            if (Digits == code)
            {
                IsUnlocked = true;
                Unlocked?.Invoke();
                return;
            }

            Digits = "";
            Rejected?.Invoke();
        }

        public void Reset()
        {
            Digits = "";
            IsUnlocked = false;
        }
    }
}
