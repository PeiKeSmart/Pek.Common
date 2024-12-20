﻿using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Pek;

public static class CoreExtension
{
    #region Array

    /// <summary>
    ///     An Array extension method that clears the array.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    public static void ClearAll([NotNull] this Array @this) => Array.Clear(@this, 0, @this.Length);

    /// <summary>
    ///     Searches an entire one-dimensional sorted  for a specific element, using the  interface implemented by each
    ///     element of the  and by the specified object.
    /// </summary>
    /// <param name="array">The sorted one-dimensional  to search.</param>
    /// <param name="value">The object to search for.</param>
    /// <returns>
    ///     The index of the specified  in the specified , if  is found. If  is not found and  is less than one or more
    ///     elements in , a negative number which is the bitwise complement of the index of the first element that is
    ///     larger than . If  is not found and  is greater than any of the elements in , a negative number which is the
    ///     bitwise complement of (the index of the last element plus 1).
    /// </returns>
    public static Int32 BinarySearch([NotNull] this Array array, Object value) => Array.BinarySearch(array, value);

    /// <summary>
    ///     Searches a range of elements in a one-dimensional sorted  for a value, using the  interface implemented by
    ///     each element of the  and by the specified value.
    /// </summary>
    /// <param name="array">The sorted one-dimensional  to search.</param>
    /// <param name="index">The starting index of the range to search.</param>
    /// <param name="length">The length of the range to search.</param>
    /// <param name="value">The object to search for.</param>
    /// <returns>
    ///     The index of the specified  in the specified , if  is found. If  is not found and  is less than one or more
    ///     elements in , a negative number which is the bitwise complement of the index of the first element that is
    ///     larger than . If  is not found and  is greater than any of the elements in , a negative number which is the
    ///     bitwise complement of (the index of the last element plus 1).
    /// </returns>
    public static Int32 BinarySearch([NotNull] this Array array, Int32 index, Int32 length, Object value) => Array.BinarySearch(array, index, length, value);

    /// <summary>
    ///     Searches an entire one-dimensional sorted  for a value using the specified  interface.
    /// </summary>
    /// <param name="array">The sorted one-dimensional  to search.</param>
    /// <param name="value">The object to search for.</param>
    /// <param name="comparer">
    ///     The  implementation to use when comparing elements.-or- null to use the  implementation
    ///     of each element.
    /// </param>
    /// <returns>
    ///     The index of the specified  in the specified , if  is found. If  is not found and  is less than one or more
    ///     elements in , a negative number which is the bitwise complement of the index of the first element that is
    ///     larger than . If  is not found and  is greater than any of the elements in , a negative number which is the
    ///     bitwise complement of (the index of the last element plus 1).
    /// </returns>
    public static Int32 BinarySearch([NotNull] this Array array, Object value, IComparer comparer) => Array.BinarySearch(array, value, comparer);

    /// <summary>
    ///     Searches a range of elements in a one-dimensional sorted  for a value, using the specified  interface.
    /// </summary>
    /// <param name="array">The sorted one-dimensional  to search.</param>
    /// <param name="index">The starting index of the range to search.</param>
    /// <param name="length">The length of the range to search.</param>
    /// <param name="value">The object to search for.</param>
    /// <param name="comparer">
    ///     The  implementation to use when comparing elements.-or- null to use the  implementation
    ///     of each element.
    /// </param>
    /// <returns>
    ///     The index of the specified  in the specified , if  is found. If  is not found and  is less than one or more
    ///     elements in , a negative number which is the bitwise complement of the index of the first element that is
    ///     larger than . If  is not found and  is greater than any of the elements in , a negative number which is the
    ///     bitwise complement of (the index of the last element plus 1).
    /// </returns>
    public static Int32 BinarySearch([NotNull] this Array array, Int32 index, Int32 length, Object value, IComparer comparer) => Array.BinarySearch(array, index, length, value, comparer);

    /// <summary>
    ///     Sets a range of elements in the  to zero, to false, or to null, depending on the element type.
    /// </summary>
    /// <param name="array">The  whose elements need to be cleared.</param>
    /// <param name="index">The starting index of the range of elements to clear.</param>
    /// <param name="length">The number of elements to clear.</param>
    public static void Clear([NotNull] this Array array, Int32 index, Int32 length) => Array.Clear(array, index, length);

    /// <summary>
    ///     Copies a range of elements from an  starting at the first element and pastes them into another  starting at
    ///     the first element. The length is specified as a 32-bit integer.
    /// </summary>
    /// <param name="sourceArray">The  that contains the data to copy.</param>
    /// <param name="destinationArray">The  that receives the data.</param>
    /// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
    public static void Copy([NotNull] this Array sourceArray, Array destinationArray, Int32 length) => Array.Copy(sourceArray, destinationArray, length);

    /// <summary>
    ///     Copies a range of elements from an  starting at the specified source index and pastes them to another
    ///     starting at the specified destination index. The length and the indexes are specified as 32-bit integers.
    /// </summary>
    /// <param name="sourceArray">The  that contains the data to copy.</param>
    /// <param name="sourceIndex">A 32-bit integer that represents the index in the  at which copying begins.</param>
    /// <param name="destinationArray">The  that receives the data.</param>
    /// <param name="destinationIndex">A 32-bit integer that represents the index in the  at which storing begins.</param>
    /// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
    public static void Copy([NotNull] this Array sourceArray, Int32 sourceIndex, Array destinationArray, Int32 destinationIndex, Int32 length) => Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

    /// <summary>
    ///     Copies a range of elements from an  starting at the first element and pastes them into another  starting at
    ///     the first element. The length is specified as a 64-bit integer.
    /// </summary>
    /// <param name="sourceArray">The  that contains the data to copy.</param>
    /// <param name="destinationArray">The  that receives the data.</param>
    /// <param name="length">
    ///     A 64-bit integer that represents the number of elements to copy. The integer must be between
    ///     zero and , inclusive.
    /// </param>
    public static void Copy([NotNull] this Array sourceArray, Array destinationArray, Int64 length) => Array.Copy(sourceArray, destinationArray, length);

    /// <summary>
    ///     Copies a range of elements from an  starting at the specified source index and pastes them to another
    ///     starting at the specified destination index. The length and the indexes are specified as 64-bit integers.
    /// </summary>
    /// <param name="sourceArray">The  that contains the data to copy.</param>
    /// <param name="sourceIndex">A 64-bit integer that represents the index in the  at which copying begins.</param>
    /// <param name="destinationArray">The  that receives the data.</param>
    /// <param name="destinationIndex">A 64-bit integer that represents the index in the  at which storing begins.</param>
    /// <param name="length">
    ///     A 64-bit integer that represents the number of elements to copy. The integer must be between
    ///     zero and , inclusive.
    /// </param>
    public static void Copy([NotNull] this Array sourceArray, Int64 sourceIndex, Array destinationArray, Int64 destinationIndex, Int64 length) => Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

    /// <summary>
    ///     Searches for the specified object and returns the index of the first occurrence within the entire one-
    ///     dimensional .
    /// </summary>
    /// <param name="array">The one-dimensional  to search.</param>
    /// <param name="value">The object to locate in .</param>
    /// <returns>
    ///     The index of the first occurrence of  within the entire , if found; otherwise, the lower bound of the array
    ///     minus 1.
    /// </returns>
    public static Int32 IndexOf([NotNull] this Array array, Object value) => Array.IndexOf(array, value);

    /// <summary>
    ///     Searches for the specified object and returns the index of the first occurrence within the range of elements
    ///     in the one-dimensional  that extends from the specified index to the last element.
    /// </summary>
    /// <param name="array">The one-dimensional  to search.</param>
    /// <param name="value">The object to locate in .</param>
    /// <param name="startIndex">The starting index of the search. 0 (zero) is valid in an empty array.</param>
    /// <returns>
    ///     The index of the first occurrence of  within the range of elements in  that extends from  to the last element,
    ///     if found; otherwise, the lower bound of the array minus 1.
    /// </returns>
    public static Int32 IndexOf([NotNull] this Array array, Object value, Int32 startIndex) => Array.IndexOf(array, value, startIndex);

    /// <summary>
    ///     Searches for the specified object and returns the index of the first occurrence within the range of elements
    ///     in the one-dimensional  that starts at the specified index and contains the specified number of elements.
    /// </summary>
    /// <param name="array">The one-dimensional  to search.</param>
    /// <param name="value">The object to locate in .</param>
    /// <param name="startIndex">The starting index of the search. 0 (zero) is valid in an empty array.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <returns>
    ///     The index of the first occurrence of  within the range of elements in  that starts at  and contains the
    ///     number of elements specified in , if found; otherwise, the lower bound of the array minus 1.
    /// </returns>
    public static Int32 IndexOf([NotNull] this Array array, Object value, Int32 startIndex, Int32 count) => Array.IndexOf(array, value, startIndex, count);

    /// <summary>
    ///     Searches for the specified object and returns the index of the last occurrence within the entire one-
    ///     dimensional .
    /// </summary>
    /// <param name="array">The one-dimensional  to search.</param>
    /// <param name="value">The object to locate in .</param>
    /// <returns>
    ///     The index of the last occurrence of  within the entire , if found; otherwise, the lower bound of the array
    ///     minus 1.
    /// </returns>
    public static Int32 LastIndexOf([NotNull] this Array array, Object value) => Array.LastIndexOf(array, value);

    /// <summary>
    ///     Searches for the specified object and returns the index of the last occurrence within the range of elements
    ///     in the one-dimensional  that extends from the first element to the specified index.
    /// </summary>
    /// <param name="array">The one-dimensional  to search.</param>
    /// <param name="value">The object to locate in .</param>
    /// <param name="startIndex">The starting index of the backward search.</param>
    /// <returns>
    ///     The index of the last occurrence of  within the range of elements in  that extends from the first element to ,
    ///     if found; otherwise, the lower bound of the array minus 1.
    /// </returns>
    public static Int32 LastIndexOf([NotNull] this Array array, Object value, Int32 startIndex) => Array.LastIndexOf(array, value, startIndex);

    /// <summary>
    ///     Searches for the specified object and returns the index of the last occurrence within the range of elements
    ///     in the one-dimensional  that contains the specified number of elements and ends at the specified index.
    /// </summary>
    /// <param name="array">The one-dimensional  to search.</param>
    /// <param name="value">The object to locate in .</param>
    /// <param name="startIndex">The starting index of the backward search.</param>
    /// <param name="count">The number of elements in the section to search.</param>
    /// <returns>
    ///     The index of the last occurrence of  within the range of elements in  that contains the number of elements
    ///     specified in  and ends at , if found; otherwise, the lower bound of the array minus 1.
    /// </returns>
    public static Int32 LastIndexOf([NotNull] this Array array, Object value, Int32 startIndex, Int32 count) => Array.LastIndexOf(array, value, startIndex, count);

    /// <summary>
    ///     Sorts the elements in an entire one-dimensional  using the  implementation of each element of the .
    /// </summary>
    /// <param name="array">The one-dimensional  to sort.</param>
    public static void Sort([NotNull] this Array array) => Array.Sort(array);

