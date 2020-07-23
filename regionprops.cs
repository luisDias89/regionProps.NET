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
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.Util.TypeEnum;
using Emgu.CV.Structure;
using System.Net;

// Construção da função regionprops   //
//                                    //
//      Autores: Luís Dias            // 
//                                    //
//________ 21/07/2020 _______________ //


namespace RegionProps
{

    class regionprops
    {
        public class Extreme_class
        {
            public PointF Mais_a_esquerda;
            public PointF Mais_a_Direita;
            public PointF Mais_em_cima;
            public PointF Mais_em_baixo;
        }
        // Declaração da subclasse Extreme
        public Extreme_class Extreme = new Extreme_class();


        public double Area;                                               // Carregado
        public PointF Centroid;                                           // Carregado
        public double AspectRatio;                                        // Carregado
        public Rectangle BoundingBox;                                     // Carregado
        public double Extent;                                             // Carregado
        public double Solidity;                                           // Carregado
        public VectorOfPoint ConvexHull = new VectorOfPoint();            // Carregado
        public double ConvexHull_area;                                    // Carregado
        public double EquivalentDiameter;                                 // Carregado
        public CircleF CirculoEnvolvente = new CircleF();                 // Carregado
        public double perimetro;                                          // Carregado
        public double Circularity;                                        // Carregado
        public bool isConvex;                                             // Carregado
        public VectorOfPoint ContourApproximation = new VectorOfPoint();  // Carregado
        public VectorOfPoint Contorno = new VectorOfPoint();              // Carregado
        public Point[] BoundingBoxRectRodado;                             // Carregado
        public float AnguloRectExterior;                                  // Carregado
        public RotatedRect EllipseValores;                                // Carregado  

        int NumeroEuler;
       
        // Maximum Value, Minimum Value and their locations
        // Mean Color or Mean Intensity

        public void calcularRegionProps(Image<Gray, byte> inputRegionIMG, double AreaMin)
        {
            // Declaração do vetor de vetores de pontos
            Emgu.CV.Util.VectorOfVectorOfPoint vetordeVetdePontos = new Emgu.CV.Util.VectorOfVectorOfPoint();
            // Declaração de uma matriz
            Mat hierarquia = new Mat();

            // Aplicação da função FindContour
            CvInvoke.FindContours(
                inputRegionIMG                                       // Recebe a imagem de entrada
                , vetordeVetdePontos                                 // Recebe um vetor de pontos de contorno
                , hierarquia                                         // Recebe a hierarquia dos pontos
                , Emgu.CV.CvEnum.RetrType.Tree                       // Recebe o tipo de arvore e contornos
                , Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone   // Tip de aproximação aos contornos
                , new Point(0, 0)                                    // Offset do ponto, posso omitir ou declarar um ponto a 0 0
                );

            Image<Bgr, Byte> input = inputRegionIMG.Convert<Bgr, byte>();

            //Até aqui encontro o contorno. Deve ser só 1!!!, portanto deve ser o contorno 0, 
            //mas mesmo assim vamos fazer um teste para ver qual o contorno a usar

            // Pontos buffer
            PointF buffer_Minx = new PointF(inputRegionIMG.Width, inputRegionIMG.Height);
            PointF buffer_MaxX = new PointF(0, 0);
            PointF buffer_MinY = new PointF(inputRegionIMG.Width, inputRegionIMG.Height);
            PointF buffer_MaxY = new PointF(0, 0);


            for (int i = 0; i < vetordeVetdePontos.Size; i++)
            {
                Area = Math.Abs(CvInvoke.ContourArea(vetordeVetdePontos[i], true));      // calcula a area do contorno

                if (Area >= AreaMin)
                {
                    
                    for (int iter = 0; iter < vetordeVetdePontos[i].Size; iter++)
                    {
                        //----------------- Calculo do extreme -----------------
                            // Calcula o valor do ponto mais à esquerda
                            if (vetordeVetdePontos[i][iter].X < buffer_Minx.X)
                                buffer_Minx = vetordeVetdePontos[i][iter];

                            // Calcula o valor do ponto mais à direita
                            if (vetordeVetdePontos[i][iter].X > buffer_MaxX.X)
                                buffer_MaxX = vetordeVetdePontos[i][iter];

                            // Calcula o valor do ponto Y mais em cima
                            if (vetordeVetdePontos[i][iter].Y < buffer_MinY.Y)
                                buffer_MinY = vetordeVetdePontos[i][iter];

                            // Calcula o valor do ponto Y mais em baixo
                            if (vetordeVetdePontos[i][iter].Y > buffer_MaxY.Y)
                                buffer_MaxY = vetordeVetdePontos[i][iter];
                            //----------------- Fim do calculo do extreme -----------------
                    }

                    // ------------- Calculo do Centroid ---------------------
                    Moments momento = CvInvoke.Moments(vetordeVetdePontos[i]);
                    int X = (int)(momento.M10 / momento.M00);
                    int Y = (int)(momento.M01 / momento.M00);
                    Centroid = new PointF(X,Y);
                    // ------------------------------------------------------

                    // ------------ Calculo do AspectRatio ------------------
                    AspectRatio = inputRegionIMG.Width / inputRegionIMG.Height;
                    //-------------------------------------------------------

                    //------------- Calculo da BoundingBox ------------------
                    BoundingBox = CvInvoke.BoundingRectangle(vetordeVetdePontos[i]);
                    //-------------------------------------------------------

                    // ------------   Calculo do Extent   -------------------
                    float rect_area = BoundingBox.Width * BoundingBox.Height;
                    Extent = (float)Area / rect_area;
                    // ------------------------------------------------------

                    // --------------- ConvectHULL --------------------------
                    CvInvoke.ConvexHull(vetordeVetdePontos[i], ConvexHull, false);
                    //-------------------------------------------------------

                    // --------------- ConvectHULL_area ---------------------
                    ConvexHull_area = CvInvoke.ContourArea(ConvexHull);
                    //-------------------------------------------------------

                    //-----------------  Solidity ---------------------------
                    Solidity = Area / ConvexHull_area;
                    // ------------------------------------------------------

                    //-------------- Diametro Equivalente -------------------
                    EquivalentDiameter = Math.Sqrt(4 * Area / Math.PI);
                    // ------------------------------------------------------

                    //--------------- Circulo Envolvente --------------------
                    CirculoEnvolvente=CvInvoke.MinEnclosingCircle(vetordeVetdePontos[i]);
                    //-------------------------------------------------------

                    //--------------- Circulo Perimetro --------------------
                    perimetro = CvInvoke.ArcLength(vetordeVetdePontos[i], true);
                    // -----------------------------------------------------

                    // -------------- Circularity (Fator de forma)----------
                    Circularity = (4 * Math.PI * Area) / (perimetro*perimetro);
                    //------------------------------------------------------

                    // --------------- Verifica se é convexo ---------------
                    isConvex = CvInvoke.IsContourConvex(vetordeVetdePontos[i]);
                    //------------------------------------------------------

                    // ------------- Apriximação do contorno ---------------
                    CvInvoke.ApproxPolyDP(
                        vetordeVetdePontos[i],              // Cada vetor de um contorno iterado
                        ContourApproximation,                             // Vetor que vai conter a aproximação
                        0.1 * perimetro,                    // Expande o perimetro
                        true                                // Calcula um aproximação ao contorno externo                         
                        );
                    // -----------------------------------------------------

                    // ------------- Devolve o contorno --------------------
                    Contorno = vetordeVetdePontos[i];
                    // ------------------------------------------------------

                    // ------------  Retangulo rodado  ---------------------
                    RotatedRect retanguloRodado = CvInvoke.MinAreaRect(vetordeVetdePontos[i]);
                    PointF[] vetorPontos= CvInvoke.BoxPoints(retanguloRodado);
                    BoundingBoxRectRodado = new Point[vetorPontos.Length];
                    for(int iterador=0;iterador<vetorPontos.Length; iterador++)
                        {
                            BoundingBoxRectRodado[iterador].X = (int)vetorPontos[iterador].X;
                            BoundingBoxRectRodado[iterador].Y = (int)vetorPontos[iterador].Y;
                        }
                    // ------------ AnguloRecExterior ----------------------
                    AnguloRectExterior = retanguloRodado.Angle;
                    // -----------------------------------------------------

                    // ------------ EllipseImagem --------------------------
                    EllipseValores= CvInvoke.FitEllipseAMS(vetordeVetdePontos[i]);
                    // -----------------------------------------------------

                    // Fitting a Line ---------------
                    //---------------------------

                    // salta do ciclo for
                    i = vetordeVetdePontos.Size;
                }
            }

            Extreme.Mais_a_esquerda = buffer_Minx;
            Extreme.Mais_a_Direita = buffer_MaxX;
            Extreme.Mais_em_baixo = buffer_MaxY;
            Extreme.Mais_em_cima = buffer_MinY;

            

        }

        
    }
}





