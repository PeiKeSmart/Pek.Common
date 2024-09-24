using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Pek;

public static class CoreExtension
{

    #region ByteArray

    public static string GetString([NotNull] this byte[] byteArray)
        => byteArray.GetString(Encoding.UTF8);

    public static string GetString([NotNull] this byte[] byteArray, Encoding encoding) => encoding.GetString(byteArray);
    #endregion
}
