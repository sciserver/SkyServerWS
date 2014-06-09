        ///Current version
        ///ID:          $Id: polyFunk.cs,v 1.6 2003/04/08 17:12:00 nieto Exp $
        ///Revision:    $Revision: 1.6 $
        ///Date:        $Date: 2003/04/08 17:12:00 $ 
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.IO;
namespace Sciserver_webService.ImgCutout
{
    public class Line
    {
        public Point p1;
        public Point p2;
        public int realY;
        public Line(Point p1_, Point p2_, int realY_)
        {
            p1 = p1_;
            p2 = p2_;
            realY = realY_;
        }
        public Line(Point p1_, Point p2_)
        {
            p1 = p1_;
            p2 = p2_;
            realY = p2.Y;
        }
        public void shift(int x, int y)
        {
            p1.X += x;
            p2.X += x;
            p1.Y += y;
            p2.Y += y;
        }
    }


    public class Tuple
    {
        public bool retval;
        public ArrayList segments;
        public int compareBy;
        public Tuple(bool _r, ref ArrayList _s, int _c)
        {
            retval = _r;
            segments = _s;
            compareBy = _c;
        }
    }


    class polyFunk
    {
        static int OFF = 0;
        static int PIX = SdssConstants.OutlinePix;
      
        const  int X   = 11;		//random references
        const  int Y   = 22;

        /// <summary>
        /// given a span, will return an arraylist of line segments 
        /// that form a bounding polygon(s)
        /// </summary>
        /// <param name="spans"></param>
        /// <returns>ArrayList</returns>
        public static ArrayList getPoly(String spans)
        {
            // if(1==1)return getAll(spans);
            ArrayList hSegments = new ArrayList();
            ArrayList vSegments = new ArrayList();
            loadLines(spans, ref hSegments, ref vSegments);
            horizontalScan(ref hSegments);
            vSegments.AddRange(hSegments);
            return vSegments;
        }


        //-----------------------------------------
        //reads lines into an ArrayList of lines
        //-----------------------------------------
        private static void loadLines(String spans,
            ref ArrayList hSegments, ref ArrayList vSegments)
        {
            String[] span = spans.Split(';');
            for (int x = 0; x < span.Length; x++)
            {
                String[] pieces = span[x].Split(',');
                int top_realY = int.Parse(pieces[0]) * PIX;
                int bottom_realY = top_realY + PIX;
                Point tL = new Point(int.Parse(pieces[1]) * PIX, top_realY);
                Point tR = new Point(int.Parse(pieces[2]) * PIX - OFF, top_realY);
                Point bL = new Point(tL.X, bottom_realY - OFF);			//shift up
                Point bR = new Point(tR.X, bottom_realY - OFF);			//tR is shifted left
                Line top = new Line(tL, tR, top_realY);
                Line bottom = new Line(bL, bR, bottom_realY);
                Line left = new Line(tL, bL, -1);
                Line right = new Line(tR, bR, -1);
                hSegments.Add(top);
                hSegments.Add(bottom);
                vSegments.Add(left);
                vSegments.Add(right);
            }
        }


        ///<summary>
        /// horizontalScan(): groups same rows together, then calls cleanUpRow to remove overlap
        ///</summary>
        private static void horizontalScan(ref ArrayList segments)
        {
            ArrayList result = new ArrayList();
            ArrayList lines = new ArrayList();
            lineSort(ref segments, Y);
            for (int a = 0; a < segments.Count; a++)
            {
                Line current = (Line)segments[a];
                lines.Add(current);
                int currentRow = ((Line)segments[a]).realY;
                if (a + 1 >= segments.Count)			//last element
                {
                    cleanUpRow(ref lines);
                    result.AddRange(lines);
                    break;
                }
                else
                {
                    int nextRow = ((Line)segments[a + 1]).realY;
                    if (nextRow != currentRow || (a + 1) >= segments.Count)
                    {
                        cleanUpRow(ref lines);
                        result.AddRange(lines);
                        lines.Clear();
                    }
                }
            }
            segments = result;
        }

        
        ///<summary>
        /// cleanUpRow(): only cleans up items on same row
        ///</summary>
        private static void cleanUpRow(ref ArrayList lines)
        {
            if (lines.Count < 2) return;
            lineSort(ref lines, X);					//leftmost line at index 0
            for (int a = 0; a < lines.Count; a++)
            {
                Line current = (Line)lines[a];
                for (int b = a; b < lines.Count; b++)
                {
                    Line compare = (Line)lines[b];
                    if (compare.Equals(current))
                        continue;					//same line
                    if (current.p2.X > compare.p1.X)
                    {
                        //Console.WriteLine("******");
                        //dumpSegments(lines);
                        int leftChunk = (compare.p1.X - current.p1.X);
                        int rightChunk = (compare.p2.X - current.p2.X);
                        //Console.WriteLine(current.p2.X+" "+compare.p1.X);
                        //Console.WriteLine("******");
                        lines.Remove(current);		//take out old lines
                        lines.Remove(compare);
                        if (leftChunk != 0)			//left piece
                        {
                            Point p1 = new Point(current.p1.X, current.p1.Y);
                            Point p2 = new Point(current.p1.X + leftChunk, current.p1.Y);
                            if (!equal(p1, p2)) lines.Add(new Line(p1, p2));
                        }
                        if (rightChunk < 0)			//right piece of current
                        {
                            Point p1 = new Point(current.p2.X + rightChunk, current.p1.Y);
                            Point p2 = new Point(current.p2.X, current.p1.Y);
                            if (!equal(p1, p2)) lines.Add(new Line(p1, p2));
                        }
                        if (rightChunk > 0)			//right piece of compare
                        {
                            Point p1 = new Point(current.p2.X, compare.p1.Y);
                            Point p2 = new Point(current.p2.X + rightChunk, compare.p1.Y);
                            if (!equal(p1, p2)) lines.Add(new Line(p1, p2));
                        }
                        //Console.WriteLine("\t***");
                        //dumpSegments(lines);
                        cleanUpRow(ref lines);
                        return;
                    }
                }
            }
        }


