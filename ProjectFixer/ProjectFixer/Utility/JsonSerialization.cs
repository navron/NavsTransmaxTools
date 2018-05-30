using System.IO;
using Newtonsoft.Json;

namespace ProjectFixer.Utility
{
    // This code has been copied from  http://blog.danskingdom.com/saving-and-loading-a-c-objects-data-to-an-xml-json-or-binary-file/

    /// <summary>
    /// Functions for performing common JSON Serialization operations.
    /// <para>Requires the Newtonsoft.Json assembly (Json.Net package in NuGet Gallery) to be referenced in your project.</para>
    /// <para>Only public properties and variables will be serialized.</para>
    /// <para>Use the [JsonIgnore] attribute to ignore specific public properties or variables.</para>
    /// <para>Object to be serialized must have a parameterless constructor.</para>
    /// </summary>
    public static class JsonSerialization
    {
        /// <summary>
        /// Writes the given object instance to a JSON file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// <para>
        ///  Only Public properties and variables will be written to the file. These can be any type though, even other classes.
        /// </para>
        /// <para>
        ///  If there are public properties/variables that you do not want written to the file, decorate them with the
        ///  [JsonIgnore] attribute.
        /// </para>
        /// </summary>
        /// <typeparam fileName="T">The file to be written to.</typeparam>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">
        ///  If false the file will be overwritten if it already exists. If true the contents will be appended to the file.
        /// </param>
        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite, Formatting.Indented);
                writer = new StreamWriter(filePath, append);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                writer?.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an JSON file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the JSON file.</returns>
        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                reader = new StreamReader(filePath);
                var fileContents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(fileContents);
            }
            finally
            {
                reader?.Close();
            }
        }
    }
}