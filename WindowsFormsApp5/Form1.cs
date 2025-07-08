using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp5
{
    public partial class Form1 : Form
    {
        public float L = 10;
        public float cX = 380, cY = 20;
        public int levels = 5;
        private List<(PointF center, float radius, Color color, int nodeId, int level)> nodes = new List<(PointF, float, Color, int, int)>();
        private int nodeCounter = 0;
        public PointF[] GetKochSegmentPoints(PointF pI, PointF pA)
        {
            double length = Math.Sqrt(Math.Pow(pI.X - pA.X, 2) + Math.Pow(pI.Y - pA.Y, 2));

            double third = length / 3;

            PointF pE = new PointF(
                pA.X + (float)((pI.X - pA.X) / 3),
                pA.Y + (float)((pI.Y - pA.Y) / 3)
            );
            PointF pD = new PointF(
                pA.X + (float)(2 * (pI.X - pA.X) / 3),
                pA.Y + (float)(2 * (pI.Y - pA.Y) / 3)
            );



            

            double baseAngle = Math.Atan2(pI.Y - pA.Y, pI.X - pA.X);
            double segmentLength = third;

            double angle1 = baseAngle - 2* Math.PI / 3;


            PointF pC = new PointF(
                pD.X + (float)(segmentLength * Math.Cos(angle1)),
                pD.Y + (float)(segmentLength * Math.Sin(angle1))
            );

            
            double angle2 = baseAngle + 2*Math.PI / 3;


            PointF pF = new PointF(
                pE.X + (float)(segmentLength * Math.Cos(angle2)),
                pE.Y + (float)(segmentLength * Math.Sin(angle2))
            );
            
            PointF pG = new PointF(
                pF.X + (float)(segmentLength * Math.Cos(baseAngle)),
                pF.Y + (float)(segmentLength * Math.Sin(baseAngle))
            );
            PointF pH = new PointF(
                pG.X + (float)(segmentLength * Math.Cos(baseAngle)),
                pG.Y + (float)(segmentLength * Math.Sin(baseAngle))
            );
            

            PointF pB = new PointF(
                pC.X - (float)(segmentLength * Math.Cos(baseAngle)),
                pC.Y - (float)(segmentLength * Math.Sin(baseAngle))
            );



            Graphics Graf = pictureBox1.CreateGraphics();

            return new PointF[] { pB, pC, pD, pE, pF, pG,pH,pI };
        }

        public PointF[] GenerateKochFractal(PointF start, float stepSize, int iterations)
        {
            string axiom = "A";
            string tempAx = "";
            Dictionary<char, string> logic = new Dictionary<char, string>
            {
                { 'A', "A-B--B+A++AA+B-" },
                { 'B', "+A-BB--B-A++A+B" }
            };

            for (int i = 0; i < iterations; i++)
            {
                foreach (char j in axiom)
                {
                    tempAx += logic.ContainsKey(j) ? logic[j] : j.ToString();
                }
                axiom = tempAx;
                tempAx = "";
            }

            List<PointF> points = new List<PointF> { start };
            float currentX = start.X;
            float currentY = start.Y;
            double angle = 0; 

            foreach (char k in axiom)
            {
                if (k == '+')
                {
                    angle -= Math.PI / 3; 
                }
                else if (k == '-')
                {
                    angle += Math.PI / 3; 
                }
                else 
                {
                    currentX += (float)(stepSize * Math.Cos(angle));
                    currentY += (float)(stepSize * Math.Sin(angle));
                    points.Add(new PointF(currentX, currentY));
                }
            }

            return points.ToArray();
        }


        public void DrawTreeOnPictureBox(int levels, PictureBox pictureBox2)
        {
            nodes.Clear(); // Очищаем список узлов
            nodeCounter = 0; // Сбрасываем счётчик

            using (Graphics g = pictureBox2.CreateGraphics())
            {
                g.Clear(Color.White); // Очищаем PictureBox

                // Параметры для размещения
                float startX = pictureBox2.Width / 2; // Центр по X
                float startY = 50; // Начальная Y-координата
                float levelHeight = 80; // Расстояние между уровнями
                float baseWidth = pictureBox2.Width * 0.75f; // Ширина дерева
                float circleRadius = 5; // Радиус кружков

                // Цвета для уровней
                Color[] levelColors = new Color[]
                {
                Color.Red,    // Уровень 1 (корень)
                Color.Blue,   // Уровень 2
                Color.Green,  // Уровень 3
                Color.Orange, // Уровень 4
                Color.Purple  // Уровень 5
                };

                // Список для хранения линий
                List<(PointF parent, PointF child)> lines = new List<(PointF, PointF)>();

                // Рекурсивная функция для рисования узлов и линий
                void DrawNode(int currentLevel, float x, float y, float width)
                {
                    if (currentLevel > levels) return;

                    // Цвет для текущего уровня
                    Color nodeColor = levelColors[Math.Min(currentLevel - 1, levelColors.Length - 1)];
                    PointF currentPoint = new PointF(x, y);

                    // Рисуем кружок для узла
                    using (SolidBrush brush = new SolidBrush(nodeColor))
                    {
                        g.FillEllipse(brush, x - circleRadius, y - circleRadius, 2 * circleRadius, 2 * circleRadius);
                    }

                    // Сохраняем узел с его уровнем
                    nodes.Add((currentPoint, circleRadius, nodeColor, nodeCounter++, currentLevel));

                    if (currentLevel == levels) return;

                    // Вычисляем позиции для 6 детей
                    float childWidth = width / 6;
                    float offsetX = x - width / 2 + childWidth / 2;
                    float nextY = y + levelHeight;

                    for (int i = 0; i < 6; i++)
                    {
                        float childX = offsetX + i * childWidth;
                        PointF childPoint = new PointF(childX, nextY);
                        lines.Add((currentPoint, childPoint));
                        DrawNode(currentLevel + 1, childX, nextY, childWidth);
                    }
                }

                // Запускаем рисование
                DrawNode(1, startX, startY, baseWidth);

                // Рисуем линии
                using (Pen pen = new Pen(Color.Black, 1))
                {
                    foreach (var (parent, child) in lines)
                    {
                        g.DrawLine(pen, parent, child);
                    }
                }
            }
        }
        public void PictureBoxClick()
        {
            Graphics Graf = pictureBox1.CreateGraphics();
            Graf.Clear(BackColor);
            PointF[] all = GenerateKochFractal(new PointF(cX, cY), L, levels);
            Graf.DrawLines(Pens.Black, all);
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PointF start = new PointF(350, 300);
            PointF end = new PointF(100, 300);

            PointF[] all = GetKochSegmentPoints(start, end);

            Graphics Graf = pictureBox1.CreateGraphics();
            Graphics Graf_2 = pictureBox2.CreateGraphics();
            Graf.Clear(BackColor);
            Graf_2.Clear(BackColor);
            PointF[] all_1 = GetKochSegmentPoints(all[1], all[0]);
            PointF[] all_2 = GetKochSegmentPoints(all[1], all[2]);
            PointF[] all_3 = GetKochSegmentPoints(all[2], all[3]);
            //PointF[] all_4 = GetKochSegmentPoints(all[3], all[4]);
            Graf.DrawLines(Pens.Black, all);
            Graf.DrawLines(Pens.Green, all_1);
            Graf.DrawLines(Pens.Red, all_2);
            Graf.DrawLines(Pens.Blue, all_3);
            //Graf.DrawLines(Pens.Red, all_4);



        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.All(char.IsDigit))
            {
                float.TryParse(this.textBox1.Text, out L);
            }
            else
            {
                L = 10;
            }
            if (L < 2)
            {
                L = 2;
            }
            else if (L > 250)
            {
                L = 250;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DrawTreeOnPictureBox(5, pictureBox2);
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (textBox6.Text.All(char.IsDigit))
            {
                float.TryParse(this.textBox6.Text, out cY);
            }
            else
            {
                cY = 20;
            }
            if (cY < 0)
            {
                cY = 0;
            }
            else if (cY > 600)
            {
                cY = 600;
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (textBox5.Text.All(char.IsDigit))
            {
                float.TryParse(this.textBox5.Text, out cX);
            }
            else
            {
                cX = 400;
            }
            if (cX < 0)
            {
                cX = 0;
            }
            else if (cX > 600)
            {
                cX = 600;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.All(char.IsDigit))
            {
                int.TryParse(this.textBox2.Text, out levels);
            }
            else
            {
                levels = 100;
            }
            if (levels < 0)
            {
                levels = 0;
            }
            else if (levels > 600)
            {
                levels = 600;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
           
        }
        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            // Проверяем, попал ли клик в узел
            foreach (var (center, radius, color, nodeId, level) in nodes)
            {
                // Расстояние от точки клика до центра узла
                float distance = (float)Math.Sqrt(Math.Pow(e.X - center.X, 2) + Math.Pow(e.Y - center.Y, 2));
                if (distance <= radius)       
                {
                    levels = level-1;
                    textBox2.Text = (levels).ToString();
                    PictureBoxClick();
                    break; // Выходим после первого найденного узла
                }
            }
        }

        private void pictureBox2_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
         
        }

        private void label8_Click(object sender, EventArgs e)
        {
            
        }
    }
}