        private static bool equal(Point p1, Point p2)
        {
            if (p1.X == p2.X)
                return p1.Y == p2.Y;
            else
                return false;
        }


        // I don't know why unwinding the tail recursion makes it stop breaking
        // on every machine except Deoyani's desktop, where it works either way,
        // but it does. - RCE 1/10/2011
        private static bool lineSort(ref ArrayList segments, int compareBy)
        {
            Tuple ret = new Tuple(false, ref segments, compareBy);
            do
            {
                ret = lineSort2(ref ret.segments, ret.compareBy);
            } while (ret.retval != true);
            return ret.retval;
        }
        //sorts line segments, highest to lowest.  pass X or Y to specify axis
        private static Tuple lineSort2(ref ArrayList segments, int compareBy)
        {
            bool switched = false;
            if (compareBy == Y)
            {
                int last = ((Line)segments[0]).p1.Y;
                for (int x = 1; x < segments.Count; x++)
                {
                    int current = ((Line)segments[x]).p1.Y;
                    if (current < last)
                    {
                        switchItem(ref segments, x, x - 1);
                        switched = true;
                    }
                    last = current;
                }
            }
            else if (compareBy == X)
            {
                int last = ((Line)segments[0]).p1.X;
                for (int x = 1; x < segments.Count; x++)
                {
                    int current = ((Line)segments[x]).p1.X;
                    if (current < last)
                    {
                        switchItem(ref segments, x, x - 1);
                        switched = true;
                    }
                    last = current;
                }
            }
            if (switched)
                return new Tuple(false, ref segments, compareBy);
            ArrayList vo = new ArrayList();
            return new Tuple(true, ref vo, -1);
        }


        private static void switchItem(ref ArrayList list, int i1, int i2)
        {
            Object temp = list[i1];
            list[i1] = list[i2];
            list[i2] = temp;
        }


        //===============================================
        // Debug stuff
        //===============================================
        //For Debug.  Will return straight translation.
        public static ArrayList getAll(String spans)
        {
            ArrayList hSegments = new ArrayList();
            ArrayList vSegments = new ArrayList();
            loadLines(spans, ref hSegments, ref vSegments);
            vSegments.AddRange(hSegments);
            return vSegments;
        }


        //For Debug. prints lists of line segments
        public static void dumpSegments(ArrayList list)
        {
            for (int x = 0; x < list.Count; x++)
            {
                Line line = (Line)list[x];
                Console.WriteLine("(" + line.p1.X + "," + line.p1.Y
                    + "), (" + line.p2.X + "," + line.p2.Y + ")");
            }
        }


        // For Debug. Draw the outline on an image
        public static void drawSegments(ArrayList list)
        {
            Pen pen = new Pen(Color.Green, 1);
            Bitmap bMap = new Bitmap(2000, 500);
            Graphics g = Graphics.FromImage(bMap);
            for (int x = 0; x < list.Count; x++)
            {
                g.DrawLine(pen, ((Line)list[x]).p1, ((Line)list[x]).p2);
            }
            FileStream fs = File.Create("h:\\out.bmp");
            bMap.Save(fs, ImageFormat.Bmp);
            fs.Close();
        }


        /*
            public static void Main(){
            //String TEST = "53,489,491;54,489,492;55,488,492;56,488,492;57,488,492;58,489,491";
            String TEST = "70,415,416;71,412,419;72,412,420;73,411,422;74,410,423;75,405,406;75,409,423;76,404,423;77,404,423;78,403,424;79,403,424;80,404,424;81,404,424;82,404,424;83,403,424;84,403,423;85,404,423;86,403,423;87,403,423;88,402,424;89,403,423;90,403,423;91,403,423;92,402,422;93,402,422;94,403,421;95,404,421;96,404,421;97, 405,420;98,406,419;99,406,419;100,407,414;100,417,418;101,409,412;102,410,411";
            ArrayList al = getPoly(TEST);
            drawSegments(al);
            dumpSegments(al);
            }
        */
    }
}
/* Revision History
        $Log: polyFunk.cs,v $
        Revision 1.6  2003/04/08 17:12:00  nieto
        Trying to include projection.cs class in project
        Revision 1.5  2003/03/13 02:34:58  nieto
        Modify Mask query to include considerated to be important Masks
*/
