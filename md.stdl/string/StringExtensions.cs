using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.String
{
    public struct EditInsert
    {
        public int Position;
        public int Length;
        public string InsertText;
    }

    public struct LineRange
    {
        public int Start;
        public int End;
        public int Length;
    }
    public static class StringExtensions
    {
        public static string[] SplitIgnoringBetween(this string input, string separator, string ignorebetween)
        {
            return input.Split(ignorebetween.ToCharArray())
                .Select((element, index) => index % 2 == 0  // If even index
                    ? element.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)  // Split the item
                    : new[] { ignorebetween + element + ignorebetween })  // Keep the entire item
                .SelectMany(element => element).ToArray();
        }

        public static string RemoveDiacritics(this string text)
        {
            return string.Concat(
                text.Normalize(NormalizationForm.FormD)
                    .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) !=
                                 UnicodeCategory.NonSpacingMark)
            ).Normalize(NormalizationForm.FormC);
        }

        public static LineRange LineRangeFromCharIndex(this string input, int charid)
        {
            int linestart = 0;
            int lineend = 0;
            while (true)
            {
                lineend = input.IndexOfAny(new[] { '\r', '\n' }, lineend) + 1;
                if (charid >= linestart && charid < lineend) break;
                linestart = lineend;
            }
            var linelength = lineend - linestart;
            //if (lineend < input.Length) linelength++;
            return new LineRange
            {
                End = lineend,
                Start = linestart,
                Length = linelength
            };
        }

        public static string MultiEdit(this string input, params EditInsert[] edits)
        {
            int offs = 0;
            string res = input;
            foreach (var edit in edits)
            {
                var diff = edit.InsertText.Length - edit.Length;
                var pos = edit.Position + offs;
                res = res.Remove(pos, edit.Length);
                res = res.Insert(pos, edit.InsertText);
                offs += diff;
            }
            return res;
        }

        public static string HashSha256_16(this string text)
        {
            var bytes = Encoding.Unicode.GetBytes(text);
            var hashstring = new SHA256Managed();
            var hash = hashstring.ComputeHash(bytes);
            return hash.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");
        }

        public static string HashSha256_64(this string text)
        {
            var bytes = Encoding.Unicode.GetBytes(text);
            var hashstring = new SHA256Managed();
            var hash = hashstring.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