    /// <summary>
    ///     Sorts a pair of one-dimensional  objects (one contains the keys and the other contains the corresponding
    ///     items) based on the keys in the first  using the  implementation of each key.
    /// </summary>
    /// <param name="array">The one-dimensional  to sort.</param>
    /// <param name="items">
    ///     The one-dimensional  that contains the items that correspond to each of the keys in the .-or-
    ///     null to sort only the .
    /// </param>
    public static void Sort([NotNull] this Array array, Array items) => Array.Sort(array, items);

    /// <summary>
    ///     Sorts the elements in a range of elements in a one-dimensional  using the  implementation of each element of
    ///     the .
    /// </summary>
    /// <param name="array">The one-dimensional  to sort.</param>
    /// <param name="index">The starting index of the range to sort.</param>
    /// <param name="length">The number of elements in the range to sort.</param>
    public static void Sort([NotNull] this Array array, Int32 index, Int32 length) => Array.Sort(array, index, length);

    /// <summary>
    ///     Sorts a range of elements in a pair of one-dimensional  objects (one contains the keys and the other contains
    ///     the corresponding items) based on the keys in the first  using the  implementation of each key.
    /// </summary>
    /// <param name="array">The one-dimensional  to sort.</param>
    /// <param name="items">
    ///     The one-dimensional  that contains the items that correspond to each of the keys in the .-or-
    ///     null to sort only the .
    /// </param>
    /// <param name="index">The starting index of the range to sort.</param>
    /// <param name="length">The number of elements in the range to sort.</param>
    public static void Sort([NotNull] this Array array, Array items, Int32 index, Int32 length) => Array.Sort(array, items, index, length);

    /// <summary>
    ///     Sorts the elements in a one-dimensional  using the specified .
    /// </summary>
    /// <param name="array">The one-dimensional  to sort.</param>
    /// <param name="comparer">
    ///     The  implementation to use when comparing elements.-or-null to use the  implementation of
    ///     each element.
    /// </param>
    public static void Sort([NotNull] this Array array, IComparer comparer) => Array.Sort(array, comparer);

    /// <summary>
    ///     Sorts a pair of one-dimensional  objects (one contains the keys and the other contains the corresponding
    ///     items) based on the keys in the first  using the specified .
    /// </summary>
    /// <param name="array">The one-dimensional  to sort.</param>
    /// <param name="items">
    ///     The one-dimensional  that contains the items that correspond to each of the keys in the .-or-
    ///     null to sort only the .
    /// </param>
    /// <param name="comparer">
    ///     The  implementation to use when comparing elements.-or-null to use the  implementation of
    ///     each element.
    /// </param>
    public static void Sort([NotNull] this Array array, Array items, IComparer comparer) => Array.Sort(array, items, comparer);

    /// <summary>
    ///     Sorts the elements in a range of elements in a one-dimensional  using the specified .
    /// </summary>
    /// <param name="array">The one-dimensional  to sort.</param>
    /// <param name="index">The starting index of the range to sort.</param>
    /// <param name="length">The number of elements in the range to sort.</param>
    /// <param name="comparer">
    ///     The  implementation to use when comparing elements.-or-null to use the  implementation of
    ///     each element.
    /// </param>
    public static void Sort([NotNull] this Array array, Int32 index, Int32 length, IComparer comparer) => Array.Sort(array, index, length, comparer);

    /// <summary>
    ///     Sorts a range of elements in a pair of one-dimensional  objects (one contains the keys and the other contains
    ///     the corresponding items) based on the keys in the first  using the specified .
    /// </summary>
    /// <param name="array">The one-dimensional  to sort.</param>
    /// <param name="items">
    ///     The one-dimensional  that contains the items that correspond to each of the keys in the .-or-
    ///     null to sort only the .
    /// </param>
    /// <param name="index">The starting index of the range to sort.</param>
    /// <param name="length">The number of elements in the range to sort.</param>
    /// <param name="comparer">
    ///     The  implementation to use when comparing elements.-or-null to use the  implementation of
    ///     each element.
    /// </param>
    public static void Sort([NotNull] this Array array, Array items, Int32 index, Int32 length, IComparer comparer) => Array.Sort(array, items, index, length, comparer);

