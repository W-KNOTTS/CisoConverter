using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace CisoConverter
{
    internal class xbeTitle
    {
        public xbeTitle(string textToInsert)
        {
            // Define the path to the binary file
            string dirPath = "xbe";
            string fileName = "default.xbe";
            string filePath = Path.Combine(dirPath, fileName);

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                // If it doesn't exist, copy it from the embedded resources
                string resourcePath = "CisoConverter.Resources.default.xbe"; // Resource path and xbe
                using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    resourceStream.CopyTo(fs);
                }
            }

            // Define offsets for text insertion
            int startOffset = 0x184; // Starting offset
            int endOffset = 0x20F; // Ending offset
            int maxBytes = endOffset - startOffset; // Maximum allowed bytes

            // Convert the string to a Unicode (UTF-16) byte array
            byte[] hexData = Encoding.Unicode.GetBytes(textToInsert);

            // Check that the data to insert does not exceed the maximum size
            if (hexData.Length > maxBytes)
            {
                MessageBox.Show($"Your CSO/ISO name is too long. Maximum allowed bytes: {maxBytes}\nRename your ISO to make it shorter please.");
                return;
            }

            // Open the file in read/write mode
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                // Navigate to the starting offset
                fs.Position = startOffset;

                // Write the Unicode data to the file
                fs.Write(hexData, 0, hexData.Length);

                // Fill remaining bytes with zeros
                for (int i = hexData.Length; i < maxBytes; i++)
                {
                    fs.WriteByte(0);
                }
            }
        }
    }
}
