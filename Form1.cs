using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;

// Construção da função regionprops         //
//                                          //
// Autores: Luís Dias & Bruno das Neves     //
//                                          //
//________ 21/07/2020 _______________       //



namespace RegionProps
{
    public partial class Form1 : Form
    {
        Image<Bgr, byte> imgInput = new Image<Bgr, byte>("C:\\Users\\Luís Dias\\Desktop\\Projetos Pessoais\\Bibliotecas de visão C#\\ImagemTeste.PNG");
        regionprops parametrosIMG = new regionprops();

        Timer TimerProcessamento = new Timer();

        Image<Bgr, byte> inIMG;

        public Form1()
        {
            InitializeComponent();            //
            timer1.Enabled = true;            // 
            timer1.Interval = 70;             // 
            timer1.Start();                   // 

            // Construtor do regionProps
            parametrosIMG.calcularRegionProps(imgInput.Convert<Gray,byte>().ThresholdBinaryInv(new Gray(120), new Gray (255)),500);

            imgInput.Draw(parametrosIMG.BoundingBox, new Bgr(0, 255, 0), 2);
            imgInput.Draw(parametrosIMG.CirculoEnvolvente, new Bgr(0, 0, 255), 2);
            

            Point[] pontoshull = new Point[parametrosIMG.ConvexHull.Size];
            for (int iter = 0; iter < parametrosIMG.ConvexHull.Size; iter++)
                pontoshull[iter] = parametrosIMG.ConvexHull[iter];
            imgInput.DrawPolyline(pontoshull, true, new Bgr(0, 100, 255), 3, LineType.EightConnected);


            CircleF pontoESQ = new CircleF(parametrosIMG.Extreme.Mais_a_esquerda,5);
            imgInput.Draw(pontoESQ, new Bgr(255, 255, 0), 10);

            CircleF pontoDIR = new CircleF(parametrosIMG.Extreme.Mais_a_Direita, 5);
            imgInput.Draw(pontoDIR, new Bgr(255, 255, 0), 10);


            CircleF pontoSUP = new CircleF(parametrosIMG.Extreme.Mais_em_cima, 5);
            imgInput.Draw(pontoSUP, new Bgr(255, 255, 0), 10);

            CircleF pontoINF = new CircleF(parametrosIMG.Extreme.Mais_em_baixo, 5);
            imgInput.Draw(pontoINF, new Bgr(255, 255, 0), 10);

            CircleF CentroidCicle = new CircleF(parametrosIMG.Centroid, 5);
            imgInput.Draw(CentroidCicle, new Bgr(0, 255, 0), 10);


           


            listBox_props.Items.Add("Area:" + parametrosIMG.Area);
            listBox_props.Items.Add("AspectRatio:" + parametrosIMG.AspectRatio);
            listBox_props.Items.Add("Extent:" + parametrosIMG.Extent);
            listBox_props.Items.Add("Diametro Equivalente:" + parametrosIMG.EquivalentDiameter);
            listBox_props.Items.Add("Solidity:" + parametrosIMG.Solidity);
            listBox_props.Items.Add("ConvexHull Area:" + parametrosIMG.ConvexHull_area);
            listBox_props.Items.Add("Perimetro:" + parametrosIMG.perimetro);
            listBox_props.Items.Add("Circularity:" + parametrosIMG.Circularity);


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBoxMostra.Image = imgInput.AsBitmap<Bgr, byte>();
        }



        private Image<Gray, byte> imFillHoles(Image<Gray, byte> image, int minArea, int maxArea)
        {
            var resultImage = image.CopyBlank();
            Gray gray = new Gray(255);
            // Declaração do vetor de vetores de pontos
            Emgu.CV.Util.VectorOfVectorOfPoint vetordeVetdePontos = new Emgu.CV.Util.VectorOfVectorOfPoint();
            // Declaração de uma matriz
            Mat hierarquia = new Mat();
            {
                CvInvoke.FindContours(
                image                                                // Recebe a imagem de entrada
                , vetordeVetdePontos                                 // Recebe um vetor de pontos de contorno
                , hierarquia                                         // Recebe a hierarquia dos pontos
                , Emgu.CV.CvEnum.RetrType.Tree                       // Recebe o tipo de arvore e contornos
                , Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone   // Tip de aproximação aos contornos
                , new Point(0, 0)                                    // Offset do ponto, posso omitir ou declarar um ponto a 0 0
                );

                for (int iter=0; iter < vetordeVetdePontos.Size; iter++)
                {
                    double areaAtual = CvInvoke.ContourArea(vetordeVetdePontos[iter]);

                    if ((areaAtual < maxArea) && (areaAtual > minArea))
                    {
                        resultImage.Draw(vetordeVetdePontos[iter].ToArray(), new Gray(0), -1, LineType.AntiAlias); // Gray 0, por ter só um canal
                    }
                }
            }
            return resultImage;
        }

    }

}

    









