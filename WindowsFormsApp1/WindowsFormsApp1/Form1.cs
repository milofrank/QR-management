﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Video;
using ZXing;
using ZXing.QrCode;
using System.Text.RegularExpressions;
using AForge.Video.DirectShow;
using System.Linq.Expressions;
using Microsoft.Web.WebView2.Core;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //Agregamos referencias
        private FilterInfoCollection Dispositivos;
        private VideoCaptureDevice Camara;
        public static bool ValidateUrl(string url) // METODO CON EXPRESION REGULAR QUE VALIDA LAS URL
        {
            if (url == null || url == "") return false;
            Regex oRegExp = new Regex(@"(http|ftp|https)://([\w-]+\.)+(/[\w- ./?%&=]*)?", RegexOptions.IgnoreCase);

            return oRegExp.Match(url).Success;
        }
        private void Camara_NewFrame(object sender, NewFrameEventArgs
       eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            int i = 0; // Contadora
                       // Listamos dispositivos
            Dispositivos = new
           FilterInfoCollection(FilterCategory.VideoInputDevice);
            //Cargamos dispositivos al combobox
            foreach (FilterInfo Device in Dispositivos)
            {
                //Contadora de camaras
                i = i + 1;
                comboBox1.Items.Add(Device.Name);
            }
            comboBox1.SelectedIndex = -1;
            if (i == 0) // Si no hay camara deshabilitar el boton
            {
                button1.Enabled = false;

            }
            else // Si hay camara el boton se habilita
            {
                button1.Enabled = true;
            }
            // Iniciar control de video
            Camara = new VideoCaptureDevice();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (comboBox1.SelectedIndex == -1) {
                MessageBox.Show("No hay ningún dispositivo seleccionado.");
                return;
            } 
            //Establecemos el dispositivo seleccionado como fuente de video
            Camara = new
            VideoCaptureDevice(Dispositivos[comboBox1.SelectedIndex].MonikerString);
            Camara.NewFrame += new
            NewFrameEventHandler(Camara_NewFrame);
            //Iniciar recepcion de video
            Camara.Start();
            // Habilitamos y empezamos el timer
            timer1.Enabled = true;
            timer1.Start();

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            // Si no es null de la caja de imagen
            if (pictureBox1.Image != null)
            {

                //Instanciamos lo necesario para leer el codigo
                BarcodeReader Reader = new BarcodeReader();
                Bitmap image = (Bitmap)pictureBox1.Image;
                image.RotateFlip(RotateFlipType.Rotate180FlipX); // Voltar la camamra horizontalmente si tiene espejo

                //Variable que contiene el resultado.
                Result result = Reader.Decode((Bitmap)pictureBox1.Image);
                

                try
                {
                    if (result != null)
                    {
                        //Convertimos el resultado en un string
                        string decoded = result.ToString();

                        //Si hay algo en el mensaje entonces
                        if (decoded != "")
                        {
                            // Si no es una url
                            if (!ValidateUrl(decoded))
                            {
                                timer1.Stop(); // detenemos el scaneo
                                MessageBox.Show("El codigo QR no corresponde a una url, pero corresponde al siguiente mensaje: " + decoded);

                                Camara.SignalToStop(); // Detenemos la camara

                                pictureBox1.Image = null; // Reiniciamos la caja de imagen
                            }

                            else // Si es una url
                            {
                                webView21.Source = new Uri(decoded);



                                timer1.Stop(); // Detenemos el Scaneo

                                MessageBox.Show("Navegando a " + decoded); // Mostramos un mensaje

                                Camara.SignalToStop(); // Detenemos la camara
                                pictureBox1.Image = null; // Reiniciamos la caja de imagen
                            }

                        }
                    }
                }                   
                catch (Exception ex) { }
            }



        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (timer1.Enabled)
                {
                    timer1.Stop();
                    timer1.Dispose();
                }

                if (Camara != null && Camara.IsRunning)
                {
                    Camara.SignalToStop();
                    Camara.WaitForStop();
                    Camara = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar: " + ex.Message);
            }

            // Forzar cierre completo de la aplicación
            Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
