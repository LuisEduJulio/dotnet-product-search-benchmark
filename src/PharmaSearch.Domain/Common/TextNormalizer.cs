using System.Globalization;
using System.Text;

namespace PharmaSearch.Domain.Common;

/// <summary>
/// Regra de linguagem ubíqua: "ácido" e "acido" são o mesmo termo para quem pesquisa.
/// Remove acentos, padroniza caixa e espaços.
/// </summary>
public static class TextNormalizer
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        var lastWasSpace = false;

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace) builder.Append(' ');
                lastWasSpace = true;
                continue;
            }

            builder.Append(c);
            lastWasSpace = false;
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
