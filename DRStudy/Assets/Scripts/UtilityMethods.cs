using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class UtilityMethods
    {
        /// <summary>
        /// Adds a header to a file as its first line, by creating a temporary file, writing the header, and copying over the permanent file's contents line by line.
        /// If the file at <paramref name="filename"/> does not exist, it will be created, only containing a single line made up of <paramref name="header"/>.
        /// </summary>
        /// <param name="header">The header to write to the first line of the file</param>
        /// <param name="filename">The path to the permanent file in which to write the header</param>
        public static void AddFirstLine(string header, string filename)
        {
            string tempfile = Path.Combine(Application.persistentDataPath, "temp.tmp");
            
            using (var writer = new StreamWriter(tempfile))
            {
                writer.WriteLine(header);
                if (File.Exists(filename))
                {
                    using var reader = new StreamReader(filename);
                    while (!reader.EndOfStream)
                        writer.WriteLine(reader.ReadLine());
                }
            }
            File.Delete(filename);
            File.Copy(tempfile, filename);
            File.Delete(tempfile);
        }

        /// <summary>
        /// Creates and initializes a list containing all integers from <paramref name="startIndex"/> to <paramref name="endIndex"/> in ascending order.
        /// </summary>
        /// <returns>A list containing all integers from <paramref name="startIndex"/> to <paramref name="endIndex"/> in ascending order.</returns>
        public static List<int> CreateAndInitializeList(int startIndex, int endIndex)
        {
            if (endIndex < startIndex)
            {
                throw new System.ArgumentException($"Ending index {endIndex} cannot be smaller than starting index {startIndex}.");
            }

            List<int> list = new List<int>(endIndex - startIndex);
            for (int i = startIndex; i < endIndex; i++)
            {
                list.Add(i);
            }
            return list;
        }

        /// <summary>
        /// Shuffles a copy of a list using <see cref="ShuffleList(List{int})"/>, then only takes the first <paramref name="count"/> elements.
        /// </summary>
        /// <param name="list">The list to copy, shuffle and filter.</param>
        /// <param name="count">The number of elements to keep in the list.</param>
        /// <returns>A shuffled copy of list <paramref name="list"/> in undefined random order containing <paramref name="count"/> elements.</returns>
        /// <exception cref="System.ArgumentException">In case <paramref name="count"/> is greater than the number of elements inside <paramref name="list"/></exception>
        public static List<int> ShuffleAndFilterList(List<int> list, int count)
        {
            if (count > list.Count)
                throw new System.ArgumentException($"[ERROR] Invalid elements number {count} for list with only {list.Count} elements.");
            else {
                List<int> listCopy = new List<int>(list);
                ShuffleList(listCopy);
                if (count < list.Count)
                {
                    listCopy = listCopy.GetRange(0, count);
                }
                return listCopy;
            }   
        }

        /// <summary>
        /// Shuffles a list using the Fisher-Yates algorithm
        /// </summary>
        /// <param name="list">The list to shuffle.</param>
        private static void ShuffleList(List<int> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Reads, removes and then returns the first (i.e. 0-th) element of a list.
        /// </summary>
        /// <param name="list">The list of which to read the head.</param>
        /// <returns>The first element of the list, which was removed.</returns>
        public static int ReadAndRemoveHead(List<int> list)
        {
            int element = list[0];
            list.RemoveAt(0);
            return element;
        }

        public static Vector3 Divide(in Vector3 a, Vector3 b)
        {
            Vector3 result = new Vector3(
                a.x / b.x,
                a.y / b.y,
                a.z / b.z
            );
            return result;
        }

    }
}