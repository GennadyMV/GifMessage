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
using Gif.Components;
using System.Windows.Media.Imaging;

namespace GifMessage
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        public static void Set(ref byte aByte, int pos, bool value)
        {
            if (value)
            {
                aByte = (byte)(aByte | (1 << pos));
            }
            else
            {
                aByte = (byte)(aByte & ~(1 << pos));
            }
        }

        public static bool Get(byte aByte, int pos)
        {
            return ((aByte & (1 << pos)) != 0);
        }
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            // Выбор файла
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
            // Получаем заголовок изображения и проверка формат изображния
            var byteGIF = File.ReadAllBytes(this.openFileDialog1.FileName);

            // Проверка выбранного файла на соответствие GIF формату
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
            
            Set(ref Range10, 7, false);
            Set(ref Range10, 6, false);
            Set(ref Range10, 5, false);
            Set(ref Range10, 4, false);
            int SF = (Range10 >> 4);

            string s3 = Convert.ToString(SF, 2).PadLeft(8, '0');

            Range10 = byteGIF[10];
            Set(ref Range10, 7, false);
            Set(ref Range10, 6, false);
            Set(ref Range10, 5, false);
            Set(ref Range10, 4, false);
            Set(ref Range10, 3, false);
            int Size = Range10;

            int bOrigColorCount = (int)Math.Pow(2, Size + 1);
            int possibleMessageLength = bOrigColorCount * 3 / 2;
            int possibleTextLength = possibleMessageLength - 2;// отводим место для записи длины сообщения
            
            var originalText = this.textBoxMsgIn.Text;
            
            if (possibleTextLength < originalText.Length)
            {
                MessageBox.Show("Размер сообщения " + originalText.Length + " превышен " + possibleMessageLength);
                return;
            }

            // Глобальная политра начинается и 13-го байта
            // сообщение записывается в глобальную политру
            int n = 13;
            // запись длины текста
            var c1 = new BitArray(new int[] {originalText.Length});
            for (int i = 0; i < c1.Length / 2; i++)
            {
                Set(ref byteGIF[n], 0, c1[2 * i]);
                Set(ref byteGIF[n], 1, c1[2 * i + 1]);
                n++;
            }

            byte[] bytes = new byte[originalText.Length];

            // Запись сообщения
            int j = -1;
            foreach (char character in originalText)
            {
                j++;
                bytes[j] = (byte)character;

                Set(ref byteGIF[n], 0, Get((byte)character, 7));
                Set(ref byteGIF[n], 1, Get((byte)character, 6));
                Set(ref byteGIF[n], 2, Get((byte)character, 5));
                Set(ref byteGIF[n], 3, Get((byte)character, 4));                
                n++;
                
                Set(ref byteGIF[n], 0, Get((byte)character, 3));
                Set(ref byteGIF[n], 1, Get((byte)character, 2));
                Set(ref byteGIF[n], 2, Get((byte)character, 1));
                Set(ref byteGIF[n], 3, Get((byte)character, 0));
                n++;

            }
            BitArray bitsText = new BitArray(bytes);
            
            File.WriteAllBytes(this.saveFileDialog1.FileName, byteGIF);
        }

        private int getIntFromBitArray(BitArray bitArray)
        {

            if (bitArray.Length > 32)
                throw new ArgumentException("Должно быть 32 бит.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

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
            // Читать длину сообщения
            var cl = new BitArray(new int[] {0});
            for (int i = 0; i < cl.Length / 2; i++) 
            {
                byte ba = byteGIF[n];

                cl[2 * i] = Get(ba, 0);
                cl[2 * i + 1] = Get(ba, 1);
                n++;
            }
            int textLenght = this.getIntFromBitArray(cl);
            // Читать сообщение
            for (int i = 0; i < textLenght; i++)
            {
                var bitsText = new BitArray(sizeof(byte) * 8);

                bitsText[00 * 4 + 7] = Get(byteGIF[n], 0);
                bitsText[00 * 4 + 6] = Get(byteGIF[n], 1);
                bitsText[00 * 4 + 5] = Get(byteGIF[n], 2);
                bitsText[00 * 4 + 4] = Get(byteGIF[n], 3);
                n++;

                bitsText[00 * 4 + 3] = Get(byteGIF[n], 0);
                bitsText[00 * 4 + 2] = Get(byteGIF[n], 1);
                bitsText[00 * 4 + 1] = Get(byteGIF[n], 2);
                bitsText[00 * 4 + 0] = Get(byteGIF[n], 3);
                n++;
                                
                byte[] bytesBack = BitArrayToByteArray(bitsText);
                string textBack = System.Text.Encoding.ASCII.GetString(bytesBack);
                this.textBoxMsgOut.Text += textBack;
            }
        }
    }
}
