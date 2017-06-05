using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

        private string FormatBytes(Byte[] bytes)
        {
            string value = "";
            foreach (var byt in bytes)
                value += String.Format("{0:X2} ", byt);

            return value;
        }
        public static void Set(ref byte aByte, int pos, bool value)
        {
            if (value)
            {
                //left-shift 1, then bitwise OR
                aByte = (byte)(aByte | (1 << pos));
            }
            else
            {
                //left-shift 1, then take complement, then bitwise AND
                aByte = (byte)(aByte & ~(1 << pos));
            }
        }

        public static bool Get(byte aByte, int pos)
        {
            //left-shift 1, then bitwise AND, then check for non-zero
            return ((aByte & (1 << pos)) != 0);
        }
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (DateTime.Now > new DateTime(2017, 6, 6))
            {
                return;
            }
            if (this.openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            var byteGIF = File.ReadAllBytes(this.openFileDialog1.FileName);

            var byteHead = byteGIF.Skip(0).Take(6).ToArray();

            string result = System.Text.Encoding.UTF8.GetString(byteHead);

            if (result.ToUpper() != "GIF89A")
            {
                MessageBox.Show("Формат файл не GIF89A");
                return;
            }
            ushort W = BitConverter.ToUInt16(byteGIF.Skip(6).Take(2).ToArray(), 0);
            ushort H = BitConverter.ToUInt16(byteGIF.Skip(8).Take(2).ToArray(), 0);
            int BG = (int)byteGIF[11];
            int R = (int)byteGIF[12];

            byte Range10 = byteGIF[10];

            string ss = Convert.ToString(Range10, 2).PadLeft(8, '0');
            string s1 = Convert.ToString(Range10 >> 7, 2).PadLeft(8, '0');
            bool CT = Convert.ToBoolean(Range10 >> 7);

            int Color = (Range10 << 1) >> 6;
            string s2 = Convert.ToString(Color, 2).PadLeft(8, '0');

            Set(ref Range10, 7, false);
            Set(ref Range10, 6, false);
            Set(ref Range10, 5, false);
            Set(ref Range10, 4, false);
            int SF = (Range10 >> 4);
            //SF = SF >> 7;

            string s3 = Convert.ToString(SF, 2).PadLeft(8, '0');


            Range10 = byteGIF[10];
            Set(ref Range10, 7, false);
            Set(ref Range10, 6, false);
            Set(ref Range10, 5, false);
            Set(ref Range10, 4, false);
            Set(ref Range10, 3, false);
            int Size = Range10;

            string s4 = Convert.ToString(Size, 2).PadLeft(8, '0');

            this.label3.Text = this.openFileDialog1.FileName;

        }

        private void buttonWrite_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName.Length == 0)
            {
                MessageBox.Show("Не выбран файл!");
                return;
            }

            var byteGIF = File.ReadAllBytes(this.openFileDialog1.FileName);

            var byteHead = byteGIF.Skip(0).Take(6).ToArray();

            string result = System.Text.Encoding.UTF8.GetString(byteHead);

            if (result.ToUpper() != "GIF89A")
            {
                MessageBox.Show("Формат файл не GIF89A");
                return;
            }

            if (this.saveFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            

            ushort W = BitConverter.ToUInt16(byteGIF.Skip(6).Take(2).ToArray(), 0);
            ushort H = BitConverter.ToUInt16(byteGIF.Skip(8).Take(2).ToArray(), 0);
            int BG = (int)byteGIF[11];
            int R = (int)byteGIF[12];

            byte Range10 = byteGIF[10];

            string ss = Convert.ToString(Range10, 2).PadLeft(8, '0');
            string s1 = Convert.ToString(Range10 >> 7, 2).PadLeft(8, '0');
            bool CT = Convert.ToBoolean(Range10 >> 7);

            int Color = (Range10 << 1) >> 6;
            string s2 = Convert.ToString(Color, 2).PadLeft(8, '0');

            Set(ref Range10, 7, false);
            Set(ref Range10, 6, false);
            Set(ref Range10, 5, false);
            Set(ref Range10, 4, false);
            int SF = (Range10 >> 4);
            //SF = SF >> 7;

            string s3 = Convert.ToString(SF, 2).PadLeft(8, '0');


            Range10 = byteGIF[10];
            Set(ref Range10, 7, false);
            Set(ref Range10, 6, false);
            Set(ref Range10, 5, false);
            Set(ref Range10, 4, false);
            Set(ref Range10, 3, false);
            int Size = Range10;

            int bOrigColorCount = (int)Math.Pow(2, Size + 1);
            int possibleMessageLength = bOrigColorCount * 3 / 4;
            int possibleTextLength = possibleMessageLength - 2;// one byte for check and one byte for message length

            var originalText = this.textBoxMsgIn.Text;

            if (possibleTextLength < originalText.Length)
            {
                MessageBox.Show("Размер сообщения " + originalText.Length + " превышен " + possibleMessageLength);
                return;
            }

            int n = 13;
            // write text length
            var c1 = new BitArray(new int[] {originalText.Length});
            for (int i = 0; i < c1.Length / 2; i++)
            {
                Set(ref byteGIF[n], 0, c1[2 * i]);
                Set(ref byteGIF[n], 1, c1[2 * i + 1]);
                n++;
            }

            byte[] bytes = new byte[originalText.Length];

            // write message
            int j = -1;
            string ss1 = "";
            string ss2 = "";
            foreach (char character in originalText)
            {
                j++;
                bytes[j] = (byte)character;

                ss1 += ToBitString(new BitArray(new byte[] {byteGIF[n]}));
                Set(ref byteGIF[n], 0, Get((byte)character, 7));
                Set(ref byteGIF[n], 1, Get((byte)character, 6));
                ss2 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                n++;

                ss1 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                Set(ref byteGIF[n], 0, Get((byte)character, 5));
                Set(ref byteGIF[n], 1, Get((byte)character, 4));
                ss2 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                n++;

                ss1 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                Set(ref byteGIF[n], 0, Get((byte)character, 3));
                Set(ref byteGIF[n], 1, Get((byte)character, 2));
                ss2 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                n++;


                ss1 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                Set(ref byteGIF[n], 0, Get((byte)character, 1));
                Set(ref byteGIF[n], 1, Get((byte)character, 0));
                ss2 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                n++;
            }

            BitArray bitsText = new BitArray(bytes);
            String sss = this.ToBitString(bitsText);
            
            File.WriteAllBytes(this.saveFileDialog1.FileName, byteGIF);
                
            //var originalbmp = new Bitmap(Bitmap.FromFile(this.openFileDialog1.FileName));
            //var encryptbmp = new Bitmap(originalbmp.Width, originalbmp.Height);

            //var originalText = this.textBoxMsgIn.Text;
            //var ascii = new List<int>(); // To store individual value of the pixels 

            //foreach (char character in originalText)
            //{
            //    int asciiValue = Convert.ToInt16(character); // Convert the character to ASCII
            //    var firstDigit = asciiValue / 1000; // Extract the first digit of the ASCII
            //    var secondDigit = (asciiValue - (firstDigit * 1000)) / 100; //Extract the second digit of the ASCII
            //    var thirdDigit = (asciiValue - ((firstDigit * 1000) + (secondDigit * 100))) / 10;//Extract the third digit of the ASCII
            //    var fourthDigit = (asciiValue - ((firstDigit * 1000) + (secondDigit * 100) + (thirdDigit * 10))); //Extract the third digit of the ASCII
            //    ascii.Add(firstDigit); // Add the first digit of the ASCII
            //    ascii.Add(secondDigit); // Add the second digit of the ASCII
            //    ascii.Add(thirdDigit); // Add the third digit of the ASCII
            //    ascii.Add(fourthDigit); // Add the fourth digit of the ASCII
            //}

            //var count = 0; // Have a count

            //for (int row = 0; row < originalbmp.Width; row++) // Indicates row number
            //{
            //    for (int column = 0; column < originalbmp.Height; column++) // Indicate column number
            //    {
            //        var color = originalbmp.GetPixel(row, column); // Get the pixel from each and every row and column
            //        encryptbmp.SetPixel(row, column, Color.FromArgb(color.A - ((count < ascii.Count) ? ascii[count] : 0), color)); // Set ascii value in A of the pixel
            //    }
            //}

            //encryptbmp.Save("EncryptedImage.png", ImageFormat.Png); // Save the encrypted image 

        }


        private int getIntFromBitArray(BitArray bitArray)
        {

            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

        }

        public string ToBitString(BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = bits.Count - 1; i >= 0; i--)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }
        public byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
        private void buttonRead_Click(object sender, EventArgs e)
        {
            this.textBoxMsgOut.Text = "";

            if (this.openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var byteGIF = File.ReadAllBytes(this.openFileDialog1.FileName);

            int n = 13;
            // read text length
            var cl = new BitArray(new int[] {0});
            for (int i = 0; i < cl.Length / 2; i++) 
            {
                byte ba = byteGIF[n];

                cl[2 * i] = Get(ba, 0);
                cl[2 * i + 1] = Get(ba, 1);
                n++;
            }

            int textLenght = this.getIntFromBitArray(cl);

            //var bitsText = new BitArray(sizeof(byte) * textLenght * 8);
            string ss2 = "";
            for (int i = 0; i < textLenght; i++)
            {
                var bitsText = new BitArray(sizeof(byte) * 8);

                ss2 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                bitsText[00 * 4 + 7] = Get(byteGIF[n], 0);
                bitsText[00 * 4 + 6] = Get(byteGIF[n], 1);
                n++;
                ss2 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                bitsText[00 * 4 + 5] = Get(byteGIF[n], 0);
                bitsText[00 * 4 + 4] = Get(byteGIF[n], 1);
                n++;

                ss2 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                bitsText[00 * 4 + 3] = Get(byteGIF[n], 0);
                bitsText[00 * 4 + 2] = Get(byteGIF[n], 1);
                n++;

                ss2 += ToBitString(new BitArray(new byte[] { byteGIF[n] }));
                bitsText[00 * 4 + 1] = Get(byteGIF[n], 0);
                bitsText[00 * 4 + 0] = Get(byteGIF[n], 1);
                n++;

                
                byte[] bytesBack = BitArrayToByteArray(bitsText);
                string textBack = System.Text.Encoding.ASCII.GetString(bytesBack);
                this.textBoxMsgOut.Text += textBack;
            }

            //string sss = ToBitString(bitsText);

            //byte[] bytesBack = BitArrayToByteArray(bitsText);
            //string textBack = System.Text.Encoding.ASCII.GetString(bytesBack);
            //this.textBoxMsgOut.Text = textBack;
            //foreach (char character in originalText)
            //{
            //    Set(ref byteGIF[n], 0, Get((byte)character, 7));
            //    Set(ref byteGIF[n], 1, Get((byte)character, 6));
            //    n++;
            //    Set(ref byteGIF[n], 0, Get((byte)character, 5));
            //    Set(ref byteGIF[n], 1, Get((byte)character, 4));
            //    n++;
            //    Set(ref byteGIF[n], 0, Get((byte)character, 3));
            //    Set(ref byteGIF[n], 1, Get((byte)character, 2));
            //    n++;
            //    Set(ref byteGIF[n], 0, Get((byte)character, 1));
            //    Set(ref byteGIF[n], 1, Get((byte)character, 0));
            //    n++;
            //}
        }
    }
}
