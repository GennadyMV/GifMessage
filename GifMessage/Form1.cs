using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GifMessage
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            this.label3.Text = this.openFileDialog1.FileName;

            

        }

        private void buttonWrite_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName.Length == 0)
            {
                MessageBox.Show("Не выбран файл!");
                return;
            }

            var originalbmp = new Bitmap(Bitmap.FromFile(this.openFileDialog1.FileName));
            var encryptbmp = new Bitmap(originalbmp.Width, originalbmp.Height);

            var originalText = this.textBoxMsgIn.Text;
            var ascii = new List<int>(); // To store individual value of the pixels 

            foreach (char character in originalText)
            {
                int asciiValue = Convert.ToInt16(character); // Convert the character to ASCII
                var firstDigit = asciiValue / 1000; // Extract the first digit of the ASCII
                var secondDigit = (asciiValue - (firstDigit * 1000)) / 100; //Extract the second digit of the ASCII
                var thirdDigit = (asciiValue - ((firstDigit * 1000) + (secondDigit * 100))) / 10;//Extract the third digit of the ASCII
                var fourthDigit = (asciiValue - ((firstDigit * 1000) + (secondDigit * 100) + (thirdDigit * 10))); //Extract the third digit of the ASCII
                ascii.Add(firstDigit); // Add the first digit of the ASCII
                ascii.Add(secondDigit); // Add the second digit of the ASCII
                ascii.Add(thirdDigit); // Add the third digit of the ASCII
                ascii.Add(fourthDigit); // Add the fourth digit of the ASCII
            }

            var count = 0; // Have a count

            for (int row = 0; row < originalbmp.Width; row++) // Indicates row number
            {
                for (int column = 0; column < originalbmp.Height; column++) // Indicate column number
                {
                    var color = originalbmp.GetPixel(row, column); // Get the pixel from each and every row and column
                    encryptbmp.SetPixel(row, column, Color.FromArgb(color.A - ((count < ascii.Count) ? ascii[count] : 0), color)); // Set ascii value in A of the pixel
                }
            }

            encryptbmp.Save("EncryptedImage.png", ImageFormat.Png); // Save the encrypted image 

        }

        private void buttonRead_Click(object sender, EventArgs e)
        {
            var characterValue = 0; // Have a variable to store the ASCII value of the character

            string encryptedText = string.Empty; // Have a variable to store the encrypted text

            var ascii = new List<int>(); // Have a collection to store the collection of ASCII

            var encryptbmp = new Bitmap(Bitmap.FromFile("EncryptedImage.png")); // Load the transparent image



            for (int row = 0; row < encryptbmp.Width; row++) // Indicates row number
            {

                for (int column = 0; column < encryptbmp.Height; column++) // Indicate column number
                {

                    var color = encryptbmp.GetPixel(row, column); // Get the pixel from each and every row and column

                    ascii.Add(255 - color.A); // Get the ascii value from A value since 255 is default value

                }

            }

            for (int i = 0; i < ascii.Count; i++)
            {
                characterValue = 0;
                characterValue += ascii[i] * 1000; // Get the first digit of the ASCII value of the encrypted character
                i++;
                characterValue += ascii[i] * 100; // Get the second digit of the ASCII value of the encrypted character
                i++;
                characterValue += ascii[i] * 10;  // Get the first third digit of the ASCII value of the encrypted character
                i++;
                characterValue += ascii[i]; // Get the first fourth digit of the ASCII value of the encrypted character
                if (characterValue != 0)
                    encryptedText += char.ConvertFromUtf32(characterValue); // Convert the ASCII characterValue into character
            }

            this.textBoxMsgOut.Text = encryptedText;

            MessageBox.Show(encryptedText); // Showing the encrypted message in message box
        }
    }
}