    /// <summary>
    ///     Copies a specified number of bytes from a source array starting at a particular offset to a destination array
    ///     starting at a particular offset.
    /// </summary>
    /// <param name="src">The source buffer.</param>
    /// <param name="srcOffset">The zero-based byte offset into .</param>
    /// <param name="dst">The destination buffer.</param>
    /// <param name="dstOffset">The zero-based byte offset into .</param>
    /// <param name="count">The number of bytes to copy.</param>
    public static void BlockCopy([NotNull] this Array src, Int32 srcOffset, Array dst, Int32 dstOffset, Int32 count) => Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);

    #endregion Array

    #region Boolean

    /// <summary>
    ///     A bool extension method that execute an Action if the value is true.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="action">The action to execute.</param>
    public static void IfTrue(this Boolean @this, Action action)
    {
        if (@this)
        {
            action();
        }
    }

    /// <summary>
    ///     A bool extension method that execute an Action if the value is false.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="action">The action to execute.</param>
    public static void IfFalse(this Boolean @this, Action action)
    {
        if (!@this)
        {
            action();
        }
    }

    /// <summary>
    ///     A bool extension method that show the trueValue when the @this value is true; otherwise show the falseValue.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="trueValue">The true value to be returned if the @this value is true.</param>
    /// <param name="falseValue">The false value to be returned if the @this value is false.</param>
    /// <returns>A string that represents of the current boolean value.</returns>
    public static String ToString(this Boolean @this, String trueValue, String falseValue) => @this ? trueValue : falseValue;

    #endregion Boolean

    #region Byte

    /// <summary>
    ///     Returns the larger of two 8-bit unsigned integers.
    /// </summary>
    /// <param name="val1">The first of two 8-bit unsigned integers to compare.</param>
    /// <param name="val2">The second of two 8-bit unsigned integers to compare.</param>
    /// <returns>Parameter  or , whichever is larger.</returns>
    public static Byte Max(this Byte val1, Byte val2) => Math.Max(val1, val2);

    /// <summary>
    ///     Returns the smaller of two 8-bit unsigned integers.
    /// </summary>
    /// <param name="val1">The first of two 8-bit unsigned integers to compare.</param>
    /// <param name="val2">The second of two 8-bit unsigned integers to compare.</param>
    /// <returns>Parameter  or , whichever is smaller.</returns>
    public static Byte Min(this Byte val1, Byte val2) => Math.Min(val1, val2);

    #endregion Byte

    #region ByteArray

    /// <summary>
    ///     Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with
    ///     base-64 digits.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <returns>The string representation, in base 64, of the contents of .</returns>
    public static String ToBase64String([NotNull] this Byte[] inArray) => Convert.ToBase64String(inArray);

    /// <summary>
    ///     Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with
    ///     base-64 digits. A parameter specifies whether to insert line breaks in the return value.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <param name="options">to insert a line break every 76 characters, or  to not insert line breaks.</param>
    /// <returns>The string representation in base 64 of the elements in .</returns>
    public static String ToBase64String([NotNull] this Byte[] inArray, Base64FormattingOptions options) => Convert.ToBase64String(inArray, options);

    /// <summary>
    ///     Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is
    ///     encoded with base-64 digits. Parameters specify the subset as an offset in the input array, and the number of
    ///     elements in the array to convert.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <param name="offset">An offset in .</param>
    /// <param name="length">The number of elements of  to convert.</param>
    /// <returns>The string representation in base 64 of  elements of , starting at position .</returns>
    public static String ToBase64String([NotNull] this Byte[] inArray, Int32 offset, Int32 length) => Convert.ToBase64String(inArray, offset, length);

    /// <summary>
    ///     Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is
    ///     encoded with base-64 digits. Parameters specify the subset as an offset in the input array, the number of
    ///     elements in the array to convert, and whether to insert line breaks in the return value.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <param name="offset">An offset in .</param>
    /// <param name="length">The number of elements of  to convert.</param>
    /// <param name="options">to insert a line break every 76 characters, or  to not insert line breaks.</param>
    /// <returns>The string representation in base 64 of  elements of , starting at position .</returns>
    public static String ToBase64String([NotNull] this Byte[] inArray, Int32 offset, Int32 length, Base64FormattingOptions options) => Convert.ToBase64String(inArray, offset, length, options);

    /// <summary>
    ///     A byte[] extension method that resizes the byte[].
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="newSize">Size of the new.</param>
    /// <returns>A byte[].</returns>
    public static Byte[] Resize([NotNull] this Byte[] @this, Int32 newSize)
    {
        Array.Resize(ref @this, newSize);
        return @this;
    }

    /// <summary>
    ///     A byte[] extension method that converts the @this byteArray to a memory stream.
    /// </summary>
    /// <param name="byteArray">The byetArray to act on</param>
    /// <returns>@this as a MemoryStream.</returns>
    public static MemoryStream ToMemoryStream([NotNull] this Byte[] byteArray) => new(byteArray);

    public static String GetString([NotNull] this Byte[] byteArray)
        => byteArray.GetString(Encoding.UTF8);

    public static String GetString([NotNull] this Byte[] byteArray, Encoding encoding) => encoding.GetString(byteArray);

    #endregion ByteArray

    #region Char

    /// <summary>
    ///     A char extension method that repeats a character the specified number of times.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="repeatCount">Number of repeats.</param>
    /// <returns>The repeated char.</returns>
    public static String Repeat(this Char @this, Int32 repeatCount) => new(@this, repeatCount);

    /// <summary>
    ///     Converts the specified numeric Unicode character to a double-precision floating point number.
    /// </summary>
    /// <param name="c">The Unicode character to convert.</param>
    /// <returns>The numeric value of  if that character represents a number; otherwise, -1.0.</returns>
    public static Double GetNumericValue(this Char c) => Char.GetNumericValue(c);

    /// <summary>
    ///     Categorizes a specified Unicode character into a group identified by one of the  values.
    /// </summary>
    /// <param name="c">The Unicode character to categorize.</param>
    /// <returns>A  value that identifies the group that contains .</returns>
    public static UnicodeCategory GetUnicodeCategory(this Char c) => Char.GetUnicodeCategory(c);

    /// <summary>
    ///     Indicates whether the specified Unicode character is categorized as a control character.
    /// </summary>
    /// <param name="c">The Unicode character to evaluate.</param>
    /// <returns>true if  is a control character; otherwise, false.</returns>
    public static Boolean IsControl(this Char c) => Char.IsControl(c);

    /// <summary>
    ///     Indicates whether the specified Unicode character is categorized as a letter or a decimal digit.
    /// </summary>
    /// <param name="c">The Unicode character to evaluate.</param>
    /// <returns>true if  is a letter or a decimal digit; otherwise, false.</returns>
    public static Boolean IsLetterOrDigit(this Char c) => Char.IsLetterOrDigit(c);

    /// <summary>
    ///     Indicates whether the specified Unicode character is categorized as a lowercase letter.
    /// </summary>
    /// <param name="c">The Unicode character to evaluate.</param>
    /// <returns>true if  is a lowercase letter; otherwise, false.</returns>
    public static Boolean IsLower(this Char c) => Char.IsLower(c);

    /// <summary>
    ///     Indicates whether the specified Unicode character is categorized as an uppercase letter.
    /// </summary>
    /// <param name="c">The Unicode character to evaluate.</param>
    /// <returns>true if  is an uppercase letter; otherwise, false.</returns>
    public static Boolean IsUpper(this Char c) => Char.IsUpper(c);

    /// <summary>
    ///     Indicates whether the specified Unicode character is categorized as a number.
    /// </summary>
    /// <param name="c">The Unicode character to evaluate.</param>
    /// <returns>true if  is a number; otherwise, false.</returns>
    public static Boolean IsNumber(this Char c) => Char.IsNumber(c);

    /// <summary>
    ///     Indicates whether the specified Unicode character is categorized as a separator character.
    /// </summary>
    /// <param name="c">The Unicode character to evaluate.</param>
    /// <returns>true if  is a separator character; otherwise, false.</returns>
    public static Boolean IsSeparator(this Char c) => Char.IsSeparator(c);

    /// <summary>
    ///     Indicates whether the specified Unicode character is categorized as a symbol character.
    /// </summary>
    /// <param name="c">The Unicode character to evaluate.</param>
    /// <returns>true if  is a symbol character; otherwise, false.</returns>
    public static Boolean IsSymbol(this Char c) => Char.IsSymbol(c);

    /// <summary>
    ///     Indicates whether the specified Unicode character is categorized as white space.
    /// </summary>
    /// <param name="c">The Unicode character to evaluate.</param>
    /// <returns>true if  is white space; otherwise, false.</returns>
    public static Boolean IsWhiteSpace(this Char c) => Char.IsWhiteSpace(c);

    /// <summary>
    ///     Converts the value of a specified Unicode character to its lowercase equivalent using specified culture-
    ///     specific formatting information.
    /// </summary>
    /// <param name="c">The Unicode character to convert.</param>
    /// <param name="culture">An object that supplies culture-specific casing rules.</param>
    /// <returns>
    ///     The lowercase equivalent of , modified according to , or the unchanged value of , if  is already lowercase or
    ///     not alphabetic.
    /// </returns>
    public static Char ToLower(this Char c, CultureInfo culture) => Char.ToLower(c, culture);

    /// <summary>
    ///     Converts the value of a Unicode character to its lowercase equivalent.
    /// </summary>
    /// <param name="c">The Unicode character to convert.</param>
    /// <returns>
    ///     The lowercase equivalent of , or the unchanged value of , if  is already lowercase or not alphabetic.
    /// </returns>
    public static Char ToLower(this Char c) => Char.ToLower(c);

    /// <summary>
    ///     Converts the value of a Unicode character to its lowercase equivalent using the casing rules of the invariant
    ///     culture.
    /// </summary>
    /// <param name="c">The Unicode character to convert.</param>
    /// <returns>
    ///     The lowercase equivalent of the  parameter, or the unchanged value of , if  is already lowercase or not
    ///     alphabetic.
    /// </returns>
    public static Char ToLowerInvariant(this Char c) => Char.ToLowerInvariant(c);

    /// <summary>
    ///     Converts the value of a specified Unicode character to its uppercase equivalent using specified culture-
    ///     specific formatting information.
    /// </summary>
    /// <param name="c">The Unicode character to convert.</param>
    /// <param name="culture">An object that supplies culture-specific casing rules.</param>
    /// <returns>
    ///     The uppercase equivalent of , modified according to , or the unchanged value of  if  is already uppercase,
    ///     has no uppercase equivalent, or is not alphabetic.
    /// </returns>
    public static Char ToUpper(this Char c, CultureInfo culture) => Char.ToUpper(c, culture);

    /// <summary>
    ///     Converts the value of a Unicode character to its uppercase equivalent.
    /// </summary>
    /// <param name="c">The Unicode character to convert.</param>
    /// <returns>
    ///     The uppercase equivalent of , or the unchanged value of  if  is already uppercase, has no uppercase
    ///     equivalent, or is not alphabetic.
    /// </returns>
    public static Char ToUpper(this Char c) => Char.ToUpper(c);

    /// <summary>
    ///     Converts the value of a Unicode character to its uppercase equivalent using the casing rules of the invariant
    ///     culture.
    /// </summary>
    /// <param name="c">The Unicode character to convert.</param>
    /// <returns>
    ///     The uppercase equivalent of the  parameter, or the unchanged value of , if  is already uppercase or not
    ///     alphabetic.
    /// </returns>
    public static Char ToUpperInvariant(this Char c) => Char.ToUpperInvariant(c);

    #endregion Char

    #region DateTime

    /// <summary>
    ///     A DateTime extension method that ages the given this.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>An int.</returns>
    public static Int32 Age(this DateTime @this)
    {
        if (DateTime.Today.Month < @this.Month ||
            DateTime.Today.Month == @this.Month &&
            DateTime.Today.Day < @this.Day)
        {
            return DateTime.Today.Year - @this.Year - 1;
        }
        return DateTime.Today.Year - @this.Year;
    }

    /// <summary>
    ///     A DateTime extension method that query if 'date' is date equal.
    /// </summary>
    /// <param name="date">The date to act on.</param>
    /// <param name="dateToCompare">Date/Time of the date to compare.</param>
    /// <returns>true if date equal, false if not.</returns>
    public static Boolean IsDateEqual(this DateTime date, DateTime dateToCompare) => date.Date == dateToCompare.Date;

    /// <summary>
    ///     A DateTime extension method that query if '@this' is today.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if today, false if not.</returns>
    public static Boolean IsToday(this DateTime @this) => @this.Date == DateTime.Today;

    /// <summary>
    ///     A DateTime extension method that query if '@this' is a week day.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if '@this' is a week day, false if not.</returns>
    public static Boolean IsWeekDay(this DateTime @this) => !(@this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday);

    /// <summary>
    ///     A DateTime extension method that query if '@this' is a week day.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if '@this' is a week day, false if not.</returns>
    public static Boolean IsWeekendDay(this DateTime @this) => @this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday;

    /// <summary>
    ///     A DateTime extension method that return a DateTime with the time set to "00:00:00:000". The first moment of
    ///     the day.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A DateTime of the day with the time set to "00:00:00:000".</returns>
    public static DateTime StartOfDay(this DateTime @this) => new(@this.Year, @this.Month, @this.Day);

    /// <summary>
    ///     A DateTime extension method that return a DateTime of the first day of the month with the time set to
    ///     "00:00:00:000". The first moment of the first day of the month.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A DateTime of the first day of the month with the time set to "00:00:00:000".</returns>
    public static DateTime StartOfMonth(this DateTime @this) => new(@this.Year, @this.Month, 1);

    /// <summary>
    ///     A DateTime extension method that starts of week.
    /// </summary>
    /// <param name="dt">The dt to act on.</param>
    /// <param name="startDayOfWeek">(Optional) the start day of week.</param>
    /// <returns>A DateTime.</returns>
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startDayOfWeek = DayOfWeek.Sunday)
    {
        var start = new DateTime(dt.Year, dt.Month, dt.Day);

        if (start.DayOfWeek != startDayOfWeek)
        {
            var d = startDayOfWeek - start.DayOfWeek;
            if (startDayOfWeek <= start.DayOfWeek)
            {
                return start.AddDays(d);
            }
            return start.AddDays(-7 + d);
        }

        return start;
    }

    /// <summary>
    ///     A DateTime extension method that return a DateTime of the first day of the year with the time set to
    ///     "00:00:00:000". The first moment of the first day of the year.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A DateTime of the first day of the year with the time set to "00:00:00:000".</returns>
    public static DateTime StartOfYear(this DateTime @this) => new(@this.Year, 1, 1);

    /// <summary>
    ///     A DateTime extension method that converts the @this to an epoch time span.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as a TimeSpan.</returns>
    public static TimeSpan ToEpochTimeSpan(this DateTime @this) => @this.ToUniversalTime().Subtract(new DateTime(1970, 1, 1));

    /// <summary>
    ///     A T extension method that check if the value is between inclusively the minValue and maxValue.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>true if the value is between inclusively the minValue and maxValue, otherwise false.</returns>
    public static Boolean InRange(this DateTime @this, DateTime minValue, DateTime maxValue) => @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;

    /// <summary>
    ///     Converts a time to the time in a particular time zone.
    /// </summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <param name="destinationTimeZone">The time zone to convert  to.</param>
    /// <returns>The date and time in the destination time zone.</returns>
    public static DateTime ConvertTime(this DateTime dateTime, TimeZoneInfo destinationTimeZone) => TimeZoneInfo.ConvertTime(dateTime, destinationTimeZone);

    /// <summary>
    ///     Converts a time from one time zone to another.
    /// </summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <param name="sourceTimeZone">The time zone of .</param>
    /// <param name="destinationTimeZone">The time zone to convert  to.</param>
    /// <returns>
    ///     The date and time in the destination time zone that corresponds to the  parameter in the source time zone.
    /// </returns>
    public static DateTime ConvertTime(this DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone) => TimeZoneInfo.ConvertTime(dateTime, sourceTimeZone, destinationTimeZone);

    /// <summary>
    ///     Converts a Coordinated Universal Time (UTC) to the time in a specified time zone.
    /// </summary>
    /// <param name="dateTime">The Coordinated Universal Time (UTC).</param>
    /// <param name="destinationTimeZone">The time zone to convert  to.</param>
    /// <returns>
    ///     The date and time in the destination time zone. Its  property is  if  is ; otherwise, its  property is .
    /// </returns>
    public static DateTime ConvertTimeFromUtc(this DateTime dateTime, TimeZoneInfo destinationTimeZone) => TimeZoneInfo.ConvertTimeFromUtc(dateTime, destinationTimeZone);

    /// <summary>
    ///     Converts the current date and time to Coordinated Universal Time (UTC).
    /// </summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <returns>
    ///     The Coordinated Universal Time (UTC) that corresponds to the  parameter. The  value&#39;s  property is always
    ///     set to .
    /// </returns>
    public static DateTime ConvertTimeToUtc(this DateTime dateTime) => TimeZoneInfo.ConvertTimeToUtc(dateTime);

    /// <summary>
    ///     Converts the time in a specified time zone to Coordinated Universal Time (UTC).
    /// </summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <param name="sourceTimeZone">The time zone of .</param>
    /// <returns>
    ///     The Coordinated Universal Time (UTC) that corresponds to the  parameter. The  object&#39;s  property is
    ///     always set to .
    /// </returns>
    public static DateTime ConvertTimeToUtc(this DateTime dateTime, TimeZoneInfo sourceTimeZone) => TimeZoneInfo.ConvertTimeToUtc(dateTime, sourceTimeZone);

    /// <summary>
    /// ToDateString("yyyy-MM-dd")
    /// </summary>
    /// <param name="this">dateTime</param>
    /// <returns></returns>
    public static String ToStandardDateString(this DateTime @this) => @this.ToString("yyyy-MM-dd");

    /// <summary>
    /// ToTimeString("yyyy-MM-dd HH:mm:ss")
    /// </summary>
    /// <param name="this">datetime</param>
    /// <returns></returns>
    public static String ToStandardTimeString(this DateTime @this) => @this.ToString("yyyy-MM-dd HH:mm:ss");

    #endregion DateTime

    #region Decimal

    /// <summary>
    ///     A T extension method that check if the value is between inclusively the minValue and maxValue.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>true if the value is between inclusively the minValue and maxValue, otherwise false.</returns>
    public static Boolean InRange(this Decimal @this, Decimal minValue, Decimal maxValue) => @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;

    /// <summary>
    ///     Returns the smallest integral value that is greater than or equal to the specified decimal number.
    /// </summary>
    /// <param name="d">A decimal number.</param>
    /// <returns>
    ///     The smallest integral value that is greater than or equal to . Note that this method returns a  instead of an
    ///     integral type.
    /// </returns>
    public static Decimal Ceiling(this Decimal d) => Math.Ceiling(d);

    /// <summary>
    ///     Returns the largest integer less than or equal to the specified decimal number.
    /// </summary>
    /// <param name="d">A decimal number.</param>
    /// <returns>
    ///     The largest integer less than or equal to .  Note that the method returns an integral value of type .
    /// </returns>
    public static Decimal Floor(this Decimal d) => Math.Floor(d);

    /// <summary>
    ///     Returns the larger of two decimal numbers.
    /// </summary>
    /// <param name="val1">The first of two decimal numbers to compare.</param>
    /// <param name="val2">The second of two decimal numbers to compare.</param>
    /// <returns>Parameter  or , whichever is larger.</returns>
    public static Decimal Max(this Decimal val1, Decimal val2) => Math.Max(val1, val2);

    /// <summary>
    ///     Returns the smaller of two decimal numbers.
    /// </summary>
    /// <param name="val1">The first of two decimal numbers to compare.</param>
    /// <param name="val2">The second of two decimal numbers to compare.</param>
    /// <returns>Parameter  or , whichever is smaller.</returns>
    public static Decimal Min(this Decimal val1, Decimal val2) => Math.Min(val1, val2);

    /// <summary>
    ///     Rounds a decimal value to the nearest integral value.
    /// </summary>
    /// <param name="d">A decimal number to be rounded.</param>
    /// <returns>
    ///     The integer nearest parameter . If the fractional component of  is halfway between two integers, one of which
    ///     is even and the other odd, the even number is returned. Note that this method returns a  instead of an
    ///     integral type.
    /// </returns>
    public static Decimal Round(this Decimal d) => Math.Round(d);

    /// <summary>
    ///     Rounds a decimal value to a specified number of fractional digits.
    /// </summary>
    /// <param name="d">A decimal number to be rounded.</param>
    /// <param name="decimals">The number of decimal places in the return value.</param>
    /// <returns>The number nearest to  that contains a number of fractional digits equal to .</returns>
    public static Decimal Round(this Decimal d, Int32 decimals) => Math.Round(d, decimals);

    /// <summary>
    ///     Rounds a decimal value to the nearest integer. A parameter specifies how to round the value if it is midway
    ///     between two numbers.
    /// </summary>
    /// <param name="d">A decimal number to be rounded.</param>
    /// <param name="mode">Specification for how to round  if it is midway between two other numbers.</param>
    /// <returns>
    ///     The integer nearest . If  is halfway between two numbers, one of which is even and the other odd, then
    ///     determines which of the two is returned.
    /// </returns>
    public static Decimal Round(this Decimal d, MidpointRounding mode) => Math.Round(d, mode);

    /// <summary>
    ///     Rounds a decimal value to a specified number of fractional digits. A parameter specifies how to round the
    ///     value if it is midway between two numbers.
    /// </summary>
    /// <param name="d">A decimal number to be rounded.</param>
    /// <param name="decimals">The number of decimal places in the return value.</param>
    /// <param name="mode">Specification for how to round  if it is midway between two other numbers.</param>
    /// <returns>
    ///     The number nearest to  that contains a number of fractional digits equal to . If  has fewer fractional digits
    ///     than ,  is returned unchanged.
    /// </returns>
    public static Decimal Round(this Decimal d, Int32 decimals, MidpointRounding mode) => Math.Round(d, decimals, mode);

    /// <summary>
    ///     Returns a value indicating the sign of a decimal number.
    /// </summary>
    /// <param name="value">A signed decimal number.</param>
    /// <returns>
    ///     A number that indicates the sign of , as shown in the following table.Return value Meaning -1  is less than
    ///     zero. 0  is equal to zero. 1  is greater than zero.
    /// </returns>
    public static Int32 Sign(this Decimal value) => Math.Sign(value);

    /// <summary>
    ///     Calculates the integral part of a specified decimal number.
    /// </summary>
    /// <param name="d">A number to truncate.</param>
    /// <returns>
    ///     The integral part of ; that is, the number that remains after any fractional digits have been discarded.
    /// </returns>
    public static Decimal Truncate(this Decimal d) => Math.Truncate(d);

    /// <summary>
    ///     A Decimal extension method that converts the @this to a money.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as a Decimal.</returns>
    public static Decimal ToMoney(this Decimal @this) => Math.Round(@this, 2);

    #endregion Decimal

    #region Delegate

    /// <summary>
    /// Concatenates the invocation lists of two delegates.
    /// </summary>
    /// <param name="a">The delegate whose invocation list comes first.</param>
    /// <param name="b">The delegate whose invocation list comes last.</param>
    /// <returns>
    ///     A new delegate with an invocation list that concatenates the invocation lists of  and  in that order. Returns
    ///     if  is null, returns  if  is a null reference, and returns a null reference if both  and  are null references.
    /// </returns>
    public static Delegate Combine([NotNull] this Delegate a, Delegate b) => Delegate.Combine(a, b);

    /// <summary>
    /// Removes the last occurrence of the invocation list of a delegate from the invocation list of another delegate.
    /// </summary>
    /// <param name="source">The delegate from which to remove the invocation list of .</param>
    /// <param name="value">The delegate that supplies the invocation list to remove from the invocation list of .</param>
    /// <returns>
    ///     A new delegate with an invocation list formed by taking the invocation list of  and removing the last
    ///     occurrence of the invocation list of , if the invocation list of  is found within the invocation list of .
    ///     Returns  if  is null or if the invocation list of  is not found within the invocation list of . Returns a
    ///     null reference if the invocation list of  is equal to the invocation list of  or if  is a null reference.
    /// </returns>
    public static Delegate? Remove([NotNull] this Delegate source, Delegate value) => Delegate.Remove(source, value);

    /// <summary>
    /// Removes all occurrences of the invocation list of a delegate from the invocation list of another delegate.
    /// </summary>
    /// <param name="source">The delegate from which to remove the invocation list of .</param>
    /// <param name="value">The delegate that supplies the invocation list to remove from the invocation list of .</param>
    /// <returns>
    ///     A new delegate with an invocation list formed by taking the invocation list of  and removing all occurrences
    ///     of the invocation list of , if the invocation list of  is found within the invocation list of . Returns  if
    ///     is null or if the invocation list of  is not found within the invocation list of . Returns a null reference
    ///     if the invocation list of  is equal to the invocation list of , if  contains only a series of invocation
    ///     lists that are equal to the invocation list of , or if  is a null reference.
    /// </returns>
    public static Delegate? RemoveAll([NotNull] this Delegate source, Delegate value) => Delegate.RemoveAll(source, value);

    #endregion Delegate

    #region Double

    /// <summary>
    ///     Returns the absolute value of a double-precision floating-point number.
    /// </summary>
    /// <param name="value">A number that is greater than or equal to , but less than or equal to .</param>
    /// <returns>A double-precision floating-point number, x, such that 0 ? x ?.</returns>
    public static Double Abs(this Double value) => Math.Abs(value);

    /// <summary>
    ///     Returns the angle whose cosine is the specified number.
    /// </summary>
    /// <param name="d">
    ///     A number representing a cosine, where  must be greater than or equal to -1, but less than or
    ///     equal to 1.
    /// </param>
    /// <returns>An angle, ?, measured in radians, such that 0 ????-or-  if  &lt; -1 or  &gt; 1 or  equals .</returns>
    public static Double Acos(this Double d) => Math.Acos(d);

    /// <summary>
    ///     Returns the angle whose sine is the specified number.
    /// </summary>
    /// <param name="d">
    ///     A number representing a sine, where  must be greater than or equal to -1, but less than or equal
    ///     to 1.
    /// </param>
    /// <returns>
    ///     An angle, ?, measured in radians, such that -?/2 ????/2 -or-  if  &lt; -1 or  &gt; 1 or  equals .
    /// </returns>
    public static Double Asin(this Double d) => Math.Asin(d);

    /// <summary>
    ///     Returns the angle whose tangent is the specified number.
    /// </summary>
    /// <param name="d">A number representing a tangent.</param>
    /// <returns>
    ///     An angle, ?, measured in radians, such that -?/2 ????/2.-or-  if  equals , -?/2 rounded to double precision (-
    ///     1.5707963267949) if  equals , or ?/2 rounded to double precision (1.5707963267949) if  equals .
    /// </returns>
    public static Double Atan(this Double d) => Math.Atan(d);

    /// <summary>
    ///     Returns the angle whose tangent is the quotient of two specified numbers.
    /// </summary>
    /// <param name="y">The y coordinate of a point.</param>
    /// <param name="x">The x coordinate of a point.</param>
    /// <returns>
    ///     An angle, ?, measured in radians, such that -?????, and tan(?) =  / , where (, ) is a point in the Cartesian
    ///     plane. Observe the following: For (, ) in quadrant 1, 0 &lt; ? &lt; ?/2.For (, ) in quadrant 2, ?/2 &lt;
    ///     ???.For (, ) in quadrant 3, -? &lt; ? &lt; -?/2.For (, ) in quadrant 4, -?/2 &lt; ? &lt; 0.For points on the
    ///     boundaries of the quadrants, the return value is the following:If y is 0 and x is not negative, ? = 0.If y is
    ///     0 and x is negative, ? = ?.If y is positive and x is 0, ? = ?/2.If y is negative and x is 0, ? = -?/2.If  or
    ///     is , or if  and  are either  or , the method returns .
    /// </returns>
    public static Double Atan2(this Double y, Double x) => Math.Atan2(y, x);

    /// <summary>
    ///     Returns the smallest integral value that is greater than or equal to the specified double-precision floating-
    ///     point number.
    /// </summary>
    /// <param name="a">A double-precision floating-point number.</param>
    /// <returns>
    ///     The smallest integral value that is greater than or equal to . If  is equal to , , or , that value is
    ///     returned. Note that this method returns a  instead of an integral type.
    /// </returns>
    public static Int32 Ceiling(this Double a)
        => Convert.ToInt32(Math.Ceiling(a));

    /// <summary>
    ///     Returns the cosine of the specified angle.
    /// </summary>
    /// <param name="d">An angle, measured in radians.</param>
    /// <returns>The cosine of . If  is equal to , , or , this method returns .</returns>
    public static Double Cos(this Double d) => Math.Cos(d);

    /// <summary>
    ///     Returns the hyperbolic cosine of the specified angle.
    /// </summary>
    /// <param name="value">An angle, measured in radians.</param>
    /// <returns>The hyperbolic cosine of . If  is equal to  or ,  is returned. If  is equal to ,  is returned.</returns>
    public static Double Cosh(this Double value) => Math.Cosh(value);

    /// <summary>
    ///     Returns e raised to the specified power.
    /// </summary>
    /// <param name="d">A number specifying a power.</param>
    /// <returns>
    ///     The number e raised to the power . If  equals  or , that value is returned. If  equals , 0 is returned.
    /// </returns>
    public static Double Exp(this Double d) => Math.Exp(d);

    /// <summary>
    ///     Returns the largest integer less than or equal to the specified double-precision floating-point number.
    /// </summary>
    /// <param name="d">A double-precision floating-point number.</param>
    /// <returns>The largest integer less than or equal to . If  is equal to , , or , that value is returned.</returns>
    public static Int32 Floor(this Double d) => Convert.ToInt32(Math.Floor(d));

    /// <summary>
    ///     Returns the remainder resulting from the division of a specified number by another specified number.
    /// </summary>
    /// <param name="x">A dividend.</param>
    /// <param name="y">A divisor.</param>
    /// <returns>
    ///     A number equal to  - ( Q), where Q is the quotient of  /  rounded to the nearest integer (if  /  falls
    ///     halfway between two integers, the even integer is returned).If  - ( Q) is zero, the value +0 is returned if
    ///     is positive, or -0 if  is negative.If  = 0,  is returned.
    /// </returns>
    public static Double IEEERemainder(this Double x, Double y) => Math.IEEERemainder(x, y);

    /// <summary>
    ///     Returns the natural (base e) logarithm of a specified number.
    /// </summary>
    /// <param name="d">The number whose logarithm is to be found.</param>
    /// <returns>
    ///     One of the values in the following table.  parameterReturn value Positive The natural logarithm of ; that is,
    ///     ln , or log eZero Negative Equal to Equal to.
    /// </returns>
    public static Double Log(this Double d) => Math.Log(d);

    /// <summary>
    ///     Returns the logarithm of a specified number in a specified base.
    /// </summary>
    /// <param name="d">The number whose logarithm is to be found.</param>
    /// <param name="newBase">The base of the logarithm.</param>
    /// <returns>
    ///     One of the values in the following table. (+Infinity denotes , -Infinity denotes , and NaN denotes .)Return
    ///     value&gt; 0(0 &lt;&lt; 1) -or-(&gt; 1)lognewBase(a)&lt; 0(any value)NaN(any value)&lt; 0NaN != 1 = 0NaN != 1
    ///     = +InfinityNaN = NaN(any value)NaN(any value) = NaNNaN(any value) = 1NaN = 00 &lt;&lt; 1 +Infinity = 0&gt; 1-
    ///     Infinity =  +Infinity0 &lt;&lt; 1-Infinity =  +Infinity&gt; 1+Infinity = 1 = 00 = 1 = +Infinity0.
    /// </returns>
    public static Double Log(this Double d, Double newBase) => Math.Log(d, newBase);

    /// <summary>
    ///     Returns the base 10 logarithm of a specified number.
    /// </summary>
    /// <param name="d">A number whose logarithm is to be found.</param>
    /// <returns>
    ///     One of the values in the following table.  parameter Return value Positive The base 10 log of ; that is, log
    ///     10. Zero Negative Equal to Equal to.
    /// </returns>
    public static Double Log10(this Double d) => Math.Log10(d);

    /// <summary>
    ///     Returns the larger of two double-precision floating-point numbers.
    /// </summary>
    /// <param name="val1">The first of two double-precision floating-point numbers to compare.</param>
    /// <param name="val2">The second of two double-precision floating-point numbers to compare.</param>
    /// <returns>Parameter  or , whichever is larger. If , , or both  and  are equal to ,  is returned.</returns>
    public static Double Max(this Double val1, Double val2) => Math.Max(val1, val2);

    /// <summary>
    ///     Returns the smaller of two double-precision floating-point numbers.
    /// </summary>
    /// <param name="val1">The first of two double-precision floating-point numbers to compare.</param>
    /// <param name="val2">The second of two double-precision floating-point numbers to compare.</param>
    /// <returns>Parameter  or , whichever is smaller. If , , or both  and  are equal to ,  is returned.</returns>
    public static Double Min(this Double val1, Double val2) => Math.Min(val1, val2);

    /// <summary>
    ///     Returns a specified number raised to the specified power.
    /// </summary>
    /// <param name="x">A double-precision floating-point number to be raised to a power.</param>
    /// <param name="y">A double-precision floating-point number that specifies a power.</param>
    /// <returns>The number  raised to the power .</returns>
    public static Double Pow(this Double x, Double y) => Math.Pow(x, y);

    /// <summary>
    ///     Rounds a double-precision floating-point value to the nearest integral value.
    /// </summary>
    /// <param name="a">A double-precision floating-point number to be rounded.</param>
    /// <returns>
    ///     The integer nearest . If the fractional component of  is halfway between two integers, one of which is even
    ///     and the other odd, then the even number is returned. Note that this method returns a  instead of an integral
    ///     type.
    /// </returns>
    public static Double Round(this Double a) => Math.Round(a);

    /// <summary>
    ///     Rounds a double-precision floating-point value to a specified number of fractional digits.
    /// </summary>
    /// <param name="a">A double-precision floating-point number to be rounded.</param>
    /// <param name="digits">The number of fractional digits in the return value.</param>
    /// <returns>The number nearest to  that contains a number of fractional digits equal to .</returns>
    public static Double Round(this Double a, Int32 digits) => Math.Round(a, digits);

    /// <summary>
    ///     Rounds a double-precision floating-point value to the nearest integer. A parameter specifies how to round the
    ///     value if it is midway between two numbers.
    /// </summary>
    /// <param name="a">A double-precision floating-point number to be rounded.</param>
    /// <param name="mode">Specification for how to round  if it is midway between two other numbers.</param>
    /// <returns>
    ///     The integer nearest . If  is halfway between two integers, one of which is even and the other odd, then
    ///     determines which of the two is returned.
    /// </returns>
    public static Double Round(this Double a, MidpointRounding mode) => Math.Round(a, mode);

    /// <summary>
    ///     Rounds a double-precision floating-point value to a specified number of fractional digits. A parameter
    ///     specifies how to round the value if it is midway between two numbers.
    /// </summary>
    /// <param name="value">A double-precision floating-point number to be rounded.</param>
    /// <param name="digits">The number of fractional digits in the return value.</param>
    /// <param name="mode">Specification for how to round  if it is midway between two other numbers.</param>
    /// <returns>
    ///     The number nearest to  that has a number of fractional digits equal to . If  has fewer fractional digits than
    ///     ,  is returned unchanged.
    /// </returns>
    public static Double Round(this Double value, Int32 digits, MidpointRounding mode) => Math.Round(value, digits, mode);

    /// <summary>
    ///     Returns a value indicating the sign of a double-precision floating-point number.
    /// </summary>
    /// <param name="value">A signed number.</param>
    /// <returns>
    ///     A number that indicates the sign of , as shown in the following table.Return value Meaning -1  is less than
    ///     zero. 0  is equal to zero. 1  is greater than zero.
    /// </returns>
    public static Int32 Sign(this Double value) => Math.Sign(value);

    /// <summary>
    ///     Returns the sine of the specified angle.
    /// </summary>
    /// <param name="a">An angle, measured in radians.</param>
    /// <returns>The sine of . If  is equal to , , or , this method returns .</returns>
    public static Double Sin(this Double a) => Math.Sin(a);

    /// <summary>
    ///     Returns the hyperbolic sine of the specified angle.
    /// </summary>
    /// <param name="value">An angle, measured in radians.</param>
    /// <returns>The hyperbolic sine of . If  is equal to , , or , this method returns a  equal to .</returns>
    public static Double Sinh(this Double value) => Math.Sinh(value);

    /// <summary>
    ///     Returns the square root of a specified number.
    /// </summary>
    /// <param name="d">The number whose square root is to be found.</param>
    /// <returns>
    ///     One of the values in the following table.  parameter Return value Zero or positive The positive square root
    ///     of . Negative Equals Equals.
    /// </returns>
    public static Double Sqrt(this Double d) => Math.Sqrt(d);

    /// <summary>
    ///     Returns the tangent of the specified angle.
    /// </summary>
    /// <param name="a">An angle, measured in radians.</param>
    /// <returns>The tangent of . If  is equal to , , or , this method returns .</returns>
    public static Double Tan(this Double a) => Math.Tan(a);

    /// <summary>
    ///     Returns the hyperbolic tangent of the specified angle.
    /// </summary>
    /// <param name="value">An angle, measured in radians.</param>
    /// <returns>
    ///     The hyperbolic tangent of . If  is equal to , this method returns -1. If value is equal to , this method
    ///     returns 1. If  is equal to , this method returns .
    /// </returns>
    public static Double Tanh(this Double value) => Math.Tanh(value);

    /// <summary>
    ///     Calculates the integral part of a specified double-precision floating-point number.
    /// </summary>
    /// <param name="d">A number to truncate.</param>
    /// <returns>
    ///     The integral part of ; that is, the number that remains after any fractional digits have been discarded, or
    ///     one of the values listed in the following table. Return value.
    /// </returns>
    public static Double Truncate(this Double d) => Math.Truncate(d);

    /// <summary>
    ///     A Double extension method that converts the @this to a moneyFormat.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as a Double.</returns>
    public static Double ToMoney(this Double @this) => Math.Round(@this, 2);

    #endregion Double

    #region Enum

    /// <summary>
    /// A T extension method to determines whether the object is equal to any of the provided values.
    /// </summary>
    /// <param name="this">The object to be compared.</param>
    /// <param name="values">The value list to compare with the object.</param>
    /// <returns>true if the values list contains the object, else false.</returns>
    public static Boolean In([NotNull] this Enum @this, params Enum[] values) => Array.IndexOf(values, @this) >= 0;

    #endregion Enum

    #region EventHandler

    /// <summary>
    ///     An EventHandler extension method that raises the event event.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="sender">Source of the event.</param>
    public static void RaiseEvent(this EventHandler @this, Object sender) => @this?.Invoke(sender, EventArgs.Empty);

    /// <summary>
    ///     An EventHandler extension method that raises.
    /// </summary>
    /// <param name="handler">The handler to act on.</param>
    /// <param name="sender">Source of the event.</param>
    /// <param name="e">Event information.</param>
    public static void RaiseEvent(this EventHandler handler, Object sender, EventArgs e) => handler?.Invoke(sender, e);

    /// <summary>
    ///     An EventHandler&lt;TEventArgs&gt; extension method that raises the event event.
    /// </summary>
    /// <typeparam name="TEventArgs">Type of the event arguments.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="sender">Source of the event.</param>
    public static void RaiseEvent<TEventArgs>(this EventHandler<TEventArgs> @this, Object sender) where TEventArgs : EventArgs => @this?.Invoke(sender, default!);

    /// <summary>
    ///     An EventHandler&lt;TEventArgs&gt; extension method that raises the event event.
    /// </summary>
    /// <typeparam name="TEventArgs">Type of the event arguments.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="sender">Source of the event.</param>
    /// <param name="e">Event information to send to registered event handlers.</param>
    public static void RaiseEvent<TEventArgs>(this EventHandler<TEventArgs> @this, Object sender, TEventArgs e) where TEventArgs : EventArgs => @this?.Invoke(sender, e);

    #endregion EventHandler

    #region Guid

    /// <summary>A GUID extension method that query if '@this' is empty.</summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if empty, false if not.</returns>
    public static Boolean IsNullOrEmpty(this Guid? @this) => !@this.HasValue || @this == Guid.Empty;

    /// <summary>A GUID extension method that query if '@this' is not null or empty.</summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if empty, false if not.</returns>
    public static Boolean IsNotNullOrEmpty(this Guid? @this) => @this.HasValue && @this.Value != Guid.Empty;

    /// <summary>A GUID extension method that query if '@this' is empty.</summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if empty, false if not.</returns>
    public static Boolean IsEmpty(this Guid @this) => @this == Guid.Empty;

    /// <summary>A GUID extension method that queries if a not is empty.</summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if a not is empty, false if not.</returns>
    public static Boolean IsNotEmpty(this Guid @this) => @this != Guid.Empty;

    #endregion Guid

    #region short

    /// <summary>
    ///     A T extension method that check if the value is between inclusively the minValue and maxValue.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>true if the value is between inclusively the minValue and maxValue, otherwise false.</returns>
    public static Boolean InRange(this Int16 @this, Int16 minValue, Int16 maxValue) => @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;

    /// <summary>
    ///     An Int16 extension method that factor of.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="factorNumer">The factor numer.</param>
    /// <returns>true if it succeeds, false if it fails.</returns>
    public static Boolean FactorOf(this Int16 @this, Int16 factorNumer) => factorNumer % @this == 0;

    /// <summary>
    ///     An Int16 extension method that query if '@this' is even.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if even, false if not.</returns>
    public static Boolean IsEven(this Int16 @this) => @this % 2 == 0;

    /// <summary>
    ///     An Int16 extension method that query if '@this' is odd.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if odd, false if not.</returns>
    public static Boolean IsOdd(this Int16 @this) => @this % 2 != 0;

    /// <summary>
    ///     An Int16 extension method that query if '@this' is prime.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if prime, false if not.</returns>
    public static Boolean IsPrime(this Int16 @this)
    {
        if (@this == 1 || @this == 2)
        {
            return true;
        }

        if (@this % 2 == 0)
        {
            return false;
        }

        var sqrt = (Int16)Math.Sqrt(@this);
        for (Int64 t = 3; t <= sqrt; t += 2)
        {
            if (@this % t == 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Returns the specified 16-bit signed integer value as an array of bytes.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>An array of bytes with length 2.</returns>
    public static Byte[] GetBytes(this Int16 value) => BitConverter.GetBytes(value);

    /// <summary>
    ///     Returns the larger of two 16-bit signed integers.
    /// </summary>
    /// <param name="val1">The first of two 16-bit signed integers to compare.</param>
    /// <param name="val2">The second of two 16-bit signed integers to compare.</param>
    /// <returns>Parameter  or , whichever is larger.</returns>
    public static Int16 Max(this Int16 val1, Int16 val2) => Math.Max(val1, val2);

    /// <summary>
    ///     Returns the smaller of two 16-bit signed integers.
    /// </summary>
    /// <param name="val1">The first of two 16-bit signed integers to compare.</param>
    /// <param name="val2">The second of two 16-bit signed integers to compare.</param>
    /// <returns>Parameter  or , whichever is smaller.</returns>
    public static Int16 Min(this Int16 val1, Int16 val2) => Math.Min(val1, val2);

    /// <summary>
    ///     Returns a value indicating the sign of a 16-bit signed integer.
    /// </summary>
    /// <param name="value">A signed number.</param>
    /// <returns>
    ///     A number that indicates the sign of , as shown in the following table.Return value Meaning -1  is less than
    ///     zero. 0  is equal to zero. 1  is greater than zero.
    /// </returns>
    public static Int32 Sign(this Int16 value) => Math.Sign(value);

    /// <summary>
    ///     Converts a short value from host byte order to network byte order.
    /// </summary>
    /// <param name="host">The number to convert, expressed in host byte order.</param>
    /// <returns>A short value, expressed in network byte order.</returns>
    public static Int16 HostToNetworkOrder(this Int16 host) => IPAddress.HostToNetworkOrder(host);

    /// <summary>
    ///     Converts a short value from network byte order to host byte order.
    /// </summary>
    /// <param name="network">The number to convert, expressed in network byte order.</param>
    /// <returns>A short value, expressed in host byte order.</returns>
    public static Int16 NetworkToHostOrder(this Int16 network) => IPAddress.NetworkToHostOrder(network);

    #endregion short

    #region int

    /// <summary>
    ///     A T extension method that check if the value is between inclusively the minValue and maxValue.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>true if the value is between inclusively the minValue and maxValue, otherwise false.</returns>
    public static Boolean InRange(this Int32 @this, Int32 minValue, Int32 maxValue) => @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;

    /// <summary>
    ///     An Int32 extension method that factor of.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="factorNumer">The factor numer.</param>
    /// <returns>true if it succeeds, false if it fails.</returns>
    public static Boolean FactorOf(this Int32 @this, Int32 factorNumer) => factorNumer % @this == 0;

    /// <summary>
    ///     An Int32 extension method that query if '@this' is even.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if even, false if not.</returns>
    public static Boolean IsEven(this Int32 @this) => @this % 2 == 0;

    /// <summary>
    ///     An Int32 extension method that query if '@this' is odd.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if odd, false if not.</returns>
    public static Boolean IsOdd(this Int32 @this) => @this % 2 != 0;

    /// <summary>
    ///     An Int32 extension method that query if '@this' is multiple of.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="factor">The factor.</param>
    /// <returns>true if multiple of, false if not.</returns>
    public static Boolean IsMultipleOf(this Int32 @this, Int32 factor) => @this % factor == 0;

    /// <summary>
    ///     An Int32 extension method that query if '@this' is prime.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if prime, false if not.</returns>
    public static Boolean IsPrime(this Int32 @this)
    {
        if (@this == 1 || @this == 2)
        {
            return true;
        }

        if (@this % 2 == 0)
        {
            return false;
        }

        var sqrt = (Int32)Math.Sqrt(@this);
        for (var t = 3; t <= sqrt; t += 2)
        {
            if (@this % t == 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Returns the specified 32-bit signed integer value as an array of bytes.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>An array of bytes with length 4.</returns>
    public static Byte[] GetBytes(this Int32 value) => BitConverter.GetBytes(value);

    /// <summary>
    ///     Converts the specified Unicode code point into a UTF-16 encoded string.
    /// </summary>
    /// <param name="utf32">A 21-bit Unicode code point.</param>
    /// <returns>
    ///     A string consisting of one  object or a surrogate pair of  objects equivalent to the code point specified by
    ///     the  parameter.
    /// </returns>
    public static String ConvertFromUtf32(this Int32 utf32) => Char.ConvertFromUtf32(utf32);

    /// <summary>
    ///     Returns the number of days in the specified month and year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month (a number ranging from 1 to 12).</param>
    /// <returns>
    ///     The number of days in  for the specified .For example, if  equals 2 for February, the return value is 28 or
    ///     29 depending upon whether  is a leap year.
    /// </returns>
    public static Int32 DaysInMonth(this Int32 year, Int32 month) => DateTime.DaysInMonth(year, month);

    /// <summary>
    ///     Returns an indication whether the specified year is a leap year.
    /// </summary>
    /// <param name="year">A 4-digit year.</param>
    /// <returns>true if  is a leap year; otherwise, false.</returns>
    public static Boolean IsLeapYear(this Int32 year) => DateTime.IsLeapYear(year);

    /// <summary>
    ///     Returns the absolute value of a 32-bit signed integer.
    /// </summary>
    /// <param name="value">A number that is greater than , but less than or equal to .</param>
    /// <returns>A 32-bit signed integer, x, such that 0 ? x ?.</returns>
    public static Int32 Abs(this Int32 value) => Math.Abs(value);

    /// <summary>
    ///     Produces the full product of two 32-bit numbers.
    /// </summary>
    /// <param name="a">The first number to multiply.</param>
    /// <param name="b">The second number to multiply.</param>
    /// <returns>The number containing the product of the specified numbers.</returns>
    public static Int64 BigMul(this Int32 a, Int32 b) => Math.BigMul(a, b);

    /// <summary>
    ///     An Int32 extension method that div rem.
    /// </summary>
    /// <param name="a">a to act on.</param>
    /// <param name="b">The Int32 to process.</param>
    /// <param name="result">[out] The result.</param>
    /// <returns>An Int32.</returns>
    public static Int32 DivRem(this Int32 a, Int32 b, out Int32 result) => Math.DivRem(a, b, out result);

    /// <summary>
    ///     Returns the larger of two 32-bit signed integers.
    /// </summary>
    /// <param name="val1">The first of two 32-bit signed integers to compare.</param>
    /// <param name="val2">The second of two 32-bit signed integers to compare.</param>
    /// <returns>Parameter  or , whichever is larger.</returns>
    public static Int32 Max(this Int32 val1, Int32 val2) => Math.Max(val1, val2);

    /// <summary>
    ///     Returns the smaller of two 32-bit signed integers.
    /// </summary>
    /// <param name="val1">The first of two 32-bit signed integers to compare.</param>
    /// <param name="val2">The second of two 32-bit signed integers to compare.</param>
    /// <returns>Parameter  or , whichever is smaller.</returns>
    public static Int32 Min(this Int32 val1, Int32 val2) => Math.Min(val1, val2);

    /// <summary>
    ///     Returns a value indicating the sign of a 32-bit signed integer.
    /// </summary>
    /// <param name="value">A signed number.</param>
    /// <returns>
    ///     A number that indicates the sign of , as shown in the following table.Return value Meaning -1  is less than
    ///     zero. 0  is equal to zero. 1  is greater than zero.
    /// </returns>
    public static Int32 Sign(this Int32 value) => Math.Sign(value);

    #endregion int

    #region long

    /// <summary>
    ///     Returns a  that represents a specified time, where the specification is in units of ticks.
    /// </summary>
    /// <param name="value">A number of ticks that represent a time.</param>
    /// <returns>An object that represents .</returns>
    public static TimeSpan FromTicks(this Int64 value) => TimeSpan.FromTicks(value);

    /// <summary>
    ///     Returns the specified 64-bit signed integer value as an array of bytes.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>An array of bytes with length 8.</returns>
    public static Byte[] GetBytes(this Int64 value) => BitConverter.GetBytes(value);

    /// <summary>
    ///     Converts the specified 64-bit signed integer to a double-precision floating point number.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>A double-precision floating point number whose value is equivalent to .</returns>
    public static Double Int64BitsToDouble(this Int64 value) => BitConverter.Int64BitsToDouble(value);

    #endregion long

    #region object

    /// <summary>
    ///     An object extension method that converts the @this to an or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A T.</returns>
    public static T? AsOrDefault<T>([NotNull] this Object @this)
    {
        try
        {
            return (T)@this;
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    ///     An object extension method that converts the @this to an or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>A T.</returns>
    public static T AsOrDefault<T>([NotNull] this Object @this, T defaultValue)
    {
        try
        {
            return (T)@this;
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    /// <summary>
    ///     An object extension method that converts the @this to an or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>A T.</returns>
    public static T AsOrDefault<T>([NotNull] this Object @this, Func<T> defaultValueFactory)
    {
        try
        {
            return (T)@this;
        }
        catch (Exception)
        {
            return defaultValueFactory();
        }
    }

    /// <summary>
    ///     An object extension method that converts the @this to an or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>A T.</returns>
    public static T AsOrDefault<T>([NotNull] this Object @this, Func<Object, T> defaultValueFactory)
    {
        try
        {
            return (T)@this;
        }
        catch (Exception)
        {
            return defaultValueFactory(@this);
        }
    }

    /// <summary>
    ///     A System.Object extension method that toes the given this.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">this.</param>
    /// <returns>A T.</returns>
    public static T? To<T>(this Object @this)
    {
        if (@this == null || @this == DBNull.Value)
        {
            return (T?)(Object?)null;
        }

        var targetType = typeof(T).Unwrap();
        var sourceType = @this.GetType().Unwrap();
        if (sourceType == targetType)
        {
            return (T)@this;
        }
        var converter = TypeDescriptor.GetConverter(sourceType);
        if (converter.CanConvertTo(targetType))
        {
            return (T?)converter.ConvertTo(@this, targetType);
        }

        converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(sourceType))
        {
            return (T?)converter.ConvertFrom(@this);
        }

        return (T)Convert.ChangeType(@this, targetType);
    }

    /// <summary>
    ///     A System.Object extension method that toes the given this.
    /// </summary>
    /// <param name="this">this.</param>
    /// <param name="type">The type.</param>
    /// <returns>An object.</returns>
    public static Object? To(this Object @this, Type type)
    {
        if (@this == null || @this == DBNull.Value)
        {
            return null;
        }

        var targetType = type.Unwrap();
        var sourceType = @this.GetType().Unwrap();

        if (sourceType == targetType)
        {
            return @this;
        }

        var converter = TypeDescriptor.GetConverter(sourceType);
        if (converter.CanConvertTo(targetType))
        {
            return converter.ConvertTo(@this, targetType);
        }

        converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(sourceType))
        {
            return converter.ConvertFrom(@this);
        }

        return Convert.ChangeType(@this, targetType);
    }

    /// <summary>
    ///     A System.Object extension method that converts this object to an or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">this.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>The given data converted to a T.</returns>
    public static T? ToOrDefault<T>(this Object @this, Func<Object, T> defaultValueFactory)
    {
        try
        {
            return @this.To<T>();
        }
        catch (Exception)
        {
            return defaultValueFactory(@this);
        }
    }

    /// <summary>
    ///     A System.Object extension method that converts this object to an or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">this.</param>
    /// <param name="defaultValueFactory">The default value factory.</param>
    /// <returns>The given data converted to a T.</returns>
    public static T? ToOrDefault<T>([NotNull] this Object @this, Func<T> defaultValueFactory) => @this.ToOrDefault(x => defaultValueFactory());

    /// <summary>
    ///     A System.Object extension method that converts this object to an or default.
    /// </summary>
    /// <param name="this">this.</param>
    /// <param name="type">type</param>
    /// <returns>The given data converted to</returns>
    public static Object? ToOrDefault([NotNull] this Object @this, [NotNull] Type type)
    {
        try
        {
            return @this.To(type);
        }
        catch (Exception)
        {
            return type.GetDefaultValue();
        }
    }

    /// <summary>
    ///     A System.Object extension method that converts this object to an or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">this.</param>
    /// <returns>The given data converted to a T.</returns>
    public static T? ToOrDefault<T>(this Object @this) => @this.ToOrDefault(x => default(T));

    /// <summary>
    ///     A System.Object extension method that converts this object to an or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">this.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The given data converted to a T.</returns>
    public static T? ToOrDefault<T>(this Object @this, T defaultValue) => @this.ToOrDefault(x => defaultValue);

    /// <summary>
    ///     An object extension method that query if '@this' is assignable from.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if assignable from, false if not.</returns>
    public static Boolean IsAssignableFrom<T>([NotNull] this Object @this)
    {
        var type = @this.GetType();
        return type.IsAssignableFrom(typeof(T));
    }

    /// <summary>
    ///     An object extension method that query if '@this' is assignable from.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <returns>true if assignable from, false if not.</returns>
    public static Boolean IsAssignableFrom([NotNull] this Object @this, Type targetType)
    {
        var type = @this.GetType();
        return type.IsAssignableFrom(targetType);
    }

    /// <summary>
    ///     A T extension method that chains actions.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="action">The action.</param>
    /// <returns>The @this acted on.</returns>
    public static T? Chain<T>(this T @this, Action<T> action)
    {
        action?.Invoke(@this);

        return @this;
    }

    ///// <summary>
    /////     A T extension method that makes a deep copy of '@this' object.
    ///// </summary>
    ///// <typeparam name="T">Generic type parameter. It should be <c>Serializable</c></typeparam>
    ///// <param name="this">The @this to act on.</param>
    ///// <returns>the copied object.</returns>
    //public static async Task<T> DeepClone<T>([NotNull] this T @this)
    //{
    //    using (var stream = new MemoryStream())
    //    {
    //        //IFormatter formatter = new BinaryFormatter();
    //        await Xfrogcn.BinaryFormatter.BinarySerializer.SerializeAsync(stream, @this);
    //        //formatter.Context = new StreamingContext(StreamingContextStates.Clone);
    //        stream.Seek(0, SeekOrigin.Begin);
    //        return (T)await Xfrogcn.BinaryFormatter.BinarySerializer.DeserializeAsync(stream);
    //    }
    //}

    /// <summary>
    ///     A T extension method that null if.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A T.</returns>
    public static T? NullIf<T>([NotNull] this T @this, Func<T, Boolean> predicate) where T : class
    {
        if (predicate(@this))
        {
            return null;
        }
        return @this;
    }

    /// <summary>
    ///     A T extension method that gets value or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="func">The function.</param>
    /// <returns>The value or default.</returns>
    public static TResult? GetValueOrDefault<T, TResult>(this T @this, Func<T, TResult> func)
    {
        try
        {
            return func(@this);
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    ///     A T extension method that gets value or default.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="func">The function.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The value or default.</returns>
    public static TResult? GetValueOrDefault<T, TResult>(this T @this, Func<T, TResult> func, TResult defaultValue)
    {
        try
        {
            return func(@this);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    /// <summary>A TType extension method that tries.</summary>
    /// <typeparam name="TType">Type of the type.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="tryFunction">The try function.</param>
    /// <param name="catchValue">The catch value.</param>
    /// <returns>A TResult.</returns>
    public static TResult Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, TResult catchValue)
    {
        try
        {
            return tryFunction(@this);
        }
        catch
        {
            return catchValue;
        }
    }

    /// <summary>A TType extension method that tries.</summary>
    /// <typeparam name="TType">Type of the type.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="tryFunction">The try function.</param>
    /// <param name="catchValueFactory">The catch value factory.</param>
    /// <returns>A TResult.</returns>
    public static TResult Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, Func<TType, TResult> catchValueFactory)
    {
        try
        {
            return tryFunction(@this);
        }
        catch
        {
            return catchValueFactory(@this);
        }
    }

    /// <summary>A TType extension method that tries.</summary>
    /// <typeparam name="TType">Type of the type.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="tryFunction">The try function.</param>
    /// <param name="result">[out] The result.</param>
    /// <returns>A TResult.</returns>
    public static Boolean Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, out TResult? result)
    {
        try
        {
            result = tryFunction(@this);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>A TType extension method that tries.</summary>
    /// <typeparam name="TType">Type of the type.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="tryFunction">The try function.</param>
    /// <param name="catchValue">The catch value.</param>
    /// <param name="result">[out] The result.</param>
    /// <returns>A TResult.</returns>
    public static Boolean Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, TResult catchValue, out TResult result)
    {
        try
        {
            result = tryFunction(@this);
            return true;
        }
        catch
        {
            result = catchValue;
            return false;
        }
    }

    /// <summary>A TType extension method that tries.</summary>
    /// <typeparam name="TType">Type of the type.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="tryFunction">The try function.</param>
    /// <param name="catchValueFactory">The catch value factory.</param>
    /// <param name="result">[out] The result.</param>
    /// <returns>A TResult.</returns>
    public static Boolean Try<TType, TResult>(this TType @this, Func<TType, TResult> tryFunction, Func<TType, TResult> catchValueFactory, out TResult result)
    {
        try
        {
            result = tryFunction(@this);
            return true;
        }
        catch
        {
            result = catchValueFactory(@this);
            return false;
        }
    }

    /// <summary>A TType extension method that attempts to action from the given data.</summary>
    /// <typeparam name="TType">Type of the type.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="tryAction">The try action.</param>
    /// <returns>true if it succeeds, false if it fails.</returns>
    public static Boolean Try<TType>(this TType @this, Action<TType> tryAction)
    {
        try
        {
            tryAction(@this);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>A TType extension method that attempts to action from the given data.</summary>
    /// <typeparam name="TType">Type of the type.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="tryAction">The try action.</param>
    /// <param name="catchAction">The catch action.</param>
    /// <returns>true if it succeeds, false if it fails.</returns>
    public static Boolean Try<TType>(this TType @this, Action<TType> tryAction, Action<TType> catchAction)
    {
        try
        {
            tryAction(@this);
            return true;
        }
        catch
        {
            catchAction(@this);
            return false;
        }
    }

    /// <summary>
    ///     A T extension method that check if the value is between inclusively the minValue and maxValue.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>true if the value is between inclusively the minValue and maxValue, otherwise false.</returns>
    public static Boolean InRange<T>([NotNull] this T @this, T minValue, T maxValue) where T : IComparable<T> => @this.CompareTo(minValue) >= 0 && @this.CompareTo(maxValue) <= 0;

    /// <summary>
    ///     A T extension method that query if 'source' is the default value.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="source">The source to act on.</param>
    /// <returns>true if default, false if not.</returns>
    public static Boolean IsDefault<T>(this T source) => typeof(T).IsValueType ? source?.Equals(default(T)) == true : source == null;

    #endregion object

    #region object[]

    /// <summary>
    ///     Gets the types of the objects in the specified array.
    /// </summary>
    /// <param name="args">An array of objects whose types to determine.</param>
    /// <returns>An array of  objects representing the types of the corresponding elements in .</returns>
    public static Type[] GetTypeArray([NotNull] this Object[] args) => Type.GetTypeArray(args);

    #endregion object[]

    #region Random

    /// <summary>
    ///     A Random extension method that return a random value from the specified values.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="values">A variable-length parameters list containing arguments.</param>
    /// <returns>One of the specified value.</returns>
    public static T OneOf<T>([NotNull] this Random @this, params T[] values) => values[@this.Next(values.Length)];

    /// <summary>
    ///     A Random extension method that flip a coin toss.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true 50% of time, otherwise false.</returns>
    public static Boolean CoinToss([NotNull] this Random @this) => @this.Next(2) == 0;

    #endregion Random

    #region string

    /// <summary>
    ///     A T extension method that query if '@this' is null.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>true if null, false if not.</returns>
    public static Boolean IsNull(this String @this) => @this == null;

    /// <summary>
    ///     A string extension method that query if '@this' is not null and not empty.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>false if null or empty, true if not.</returns>
    public static Boolean IsNotNullOrEmpty(this String @this)
        => !String.IsNullOrEmpty(@this);

    /// <summary>
    ///     A string extension method that query if '@this' is not null and not whiteSpace.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>false if null or whiteSpace, true if not.</returns>
    public static Boolean IsNotNullOrWhiteSpace(this String @this) => !String.IsNullOrWhiteSpace(@this);

    /// <summary>
    ///     Creates a new instance of  with the same value as a specified .
    /// </summary>
    /// <param name="str">The string to copy.</param>
    /// <returns>A new string with the same value as .</returns>
    public static String Copy([NotNull] this String str) => new(str.ToCharArray());

    /// <summary>
    ///     Retrieves the system&#39;s reference to the specified .
    /// </summary>
    /// <param name="str">A string to search for in the intern pool.</param>
    /// <returns>
    ///     The system&#39;s reference to , if it is interned; otherwise, a new reference to a string with the value of .
    /// </returns>
    public static String Intern([NotNull] this String str) => String.Intern(str);

    /// <summary>
    ///     Retrieves a reference to a specified .
    /// </summary>
    /// <param name="str">The string to search for in the intern pool.</param>
    /// <returns>A reference to  if it is in the common language runtime intern pool; otherwise, null.</returns>
    public static String? IsInterned([NotNull] this String str) => String.IsInterned(str);

    /// <summary>
    ///     Concatenates the elements of an object array, using the specified separator between each element.
    /// </summary>
    /// <param name="separator">
    ///     The string to use as a separator.  is included in the returned string only if  has more
    ///     than one element.
    /// </param>
    /// <param name="values">An array that contains the elements to concatenate.</param>
    /// <returns>
    ///     A string that consists of the elements of  delimited by the  string. If  is an empty array, the method
    ///     returns .
    /// </returns>
    public static String Join<T>([NotNull] this String separator, IEnumerable<T> values) => String.Join(separator, values);

    /// <summary>
    ///     Indicates whether the specified regular expression finds a match in the specified input string.
    /// </summary>
    /// <param name="input">The string to search for a match.</param>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <returns>true if the regular expression finds a match; otherwise, false.</returns>
    public static Boolean IsMatch([NotNull] this String input, String pattern) => Regex.IsMatch(input, pattern);

    /// <summary>
    ///     Indicates whether the specified regular expression finds a match in the specified input string, using the
    ///     specified matching options.
    /// </summary>
    /// <param name="input">The string to search for a match.</param>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <param name="options">A bitwise combination of the enumeration values that provide options for matching.</param>
    /// <returns>true if the regular expression finds a match; otherwise, false.</returns>
    public static Boolean IsMatch([NotNull] this String input, String pattern, RegexOptions options) => Regex.IsMatch(input, pattern, options);

    /// <summary>An IEnumerable&lt;string&gt; extension method that concatenates the given this.</summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>A string.</returns>
    public static String Concatenate([NotNull] this IEnumerable<String> @this)
    {
        var sb = new StringBuilder();

        foreach (var s in @this)
        {
            sb.Append(s);
        }

        return sb.ToString();
    }

    /// <summary>An IEnumerable&lt;T&gt; extension method that concatenates.</summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="source">The source to act on.</param>
    /// <param name="func">The function.</param>
    /// <returns>A string.</returns>
    public static String Concatenate<T>([NotNull] this IEnumerable<T> source, Func<T, String> func)
    {
        var sb = new StringBuilder();
        foreach (var item in source)
        {
            sb.Append(func(item));
        }

        return sb.ToString();
    }

    /// <summary>
    ///     A string extension method that extracts this object.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A string.</returns>
    public static String Extract([NotNull] this String @this, Func<Char, Boolean> predicate) => new(@this.ToCharArray().Where(predicate).ToArray());

    /// <summary>
    ///     A string extension method that removes the letter.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A string.</returns>
    public static String RemoveWhere([NotNull] this String @this, Func<Char, Boolean> predicate) => new(@this.ToCharArray().Where(x => !predicate(x)).ToArray());

    /// <summary>
    /// SafeSubstring
    /// </summary>
    /// <param name="this"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static String SafeSubstring([NotNull] this String @this, Int32 startIndex)
    {
        if (startIndex < 0 || startIndex > @this.Length)
        {
            return String.Empty;
        }
        return @this[startIndex..];
    }

    /// <summary>
    /// SafeSubstring
    /// </summary>
    /// <param name="str"></param>
    /// <param name="startIndex"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static String SafeSubstring([NotNull] this String str, Int32 startIndex, Int32 length)
    {
        if (startIndex < 0 || startIndex >= str.Length || length < 0)
        {
            return String.Empty;
        }
        return str.Substring(startIndex, Math.Min(str.Length - startIndex, length));
    }

    /// <summary>
    /// Sub, not only substring but support for negative numbers
    /// </summary>
    /// <param name="this">string to be handled</param>
    /// <param name="startIndex">startIndex to substract</param>
    /// <returns>substring</returns>
    public static String Sub([NotNull] this String @this, Int32 startIndex)
    {
        if (startIndex >= 0)
        {
            return @this.SafeSubstring(startIndex);
        }
        if (Math.Abs(startIndex) > @this.Length)
        {
            return String.Empty;
        }
        return @this[(@this.Length + startIndex)..];
    }

    /// <summary>
    ///     A string extension method that repeats the string a specified number of times.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="repeatCount">Number of repeats.</param>
    /// <returns>The repeated string.</returns>
    public static String Repeat([NotNull] this String @this, Int32 repeatCount)
    {
        if (@this.Length == 1)
        {
            return new String(@this[0], repeatCount);
        }

        var sb = new StringBuilder(repeatCount * @this.Length);
        while (repeatCount-- > 0)
        {
            sb.Append(@this);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     A string extension method that reverses the given string.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>The string reversed.</returns>
    public static String Reverse([NotNull] this String @this)
    {
        if (@this.Length <= 1)
        {
            return @this;
        }

        var chars = @this.ToCharArray();
        Array.Reverse(chars);
        return new String(chars);
    }

    /// <summary>
    ///     A string extension method that converts the @this to a byte array.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as a byte[].</returns>
    public static Byte[] ToByteArray([NotNull] this String @this) => Encoding.UTF8.GetBytes(@this);

    /// <summary>
    ///     A string extension method that converts the @this to a byte array.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="encoding">encoding</param>
    /// <returns>@this as a byte[].</returns>
    public static Byte[] ToByteArray([NotNull] this String @this, Encoding encoding) => encoding.GetBytes(@this);

    /// <summary>
    ///     A string extension method that converts the @this to an enum.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <returns>@this as a T.</returns>
    public static T ToEnum<T>([NotNull] this String @this) => (T)Enum.Parse(typeof(T), @this);

    /// <summary>
    ///     A string extension method that truncates.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns>A string.</returns>
    public static String? Truncate(this String @this, Int32 maxLength) => @this.Truncate(maxLength, "...");

    /// <summary>
    ///     A string extension method that truncates.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="suffix">The suffix.</param>
    /// <returns>A string.</returns>
    public static String? Truncate(this String @this, Int32 maxLength, String suffix)
    {
        if (@this == null || @this.Length <= maxLength)
        {
            return @this;
        }
        return @this.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// EqualsIgnoreCase
    /// </summary>
    /// <param name="s1">string1</param>
    /// <param name="s2">string2</param>
    /// <returns></returns>
    public static Boolean EqualsIgnoreCase(this String s1, String s2)
        => String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);

    #endregion string

    #region StringBuilder

    /// <summary>A StringBuilder extension method that substrings.</summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>A string.</returns>
    public static String Substring([NotNull] this StringBuilder @this, Int32 startIndex) => @this.ToString(startIndex, @this.Length - startIndex);

    /// <summary>A StringBuilder extension method that substrings.</summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="length">The length.</param>
    /// <returns>A string.</returns>
    public static String Substring([NotNull] this StringBuilder @this, Int32 startIndex, Int32 length) => @this.ToString(startIndex, length);

    /// <summary>A StringBuilder extension method that appends a join.</summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="values">The values.</param>
    public static StringBuilder AppendJoin<T>([NotNull] this StringBuilder @this, String separator, IEnumerable<T> values)
    {
        @this.Append(String.Join(separator, values));

        return @this;
    }

    /// <summary>A StringBuilder extension method that appends a line join.</summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="values">The values.</param>
    public static StringBuilder AppendLineJoin<T>([NotNull] this StringBuilder @this, String separator, IEnumerable<T> values)
    {
        @this.AppendLine(String.Join(separator, values));

        return @this;
    }

    #endregion StringBuilder

    #region TimeSpan

    /// <summary>
    ///     A TimeSpan extension method that substract the specified TimeSpan to the current DateTime.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>The current DateTime with the specified TimeSpan substracted from it.</returns>
    public static DateTime Ago(this TimeSpan @this) => DateTime.Now.Subtract(@this);

    /// <summary>
    ///     A TimeSpan extension method that add the specified TimeSpan to the current DateTime.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>The current DateTime with the specified TimeSpan added to it.</returns>
    public static DateTime FromNow(this TimeSpan @this) => DateTime.Now.Add(@this);

    /// <summary>
    ///     A TimeSpan extension method that substract the specified TimeSpan to the current UTC (Coordinated Universal
    ///     Time)
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>The current UTC (Coordinated Universal Time) with the specified TimeSpan substracted from it.</returns>
    public static DateTime UtcAgo(this TimeSpan @this) => DateTime.UtcNow.Subtract(@this);

    /// <summary>
    ///     A TimeSpan extension method that add the specified TimeSpan to the current UTC (Coordinated Universal Time)
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <returns>The current UTC (Coordinated Universal Time) with the specified TimeSpan added to it.</returns>
    public static DateTime UtcFromNow(this TimeSpan @this) => DateTime.UtcNow.Add(@this);

    #endregion TimeSpan

    #region Type

    /// <summary>
    ///     A Type extension method that creates an instance.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <returns>The new instance.</returns>
    public static T? CreateInstance<T>([NotNull] this Type @this) => (T?)Activator.CreateInstance(@this);

    /// <summary>
    ///     A Type extension method that creates an instance.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="this">The @this to act on.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The new instance.</returns>
    public static T? CreateInstance<T>([NotNull] this Type @this, params Object[] args) => (T?)Activator.CreateInstance(@this, args);

    /// <summary>
    /// if a type has empty constructor
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    public static Boolean HasEmptyConstructor([NotNull] this Type type)
        => type.GetConstructors(BindingFlags.Instance).Any(c => c.GetParameters().Length == 0);

    private static readonly ConcurrentDictionary<Type, Object> _defaultValues =
        new();

    /// <summary>
    /// 根据 Type 获取默认值，实现类似 default(T) 的功能
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    public static Object? GetDefaultValue([NotNull] this Type type) =>
        type.IsValueType && type != typeof(void) ? _defaultValues.GetOrAdd(type, Activator.CreateInstance!) : null;

    /// <summary>
    /// GetUnderlyingType if nullable else return self
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    public static Type Unwrap([NotNull] this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

    /// <summary>
    /// GetUnderlyingType
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    public static Type? GetUnderlyingType([NotNull] this Type type)
        => Nullable.GetUnderlyingType(type);

    #endregion Type

    /// <summary>
    /// 大写转下划线
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static String UpperToUnderline(this String source)
    {
        var result = String.Empty;
        foreach (var item in source)
        {
            if (item >= 65 && item <= 90)
            {
                var temp = String.Empty;
                if (!String.IsNullOrWhiteSpace(result))
                {
                    temp = "_";
                }
                result += temp + item.ToString().ToLower();
            }
            else
            {
                result += item.ToString();
            }
        }
        return result;
    }

    /// <summary>
    /// 下划线转大写
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static String UnderlineToUpper(this String source)
    {
        var result = String.Empty;
        var toUpper = false;
        foreach (var item in source)
        {
            if (item == '_' && !String.IsNullOrWhiteSpace(result))
            {
                toUpper = true;
            }
            else
            {
                if (toUpper || String.IsNullOrWhiteSpace(result))
                {
                    result += item.ToString().ToUpper();
                    toUpper = false;
                }
                else
                {
                    result += item.ToString();
                }
            }
        }
        return result;
    }

}