// Desenha pontos

/*

CircleF nCicle = new CircleF(Extreme.Mais_em_cima, 5);
Image<Bgr, byte> inIMG = inputRegionIMG.Convert<Bgr, Byte>();
inIMG.Draw(nCicle, new Bgr(0, 255, 0), 10);
            CvInvoke.Imshow("Pic1", inIMG);
*/



// Fecha os contornos com a cor preta!! imfillholes
//input.Draw(storage[i].ToArray(), new Bgr(0, 0, 0), -1, LineType.AntiAlias);


/*

   

    
}
    */






/*
 if (storage.Size >= 0)
                            {
                                for (int i = 0; i < storage.Size; i++)
                                {

                                    area = Math.Abs(CvInvoke.ContourArea(storage[i], true));      // calcula a area do contorno



                                    if (-size <= area && area <= 0)
                                    {
                                        CvInvoke.DrawContours(
                                            input,                  //       Imagem one vai escrever os contornos
                                            storage[i],             //       Vetor de pontos do contono
                                            i,                      //       index do contorno
                                            new MCvScalar(0, 255, 0), //       Cor do contorno
                                            1                       //       Tickness da linha de contorno
                                            , LineType.Filled        //       Tipo de ligação
                                            );



                                        // removes white dots
                                        // CvInvoke.DrawContours(binimg.Ptr, contour.Ptr, new MCvScalar(0, 0, 0), new MCvScalar(0, 0, 0), -1, -1, Emgu.CV.CvEnum.LineType.EightConnected, new Point(0, 0));
                                    }



                            if (0 < area && area <= size)
                            {
                                CvInvoke.DrawContours(
                                     input,                  //       Imagem one vai escrever os contornos
                                     storage,                 //       Vetor de pontos do contono
                                     i,                      //       index do contorno
                                     new MCvScalar(0,55, 0)//       Cor do contorno
                                    , 4                //       Tickness da linha de contorno1
                                     , LineType.Filled        //       Tipo de ligação
                                     );
                                //CvInvoke.FloodFill(input, storage[i],);



                                input.Draw(storage[i].ToArray(), new Bgr(0, 0, 0), -1, LineType.AntiAlias);
                            }
 */


/*  Exemplo de tupula
 RotatedRect retanguloRodado = CvInvoke.MinAreaRect(vetordeVetdePontos);
  var valoresRetanguloRodado = Tuple.Create<PointF, SizeF, PointF>(retanguloRodado.Center, retanguloRodado.Size, retanguloRodado.Center);
*/
